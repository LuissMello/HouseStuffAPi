using HouseStuff.Application.Identity;

namespace HouseStuff.Application.Residences;

public sealed record ResidenceMember(string Id, string Name, string Email, bool IsAdministrator, string ProfileColor = ProfileColors.Default);

public sealed record ResidenceView(Guid Id, string Name, IReadOnlyList<ResidenceMember> Members);

public sealed record ResidenceResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class ResidenceResult
{
    public static ResidenceResult<T> Success<T>(T value) => new(true, value, null, null);
    public static ResidenceResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IResidenceService
{
    Task<ResidenceResult<ResidenceView>> GetCurrentAsync(CancellationToken cancellationToken);
    Task<ResidenceResult<ResidenceView>> CreateAsync(string name, CancellationToken cancellationToken);
    Task<ResidenceResult<ResidenceView>> AddMemberAsync(string userId, CancellationToken cancellationToken);
}
