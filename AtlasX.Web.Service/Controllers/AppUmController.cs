using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Constants;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppUmController : ControllerBase
{
    private readonly ILogger<AppUmController> _logger;
    private readonly IDbDataAccessService _dbDataAccessService;
    private readonly AppSettings _appSettings;

    public AppUmController(ILogger<AppUmController> logger, IDbDataAccessService dbDataAccessService,
        IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _dbDataAccessService = dbDataAccessService;
        _appSettings = appSettings.Value;
    }

    [HttpGet("users/{userId}")]
    public IActionResult GetUser(string userId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_Q");
            queryParameter.Add(_appSettings.General.UserIdField, userId);

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("users/exists")]
    public IActionResult UserExists()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_EXISTS");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("users/search")]
    public IActionResult SearchUser()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_SEARCH_Q");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPost("users")]
    public IActionResult CreateUser()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_I");
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());

            if (queryParameter.Parameters.ContainsKey("PASSWORD"))
            {
                var password = queryParameter["PASSWORD"].ToString();
                var hashPassword = password.GetSHA256HashString();
                queryParameter.Add("PASSWORD", hashPassword);
            }

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPut("users/{userId}")]
    public IActionResult UpdateUser(string userId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_U");
            queryParameter.Add(_appSettings.General.UserIdField, userId);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPut("users/password")]
    public IActionResult UpdatePassword()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_PWD_U");

            if (queryParameter.Parameters.ContainsKey("PASSWORD"))
            {
                var password = queryParameter["PASSWORD"].ToString();
                var hashPassword = password.GetSHA256HashString();
                queryParameter.Add("PASSWORD", hashPassword);
            }

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPut("users/{userId}/password")]
    public IActionResult UpdateUserPassword(string userId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_PWD_U");
            queryParameter.Add(_appSettings.General.UserIdField, userId);

            if (queryParameter.Parameters.ContainsKey("PASSWORD"))
            {
                var password = queryParameter["PASSWORD"].ToString();
                var hashPassword = password.GetSHA256HashString();
                queryParameter.Add("PASSWORD", hashPassword);
            }

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpDelete("users")]
    public IActionResult DeleteUser()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_USER_D");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    public IActionResult ForgetPassword()
    {
        var queryParameter = new QueryParameter(Request);
        var userId = queryParameter[_appSettings.General.UserIdField].ToString();
        queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.UM_USER_Q);

        var result = _dbDataAccessService.ExecuteProcedure(queryParameter);

        if (result.Success)
        {
            var email = result.DataTable.Rows[0]["EMAIL"].ToString();
            var token = Guid.NewGuid().ToString("n").GetSHA256HashString();
            queryParameter = new QueryParameter();
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter,
                ProcedureName.APP_FORGET_PWD_TOKEN_I);
            queryParameter.Add(_appSettings.General.UserIdField, userId);
            queryParameter.Add("TOKEN", token);

            var resetPasswordResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (resetPasswordResult.Success)
            {
                var baseUrl = _appSettings.UM.ForgetPasswordBaseUrl;
                var userIdField = _appSettings.UM.ForgetPasswordUserIdField;
                var tokenField = _appSettings.UM.ForgetPasswordTokenField;
                var passwordResetUrl = $"{baseUrl}?{userIdField}={userId}&{tokenField}={token}";
                queryParameter = new QueryParameter();
                queryParameter.Add("MAIL_TO", email);
                queryParameter.Add("MAIL_SUBJECT", "Reset Password");
                queryParameter.Add("MAIL_BODY", $@"
                        <h1>Reset Password</h1>
                        <div>
                            You have requested to reset password for account {userId} <br/>
                            <b>Please contact administrator if you have not issued reset password request.</b>
                        </div>
                        <br/>
                        Click <a href=""{passwordResetUrl}"">here</a> to reset password.
                    ");

                // TODO: Send mail
                // result = MailUtil.SendEmail(mailParameter);
            }
        }
        else
        {
            return StatusCode(StatusCodes.Status500InternalServerError, result.Message);
        }

        return Ok(result.ToDictionary());
    }

    [HttpGet("roles")]
    public IActionResult GetRole()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_LIST_Q");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("roles/{roleId}")]
    public IActionResult GetRole(string roleId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_Q");
            queryParameter.Add("ROLE_ID", roleId);

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("roles/search")]
    public IActionResult SearchRole()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_SEARCH_Q");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("roles/{roleId}/users")]
    public IActionResult SearchUserByRole(string roleId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_USER_SEARCH_Q");
            queryParameter.Add("ROLE_ID", roleId);

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPost("roles")]
    public IActionResult CreateRole()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_I");
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPut("roles/{roleId}")]
    public IActionResult UpdateRole(string roleId)
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_U");
            queryParameter.Add("ROLE_ID", roleId);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.UserIdProcedureParameter, User.GetUserId());

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpDelete("roles")]
    public IActionResult DeleteRole()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_ROLE_D");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpGet("permissions")]
    public IActionResult GetPermission()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_PERM_FUNCTION_ROLE_Q");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }

    [HttpPut("permissions")]
    public IActionResult UpdatePermission()
    {
        try
        {
            var queryParameter = new QueryParameter(Request);
            queryParameter.Add(_dbDataAccessService.DatabaseConfigure.ProcedureParameter, "UM_PERM_FUNCTION_ROLE_I");

            var queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);

            if (queryResult.Success)
            {
                return Ok(queryResult.ToDictionary());
            }

            _logger.LogError(queryResult.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, queryResult.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.GetBaseException().Message);
        }
    }
}