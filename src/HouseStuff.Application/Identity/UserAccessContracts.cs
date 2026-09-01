namespace HouseStuff.Application.Identity;

public static class HouseStuffRoles
{
    public const string Administrator = "Administrator";
    public const string Member = "Member";
}

public static class ProfileColors
{
    public const string Default = "#2F6B50";

    public static string? Normalize(string? value)
    {
        var trimmed = value?.Trim();
        return trimmed is { Length: 7 }
            && trimmed[0] == '#'
            && trimmed.AsSpan(1).ToArray().All(Uri.IsHexDigit)
                ? trimmed.ToUpperInvariant()
                : null;
    }
}

public sealed record CurrentUser(string Id, string Email, string Name, bool IsAdministrator, Guid? ResidenceId = null, string? ResidenceName = null, string ProfileColor = ProfileColors.Default);

public sealed record UserSummary(string Id, string Email, string Name, bool IsAdministrator, Guid? ResidenceId = null, string? ResidenceName = null, string ProfileColor = ProfileColors.Default);

public sealed record CreateUserCommand(string Email, string Name, string TemporaryPassword, bool IsAdministrator);

public sealed record ChangeUserRoleCommand(string UserId, bool IsAdministrator);

public sealed record AccessResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class AccessResult
{
    public static AccessResult<T> Success<T>(T value) => new(true, value, null, null);

    public static AccessResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IUserAccessService
{
    Task<AccessResult<bool>> SignInWithTokenAsync(string email, string password, CancellationToken cancellationToken);
    Task<AccessResult<bool>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task SignOutAsync();
    Task<CurrentUser?> GetCurrentAsync(CancellationToken cancellationToken);
    Task<AccessResult<CurrentUser>> UpdateProfileColorAsync(string profileColor, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken);
    Task<AccessResult<UserSummary>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);
    Task<AccessResult<UserSummary>> ChangeRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken);
}
