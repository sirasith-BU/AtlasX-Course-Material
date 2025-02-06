using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AtlasX.Web.Service.OAuth.Repositories;

public class AuthorizationCodeInMemoryRepository : IAuthorizationCodeRepository
{
    private readonly List<AuthorizationCode> _authorizationCodes = new List<AuthorizationCode>();

    public AuthorizationCode Get(string code)
    {
        var authorizationCode = _authorizationCodes.FirstOrDefault(a => a.Code == code);
        return authorizationCode;
    }

    public AuthorizationCode Add(string codeChallenge, string codeChallengeMethod, string code
        , int userId, int authorizationCodeExpires, string clientId, string redirectUri, string checkSum)
    {
        var authorizationCodes = new AuthorizationCode
        {
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            UserId = userId,
            Issued = DateTime.Now,
            Expires = DateTime.Now.AddSeconds(authorizationCodeExpires),
            ClientId = clientId,
            Code = code,
            RedirectUri = redirectUri,
            CheckSum = checkSum
        };

        _authorizationCodes.Add(authorizationCodes);

        return authorizationCodes;
    }

    public void Remove(string code)
    {
        _authorizationCodes.RemoveAll(a => a.Code == code);
    }

    public void RemoveExpired()
    {
        _authorizationCodes.RemoveAll(a => a.Expires.CompareTo(DateTime.Now) < 0);
    }
}