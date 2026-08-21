namespace HouseStuff.Application.Shopping;

public sealed record ShoppingItemView(Guid Id, Guid CategoryId, string Name);
public sealed record ShoppingCategoryView(Guid Id, string Name, int DisplayOrder, IReadOnlyList<ShoppingItemView> Items);
public sealed record SaveShoppingCategoryCommand(string Name);
public sealed record SaveShoppingItemCommand(Guid CategoryId, string Name);
public sealed record ShoppingResult<T>(bool Succeeded, T? Value, string? Code, string? Message);

public static class ShoppingResult
{
    public static ShoppingResult<T> Success<T>(T value) => new(true, value, null, null);
    public static ShoppingResult<T> Failure<T>(string code, string message) => new(false, default, code, message);
}

public interface IShoppingCatalogService
{
    Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> GetCatalogAsync(CancellationToken cancellationToken);
    Task<ShoppingResult<ShoppingCategoryView>> CreateCategoryAsync(SaveShoppingCategoryCommand command, CancellationToken cancellationToken);
    Task<ShoppingResult<ShoppingCategoryView>> UpdateCategoryAsync(Guid id, SaveShoppingCategoryCommand command, CancellationToken cancellationToken);
    Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> MoveCategoryAsync(Guid id, int offset, CancellationToken cancellationToken);
    Task<ShoppingResult<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken);
    Task<ShoppingResult<ShoppingItemView>> CreateItemAsync(SaveShoppingItemCommand command, CancellationToken cancellationToken);
    Task<ShoppingResult<ShoppingItemView>> UpdateItemAsync(Guid id, SaveShoppingItemCommand command, CancellationToken cancellationToken);
    Task<ShoppingResult<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken);
}
