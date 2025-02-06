using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Extensions;
using AtlasX.Engine.RemoteDirectory.Services;
using AtlasX.Web.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppSyncOfflineController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly IDbDataAccessService _dbDataAccessService;
    private readonly AppSettings _appSettings;
    private readonly AppTransactionController _appTransaction;
    private readonly IDirectoryAccessService _directoryAccessService;

    public AppSyncOfflineController(IDiagnosticContext diagnosticContext, IDbDataAccessService dbDataAccessService,
        IOptions<AppSettings> appSettings, IWebHostEnvironment hostingEnvironment,
        IDirectoryAccessService directoryAccessService)
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _appSettings = appSettings.Value;
        _dbDataAccessService = dbDataAccessService;
        _directoryAccessService = directoryAccessService;
        _appTransaction = new AppTransactionController(_diagnosticContext, _dbDataAccessService, hostingEnvironment);
    }


    [HttpPost("upload")]
    public IActionResult Upload()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);
        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        // Create QueryResult and set column definition.
        var queryResult = new QueryResult();
        queryResult.DataTable.Columns.Add("PARAMETER_NAME", typeof(string));
        queryResult.DataTable.Columns.Add("FILE_PATH", typeof(string));
        queryResult.DataTable.Columns.Add("FILE_ID", typeof(string));
        queryResult.DataTable.Columns.Add("FILE_NAME", typeof(string));
        queryResult.DataTable.Columns.Add("CONTENT_TYPE", typeof(string));
        queryResult.DataTable.Columns.Add("STATUS", typeof(bool));
        queryResult.DataTable.Columns.Add("DESCRIPTION", typeof(string));

        try
        {
            var directoryAccess = _directoryAccessService.CreateDirectoryAccess(queryParameter);

            using var file = queryParameter.FileParameters.GetEnumerator();

            while (file.MoveNext())
            {
                var dataRow = queryResult.DataTable.NewRow();
                dataRow["PARAMETER_NAME"] = file.Current.ParameterName;
                dataRow["FILE_PATH"] = directoryAccess.PathName;
                dataRow["FILE_ID"] = file.Current.FileId;
                dataRow["FILE_NAME"] = file.Current.FileName;
                dataRow["CONTENT_TYPE"] = file.Current.ContentType;

                try
                {
                    directoryAccess.SaveFile(file.Current.FileId, file.Current.FileContent);
                    dataRow["STATUS"] = true;
                    dataRow["DESCRIPTION"] = "";

                    //Call SP
                    var qParam = new QueryParameter();
                    qParam.Add("APP_DATA_PROCEDURE", "APP_SYNC_OFFLINE_I");
                    qParam.Add("USER_ID", queryParameter.Parameters["USER_ID"]);
                    qParam.Add("WORK_IDS", queryParameter.Parameters["WORK_IDS"]);
                    qParam.Add("FILE_PATH",
                        Path.Combine(directoryAccess.DestinationPath, file.Current.FileId).ToString());
                    var queryResultSyncOffline = _dbDataAccessService.ExecuteProcedure(qParam);
                }
                catch (Exception ex)
                {
                    dataRow["STATUS"] = false;
                    dataRow["DESCRIPTION"] = ex.Message;
                }

                queryResult.DataTable.Rows.Add(dataRow);
            }

            return Ok(queryResult.ToDictionary());
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
            return BadRequest(ex.Message);
        }
        finally
        {
            queryParameter.Dispose();
            queryResult.Dispose();
        }
    }


    [HttpPost("execute")]
    public IActionResult Execute()
    {
        var queryParameter = new QueryParameter(Request);
        var queryResult = new QueryResult();

        try
        {
            //Call SP
            var param = new QueryParameter();
            param.Add("APP_DATA_PROCEDURE", "APP_SYNC_OFFLINE_Q");
            param.Add("USER_ID", queryParameter.Parameters["USER_ID"]);
            param.Add("WORK_IDS", queryParameter.Parameters["WORK_IDS"]);

            var queryResultAppSyncQ = _dbDataAccessService.ExecuteProcedure(param);

            //Extract Zip
            //DirectoryAccess directoryAccess = _directoryAccessService.GetFileSource()

            if (!queryResultAppSyncQ.Success || queryResultAppSyncQ.DataTable.Rows.Count <= 0)
                return Ok(queryResult.ToDictionary());

            var zipPath = Path.Combine(queryResultAppSyncQ.DataTable.Rows[0]["FILE_PATH"].ToString() ??
                                       throw new InvalidOperationException());
            var extractPath = Path.GetDirectoryName(zipPath);
            if (extractPath != null && Directory.Exists(Path.Combine(extractPath, "Sync")))
            {
                Directory.Delete(Path.Combine(extractPath, "Sync"), true);
            }

            ZipFile.ExtractToDirectory(zipPath, extractPath ?? throw new InvalidOperationException());

            //Call
            ExecuteAppTransaction(Path.Join(extractPath, "Sync", "TransactionOrder.json"));

            return Ok(queryResult.ToDictionary());
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
            return BadRequest(ex.Message);
        }
        finally
        {
            queryParameter.Dispose();
            queryResult.Dispose();
        }
    }


    public bool ExecuteAppTransaction(string pathFileTransaction)
    {
        var isSuccess = false;
        //CHANGE TO GET FILE NAME
        //string pathFileTransaction = Path.Join(_hostEnvironment.ContentRootPath, "Offline", "TransactionOrder.json");

        var jsonParameters = ReadTransactionOrder(pathFileTransaction);

        foreach (var transactionWork in jsonParameters.Works)
        {
            foreach (var transaction in transactionWork.Transactions)
            {
                switch (transaction.TRANSACTION_TYPE)
                {
                    case "SP":
                        var queryParameter = new QueryParameter(transaction.PARAMETERS);
                        _appTransaction.AddProcedure(queryParameter);
                        break;
                    case "FILE":
                        //TransactionTypeFile tsFileParameter = transaction.PARAMETERS as TransactionTypeFile;
                        var json = JsonSerializer.Serialize(transaction.PARAMETERS);
                        var tsFileParameter = JsonSerializer.Deserialize<TransactionTypeFile>(json);
                        _appTransaction.AddFile(tsFileParameter);
                        break;
                    case "GIS":
                        var jsonGis = JsonSerializer.Serialize(transaction.PARAMETERS);
                        var tsGisParameter = JsonSerializer.Deserialize<TransactionTypeGis>(jsonGis);
                        _appTransaction.AddGis(tsGisParameter);
                        break;
                    default:
                        Console.WriteLine("Default case");
                        break;
                }
            }
        }

        _appTransaction.Execute();
        //fix
        isSuccess = true;

        return isSuccess;
    }

    private TransactionOrder ReadTransactionOrder(string pathFileTransaction)
    {
        TransactionOrder jsonParameters = null;

        try
        {
            if (System.IO.File.Exists(pathFileTransaction))
            {
                //read json file & serialize to TransactionOrder
                var jsonContent = System.IO.File.ReadAllText(pathFileTransaction);
                jsonParameters = JsonSerializer.Deserialize<TransactionOrder>(jsonContent);
            }
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
        }

        return jsonParameters;
    }

    private T GetObject<T>(Dictionary<string, object> dict)
    {
        var type = typeof(T);
        var obj = Activator.CreateInstance(type);

        foreach (var kv in dict)
        {
            //type.GetProperty(kv.Key).SetValue(kv.Key, kv.Value);
            type.GetProperty(kv.Key)?.SetValue(obj, Convert.ChangeType(kv.Value, type));
        }

        return (T)obj;
    }
}