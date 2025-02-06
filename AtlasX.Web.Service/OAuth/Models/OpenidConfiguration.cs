using System.Collections.Generic;

namespace AtlasX.Web.Service.OAuth.Models;

public class OpenidConfiguration
{
    public string authorization_endpoint { get; set; }
    public string token_endpoint { get; set; }
    public string introspection_endpoint { get; set; }
    public string revocation_endpoint { get; set; }
    public string end_session_endpoint { get; set; }
    public string userinfo_endpoint { get; set; }
    public List<string> userinfo_signing_alg_values_supported { get; set; }
    public List<string> id_token_signing_alg_values_supported { get; set; }
    public List<string> response_types_supported { get; set; }
    public List<string> token_endpoint_auth_signing_alg_values_supported { get; set; }
    public List<string> request_object_signing_alg_values_supported { get; set; }
    public List<string> claim_types_supported { get; set; }
    public List<string> grant_types_supported { get; set; }
    public List<string> claims_supported { get; set; }
}