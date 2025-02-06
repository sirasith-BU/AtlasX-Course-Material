using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Constants;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.OAuth.Repositories;

public class UserTokenRepository : IUserTokenRepository
{
    private readonly ILogger<UserTokenRepository> _logger;
    private readonly IDbDataAccessService _dbDataAccessService;

    public UserTokenRepository(ILogger<UserTokenRepository> logger, IDbDataAccessService dbDataAccessService)
    {
        _logger = logger;
        _dbDataAccessService = dbDataAccessService;
    }

    public UserToken Add(string refreshToken, int userId, string clientId, string nonce, int refreshTokenExpires,
        string checksum)
    {
        var userToken = new UserToken
        {
            UserId = userId,
            Expires = DateTime.Now.AddSeconds(refreshTokenExpires),
            Issued = DateTime.Now,
            RefreshToken = refreshToken,
            ClientId = clientId,
            Nonce = nonce,
            CheckSum = checksum
        };

        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_I },
            { "USER_ID", userToken.UserId },
            { "CREATED_DATE", userToken.Issued.ToUnixTimeMilliseconds() },
            { "EXPIRED_DATE", userToken.Expires.ToUnixTimeMilliseconds() },
            { "REFRESH_TOKEN", userToken.RefreshToken },
            { "FCM_TOKEN", userToken.FcmToken },
            { "CLIENT_ID", userToken.ClientId },
            { "NONCE", userToken.Nonce },
            { "CHECK_SUM", userToken.CheckSum }
        };
        var queryParams = new QueryParameter(parameters);
        var queryResult = _dbDataAccessService.ExecuteProcedure(queryParams);
        if (queryResult.Success)
        {
            return userToken;
        }

        _logger.LogError(queryResult.Message);
        return null;
    }

    public UserToken Get(string refreshToken)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_Q },
            { "REFRESH_TOKEN", refreshToken },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            var userToken = queryResult.DataTable
                .ToDictionary()
                .Select(q => new UserToken(q))
                .FirstOrDefault();
            return userToken;
        }

        _logger.LogError(queryResult.Message);
        return null;
    }

    public IEnumerable<UserToken> GetAll()
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_Q },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            var userTokens = queryResult.DataTable
                .ToDictionary()
                .Select(result => new UserToken(result));
            return userTokens;
        }

        _logger.LogError(queryResult.Message);
        return new List<UserToken>();
    }

    public UserToken Get(int userId, string nonce)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_Q },
            { "USER_ID", userId },
            { "NONCE", nonce }
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success && queryResult.DataTable.Rows.Count > 0)
        {
            UserToken userToken = queryResult.DataTable
                .ToDictionary()
                .Select(q => new UserToken(q))
                .FirstOrDefault();
            return userToken;
        }

        _logger.LogError(queryResult.Message);
        return null;
    }

    public bool IsAlive(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_Q },
            { "USER_ID", userId },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            var userTokens = queryResult.DataTable
                .ToDictionary()
                .Select(result => new UserToken(result))
                .Where(u => u.Expires.CompareTo(DateTime.Now) > 0);
            return userTokens.Any();
        }

        _logger.LogError(queryResult.Message);
        return false;
    }

    public void Remove(string refreshToken)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_D },
            { "REFRESH_TOKEN", refreshToken },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            _logger.LogInformation("Success");
        }
        else
        {
            _logger.LogError(queryResult.Message);
        }
    }

    public void Remove(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_D },
            { "USER_ID", userId },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            _logger.LogInformation("Success");
        }
        else
        {
            _logger.LogError(queryResult.Message);
        }
    }

    public void RemoveExpired()
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_EXPIRED_D },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            _logger.LogInformation("Success");
        }
        else
        {
            _logger.LogError(queryResult.Message);
        }
    }

    public void UpdateFcmToken(int userId, string nonce, string fcmToken)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_FCM_U },
            { "USER_ID", userId },
            { "NONCE", nonce },
            { "FCM_TOKEN", fcmToken },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            _logger.LogInformation("Success");
        }
        else
        {
            _logger.LogError(queryResult.Message);
        }
    }

    public IEnumerable<string> GetRefreshTokens(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { _dbDataAccessService.DatabaseConfigure.ProcedureParameter, ProcedureName.APP_USER_TOKEN_Q },
            { "USER_ID", userId },
        };
        var queryResult = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (queryResult.Success)
        {
            IEnumerable<string> userTokens = queryResult.DataTable
                .ToDictionary()
                .Select(q => new UserToken(q))
                .Select(q => q.RefreshToken);
            return userTokens;
        }

        _logger.LogError(queryResult.Message);
        return new List<string>();
    }
}