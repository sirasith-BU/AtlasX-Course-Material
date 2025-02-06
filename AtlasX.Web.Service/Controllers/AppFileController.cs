using AtlasX.Engine.Connector;
using AtlasX.Engine.Extensions;
using AtlasX.Engine.RemoteDirectory;
using AtlasX.Engine.RemoteDirectory.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppFileController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly IDirectoryAccessService _directoryAccessService;

    public AppFileController(
        IDiagnosticContext diagnosticContext,
        IDirectoryAccessService directoryAccessService
    )
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _directoryAccessService = directoryAccessService;
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
            DataRow dataRow;

            queryParameter.FileParameters.ForEach(file =>
            {
                dataRow = queryResult.DataTable.NewRow();
                dataRow["PARAMETER_NAME"] = file.ParameterName;
                dataRow["FILE_PATH"] = directoryAccess.PathName;
                dataRow["FILE_ID"] = file.FileId;
                dataRow["FILE_NAME"] = file.FileName;
                dataRow["CONTENT_TYPE"] = file.ContentType;

                try
                {
                    directoryAccess.SaveFile(file.FileId, file.FileContent);
                    dataRow["STATUS"] = true;
                    dataRow["DESCRIPTION"] = "";
                }
                catch (Exception ex)
                {
                    dataRow["STATUS"] = false;
                    dataRow["DESCRIPTION"] = ex.Message;
                }

                queryResult.DataTable.Rows.Add(dataRow);
            });

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

    [HttpGet("download")]
    public IActionResult Download()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);
        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        string fileId;

        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_directoryAccessService.FileServerConfigure.FileIdParameter))
        {
            fileId = queryParameter[_directoryAccessService.FileServerConfigure.FileIdParameter].ToString();
        }
        else
        {
            return BadRequest(
                $"The '{_directoryAccessService.FileServerConfigure.FileIdParameter}' can't be undefined or empty value.");
        }

        try
        {
            var directoryAccess = _directoryAccessService.CreateDirectoryAccess(queryParameter);

            if (directoryAccess.FileExists(fileId))
            {
                return File(directoryAccess.GetFile(fileId), "application/octet-stream", fileId);
            }
            else
            {
                return NotFound($"The '{fileId}' doesn't exist in file source.");
            }
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
            return BadRequest(ex.Message);
        }
        finally
        {
            queryParameter.Dispose();
        }
    }

    [HttpGet("preview")]
    [Authorize]
    [AllowAnonymous]
    public IActionResult Preview()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);
        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        string fileId;

        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_directoryAccessService.FileServerConfigure.FileIdParameter))
        {
            fileId = queryParameter[_directoryAccessService.FileServerConfigure.FileIdParameter].ToString();
        }
        else
        {
            return BadRequest(
                $"The '{_directoryAccessService.FileServerConfigure.FileIdParameter}' can't be undefined or empty value.");
        }

        try
        {
            var directoryAccess = _directoryAccessService.CreateDirectoryAccess(queryParameter);
            if (directoryAccess.FileExists(fileId))
            {
                var extension = directoryAccess.FileExtension(fileId, true);

                if (!Enum.TryParse(extension, out MimeType mimeType))
                {
                    mimeType = MimeType.download;
                }

                var description = (mimeType.GetType()
                        .GetMember(mimeType.ToString())
                        .FirstOrDefault() ?? throw new InvalidOperationException())
                    .GetCustomAttribute<DescriptionAttribute>()
                    ?.Description;

                return File(directoryAccess.GetFile(fileId), description ?? string.Empty);
            }
            else
            {
                return NotFound($"The '{fileId}' doesn't exist in file source.");
            }
        }
        catch (Exception ex)
        {
            _diagnosticContext.Set("Exception", ex, true);
            return BadRequest(ex.Message);
        }
        finally
        {
            queryParameter.Dispose();
        }
    }

    [HttpDelete("remove")]
    public IActionResult Remove()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);
        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        var queryResult = new QueryResult();
        var fileList = new List<string>();
        DataRow dataRow;

        // Set column definition.
        queryResult.DataTable.Columns.Add("NAME", typeof(string));
        queryResult.DataTable.Columns.Add("STATUS", typeof(bool));
        queryResult.DataTable.Columns.Add("DESCRIPTION", typeof(string));

        if (queryParameter.ContainsKeyAndValueIsNotEmpty(_directoryAccessService.FileServerConfigure.FileListParameter))
        {
            fileList = queryParameter[_directoryAccessService.FileServerConfigure.FileListParameter].ToString()
                ?.Split(";").ToList();
        }

        try
        {
            var directoryAccess = _directoryAccessService.CreateDirectoryAccess(queryParameter);

            // Both the PathName and fileList is empty values, web service will return bad request response.
            if (fileList != null && string.IsNullOrEmpty(directoryAccess.PathName) && fileList.Count == 0)
            {
                return BadRequest(
                    $"'{_directoryAccessService.FileServerConfigure.FilePathParameter}' or '{_directoryAccessService.FileServerConfigure.FileListParameter}' parameters can't be undefined or empty values.");
            }

            fileList?.ForEach(item =>
            {
                dataRow = queryResult.DataTable.NewRow();
                dataRow["NAME"] = item;

                if (directoryAccess.FileExists(item))
                {
                    try
                    {
                        directoryAccess.RemoveFile(item);
                        dataRow["STATUS"] = true;
                        dataRow["DESCRIPTION"] = "File deleted";
                    }
                    catch (Exception ex)
                    {
                        dataRow["STATUS"] = false;
                        dataRow["DESCRIPTION"] = ex.Message;
                    }
                }
                else if (directoryAccess.DirectoryExists(item))
                {
                    try
                    {
                        directoryAccess.RemoveDirectory(item);
                        dataRow["STATUS"] = true;
                        dataRow["DESCRIPTION"] = "Directory deleted";
                    }
                    catch (Exception ex)
                    {
                        dataRow["STATUS"] = false;
                        dataRow["DESCRIPTION"] = ex.Message;
                    }
                }
                else
                {
                    dataRow["STATUS"] = false;
                    dataRow["DESCRIPTION"] = "File or directory doesn't exist";
                }

                queryResult.DataTable.Rows.Add(dataRow);
            });

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
            fileList?.Clear();
        }
    }
}