using System.Collections.Generic;

namespace AtlasX.Web.Service.Notification.Dto;

public class SendMessageRequestDto
{
    public string Title { get; set; }
    public string Body { get; set; }
    public int Badge { get; set; }
    public Dictionary<string, string> Payload { get; set; }
    public List<string> Tokens { get; set; }
    public string Icon { get; set; }
    public string Sound { get; set; }
}