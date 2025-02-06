using AtlasX.Engine.Extensions;
using AtlasX.Web.Service.Notification.Dto;
using AtlasX.Web.Service.Notification.Models;
using AtlasX.Web.Service.Notification.Services;
using AtlasX.Web.Service.OAuth.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class AppNotiController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IUserTokenRepository _userTokenRepository;

    public AppNotiController(INotificationService notification, IUserTokenRepository userTokenRepository)
    {
        _notificationService = notification;
        _userTokenRepository = userTokenRepository;
    }

    [HttpPost("registerFCM")]
    public IActionResult RegisterFcm([FromForm] RegisterFcmRequest model)
    {
        // TODO: Implement DataSource
        var nonce = User.GetNonce();
        var userId = User.GetUserId();
        _userTokenRepository.UpdateFcmToken(userId, nonce, model.FcmToken);
        return Ok();
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessageAsync([FromForm] SendMessageRequestDto model)
    {
        var successCount = await _notificationService.SendMessageAsync(model.Tokens, new NotificationMessage
        {
            Title = model.Title,
            Body = model.Body,
            Badge = model.Badge,
            Payload = model.Payload,
            Icon = model.Icon,
            Sound = model.Sound
        });
        return Ok($"SuccessCount:{successCount}");
    }
}