using System.Security.Claims;
using HouseStuff.Application.Notifications;
using HouseStuff.Domain.Notifications;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HouseStuff.Infrastructure.Notifications;

internal sealed class PushSubscriptionService(
    HouseStuffDbContext database,
    IHttpContextAccessor httpContextAccessor,
    IOptions<VapidOptions> vapidOptions) : IPushSubscriptionService
{
    public string GetVapidPublicKey() => vapidOptions.Value.PublicKey;

    public async Task<NotificationResult<bool>> SubscribeAsync(SubscribePushCommand command, CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return NotificationResult.Failure<bool>("current_user_not_found", "Não foi possível identificar o usuário atual.");
        }

        var creation = PushSubscription.Create(userId, command.Endpoint, command.P256dh, command.Auth, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return NotificationResult.Failure<bool>(creation.Code!, creation.Message!);
        }

        var existing = await database.PushSubscriptions.SingleOrDefaultAsync(subscription => subscription.Endpoint == command.Endpoint, cancellationToken);
        if (existing is not null)
        {
            database.PushSubscriptions.Remove(existing);
        }

        database.PushSubscriptions.Add(creation.Subscription!);
        await database.SaveChangesAsync(cancellationToken);
        return NotificationResult.Success(true);
    }

    public async Task<NotificationResult<bool>> UnsubscribeAsync(string endpoint, CancellationToken cancellationToken)
    {
        var existing = await database.PushSubscriptions.SingleOrDefaultAsync(subscription => subscription.Endpoint == endpoint, cancellationToken);
        if (existing is not null)
        {
            database.PushSubscriptions.Remove(existing);
            await database.SaveChangesAsync(cancellationToken);
        }

        return NotificationResult.Success(true);
    }
}
