namespace HouseStuff.Domain.Notifications;

public sealed class PushSubscription
{
    private PushSubscription(Guid id, string userId, string endpoint, string p256dh, string auth, DateTimeOffset now)
    {
        Id = id;
        UserId = userId;
        Endpoint = endpoint;
        P256dh = p256dh;
        Auth = auth;
        CreatedAt = now;
    }

    private PushSubscription()
    {
        UserId = string.Empty;
        Endpoint = string.Empty;
        P256dh = string.Empty;
        Auth = string.Empty;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string Endpoint { get; private set; }
    public string P256dh { get; private set; }
    public string Auth { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public static PushSubscriptionChangeResult Create(string userId, string endpoint, string p256dh, string auth, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(endpoint) || string.IsNullOrWhiteSpace(p256dh) || string.IsNullOrWhiteSpace(auth))
        {
            return PushSubscriptionChangeResult.Failure("push_subscription_invalid", "Inscrição de notificação inválida.");
        }

        if (endpoint.Length > 500 || p256dh.Length > 200 || auth.Length > 100)
        {
            return PushSubscriptionChangeResult.Failure("push_subscription_invalid", "Inscrição de notificação inválida.");
        }

        return PushSubscriptionChangeResult.Success(new PushSubscription(Guid.NewGuid(), userId, endpoint.Trim(), p256dh.Trim(), auth.Trim(), now));
    }
}

public sealed record PushSubscriptionChangeResult(bool Succeeded, PushSubscription? Subscription, string? Code, string? Message)
{
    public static PushSubscriptionChangeResult Success(PushSubscription? subscription) => new(true, subscription, null, null);
    public static PushSubscriptionChangeResult Failure(string code, string message) => new(false, null, code, message);
}
