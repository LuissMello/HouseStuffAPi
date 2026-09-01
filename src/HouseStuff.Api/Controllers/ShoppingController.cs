using HouseStuff.Application.Shopping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/shopping")]
public sealed class ShoppingController(IShoppingCatalogService shopping) : ControllerBase
{
    [HttpGet("catalog")]
    public async Task<ObjectResult> GetCatalog(CancellationToken cancellationToken) =>
        ToActionResult(await shopping.GetCatalogAsync(cancellationToken), StatusCodes.Status200OK);

    [HttpPost("categories")]
    public async Task<ObjectResult> CreateCategory(SaveShoppingCategoryRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.CreateCategoryAsync(new SaveShoppingCategoryCommand(request.Name), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("categories/{id:guid}")]
    public async Task<ObjectResult> UpdateCategory(Guid id, SaveShoppingCategoryRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.UpdateCategoryAsync(id, new SaveShoppingCategoryCommand(request.Name), cancellationToken), StatusCodes.Status200OK);

    [HttpPost("categories/{id:guid}/move")]
    public async Task<ObjectResult> MoveCategory(Guid id, MoveShoppingCategoryRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.MoveCategoryAsync(id, request.Offset, cancellationToken), StatusCodes.Status200OK);

    [HttpDelete("categories/{id:guid}")]
    public async Task<ObjectResult> DeleteCategory(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.DeleteCategoryAsync(id, cancellationToken), StatusCodes.Status200OK);

    [HttpPost("items")]
    public async Task<ObjectResult> CreateItem(SaveShoppingItemRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.CreateItemAsync(new SaveShoppingItemCommand(request.CategoryId, request.Name), cancellationToken), StatusCodes.Status201Created);

    [HttpPut("items/{id:guid}")]
    public async Task<ObjectResult> UpdateItem(Guid id, SaveShoppingItemRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.UpdateItemAsync(id, new SaveShoppingItemCommand(request.CategoryId, request.Name), cancellationToken), StatusCodes.Status200OK);

    [HttpDelete("items/{id:guid}")]
    public async Task<ObjectResult> DeleteItem(Guid id, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.DeleteItemAsync(id, cancellationToken), StatusCodes.Status200OK);

    [HttpPost("purchases")]
    public async Task<ObjectResult> CompletePurchase(CompleteShoppingPurchaseRequest request, CancellationToken cancellationToken) =>
        ToActionResult(await shopping.CompletePurchaseAsync(new CompleteShoppingPurchaseCommand(request.ItemIds), cancellationToken), StatusCodes.Status201Created);

    [HttpGet("purchases/history")]
    public async Task<ObjectResult> GetPurchaseHistory(CancellationToken cancellationToken) =>
        ToActionResult(await shopping.GetPurchaseHistoryAsync(cancellationToken), StatusCodes.Status200OK);

    private ObjectResult ToActionResult<T>(ShoppingResult<T> result, int successStatus)
    {
        if (result.Succeeded)
        {
            return StatusCode(successStatus, result.Value);
        }

        var status = result.Code switch
        {
            "shopping_category_not_found" or "shopping_item_not_found" => StatusCodes.Status404NotFound,
            "shopping_category_not_empty" or "shopping_category_duplicated" or "shopping_item_duplicated" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest,
        };
        return this.ProblemWithCode(status, result.Message, result.Code);
    }
}

public sealed record SaveShoppingCategoryRequest(string Name);
public sealed record MoveShoppingCategoryRequest(int Offset);
public sealed record SaveShoppingItemRequest(Guid CategoryId, string Name);
public sealed record CompleteShoppingPurchaseRequest(IReadOnlyCollection<Guid> ItemIds);
