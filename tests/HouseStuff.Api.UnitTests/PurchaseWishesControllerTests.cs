using HouseStuff.Api.Controllers;
using HouseStuff.Application.Purchases;
using Microsoft.AspNetCore.Authorization;

namespace HouseStuff.Api.UnitTests;

public sealed class PurchaseWishesControllerTests
{
    [Fact]
    public async Task ResidentCanCreatePurchaseWish()
    {
        var wish = new PurchaseWishView(Guid.NewGuid(), "Mesa da cozinha", "https://loja.example/mesa", 0);
        var service = new StubPurchaseWishService { WishResult = PurchaseWishResult.Success(wish) };

        var result = await new PurchaseWishesController(service)
            .Create(new SavePurchaseWishRequest(wish.Name, wish.StoreUrl), CancellationToken.None);

        Assert.Equal(201, result.StatusCode);
        Assert.Same(wish, result.Value);
    }

    [Fact]
    public void PurchaseWishesRequireAuthenticationWithoutAdministratorRole()
    {
        var authorization = Assert.Single(typeof(PurchaseWishesController)
            .GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());

        Assert.Null(authorization.Roles);
    }

    [Fact]
    public async Task InvalidCompleteOrderReturnsBadRequest()
    {
        var service = new StubPurchaseWishService
        {
            ListResult = PurchaseWishResult.Failure<IReadOnlyList<PurchaseWishView>>(
                "purchase_wish_order_invalid", "Ordem inválida."),
        };

        var result = await new PurchaseWishesController(service)
            .Reorder(new ReorderPurchaseWishesRequest([]), CancellationToken.None);

        Assert.Equal(400, result.StatusCode);
    }

    private sealed class StubPurchaseWishService : IPurchaseWishService
    {
        public PurchaseWishResult<IReadOnlyList<PurchaseWishView>> ListResult { get; init; } =
            PurchaseWishResult.Success<IReadOnlyList<PurchaseWishView>>([]);
        public PurchaseWishResult<PurchaseWishView> WishResult { get; init; } =
            PurchaseWishResult.Failure<PurchaseWishView>("missing", "missing");
        public PurchaseWishResult<bool> BooleanResult { get; init; } = PurchaseWishResult.Success(true);

        public Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> GetAsync(CancellationToken cancellationToken) => Task.FromResult(ListResult);
        public Task<PurchaseWishResult<PurchaseWishView>> CreateAsync(SavePurchaseWishCommand command, CancellationToken cancellationToken) => Task.FromResult(WishResult);
        public Task<PurchaseWishResult<PurchaseWishView>> UpdateAsync(Guid id, SavePurchaseWishCommand command, CancellationToken cancellationToken) => Task.FromResult(WishResult);
        public Task<PurchaseWishResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(BooleanResult);
        public Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> ReorderAsync(ReorderPurchaseWishesCommand command, CancellationToken cancellationToken) => Task.FromResult(ListResult);
    }
}
