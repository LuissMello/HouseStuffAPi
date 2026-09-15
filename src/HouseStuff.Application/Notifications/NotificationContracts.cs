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
