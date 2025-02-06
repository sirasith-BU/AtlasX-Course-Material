using AtlasX.Web.Service.OAuth.Models;

namespace AtlasX.Web.Service.OAuth.Repositories.Interfaces;

public interface IAuthorizationCodeRepository
{
    AuthorizationCode Get(string code);
    AuthorizationCode Add(string codeChallenge, string codeChallengeMethod, string code, int userId,
        int authorizationCodeExpires, string clientId, string redirectUri, string checkSum);
    void Remove(string code);
    void RemoveExpired();
}