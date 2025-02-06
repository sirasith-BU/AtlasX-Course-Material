using AtlasX.Engine.AppSettings;
using AtlasX.Engine.Connector;
using AtlasX.Engine.Connector.Services;
using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using AtlasX.Web.Service.Mail;
using AtlasX.Web.Service.Mail.Services;
using AtlasX.Web.Service.OAuth.Dto;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.OAuth.Services;

public class AppAuthenService : IAppAuthenService
{
    private readonly FacebookService _facebookService;
    private readonly HttpClient _httpClient;
    private readonly AppSettings _appSettings;
    private readonly IDbDataAccessService _dbDataAccessService;
    private readonly IAppMailService _appMailService;
    private Random _random = new Random();

    public AppAuthenService(
        IOptions<AppSettings> appSettings
        , IDbDataAccessService dbDataAccessService
        , IAppMailService appMailService
    )
    {
        _httpClient = new HttpClient();
        _facebookService = new FacebookService(_httpClient);

        _appSettings = appSettings.Value;
        _dbDataAccessService = dbDataAccessService;
        _appMailService = appMailService;
    }

    public IActionResult Authorization(ControllerBase controllerBase,
        IAuthorizationCodeRepository authorizationCodeRepository, IUserInfoRepository userInfoRepository,
        AuthorizeRequestDto model, IOAuth appSettingsOAuth)
    {
        if (string.IsNullOrWhiteSpace(model.RedirectUri)) // TODO: Validate uri from appsettings
        {
            return controllerBase.Content("invalid redirect_uri.");
        }

        if (model.ResponseType != "code")
        {
            return RedirectWithError(controllerBase, model.RedirectUri, model.State, "unsupported_response_type",
                "The server does not support obtaining an authorization code using this method.");
        }

        if (model.CodeChallenge == null || model.CodeChallengeMethod != "S256")
        {
            return RedirectWithError(controllerBase, model.RedirectUri, model.State, "invalid_request",
                "The request is missing a parameter, contains an invalid parameter, includes a parameter more than once, or is otherwise invalid.");
        }

        if (controllerBase.User.GetUserId() == -1)
        {
            return controllerBase.Unauthorized();
        }

        int userId = controllerBase.User.GetUserId();
        UserInfo userInfo = userInfoRepository.Get(userId);
        if (userInfo == null)
        {
            return controllerBase.Unauthorized();
        }

        string code = GenerateAuthenticationCode();
        string checksum = (code + userInfo.Username).GetSHA256HashString();
        authorizationCodeRepository.Add(
            model.CodeChallenge
            , model.CodeChallengeMethod
            , code
            , userId
            , appSettingsOAuth.AuthorizationCodeExpires
            , model.ClientId
            , model.RedirectUri
            , checksum
        );

        return controllerBase.Redirect($"{model.RedirectUri}#code={code}&state={model.State}");
    }

    public async Task<IActionResult> TokenAsync(ControllerBase controllerBase, IUserTokenRepository userTokenRepository,
        IUserInfoRepository userInfoRepository, IAuthorizationCodeRepository authorizationCodeRepository,
        TokenRequestDto model, IOAuth appSettingsOAuth, string codeVerifier)
    {
        switch (model.GrantType)
        {
            case "password":
                return await RequestTokenByPasswordAsync(controllerBase, userTokenRepository, userInfoRepository,
                    appSettingsOAuth, model);
            case "refresh_token":
                return RequestTokenByRefreshToken(controllerBase, userTokenRepository, userInfoRepository,
                    appSettingsOAuth, model);
            case "authorization_code":
            {
                if (!string.IsNullOrWhiteSpace(codeVerifier))
                {
                    model.CodeVerifier = codeVerifier;
                }

                return RequestTokenByAuthorizationCode(controllerBase, userInfoRepository, authorizationCodeRepository,
                    userTokenRepository, appSettingsOAuth, model);
            }
            default:
                return controllerBase.BadRequest(new OAuthResponseErrorDto("unsupported_grant_type",
                    "The grant_type doesn't recognize."));
        }
    }

