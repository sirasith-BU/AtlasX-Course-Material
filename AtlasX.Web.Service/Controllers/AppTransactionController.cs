using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.RemoteDirectory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Transactions;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppTransactionController : ControllerBase
{
    // private readonly IHttpClientFactory _clientFactory;
    private enum TransactionType
    {
        Gis,
        File,
        Sp
    }

    private readonly IDiagnosticContext _diagnosticContext;
    private readonly IDbDataAccessService _dbDataAccessService;
    private IHostEnvironment HostEnvironment { get; }

    private readonly List<QueryParameter> _queryParams = new();
    private readonly List<TransactionTypeFile> _fileParams = new();
    private readonly List<TransactionTypeGis> _gisParams = new();

    public AppTransactionController(
        IDiagnosticContext diagnosticContext
        , IDbDataAccessService dbDataAccessService
        , IHostEnvironment hostingEnvironment
    )
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _dbDataAccessService = dbDataAccessService;
        HostEnvironment = hostingEnvironment;
    }

    [NonAction]
    public void AddProcedures(List<QueryParameter> queryParamList)
    {
        foreach (var queryParam in queryParamList)
        {
            AddProcedure(queryParam);
        }
    }

    [NonAction]
    public void AddProcedure(QueryParameter queryParam)
    {
        _queryParams.Add(queryParam);
    }

    [NonAction]
    public void AddFiles(List<TransactionTypeFile> tsFileParameterList)
    {
        foreach (var tsFileParameter in tsFileParameterList)
        {
            AddFile(tsFileParameter);
        }
    }

    [NonAction]
    public void AddFile(TransactionTypeFile tsFileParameter)
    {
        _fileParams.Add(tsFileParameter);
    }

    [NonAction]
    public void AddGisList(List<TransactionTypeGis> tsGisParameterList)
    {
        foreach (var tsGisParameter in tsGisParameterList)
        {
            AddGis(tsGisParameter);
        }
    }

    [NonAction]
    public void AddGis(TransactionTypeGis tsGisParameter)
    {
        _gisParams.Add(tsGisParameter);
    }

    [NonAction]
    public void Execute()
    {
        //File
        var isFileSuccess = ExecuteFiles(_fileParams);

        //SP
        if (isFileSuccess)
        {
            //isSPSuccess = this.ExecuteProcedures(_queryParams);

            var isSpSuccess = true;
            using var scope = new TransactionScope(TransactionScopeOption.Required);
            try
            {
                foreach (var _ in _queryParams
                             .Select(param => _dbDataAccessService.ExecuteProcedure(param, false))
                             .Where(queryResult => !queryResult.Success))
                {
                    isSpSuccess = false;
                }


                //GIS
                if (isSpSuccess)
                {
                    var isGisSuccess = ExecuteGis(_gisParams);
                    if (isGisSuccess)
                    {
                        scope.Complete();
                    }
                    else
                    {
                        scope.Dispose();
                    }
                }
                else
                {
                    RollbackFiles(_fileParams);
                    scope.Dispose();
                }
            }
            catch (Exception)
            {
                RollbackFiles(_fileParams);
                scope.Dispose();
            }
        }
        else
        {
            RollbackFiles(_fileParams);
        }
    }

    private bool ExecuteProcedures(List<QueryParameter> queryParameterList)
    {
        var isSuccess = true;


        using var scope = new TransactionScope(TransactionScopeOption.Required);
        try
        {
            foreach (var _ in queryParameterList.Select(param => _dbDataAccessService.ExecuteProcedure(param, false))
                         .Where(queryResult => !queryResult.Success))
            {
                isSuccess = false;
            }

            scope.Dispose();
        }
        catch (Exception)
        {
            // Log error
            isSuccess = false;
            scope.Dispose();
        }

        return isSuccess;
    }


    private static bool ExecuteFiles(List<TransactionTypeFile> tsFileParameterList)
    {
        const bool isSuccess = true;

        foreach (var tsFileParameter in tsFileParameterList)
        {
            var rem = new RemoteConnector("AtlasX");
            var directoryAccessSource = rem.Connect(); //new DirectoryAccess(tsFileParameter.SOURCE_PATH);
            var
                directoryAccessDestination = rem.Connect(); //new DirectoryAccess(tsFileParameter.DESTINATION_PATH);

            directoryAccessDestination.CreateDirectory(tsFileParameter.DESTINATION_PATH);

            if (!directoryAccessSource.FileExists(Path.Join("Sync", tsFileParameter.SOURCE_PATH,
                    tsFileParameter.SOURCE_FILE))) continue;
            if (directoryAccessDestination.FileExists(Path.Join(tsFileParameter.DESTINATION_PATH,
                    tsFileParameter.DESTINATION_FILE))) continue;
            using var fileStream = directoryAccessSource.GetFile(Path.Join("Sync",
                tsFileParameter.SOURCE_PATH, tsFileParameter.SOURCE_FILE));
            directoryAccessDestination.SaveFile(
                Path.Join(tsFileParameter.DESTINATION_PATH, tsFileParameter.DESTINATION_FILE),
                ReadFully(fileStream));
        }

        return isSuccess;
    }

    private bool ExecuteGis(List<TransactionTypeGis> tsGisParameterList)
    {
        var isSuccess = true;

        foreach (var tsGisParameter in tsGisParameterList)
        {
            var client = new HttpClient();
            using var response = client.PostAsync(tsGisParameter.URL,
                new StringContent("edits=" + tsGisParameter.JSON, Encoding.UTF8,
                    "application/x-www-form-urlencoded")).Result;
            isSuccess = response.IsSuccessStatusCode;
        }

        return isSuccess;
    }

    private bool RollbackFiles(List<TransactionTypeFile> tsFileParameterList)
    {
        const bool isSuccess = true;

        foreach (var tsFileParameter in tsFileParameterList)
        {
            var pathSource = Path.Join(HostEnvironment.ContentRootPath, tsFileParameter.SOURCE_PATH);
            var pathDestination = Path.Join(HostEnvironment.ContentRootPath, tsFileParameter.DESTINATION_PATH);

            var directoryAccessDestination = new DirectoryAccess(pathDestination);

            if (directoryAccessDestination.FileExists(tsFileParameter.DESTINATION_FILE))
            {
                directoryAccessDestination.RemoveFile(tsFileParameter.DESTINATION_FILE);
            }
        }

        return isSuccess;
    }


    private static byte[] ReadFully(Stream input)
    {
        var buffer = new byte[16 * 1024];
        using var ms = new MemoryStream();
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, read);
        }

        return ms.ToArray();
    }
}

public class TransactionOrder
{
    public TransactionWork[] Works { get; set; }
}

public class TransactionWork
{
    public int WorkId { get; set; }
    public Transaction[] Transactions { get; set; }
}

public class Transaction
{
    public int TRANSACTION_ID { get; set; }
    public string TRANSACTION_TYPE { get; set; }
    public Dictionary<string, object> PARAMETERS { get; set; }
}

public class TransactionTypeFile
{
    public string SOURCE_PATH { get; set; }
    public string SOURCE_FILE { get; set; }
    public string DESTINATION_PATH { get; set; }
    public string DESTINATION_FILE { get; set; }
}

public class TransactionTypeGis
{
    public string URL { get; set; }
    public string JSON { get; set; }
}