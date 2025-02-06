using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Notification.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Collections.Generic;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize] // We recommended. Required access_token in authorization request header.
[AllowAnonymous] // For development only, but can be used in production if necessary.
public class AppDataController : ControllerBase
{
    private readonly IDiagnosticContext _diagnosticContext;
    private readonly IDbDataAccessService _dbDataAccessService;
    private readonly INotificationService _notificationService;

    public AppDataController(
        IDiagnosticContext diagnosticContext
        , IDbDataAccessService dbDataAccessService
        , INotificationService notificationService
    )
    {
        _diagnosticContext = diagnosticContext ?? throw new ArgumentNullException(nameof(diagnosticContext));
        _dbDataAccessService = dbDataAccessService;
        _notificationService = notificationService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Get()
    {
        return ExecuteProcedure(Request);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Post()
    {
        return ExecuteProcedure(Request);
    }

    private IActionResult ExecuteProcedure(HttpRequest request)
    {
        _diagnosticContext.Set("UserId", User.GetUserId());

        var queryParameter = new QueryParameter(request);

        if (!ValidateSecureProcedure(_dbDataAccessService.DatabaseConfigure.SecureProcedures,
                queryParameter[_dbDataAccessService.DatabaseConfigure.ProcedureParameter].ToString()))
        {
            return Unauthorized(
                $"The {queryParameter[_dbDataAccessService.DatabaseConfigure.ProcedureParameter]} procedure does not execute! Its require authentication.");
        }

        // If found user authentication, add user id to parameters.
        if (User.Identity is { IsAuthenticated: true })
        {
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());
        }

        _diagnosticContext.Set("QueryParameter", queryParameter.Parameters);

        try
        {
            using var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            _diagnosticContext.Set("QueryResultSuccess", queryResult.Success);

            if (queryResult.Success)
            {
                if (queryResult.NotiTable.Rows.Count > 0)
                {
                    _notificationService.SendMessageAsync(queryResult.NotiTable);
                }

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

    private bool ValidateSecureProcedure(List<string> secureProcedures, string procedureName)
    {
        var matchProcedure = secureProcedures.Find((procedure => procedure == procedureName));

        return User.Identity != null && (string.IsNullOrEmpty(matchProcedure) || User.Identity.IsAuthenticated);
    }
}