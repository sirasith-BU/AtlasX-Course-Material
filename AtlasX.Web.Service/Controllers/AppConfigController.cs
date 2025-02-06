using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AppConfigController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly IDbDataAccessService _dbDataAccessService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly AppSettings _appSettings;
    private const string AppConfigProcedure = "APP_CONFIG_Q";

    public AppConfigController(
        IDiagnosticContext diagnosticContext
        , IDbDataAccessService dbDataAccessService
        , IWebHostEnvironment hostingEnvironment
        , IOptions<AppSettings> appSettings
    )
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _dbDataAccessService = dbDataAccessService;
        _hostingEnvironment = hostingEnvironment;
        _appSettings = appSettings.Value;
    }

    [HttpGet]
    [Authorize]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Get()
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(Request);

        // If found user authentication, add user id to parameters.
        // Or you need to use default configure from database.
        if (User.Identity is { IsAuthenticated: true })
        {
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());

            // Add procedure name to parameter.
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, AppConfigProcedure);
            _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

            try
            {
                using var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
                _diagnosticContext.Set("QueryResultSuccess", queryResult.Success);

                if (queryResult.Success)
                {
                    return Ok(queryResult.ToDictionary());
                }
                else
                {
                    return BadRequest(queryResult.Message);
                }
            }
            catch (Exception ex)
            {
                _diagnosticContext.Set("Exception", ex, true);
                return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
            }
            finally
            {
                queryParameter.Dispose();
            }
        }

        switch (Request.Headers.ContainsKey("Authorization"))
        {
            case false when _appSettings.UM.UseDefaultConfigFromDatabase:
                queryParameter.Add("ROLE_ID", _appSettings.UM.DefaultConfigRoleId);

                // Add procedure name to parameter.
                queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, AppConfigProcedure);
                _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

                try
                {
                    using var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
                    _diagnosticContext.Set("QueryResultSuccess", queryResult.Success);

                    if (queryResult.Success)
                    {
                        return Ok(queryResult.ToDictionary());
                    }
                    else
                    {
                        return BadRequest(queryResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _diagnosticContext.Set("Exception", ex, true);
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
                }
                finally
                {
                    queryParameter.Dispose();
                }

            case false:
            {
                var contentRootPath = _hostingEnvironment.ContentRootPath;
                contentRootPath = Path.Join(contentRootPath, "Config",
                    _hostingEnvironment.IsProduction()
                        ? "app.config.json"
                        : $"app.config.{_hostingEnvironment.EnvironmentName}.json");

                var json = await System.IO.File.ReadAllTextAsync(contentRootPath);

                return Ok(json);
            }
            default:
                return Unauthorized("Invalid token.");
        }
    }
}