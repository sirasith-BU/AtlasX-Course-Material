using AtlasX.Web.Service.OAuth.Models;
using System.Collections.Generic;

namespace AtlasX.Web.Service.OAuth.Repositories.Interfaces;

public interface IUserTokenRepository
{
    UserToken Add(string refreshToken, int userId, string clientId, string nonce, int refreshTokenExpires,
        string checksum);

    UserToken Get(string refreshToken);
    UserToken Get(int userId, string nonce);
    IEnumerable<UserToken> GetAll();
    bool IsAlive(int userId);
    void Remove(string refreshToken);
    void Remove(int userId);
    void RemoveExpired();
    void UpdateFcmToken(int userId, string nonce, string fcmToken);
    IEnumerable<string> GetRefreshTokens(int userId);
}