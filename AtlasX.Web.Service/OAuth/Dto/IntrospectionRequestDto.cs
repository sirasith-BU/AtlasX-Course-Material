using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AtlasX.Web.Service.OAuth.Dto;

public class IntrospectionRequestDto
{
    [BindProperty(Name = "token")]
    [BindRequired]
    public string Token { get; set; }
}