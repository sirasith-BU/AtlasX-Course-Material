using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using AtlasX.Web.Service.OAuth.Dto;

namespace AtlasX.Web.Service.OAuth.Services;

public class FacebookService
{
    private readonly HttpClient _client;

    public FacebookService(HttpClient client)
    {
        _client = client;
        _client.BaseAddress = new System.Uri("https://graph.facebook.com");
    }

    public async Task<FacebookUserProfileResponseDto> GetFacebookUserAsync(string facebookAccessToken)
    {
        string uri = $"/me?access_token={facebookAccessToken}";
        HttpResponseMessage response = await _client.GetAsync(uri);
        string restponseString = await response.Content.ReadAsStringAsync();
        FacebookUserProfileResponseDto result =
            JsonConvert.DeserializeObject<FacebookUserProfileResponseDto>(restponseString);
        return result;
    }
}