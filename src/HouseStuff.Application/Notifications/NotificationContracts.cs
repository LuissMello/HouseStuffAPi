namespace HouseStuff.Application.Notifications;

public sealed record SubscribePushCommand(string Endpoint, string P256dh, string Auth);

public sealed record NotificationResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class NotificationResult
{
    public static NotificationResult<T> Success<T>(T value) => new(true, value, null, null);
    public static NotificationResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IPushSubscriptionService
{
    string GetVapidPublicKey();
    Task<NotificationResult<bool>> SubscribeAsync(SubscribePushCommand command, CancellationToken cancellationToken);
    Task<NotificationResult<bool>> UnsubscribeAsync(string endpoint, CancellationToken cancellationToken);
}

public interface IDailyDigestService
{
    /// <summary>Monta e envia o resumo diário para cada residência ainda não processada hoje. Retorna quantas receberam notificação.</summary>
    Task<int> RunAsync(CancellationToken cancellationToken);
}

public sealed record AppNotificationView(Guid Id, string Title, string Body, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt);

public interface IAppNotificationService
{
    Task<NotificationResult<IReadOnlyList<AppNotificationView>>> ListAsync(CancellationToken cancellationToken);
    Task<NotificationResult<bool>> MarkReadAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<NotificationResult<bool>> MarkAllReadAsync(CancellationToken cancellationToken);
}

/// <summary>Envia push e registra a notificação in-app para um ou mais usuários — usado pelo resumo diário e pela atribuição de tarefas.</summary>
public interface IUserNotifier
{
    Task NotifyAsync(IReadOnlyCollection<string> userIds, string title, string body, CancellationToken cancellationToken);
}
