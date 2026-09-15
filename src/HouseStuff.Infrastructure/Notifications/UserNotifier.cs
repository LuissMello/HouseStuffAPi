using System.Net;
using System.Text.Json;
using HouseStuff.Application.Notifications;
using HouseStuff.Domain.Notifications;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebPush;

namespace HouseStuff.Infrastructure.Notifications;

internal sealed class UserNotifier(HouseStuffDbContext database, IOptions<VapidOptions> vapidOptions) : IUserNotifier
{
    public async Task NotifyAsync(IReadOnlyCollection<string> userIds, string title, string body, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        foreach (var userId in userIds.Distinct(StringComparer.Ordinal))
        {
            database.AppNotifications.Add(AppNotification.Create(userId, title, body, now));
        }

        await database.SaveChangesAsync(cancellationToken);
        await SendPushAsync(userIds, title, body, cancellationToken);
    }

    private async Task SendPushAsync(IReadOnlyCollection<string> userIds, string title, string body, CancellationToken cancellationToken)
    {
        var subscriptions = await database.PushSubscriptions.Where(subscription => userIds.Contains(subscription.UserId)).ToListAsync(cancellationToken);
        if (subscriptions.Count == 0)
        {
            return;
        }

        var vapidDetails = new VapidDetails(vapidOptions.Value.Subject, vapidOptions.Value.PublicKey, vapidOptions.Value.PrivateKey);
        var client = new WebPushClient();
        var payload = JsonSerializer.Serialize(new { title, body });

        var expired = new List<Domain.Notifications.PushSubscription>();
        foreach (var subscription in subscriptions)
        {
            try
            {
                await client.SendNotificationAsync(
                    new WebPush.PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth),
                    payload,
                    vapidDetails,
                    cancellationToken);
            }
            catch (WebPushException exception) when (exception.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            {
                expired.Add(subscription);
            }
            catch (WebPushException)
            {
                // Outras falhas (limite de taxa, payload grande etc.) — tenta de novo no próximo dia.
            }
        }

        if (expired.Count > 0)
        {
            database.PushSubscriptions.RemoveRange(expired);
            await database.SaveChangesAsync(cancellationToken);
        }
    }
}
