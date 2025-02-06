using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Constants;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.ProcedureModels;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.OAuth.Repositories;

public class UserInfoLdapRepository : IUserInfoRepository
{
    private readonly ILogger<UserInfoLdapRepository> _logger;
    private readonly AppSettings _appSettings;
    private readonly IDbDataAccessService _dbDataAccessService;

    public UserInfoLdapRepository(ILogger<UserInfoLdapRepository> logger, IOptions<AppSettings> appSettings,
        IDbDataAccessService dbDataAccessService)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
        _dbDataAccessService = dbDataAccessService;
    }

    public UserInfo Get(string username, string password, string dataSource)
    {
        var ldapHost = _appSettings.LDAP.Host;
        var ldapPort = _appSettings.LDAP.Port;
        var adminUser = _appSettings.LDAP.AdminUser;
        var adminPass = _appSettings.LDAP.AdminPassword;
        var distinguishedName = _appSettings.LDAP.DistinguishedName;
        var secureSocketLayer = _appSettings.LDAP.SecureSocketLayer;
        var usernameField = _appSettings.LDAP.UsernameField;
        var firstNameField = _appSettings.LDAP.FirstNameField;
        var lastNameField = _appSettings.LDAP.LastNameField;
        var mailField = _appSettings.LDAP.MailField;
        var userIdField = _appSettings.LDAP.UserIdField;

        using var ldapConn = new LdapConnection { SecureSocketLayer = secureSocketLayer };
        ldapConn.Connect(ldapHost, ldapPort);
        ldapConn.Bind(adminUser, adminPass);

        var filter = $"({usernameField}={username})";
        var attrs = new[]
        {
            usernameField,
            firstNameField,
            lastNameField,
            mailField,
            userIdField
        };

        var searchResult =
            ldapConn.Search(distinguishedName, LdapConnection.ScopeSub, filter, attrs, false);
        var entry = searchResult.Next();

        try
        {
            ldapConn.Bind(entry.Dn, password);

            username = entry.GetAttribute(usernameField).StringValue;
            password = password.GetSHA256HashString();
            var name = entry.GetAttribute(firstNameField).StringValue;
            var surname = entry.GetAttribute(lastNameField).StringValue;
            var email = entry.GetAttribute(mailField).StringValue;

            var userInfo = Get(username);
            if (userInfo != null)
            {
                UpdatePassword(userInfo.Id, password);
            }
            else
            {
                var userId = Insert(new UmUserIParameter
                {
                    Username = username,
                    Password = password,
                    Name = name,
                    Surname = surname,
                    Email = email,
                    Status = 1, // 1 = Active
                    Source = 2 // Ref. to the source id with LUT_USER_SOURCE table (2 = LDAP)
                });
                if (userId == -1)
                {
                    return null;
                }

                userInfo = Get(userId);
            }

            return userInfo;
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
            return null;
        }
    }

    public UserInfo Get(int userId)
    {
        var parameters = new Dictionary<string, object>
        {
            { _appSettings.Database.ProcedureParameter, ProcedureName.UM_USER_Q },
            { "USER_ID", userId }
        };

        var result = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));

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

    private int Insert(UmUserIParameter parameter)
    {
        var result =
            _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameter.ToDictionary(_appSettings)));
        if (!result.Success)
        {
            _logger.LogError(result.Message);
            return -1;
        }

        var user = result.DataTable.ToDictionary().FirstOrDefault();
        var userId = int.Parse(user.GetValueOrDefault(_appSettings.General.UserIdField)?.ToString() ?? "-1");
        return userId;
    }

    private bool UpdatePassword(int userId, string password)
    {
        var parameters = new Dictionary<string, object>
        {
            { _appSettings.Database.ProcedureParameter, ProcedureName.UM_USER_PWD_U },
            { "USER_ID", userId },
            { "PASSWORD", password },
        };
        var result = _dbDataAccessService.ExecuteProcedure(new QueryParameter(parameters));
        if (!result.Success)
        {
            _logger.LogError(result.Message);
        }

        return result.Success;
    }
}