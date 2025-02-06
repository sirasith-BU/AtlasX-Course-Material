using Microsoft.AspNetCore.Mvc;

namespace AtlasX.Web.Service.OAuth.Dto;

public class IntrospectionResponseDto
{
    [BindProperty(Name = "active")] public bool Active { get; set; }
    
    [BindProperty(Name = "sub")] public string Sub { get; set; }
}