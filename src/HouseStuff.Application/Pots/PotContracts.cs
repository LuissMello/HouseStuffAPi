namespace HouseStuff.Application.Pots;

public sealed record PotView(Guid Id, string Name, string? Description, int DisplayOrder, bool IsActive);

public sealed record SavePotCommand(string Name, string? Description);

public sealed record PotResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class PotResult
{
    public static PotResult<T> Success<T>(T value) => new(true, value, null, null);
    public static PotResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface ICurrentResidenceContext
{
    Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken);
}

public interface IPotService
{
    Task<PotResult<IReadOnlyList<PotView>>> ListAsync(bool includeArchived, CancellationToken cancellationToken);
    Task<PotResult<PotView>> CreateAsync(SavePotCommand command, CancellationToken cancellationToken);
    Task<PotResult<PotView>> UpdateAsync(Guid id, SavePotCommand command, CancellationToken cancellationToken);
    Task<PotResult<PotView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
    Task<PotResult<IReadOnlyList<PotView>>> MoveAsync(Guid id, int offset, CancellationToken cancellationToken);
}
