using HouseStuff.Api.Controllers;
using HouseStuff.Application.Shopping;
using Microsoft.AspNetCore.Authorization;

namespace HouseStuff.Api.UnitTests;

public sealed class ShoppingControllerTests
{
    [Fact]
    public async Task ResidentCanCreateCategoryAndItem()
    {
        var categoryId = Guid.NewGuid();
        var category = new ShoppingCategoryView(categoryId, "Limpeza", 0, []);
        var item = new ShoppingItemView(Guid.NewGuid(), categoryId, "Detergente");
        var service = new StubShoppingCatalogService
        {
            CategoryResult = ShoppingResult.Success(category),
            ItemResult = ShoppingResult.Success(item),
        };
        var controller = new ShoppingController(service);

        var categoryResult = await controller.CreateCategory(new SaveShoppingCategoryRequest("Limpeza"), CancellationToken.None);
        var itemResult = await controller.CreateItem(new SaveShoppingItemRequest(categoryId, "Detergente"), CancellationToken.None);

        Assert.Equal(201, categoryResult.StatusCode);
        Assert.Equal(201, itemResult.StatusCode);
        Assert.Same(category, categoryResult.Value);
        Assert.Same(item, itemResult.Value);
    }

    [Fact]
    public void ShoppingManagementRequiresAuthenticationWithoutAdministratorRole()
    {
        var authorization = Assert.Single(typeof(ShoppingController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());

        Assert.Null(authorization.Roles);
    }

    [Fact]
    public async Task CategoryWithItemsReturnsConflictOnDelete()
    {
        var service = new StubShoppingCatalogService
        {
            BooleanResult = ShoppingResult.Failure<bool>("shopping_category_not_empty", "Categoria possui itens."),
        };

        var result = await new ShoppingController(service).DeleteCategory(Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(409, result.StatusCode);
    }

    private sealed class StubShoppingCatalogService : IShoppingCatalogService
    {
        public ShoppingResult<IReadOnlyList<ShoppingCategoryView>> CatalogResult { get; init; } = ShoppingResult.Success<IReadOnlyList<ShoppingCategoryView>>([]);
        public ShoppingResult<ShoppingCategoryView> CategoryResult { get; init; } = ShoppingResult.Failure<ShoppingCategoryView>("missing", "missing");
        public ShoppingResult<ShoppingItemView> ItemResult { get; init; } = ShoppingResult.Failure<ShoppingItemView>("missing", "missing");
        public ShoppingResult<bool> BooleanResult { get; init; } = ShoppingResult.Success(true);

        public Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> GetCatalogAsync(CancellationToken cancellationToken) => Task.FromResult(CatalogResult);
        public Task<ShoppingResult<ShoppingCategoryView>> CreateCategoryAsync(SaveShoppingCategoryCommand command, CancellationToken cancellationToken) => Task.FromResult(CategoryResult);
        public Task<ShoppingResult<ShoppingCategoryView>> UpdateCategoryAsync(Guid id, SaveShoppingCategoryCommand command, CancellationToken cancellationToken) => Task.FromResult(CategoryResult);
        public Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> MoveCategoryAsync(Guid id, int offset, CancellationToken cancellationToken) => Task.FromResult(CatalogResult);
        public Task<ShoppingResult<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(BooleanResult);
        public Task<ShoppingResult<ShoppingItemView>> CreateItemAsync(SaveShoppingItemCommand command, CancellationToken cancellationToken) => Task.FromResult(ItemResult);
        public Task<ShoppingResult<ShoppingItemView>> UpdateItemAsync(Guid id, SaveShoppingItemCommand command, CancellationToken cancellationToken) => Task.FromResult(ItemResult);
        public Task<ShoppingResult<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(BooleanResult);
    }
}
