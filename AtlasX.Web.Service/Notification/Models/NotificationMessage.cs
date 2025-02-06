using System.Collections.Generic;

namespace AtlasX.Web.Service.Notification.Models;

public class NotificationMessage
{
    public int UserId { get; set; }
    public string Title { get; set; }
    public string Body { get; set; }
    public Dictionary<string, string> Payload { get; set; }

    /// <summary>
    /// iOS only
    /// </summary>
    public int Badge { get; set; } = 1;

    /// <summary>
    /// Android only
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Android only
    /// </summary>

    public string Sound { get; set; } = "default";
}