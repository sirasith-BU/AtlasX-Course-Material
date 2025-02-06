using System.Collections.Generic;

namespace AtlasX.Web.Service.Notification.Models;

public class FCMLegacyHTTPNotificationPack
{
    public string to { get; set; }
    public NotificationMessage notification { get; set; }
    public Dictionary<string, object> data { get; set; }
    public string priority { get; set; }
    public bool content_available { get; set; }

    public FCMLegacyHTTPNotificationPack()
    {
        notification = new NotificationMessage();
        priority = "high";
        content_available = true;
    }
}