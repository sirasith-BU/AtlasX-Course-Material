using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AtlasX.Web.Service.OAuth.Dto;

public class EndSessionRequestDto
{
    /// <summary>
    /// The token that the client wants to get revoked.
    /// </summary>
    [BindProperty(Name = "id_token_hint")]
    [BindRequired]
    public string Token { get; set; }

    /// <summary>
    /// The token that the client wants to get revoked.
    /// </summary>
    [BindProperty(Name = "post_logout_redirect_uri")]
    public string RedirectUri { get; set; }
}