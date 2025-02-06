using Newtonsoft.Json;

namespace AtlasX.Web.Service.OAuth.Dto;

public class OAuthResponseErrorDto
{
    [JsonProperty("error")] public string Error { get; set; }

    [JsonProperty("error_description")] public string ErrorDescription { get; set; }

    public OAuthResponseErrorDto(string error, string errorDescription)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }
}