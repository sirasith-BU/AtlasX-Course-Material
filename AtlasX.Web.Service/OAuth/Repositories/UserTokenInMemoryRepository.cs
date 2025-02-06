using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.OAuth.Repositories;

public class UserTokenInMemoryRepository : IUserTokenRepository
{
    private readonly List<UserToken> _userTokens = new List<UserToken>();

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
        _userTokens.Add(userToken);
        return userToken;
    }

    public UserToken Get(string refreshToken)
    {
        return _userTokens.FirstOrDefault(r => r.RefreshToken == refreshToken);
    }

    public UserToken Get(int userId, string nonce)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<UserToken> GetAll()
    {
        return _userTokens;
    }

    public bool IsAlive(int userId)
    {
        return _userTokens.Exists(r => r.UserId == userId && r.Expires.CompareTo(DateTime.Now) > 0);
    }

    public void Remove(string refreshToken)
    {
        _userTokens.RemoveAll(r => r.RefreshToken == refreshToken);
    }

    public void Remove(int userId)
    {
        _userTokens.RemoveAll(r => r.UserId == userId);
    }

    public void RemoveExpired()
    {
        _userTokens.RemoveAll(r => r.Expires.CompareTo(DateTime.Now) < 0);
    }

    public void UpdateFcmToken(int userId, string nonce, string fcmToken)
    {
        var refreshToken = _userTokens.FirstOrDefault(r => r.UserId == userId && r.Nonce == nonce);
        if (refreshToken != null)
        {
            refreshToken.FcmToken = fcmToken;
        }
    }

    public IEnumerable<string> GetRefreshTokens(int userId)
    {
        return _userTokens.Where(r => r.UserId == userId).Select(r => r.FcmToken).ToList();
    }
}