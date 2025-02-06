using System;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using AtlasX.Web.Service.Repositories;
using Microsoft.Extensions.Logging;

namespace AtlasX.Web.Service.OAuth.Repositories;

public class UserInfoMultiSourceRepository : IUserInfoRepository
{
    private readonly ILogger<UserInfoMultiSourceRepository> _logger;

    // Inject repositories here
    private readonly UserInfoLdapRepository _userInfoLdapRepository;
    private readonly UserInfoRepository _userInfoRepository;

    public UserInfoMultiSourceRepository(
        ILogger<UserInfoMultiSourceRepository> logger,
        UserInfoLdapRepository userInfoLdapRepository,
        UserInfoRepository userInfoRepository
    )
    {
        _logger = logger;
        _userInfoLdapRepository = userInfoLdapRepository;
        _userInfoRepository = userInfoRepository;
    }

    public UserInfo Get(string username, string password, string dataSource)
    {
        UserInfo userInfo;

        // LDAP
        try
        {
            userInfo = _userInfoLdapRepository.Get(username, password, dataSource);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        // Database
        try
        {
            userInfo = _userInfoRepository.Get(username, password, dataSource);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return null;
    }

    public UserInfo Get(int userId)
    {
        UserInfo userInfo;

        // LDAP
        try
        {
            userInfo = _userInfoLdapRepository.Get(userId);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        // Database
        try
        {
            userInfo = _userInfoRepository.Get(userId);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return null;
    }

    public UserInfo Get(string username)
    {
        UserInfo userInfo;

        // LDAP
        try
        {
            userInfo = _userInfoLdapRepository.Get(username);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        // Database
        try
        {
            userInfo = _userInfoRepository.Get(username);
            if (userInfo != null)
            {
                return userInfo;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message);
        }

        return null;
    }
}