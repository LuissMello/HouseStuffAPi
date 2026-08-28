using HouseStuff.Application.Purchases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/purchase-wishes")]
public sealed class PurchaseWishesController(IPurchaseWishService wishes) : ControllerBase
{
    [HttpGet]
    public async Task<ObjectResult> Get(CancellationToken cancellationToken) =>
        ToActionResult(await wishes.GetAsync(cancellationToken), StatusCodes.Status200OK);

    [HttpPost]
    public async Task<ObjectResult> Create(SavePurchaseWishRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await wishes.CreateAsync(new SavePurchaseWishCommand(request.Name, request.StoreUrl), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("{id:guid}")]
    public async Task<ObjectResult> Update(Guid id, SavePurchaseWishRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await wishes.UpdateAsync(id, new SavePurchaseWishCommand(request.Name, request.StoreUrl), cancellationToken), StatusCodes.Status200OK);

    [HttpDelete("{id:guid}")]
    public async Task<ObjectResult> Delete(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await wishes.DeleteAsync(id, cancellationToken), StatusCodes.Status200OK);

    [HttpPut("order")]
    public async Task<ObjectResult> Reorder(ReorderPurchaseWishesRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await wishes.ReorderAsync(new ReorderPurchaseWishesCommand(request.OrderedIds), cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(PurchaseWishResult<T> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code == "purchase_wish_not_found" ? StatusCodes.Status404NotFound : StatusCodes.Status400BadRequest;
        return this.ProblemWithCode(status, result.Message, result.Code);
    }
}

public sealed record SavePurchaseWishRequest(string Name, string? StoreUrl);
public sealed record ReorderPurchaseWishesRequest(IReadOnlyList<Guid> OrderedIds);
