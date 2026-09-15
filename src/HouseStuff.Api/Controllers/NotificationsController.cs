using HouseStuff.Application.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/notifications")]
public sealed class NotificationsController(IPushSubscriptionService subscriptions) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("vapid-public-key")]
    public IActionResult VapidPublicKey() => Ok(new { publicKey = subscriptions.GetVapidPublicKey() });

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(SubscribePushRequest request, CancellationToken cancellationToken)
    {
        var result = await subscriptions.SubscribeAsync(new SubscribePushCommand(request.Endpoint, request.P256dh, request.Auth), cancellationToken);
        return result.Succeeded
            ? NoContent()
            : this.ProblemWithCode(StatusCodes.Status400BadRequest, result.Message, result.Code);
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe(UnsubscribePushRequest request, CancellationToken cancellationToken)
    {
        await subscriptions.UnsubscribeAsync(request.Endpoint, cancellationToken);
        return NoContent();
    }
}

public sealed record SubscribePushRequest(string Endpoint, string P256dh, string Auth);

public sealed record UnsubscribePushRequest(string Endpoint);
