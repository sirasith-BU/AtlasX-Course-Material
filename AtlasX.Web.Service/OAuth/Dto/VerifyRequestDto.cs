using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace AtlasX.Web.Service.OAuth.Dto;

public class VerifyRequestDto
{
    [BindProperty(Name = "action")]
    [BindRequired]
    public VerifyAction Action { get; set; }

    [BindProperty(Name = "verify_with")]
    [BindRequired]
    public VerifyType VerifyWith { get; set; }

    [BindProperty(Name = "email")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Email invalid.")]
    public string Email { get; set; }

    [BindProperty(Name = "telephone")]
    [RegularExpression(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}", ErrorMessage = "Telephone number invalid.")]
    public string Telephone { get; set; }
}

public enum VerifyAction
{
    register,
    forget_password
}

public enum VerifyType
{
    email,
    telephone
}