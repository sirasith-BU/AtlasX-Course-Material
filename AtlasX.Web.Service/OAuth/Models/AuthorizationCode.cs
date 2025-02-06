using System;

namespace AtlasX.Web.Service.OAuth.Models;

public class AuthorizationCode
{
    public int UserId { get; set; }
    public DateTime Issued { get; set; }
    public DateTime Expires { get; set; }
    public string Code { get; set; }
    public string ClientId { get; set; }
    public string RedirectUri { get; set; }
    public string CodeChallenge { get; set; }
    public string CodeChallengeMethod { get; set; }
    public string CheckSum { get; set; }
}