    private static string GenerateAccessToken(int userId, string clientId, string nonce, string secretKey,
        string issuer, int expiresAfter)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        // var scope = "read write";

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: issuer,
            expires: DateTime.Now.AddSeconds(expiresAfter),
            signingCredentials: signingCredentials)
        {
            Payload =
            {
                ["sub"] = userId.ToString(),
                ["cid"] = clientId,
                ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            }
        };

        // token.Payload["scope"] = scope;

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        string token = Guid.NewGuid().ToString("n");
        return token;
    }

    private static string GenerateNonce()
    {
        string nonce = Guid.NewGuid().ToString("n");
        return nonce;
    }

    private static string GenerateAuthenticationCode()
    {
        string code = Guid.NewGuid().ToString("n");
        return code;
    }

    private static IActionResult RedirectWithError(ControllerBase controllerBase, string redirectUri, string state,
        string error, string errorDescription)
    {
        string url = $"{redirectUri}?error={error}&error_description={errorDescription}&state={state}";
        return controllerBase.Redirect(url);
    }

    private async Task<IActionResult> RequestTokenByPasswordAsync(ControllerBase controllerBase,
        IUserTokenRepository userTokenRepository, IUserInfoRepository userInfoRepository, IOAuth appSettingsOAuth,
        TokenRequestDto model)
    {
        UserInfo userInfo;

        if (model.Source == 0) // Default: Username & Password
        {
            if (model.Username == null || model.Password == null)
            {
                return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_request",
                    "The request is missing a required parameter."));
            }

            userInfo = userInfoRepository.Get(model.Username, model.Password, null);
        }
        else if (model.Source == 3) // Facebook: FacebookAccessToken (model.Password)
        {
            if (model.Password == null)
            {
                return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_request",
                    "The request is missing a required parameter."));
            }

            FacebookUserProfileResponseDto facebookResult = await _facebookService.GetFacebookUserAsync(model.Password);

            if (facebookResult.Error != null)
            {
                return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                    facebookResult.Error.Message));
            }

            userInfo = userInfoRepository.Get(facebookResult.Id);

            if (userInfo == null)
            {
                return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant", "User not found."));
            }

            if (model.Source != userInfo.Source)
            {
                return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                    "The user has not signed up yet."));
            }
        }
        else
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_request", "The source not support."));
        }

        if (userInfo == null)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                "The username or password is incorrect."));
        }

        int userId = userInfo.Id;
        string clientId = model.ClientId;

        switch (appSettingsOAuth.Strategy)
        {
            case RefreshTokenStrategy.First when userTokenRepository.IsAlive(userId):
                return controllerBase.Unauthorized(new OAuthResponseErrorDto("invalid_grant",
                    "The user logon already exists."));
            case RefreshTokenStrategy.Last:
                userTokenRepository.Remove(userId);
                break;
            case RefreshTokenStrategy.Multiple:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return controllerBase.Ok(GenerateAccessTokenResponse(userTokenRepository, appSettingsOAuth, userId, clientId,
            userInfo.Username));
    }

    private static IActionResult RequestTokenByAuthorizationCode(ControllerBase controllerBase,
        IUserInfoRepository userInfoRepository, IAuthorizationCodeRepository authorizationCodeRepository,
        IUserTokenRepository userTokenRepository, IOAuth appSettingsOAuth, TokenRequestDto model)
    {
        if (model.Code == null || model.CodeVerifier == null)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_request",
                "The request is missing a required parameter, includes an unsupported parameter value (other than grant type)."));
        }

        string code = model.Code;
        AuthorizationCode authorizationCode = authorizationCodeRepository.Get(code);

        if (authorizationCode == null)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                "Invalid authorization_code or expired."));
        }

        if (authorizationCode.Expires.CompareTo(DateTime.Now) < 0)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                "The authorization_code has expired."));
        }

        if (authorizationCode.RedirectUri != model.RedirectUri)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant", "Invalid redirect_uri."));
        }

        if (authorizationCode.CodeChallenge != model.CodeVerifier.GetSHA256HashString())
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant", "Invalid code_challenge."));
        }

        int userId = authorizationCode.UserId;
        UserInfo userInfo = userInfoRepository.Get(userId);

        if (userInfo == null)
        {
            return controllerBase.Unauthorized();
        }

        if (authorizationCode.CheckSum != (authorizationCode.Code + userInfo.Username).GetSHA256HashString())
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant", "Invalid authorization_code."));
        }

        string clientId = model.ClientId;


        switch (appSettingsOAuth.Strategy)
        {
            case RefreshTokenStrategy.First when userTokenRepository.IsAlive(userId):
                return controllerBase.Unauthorized(new OAuthResponseErrorDto("invalid_grant",
                    "The user logon already exists."));
            case RefreshTokenStrategy.Last:
                userTokenRepository.Remove(userId);
                break;
            case RefreshTokenStrategy.Multiple:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        AccessTokenResponseDto accessTokenResponse = GenerateAccessTokenResponse(userTokenRepository, appSettingsOAuth,
            userId, clientId, userInfo.Username);

        authorizationCodeRepository.Remove(code);
        authorizationCodeRepository.RemoveExpired();

        return controllerBase.Ok(accessTokenResponse);
    }

    private static IActionResult RequestTokenByRefreshToken(ControllerBase controllerBase,
        IUserTokenRepository userTokenRepository, IUserInfoRepository userInfoRepository, IOAuth appSettingsOAuth,
        TokenRequestDto model)
    {
        if (model.RefreshToken == null)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_request",
                "The request is missing a required parameter, includes an unsupported parameter value (other than grant type)."));
        }

        UserToken existsRefreshToken = userTokenRepository.Get(refreshToken: model.RefreshToken);
        if (existsRefreshToken == null)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                "Invalid refresh_token or expired."));
        }

        string password = userInfoRepository.Get(existsRefreshToken.UserId).Username;
        if (existsRefreshToken.CheckSum != (existsRefreshToken.RefreshToken + password).GetSHA256HashString())
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant", "Invalid refresh_token."));
        }

        if (existsRefreshToken.Expires.CompareTo(DateTime.Now) < 0)
        {
            return controllerBase.BadRequest(new OAuthResponseErrorDto("invalid_grant",
                "The refresh_token has expired."));
        }

        // Remove old refresh token.
        userTokenRepository.Remove(model.RefreshToken);

        return controllerBase.Ok(GenerateAccessTokenResponse(userTokenRepository, appSettingsOAuth,
            existsRefreshToken.UserId, existsRefreshToken.ClientId, password));
    }

    private static AccessTokenResponseDto GenerateAccessTokenResponse(IUserTokenRepository userTokenRepository,
        IOAuth appSettingsOAuth, int userId, string clientId, string username)
    {
        string nonce = GenerateNonce();
        string accessToken = GenerateAccessToken(
            userId: userId,
            clientId: clientId,
            nonce: nonce,
            secretKey: appSettingsOAuth.SecretKey,
            issuer: appSettingsOAuth.Issuer,
            expiresAfter: appSettingsOAuth.AccessTokenExpires);
        string refreshToken = GenerateRefreshToken();
        string checksum = (refreshToken + username).GetSHA256HashString();
        userTokenRepository.Add(refreshToken, userId, clientId, nonce, appSettingsOAuth.RefreshTokenExpires, checksum);
        userTokenRepository.RemoveExpired();

        return new AccessTokenResponseDto
        {
            AccessToken = accessToken,
            TokenType = "bearer",
            ExpiresIn = appSettingsOAuth.AccessTokenExpires,
            RefreshToken = refreshToken
        };
    }

    public bool Verify(VerifyRequestDto verifyRequest)
    {
        VerifyCode verifyCode = GenerateVerifyCode();

        return SaveVerifyCode(verifyRequest, verifyCode)
               && DeliveryVerifyCode(verifyRequest, verifyCode);
    }

    public VerifyCode GenerateVerifyCode()
    {
        // Generate code.
        string code = _random.Next(000000, 999999).ToString("D6");

        // Create expired from configure.
        // Default expired is 5 minutes.
        DateTime expired = DateTime.Now;
        int expiresInMinutes = _appSettings.OAuth.VerifyCodeExpires > 0
            ? _appSettings.OAuth.VerifyCodeExpires / 60
            : 5;
        expired = expired.AddMinutes(expiresInMinutes);

        return new VerifyCode
        {
            Code = code,
            Expires = expired,
            Age = expiresInMinutes
        };
    }

    public bool IdentityExisting(VerifyType verifyType, string identity)
    {
        string verifyTypeParam = verifyType == VerifyType.email ? "EMAIL" : "TELEPHONE";

        QueryParameter queryParameter = new QueryParameter();
        queryParameter.Add(_appSettings.Database.ProcedureParameter, "APP_USER_EXISTS_Q");
        queryParameter.Add(verifyTypeParam, identity);

        QueryResult queryResult = _dbDataAccessService.ExecuteProcedure(queryParameter);
        if (queryResult.Success)
        {
            return queryResult.Total > 0;
        }
        else
        {
            Log.Error($"File: AppAuthenService.cs; Function: IdentityExisting; Detail: ${queryResult.Message}");
            return false;
        }
    }

    private bool SaveVerifyCode(VerifyRequestDto verifyRequest, VerifyCode verifyCode)
    {
        QueryParameter queryParameter = new QueryParameter();
        queryParameter.Add(_appSettings.Database.ProcedureParameter, "APP_CONFIRM_CODE_I");
        queryParameter.Add("IDENTITY", verifyRequest.Email);
        queryParameter.Add("CODE", verifyCode.Code);
        queryParameter.Add("EXPIRE_TIME", verifyCode.Expires.ToUnixTimeMilliseconds());
        queryParameter.Add("ACTION", verifyRequest.Action);
        queryParameter.Add("TYPE", verifyRequest.VerifyWith);

        return _dbDataAccessService
            .ExecuteProcedure(queryParameter)
            .Success;
    }

    private bool DeliveryVerifyCode(VerifyRequestDto verifyRequest, VerifyCode verifyCode)
    {
        if (verifyRequest.VerifyWith == VerifyType.email)
        {
            MailMessage mailMessage = new MailMessage();
            mailMessage.Sender = _appSettings.Email.SenderAddress.ToMailAddress();
            mailMessage.From = _appSettings.Email.SenderAddress.ToMailAddress();
            mailMessage.To.Add(verifyRequest.Email.ToMailAddress());
            mailMessage.Subject = "[SYSTEM/APP] กรุณายืนยันอีเมลของคุณ";
            mailMessage.SubjectEncoding = Encoding.UTF8;

            string templateName = verifyRequest.Action == VerifyAction.register
                ? "register.html"
                : "forget-password.html";
            string templatePath = Path.Join(Directory.GetCurrentDirectory(), "OAuth", "Assets", "Verify", templateName);
            string mailBodyTemplate = File.ReadAllText(templatePath);
            CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("th-TH");

            mailMessage.Body = mailBodyTemplate
                .Replace("{EMAIL}", verifyRequest.Email)
                .Replace("{CODE}", verifyCode.Code)
                .Replace("{AGE}", verifyCode.Age.ToString())
                .Replace("{EXPIRED}",
                    $"{verifyCode.Expires.ToString("d MMMM yyyy", cultureInfo)} เวลา {verifyCode.Expires.ToLongTimeString()} น.");
            mailMessage.BodyEncoding = Encoding.UTF8;
            mailMessage.IsBodyHtml = true;
            mailMessage.Priority = MailPriority.Normal;

            return _appMailService.Send(mailMessage);
        }
        else
        {
            // TO DO: Implement send message api(OTP).
            // ...

            throw new NotImplementedException();
        }
    }
}