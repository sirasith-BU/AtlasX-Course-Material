using Microsoft.AspNetCore.Mvc;

namespace AtlasX.Web.Service.OAuth.Dto;

public class TokenRequestDto
{
    [BindProperty(Name = "grant_type")] public string GrantType { get; set; }

    // grant_type: password
    [BindProperty(Name = "username")] public string Username { get; set; }
    [BindProperty(Name = "password")] public string Password { get; set; }


    // grant_type: authorization_code
    [BindProperty(Name = "code")] public string Code { get; set; }
    [BindProperty(Name = "redirect_uri")] public string RedirectUri { get; set; }
    [BindProperty(Name = "code_verifier")] public string CodeVerifier { get; set; }


    [BindProperty(Name = "client_id")] public string ClientId { get; set; }
    [BindProperty(Name = "client_secret")] public string ClientSecret { get; set; }
    [BindProperty(Name = "refresh_token")] public string RefreshToken { get; set; }
    [BindProperty(Name = "source")] public int Source { get; set; }
}