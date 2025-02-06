using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AtlasX.Web.Service.OAuth.Dto;

public class RevocationRequestDto
{
    /// <summary>
    /// The token that the client wants to get revoked.
    /// </summary>
    [BindProperty(Name = "token")]
    [BindRequired]
    public string Token { get; set; }

    // /// <summary>
    // /// Optional hint about the type of the submitted token
    // /// either access_token or refresh_token.
    // /// </summary>
    // [BindProperty(Name = "token_type_hint")]
    // public string TokenTypeHint { get; set; }
}