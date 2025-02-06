using Microsoft.AspNetCore.Mvc;

namespace AtlasX.Web.Service.OAuth.Dto;

public class AuthorizeRequestDto
{
    [BindProperty(Name = "response_type")] public string ResponseType { get; set; }

    [BindProperty(Name = "client_id")] public string ClientId { get; set; }

    [BindProperty(Name = "redirect_uri")] public string RedirectUri { get; set; }

    [BindProperty(Name = "state")] public string State { get; set; }

    [BindProperty(Name = "code_challenge")]
    public string CodeChallenge { get; set; }

    [BindProperty(Name = "code_challenge_method")]
    public string CodeChallengeMethod { get; set; }
}