using Microsoft.AspNetCore.Mvc;

namespace AtlasX.Web.Service.Notification.Dto;

public class RegisterFcmRequest
{
    [BindProperty(Name = "fcm_token")] public string FcmToken { get; set; }

    [BindProperty(Name = "data_source")] public string DataSource { get; set; }
}