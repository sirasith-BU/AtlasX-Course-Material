using AtlasX.Web.Service.Notification.Models;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Notification.Services;

public class NotificationServiceFcmLegacyHttp : NotificationServiceBase, INotificationService
{
    private readonly IUserTokenRepository _userTokenRepository;
    private readonly HttpClient _httpClient;
    private readonly string _fcmPushUrl;


    public NotificationServiceFcmLegacyHttp(IUserTokenRepository userTokenRepository, IConfiguration configuration)
    {
        _userTokenRepository = userTokenRepository;
        _fcmPushUrl = configuration.GetValue<string>("WebServiceSettings:Firebase:FcmPushUrl");
        var serverKey = configuration.GetValue<string>("WebServiceSettings:Firebase:ServerKey");
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"key={serverKey}");
    }

    public async Task<int> SendMessageAsync(List<string> tokens, NotificationMessage notification)
    {
        var successCount = 0;
        var notificationPacks = new List<FCMLegacyHTTPNotificationPack>();

        try
        {
            notificationPacks.AddRange(tokens.Select(to => new FCMLegacyHTTPNotificationPack
            {
                data = notification.Payload?.ToDictionary(k => k.Key, k => (object)k.Value),
                notification = new NotificationMessage
                {
                    Badge = notification.Badge, Body = notification.Body, Title = notification.Title, Icon = notification.Icon,
                },
                to = to
            }));

            foreach (var stringContent in notificationPacks.Select(pack => JsonConvert.SerializeObject(pack)).Select(content => new StringContent(content, Encoding.UTF8, "application/json")))
            {
                var responseMessage = await _httpClient.PostAsync(_fcmPushUrl, stringContent);
                var responseString = await responseMessage.Content.ReadAsStringAsync();
                var response = (JObject)JsonConvert.DeserializeObject(responseString);
                if (response != null) successCount += response["success"]?.ToObject<int>() ?? 0;
            }

            return successCount;
        }
        catch (Exception e)
        {
            // todo: Write Log.
            Console.WriteLine(e);
            return successCount;
        }
    }

    public Task<int> SendMessageAsync(DataTable dataTable)
    {
        return SendMessageAsync(dataTable, _userTokenRepository, SendMessageAsync);
    }
}