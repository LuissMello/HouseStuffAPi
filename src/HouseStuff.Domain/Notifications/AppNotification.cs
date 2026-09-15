namespace HouseStuff.Domain.Notifications;

public sealed class AppNotification
{
    private AppNotification(Guid id, string userId, string title, string body, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Title = title;
        Body = body;
        CreatedAt = createdAt;
    }

    private AppNotification()
    {
        UserId = string.Empty;
        Title = string.Empty;
        Body = string.Empty;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; }
    public string Title { get; private set; }
    public string Body { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? ReadAt { get; private set; }

    public static AppNotification Create(string userId, string title, string body, DateTimeOffset now) =>
        new(Guid.NewGuid(), userId, title.Trim(), body.Trim(), now);

    public void MarkRead(DateTimeOffset now) => ReadAt ??= now;
}
