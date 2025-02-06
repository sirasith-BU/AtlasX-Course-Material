using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Constants;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.Repositories;

public class UserInfoRepository : IUserInfoRepository
{
    private readonly ILogger<UserInfoRepository> _logger;
    private readonly IDbDataAccessService _dbDataAccessService;

    public UserInfoRepository(ILogger<UserInfoRepository> logger, IDbDataAccessService dbDataAccessService)
    {
        _logger = logger;
        _dbDataAccessService = dbDataAccessService;
    }

    public UserInfo Get(string username, string password, string dataSource)
    {
        password = password.GetSHA256HashString();
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_LOGIN_Q },
            { "USERNAME", username },
            { "PASSWORD", password }
        };

        var queryParams = new QueryParameter(parameters);
        var result = _dbDataAccessService.ExecuteProcedure(queryParams);

        if (!result.Success)
        {
            return null;
        }

        var userInfoResult = result.DataTable.ToDictionary().FirstOrDefault();
        if (userInfoResult == null)
        {
            return null;
        }

        var userInfo = new UserInfo(userInfoResult);

        return userInfo;
    }

    public UserInfo Get(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.UM_USER_Q },
            { "USER_ID", userId }
        };

        var queryParams = new QueryParameter(parameters);
        var result = _dbDataAccessService.ExecuteProcedure(queryParams);

        if (!result.Success)
        {
            _logger.LogError(result.Message);
            return null;
        }

        var userInfoResult = result.DataTable.ToDictionary().FirstOrDefault();
        if (userInfoResult == null)
        {
            return null;
        }

        var userInfo = new UserInfo(userInfoResult);

        return userInfo;
    }

    public UserInfo Get(string username)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.UM_USER_ID_Q },
            { "USERNAME", username }
        };

        var queryParams = new QueryParameter(parameters);
        var result = _dbDataAccessService.ExecuteProcedure(queryParams);

        if (!result.Success)
        {
            _logger.LogError(result.Message);
            return null;
        }

        var userInfoResult = result.DataTable.ToDictionary().FirstOrDefault();
        if (userInfoResult == null)
        {
            return null;
        }

        var userInfo = new UserInfo(userInfoResult);

        return userInfo;
    }
}