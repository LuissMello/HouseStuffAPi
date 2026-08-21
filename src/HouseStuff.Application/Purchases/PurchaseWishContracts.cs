namespace HouseStuff.Application.Purchases;

public sealed record PurchaseWishView(Guid Id, string Name, string? StoreUrl, int Priority);
public sealed record SavePurchaseWishCommand(string Name, string? StoreUrl);
public sealed record ReorderPurchaseWishesCommand(IReadOnlyList<Guid> OrderedIds);
public sealed record PurchaseWishResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class PurchaseWishResult
{
    public static PurchaseWishResult<T> Success<T>(T value) => new(true, value, null, null);
    public static PurchaseWishResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IPurchaseWishService
{
    Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> GetAsync(CancellationToken cancellationToken);
    Task<PurchaseWishResult<PurchaseWishView>> CreateAsync(SavePurchaseWishCommand command, CancellationToken cancellationToken);
    Task<PurchaseWishResult<PurchaseWishView>> UpdateAsync(Guid id, SavePurchaseWishCommand command, CancellationToken cancellationToken);
    Task<PurchaseWishResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> ReorderAsync(ReorderPurchaseWishesCommand command, CancellationToken cancellationToken);
}
