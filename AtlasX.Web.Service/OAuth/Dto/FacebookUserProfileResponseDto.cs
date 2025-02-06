using Newtonsoft.Json;

namespace AtlasX.Web.Service.OAuth.Dto;

public class FacebookUserProfileResponseDto
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("name")] public string Name { get; set; }

    public ErrorResponseDto Error { get; set; }
}