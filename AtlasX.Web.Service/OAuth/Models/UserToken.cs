using AtlasX.Engine.Extensions;
using System;
using System.Collections.Generic;

namespace AtlasX.Web.Service.OAuth.Models;

public class UserToken
{
    public UserToken()
    {
    }

    public UserToken(Dictionary<string, object> dictionary)
    {
        UserId = int.Parse(dictionary.GetValueOrDefault("USER_ID")?.ToString() ?? "0");
        Issued = long.Parse(dictionary.GetValueOrDefault("CREATED_DATE")?.ToString() ?? "0").UnixTimeStampToDateTime();
        Expires = long.Parse(dictionary.GetValueOrDefault("EXPIRED_DATE")?.ToString() ?? "0").UnixTimeStampToDateTime();
        RefreshToken = dictionary.GetValueOrDefault("REFRESH_TOKEN")?.ToString();
        FcmToken = dictionary.GetValueOrDefault("FCM_TOKEN")?.ToString();
        ClientId = dictionary.GetValueOrDefault("CLIENT_ID")?.ToString();
        Nonce = dictionary.GetValueOrDefault("NONCE")?.ToString();
        CheckSum = dictionary.GetValueOrDefault("CHECK_SUM")?.ToString();
    }

    public int UserId { get; set; }
    public DateTime Issued { get; set; }
    public DateTime Expires { get; set; }
    public string RefreshToken { get; set; }
    public string FcmToken { get; set; }
    public string ClientId { get; set; }
    public string Nonce { get; set; }
    public string CheckSum { get; set; }
}