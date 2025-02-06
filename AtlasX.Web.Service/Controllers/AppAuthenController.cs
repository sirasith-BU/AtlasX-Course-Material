using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Core;
using AtlasX.Web.Service.OAuth.Dto;
using AtlasX.Web.Service.OAuth.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using AtlasX.Web.Service.OAuth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Controllers;

/// <summary>
/// Authentication client application with OAuth 2.0 API.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[OpenApiTag("AppAuthen", Description = "Authentication client application with OAuth 2.0 API.")]
public class AppAuthenController : ControllerBase
{
    private readonly IAppAuthenService _appAuthenService;
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly IUserInfoRepository _userInfoRepository;
    private readonly IAuthorizationCodeRepository _authorizationCodeRepository;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly AppSettings _appSettings;

    public AppAuthenController(IAppAuthenService appAuthenService
        , IUserTokenRepository refreshTokenRepository
        , IUserInfoRepository userInfoRepository
        , IAuthorizationCodeRepository authorizationCodeRepository
        , IOptions<AppSettings> appSettings
        , IWebHostEnvironment hostingEnvironment)
    {
        _appAuthenService = appAuthenService;
        _userTokenRepository = refreshTokenRepository;
        _userInfoRepository = userInfoRepository;
        _authorizationCodeRepository = authorizationCodeRepository;
        _appSettings = appSettings.Value;
        _hostingEnvironment = hostingEnvironment;
    }

    /// <summary>
    /// Get OpenID Connect metadata.
    /// </summary>
    /// <remarks>
    /// Return OpenID Connect metadata related to the specified authorization server.
    /// </remarks>
    /// <response code="200">OpenID Connect metadata.</response>
    [HttpGet(".well-known/openid-configuration")]
    [ProducesResponseType(typeof(OpenidConfiguration), StatusCodes.Status200OK)]
    public async Task<OpenidConfiguration> WellKnown()
    {
        var filePath = _hostingEnvironment.ContentRootPath;
        filePath = Path.Join(filePath, "Config", "openid-configuration.json");

        var json = await System.IO.File.ReadAllTextAsync(filePath);
        json = json.Replace("{issuer_endpoint}", $"{_appSettings.OAuth.Issuer}/api/appauthen");

        return JsonSerializer.Deserialize<OpenidConfiguration>(json);
    }

    /// <summary>
    /// The authorize endpoint can be used to request tokens or authorization codes via the browser.
    /// This process typically involves authentication of the end-user and optionally consent.
    /// </summary>
    [HttpGet("authorize")]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public IActionResult Authorize([FromQuery] AuthorizeRequestDto model)
    {
        return _appAuthenService.Authorization(this,
            _authorizationCodeRepository,
            _userInfoRepository,
            model,
            _appSettings.OAuth);
    }

    /// <summary>
    /// The token endpoint can be used to programmatically request tokens.
    /// It supports the password, authorization_code and refresh_token grant types.
    /// </summary>
    [HttpPost("token")]
    public async Task<IActionResult> TokenAsync([FromForm] TokenRequestDto model, [FromQuery] string codeVerifier)
    {
        return await _appAuthenService.TokenAsync(this,
            _userTokenRepository,
            _userInfoRepository,
            _authorizationCodeRepository,
            model, _appSettings.OAuth, codeVerifier);
    }

    /// <summary>
    /// The UserInfo endpoint can be used to retrieve identity information about a user.
    /// </summary>
    [HttpGet("userinfo")]
    [Authorize]
    public UserInfo UserInfo()
    {
        var userId = User.GetUserId();
        var user = _userInfoRepository.Get(userId);

        return user;
    }

    /// <summary>
    /// It can be used to validate reference tokens.
    /// A successful response will return a status code of 200 
    /// and either an active or inactive token.
    /// Unknown or expired tokens will be marked as inactive.
    /// An invalid request will return a 400, an unauthorized request 401.
    /// </summary>
    [HttpPost("introspect")]
    [Authorize]
    public IActionResult Introspect([FromForm] IntrospectionRequestDto introspectionRequest)
    {
        var userToken = _userTokenRepository.Get(introspectionRequest.Token);

        if (userToken == null || userToken.Expires.CompareTo(DateTime.Now) < 0)
            // Unknown or expired tokens will be marked as inactive.
            return Ok(new IntrospectionResponseDto
            {
                Active = false
            });
        // A successful response will return a status code of 200 
        // and either an active or inactive token.
        return Ok(new IntrospectionResponseDto
        {
            Active = true,
            Sub = introspectionRequest.Token
        });
    }

    /// <summary>
    /// The Token Revocation extension defines a mechanism for clients
    /// to indicate to the authorization server that an access token is no longer needed.
    /// This is used to enable a "log out" feature in clients, allowing the authorization server
    /// to clean up any security credentials associated with the authorization.
    /// </summary>
    [HttpPost("revoke")]
    // [Authorize] // Comment this line because openid-js doesn't support.
    public IActionResult Revocation([FromForm] RevocationRequestDto revocationRequest)
    {
        _userTokenRepository.Remove(revocationRequest.Token);

        return Ok();
    }

    /// <summary>
    /// Redirecting to the logout endpoint clears the authentication session and cookie.
    /// </summary>
    [HttpGet("endsession")]
    public async Task<IActionResult> EndSession([FromQuery] EndSessionRequestDto endSessionRequest)
    {
        // Remove cookie.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        // Remove token.
        _userTokenRepository.Remove(endSessionRequest.Token);

        switch (string.IsNullOrEmpty(endSessionRequest.RedirectUri))
        {
            case false:
                return Redirect(endSessionRequest.RedirectUri);
            default:
            {
                if (Request.Headers.TryGetValue("Referer", out var referer))
                    return Redirect(referer);

                return Content("You have successfully logged out. Please close the browser to complete sign out.");
            }
        }
    }

    /// <summary>
    /// Redirecting to the logout endpoint clears the authentication session and cookie.
    /// </summary>
    [HttpPost("verify")]
    public IActionResult Verify([FromForm] VerifyRequestDto verifyRequest)
    {
        // TO DO: Verify with telephone number are future feature.
        if (verifyRequest.VerifyWith == VerifyType.telephone)
            return StatusCode(
                StatusCodes.Status501NotImplemented,
                "This feature only supported with email verification. Please contact AtlasX Developer to resolve your requirement."
            );

        switch (verifyRequest.Action)
        {
            case VerifyAction.register:
                return _appAuthenService.Verify(verifyRequest)
                    ? Ok()
                    : StatusCode(StatusCodes.Status503ServiceUnavailable);
            case VerifyAction.forget_password:
            {
                var identity = verifyRequest.VerifyWith == VerifyType.email
                    ? verifyRequest.Email
                    : verifyRequest.Telephone;
                if (_appAuthenService.IdentityExisting(verifyRequest.VerifyWith, identity))
                    return _appAuthenService.Verify(verifyRequest)
                        ? Ok()
                        : StatusCode(StatusCodes.Status503ServiceUnavailable);

                return StatusCode(StatusCodes.Status403Forbidden, "Not found this identity in system.");
            }
            default:
                return StatusCode(StatusCodes.Status400BadRequest, "Unknow this verify action.");
        }
    }
}