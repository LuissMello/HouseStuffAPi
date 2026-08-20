namespace HouseStuff.Application.Identity;

public static class HouseStuffRoles
{
    public const string Administrator = "Administrator";
    public const string Member = "Member";
}

public sealed record CurrentUser(string Id, string Email, string Name, bool IsAdministrator, Guid? ResidenceId = null, string? ResidenceName = null);

public sealed record UserSummary(string Id, string Email, string Name, bool IsAdministrator, Guid? ResidenceId = null, string? ResidenceName = null);

public sealed record CreateUserCommand(string Email, string Name, string TemporaryPassword, bool IsAdministrator);

public sealed record AccessResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class AccessResult
{
    public static AccessResult<T> Success<T>(T value) => new(true, value, null, null);

    public static AccessResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IUserAccessService
{
    Task<AccessResult<CurrentUser>> SignInAsync(string email, string password, bool rememberMe, CancellationToken cancellationToken);
    Task SignOutAsync();
    Task<CurrentUser?> GetCurrentAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken);
    Task<AccessResult<UserSummary>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken);
}
