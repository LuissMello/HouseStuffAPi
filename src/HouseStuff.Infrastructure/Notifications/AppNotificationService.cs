using System.Security.Claims;
using HouseStuff.Application.Notifications;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Notifications;

internal sealed class AppNotificationService(HouseStuffDbContext database, IHttpContextAccessor httpContextAccessor) : IAppNotificationService
{
    private const int MaxResults = 50;

    public async Task<NotificationResult<IReadOnlyList<AppNotificationView>>> ListAsync(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return NotificationResult.Failure<IReadOnlyList<AppNotificationView>>("current_user_not_found", "Não foi possível identificar o usuário atual.");
        }

        var notifications = await database.AppNotifications
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.CreatedAt)
            .Take(MaxResults)
            .Select(notification => new AppNotificationView(notification.Id, notification.Title, notification.Body, notification.CreatedAt, notification.ReadAt))
            .ToListAsync(cancellationToken);

        return NotificationResult.Success<IReadOnlyList<AppNotificationView>>(notifications);
    }

    public async Task<NotificationResult<bool>> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return NotificationResult.Failure<bool>("current_user_not_found", "Não foi possível identificar o usuário atual.");
        }

        var notification = await database.AppNotifications.SingleOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId, cancellationToken);
        if (notification is null)
        {
            return NotificationResult.Failure<bool>("notification_not_found", "Notificação não encontrada.");
        }

        notification.MarkRead(DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(cancellationToken);
        return NotificationResult.Success(true);
    }

    public async Task<NotificationResult<bool>> MarkAllReadAsync(CancellationToken cancellationToken)
    {
        var userId = CurrentUserId();
        if (userId is null)
        {
            return NotificationResult.Failure<bool>("current_user_not_found", "Não foi possível identificar o usuário atual.");
        }

        var now = DateTimeOffset.UtcNow;
        var unread = await database.AppNotifications.Where(item => item.UserId == userId && item.ReadAt == null).ToListAsync(cancellationToken);
        foreach (var notification in unread)
        {
            notification.MarkRead(now);
        }

        if (unread.Count > 0)
        {
            await database.SaveChangesAsync(cancellationToken);
        }

        return NotificationResult.Success(true);
    }

    private string? CurrentUserId() => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
