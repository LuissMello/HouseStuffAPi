using HouseStuff.Application.Pots;
using HouseStuff.Application.Assignments;
using HouseStuff.Application.Shopping;
using HouseStuff.Domain.Shopping;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Shopping;

internal sealed class ShoppingCatalogService(HouseStuffDbContext database, ICurrentResidenceContext residenceContext, ICurrentUserContext currentUserContext, TimeProvider timeProvider) : IShoppingCatalogService
{
    public async Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> GetCatalogAsync(CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return ShoppingResult.Failure<IReadOnlyList<ShoppingCategoryView>>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        return ShoppingResult.Success(await GetCatalogAsync(residenceId.Value, cancellationToken));
    }

    public async Task<ShoppingResult<ShoppingCategoryView>> CreateCategoryAsync(SaveShoppingCategoryCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return ShoppingResult.Failure<ShoppingCategoryView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var normalizedName = ShoppingCategory.NormalizeName(command.Name);
        if (await database.ShoppingCategories.AnyAsync(category => category.ResidenceId == residenceId && category.NormalizedName == normalizedName, cancellationToken))
        {
            return ShoppingResult.Failure<ShoppingCategoryView>("shopping_category_duplicated", "Já existe uma categoria com este nome na sua casa.");
        }

        var nextOrder = (await database.ShoppingCategories.Where(category => category.ResidenceId == residenceId).MaxAsync(category => (int?)category.DisplayOrder, cancellationToken) ?? -1) + 1;
        var creation = ShoppingCategory.Create(residenceId.Value, command.Name, nextOrder, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingCategoryView>(creation.Code!, creation.Message!);
        }

        database.ShoppingCategories.Add(creation.Value!);
        await database.SaveChangesAsync(cancellationToken);
        return ShoppingResult.Success(ToCategoryView(creation.Value!, []));
    }

    public async Task<ShoppingResult<ShoppingCategoryView>> UpdateCategoryAsync(Guid id, SaveShoppingCategoryCommand command, CancellationToken cancellationToken)
    {
        var scoped = await GetCategoryAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingCategoryView>(scoped.Code!, scoped.Message!);
        }

        var category = scoped.Value!;
        var normalizedName = ShoppingCategory.NormalizeName(command.Name);
        if (await database.ShoppingCategories.AnyAsync(item => item.ResidenceId == category.ResidenceId && item.Id != id && item.NormalizedName == normalizedName, cancellationToken))
        {
            return ShoppingResult.Failure<ShoppingCategoryView>("shopping_category_duplicated", "Já existe uma categoria com este nome na sua casa.");
        }

        var update = category.Update(command.Name, DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingCategoryView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        var items = await database.ShoppingItems.Where(item => item.CategoryId == category.Id && item.ResidenceId == category.ResidenceId)
            .OrderBy(item => item.Name).Select(item => ToItemView(item)).ToListAsync(cancellationToken);
        return ShoppingResult.Success(ToCategoryView(category, items));
    }

    public async Task<ShoppingResult<IReadOnlyList<ShoppingCategoryView>>> MoveCategoryAsync(Guid id, int offset, CancellationToken cancellationToken)
    {
        if (offset is not (-1 or 1))
        {
            return ShoppingResult.Failure<IReadOnlyList<ShoppingCategoryView>>("shopping_move_invalid", "O deslocamento deve ser -1 ou 1.");
        }

        var scoped = await GetCategoryAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return ShoppingResult.Failure<IReadOnlyList<ShoppingCategoryView>>(scoped.Code!, scoped.Message!);
        }

        var ordered = await database.ShoppingCategories.Where(category => category.ResidenceId == scoped.Value!.ResidenceId)
            .OrderBy(category => category.DisplayOrder).ThenBy(category => category.Name).ToListAsync(cancellationToken);
        var index = ordered.FindIndex(category => category.Id == id);
        var targetIndex = index + offset;
        if (targetIndex >= 0 && targetIndex < ordered.Count)
        {
            var target = ordered[targetIndex];
            var currentOrder = scoped.Value!.DisplayOrder;
            scoped.Value.SetDisplayOrder(target.DisplayOrder, DateTimeOffset.UtcNow);
            target.SetDisplayOrder(currentOrder, DateTimeOffset.UtcNow);
            await database.SaveChangesAsync(cancellationToken);
        }

        return ShoppingResult.Success(await GetCatalogAsync(scoped.Value!.ResidenceId, cancellationToken));
    }

    public async Task<ShoppingResult<bool>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var scoped = await GetCategoryAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return ShoppingResult.Failure<bool>(scoped.Code!, scoped.Message!);
        }

        if (await database.ShoppingItems.AnyAsync(item => item.CategoryId == id && item.ResidenceId == scoped.Value!.ResidenceId, cancellationToken))
        {
            return ShoppingResult.Failure<bool>("shopping_category_not_empty", "Remova os itens desta categoria antes de excluí-la.");
        }

        database.ShoppingCategories.Remove(scoped.Value!);
        await database.SaveChangesAsync(cancellationToken);
        return ShoppingResult.Success(true);
    }

    public async Task<ShoppingResult<ShoppingItemView>> CreateItemAsync(SaveShoppingItemCommand command, CancellationToken cancellationToken)
    {
        var category = await GetCategoryAsync(command.CategoryId, cancellationToken);
        if (!category.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingItemView>(category.Code!, category.Message!);
        }

        var normalizedName = ShoppingItem.NormalizeName(command.Name);
        if (await database.ShoppingItems.AnyAsync(item => item.ResidenceId == category.Value!.ResidenceId && item.CategoryId == command.CategoryId && item.NormalizedName == normalizedName, cancellationToken))
        {
            return ShoppingResult.Failure<ShoppingItemView>("shopping_item_duplicated", "Já existe um item com este nome nesta categoria.");
        }

        var creation = ShoppingItem.Create(category.Value!.ResidenceId, command.CategoryId, command.Name, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingItemView>(creation.Code!, creation.Message!);
        }

        database.ShoppingItems.Add(creation.Value!);
        await database.SaveChangesAsync(cancellationToken);
        return ShoppingResult.Success(ToItemView(creation.Value!));
    }

    public async Task<ShoppingResult<ShoppingItemView>> UpdateItemAsync(Guid id, SaveShoppingItemCommand command, CancellationToken cancellationToken)
    {
        var item = await GetItemAsync(id, cancellationToken);
        if (!item.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingItemView>(item.Code!, item.Message!);
        }

        var category = await GetCategoryAsync(command.CategoryId, cancellationToken);
        if (!category.Succeeded || category.Value!.ResidenceId != item.Value!.ResidenceId)
        {
            return ShoppingResult.Failure<ShoppingItemView>("shopping_category_not_found", "Categoria não encontrada na sua casa.");
        }

        var normalizedName = ShoppingItem.NormalizeName(command.Name);
        if (await database.ShoppingItems.AnyAsync(stored => stored.ResidenceId == item.Value!.ResidenceId && stored.CategoryId == command.CategoryId && stored.Id != id && stored.NormalizedName == normalizedName, cancellationToken))
        {
            return ShoppingResult.Failure<ShoppingItemView>("shopping_item_duplicated", "Já existe um item com este nome nesta categoria.");
        }

        var update = item.Value!.Update(command.CategoryId, command.Name, DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingItemView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        return ShoppingResult.Success(ToItemView(item.Value!));
    }

    public async Task<ShoppingResult<bool>> DeleteItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var scoped = await GetItemAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return ShoppingResult.Failure<bool>(scoped.Code!, scoped.Message!);
        }

        database.ShoppingItems.Remove(scoped.Value!);
        await database.SaveChangesAsync(cancellationToken);
        return ShoppingResult.Success(true);
    }

    public async Task<ShoppingResult<ShoppingPurchaseView>> CompletePurchaseAsync(CompleteShoppingPurchaseCommand command, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContext.GetAsync(cancellationToken);
        if (currentUser is null)
        {
            return ShoppingResult.Failure<ShoppingPurchaseView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var itemIds = command.ItemIds.Distinct().ToArray();
        if (itemIds.Length == 0)
        {
            return ShoppingResult.Failure<ShoppingPurchaseView>("shopping_purchase_empty", "Marque ao menos um item antes de finalizar a compra.");
        }

        var selected = await (
            from item in database.ShoppingItems
            join category in database.ShoppingCategories
                on new { item.CategoryId, item.ResidenceId } equals new { CategoryId = category.Id, category.ResidenceId }
            where item.ResidenceId == currentUser.ResidenceId && itemIds.Contains(item.Id)
            select new { Item = item, CategoryName = category.Name })
            .ToListAsync(cancellationToken);

        if (selected.Count != itemIds.Length)
        {
            return ShoppingResult.Failure<ShoppingPurchaseView>("shopping_item_not_found", "Um ou mais itens não estão pendentes na sua casa.");
        }

        var completedAt = timeProvider.GetUtcNow();
        var creation = ShoppingPurchase.Create(
            currentUser.ResidenceId,
            currentUser.UserId,
            selected.Select(entry => new ShoppingPurchaseLine(entry.CategoryName, entry.Item.Name)).ToArray(),
            completedAt);
        if (!creation.Succeeded)
        {
            return ShoppingResult.Failure<ShoppingPurchaseView>(creation.Code!, creation.Message!);
        }

        var completedByName = await database.Users.Where(user => user.Id == currentUser.UserId)
            .Select(user => user.Name).SingleAsync(cancellationToken);
        database.ShoppingPurchases.Add(creation.Value!);
        database.ShoppingItems.RemoveRange(selected.Select(entry => entry.Item));
        await database.SaveChangesAsync(cancellationToken);

        return ShoppingResult.Success(ToPurchaseView(creation.Value!, completedByName));
    }

    public async Task<ShoppingResult<IReadOnlyList<ShoppingPurchaseView>>> GetPurchaseHistoryAsync(CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return ShoppingResult.Failure<IReadOnlyList<ShoppingPurchaseView>>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var purchases = await database.ShoppingPurchases.Where(purchase => purchase.ResidenceId == residenceId)
            .Include(purchase => purchase.Items)
            .OrderByDescending(purchase => purchase.CompletedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
        var userIds = purchases.Select(purchase => purchase.CompletedByUserId).Distinct().ToArray();
        var names = await database.Users.Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.Name, cancellationToken);

        return ShoppingResult.Success<IReadOnlyList<ShoppingPurchaseView>>(purchases
            .Select(purchase => ToPurchaseView(purchase, names.GetValueOrDefault(purchase.CompletedByUserId, "Morador")))
            .ToList());
    }

    private async Task<ShoppingResult<ShoppingCategory>> GetCategoryAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return ShoppingResult.Failure<ShoppingCategory>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var category = await database.ShoppingCategories.SingleOrDefaultAsync(item => item.Id == id && item.ResidenceId == residenceId, cancellationToken);
        return category is null
            ? ShoppingResult.Failure<ShoppingCategory>("shopping_category_not_found", "Categoria não encontrada na sua casa.")
            : ShoppingResult.Success(category);
    }

    private async Task<ShoppingResult<ShoppingItem>> GetItemAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return ShoppingResult.Failure<ShoppingItem>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var item = await database.ShoppingItems.SingleOrDefaultAsync(stored => stored.Id == id && stored.ResidenceId == residenceId, cancellationToken);
        return item is null
            ? ShoppingResult.Failure<ShoppingItem>("shopping_item_not_found", "Item não encontrado na sua casa.")
            : ShoppingResult.Success(item);
    }

    private async Task<IReadOnlyList<ShoppingCategoryView>> GetCatalogAsync(Guid residenceId, CancellationToken cancellationToken)
    {
        var categories = await database.ShoppingCategories.Where(category => category.ResidenceId == residenceId)
            .OrderBy(category => category.DisplayOrder).ThenBy(category => category.Name).ToListAsync(cancellationToken);
        var items = await database.ShoppingItems.Where(item => item.ResidenceId == residenceId)
            .OrderBy(item => item.Name).Select(item => ToItemView(item)).ToListAsync(cancellationToken);
        return categories.Select(category => ToCategoryView(category, items.Where(item => item.CategoryId == category.Id).ToList())).ToList();
    }

    private static ShoppingCategoryView ToCategoryView(ShoppingCategory category, IReadOnlyList<ShoppingItemView> items) =>
        new(category.Id, category.Name, category.DisplayOrder, items);

    private static ShoppingItemView ToItemView(ShoppingItem item) => new(item.Id, item.CategoryId, item.Name);

    private static ShoppingPurchaseView ToPurchaseView(ShoppingPurchase purchase, string completedByName) =>
        new(purchase.Id, purchase.CompletedAt, completedByName, purchase.Items
            .OrderBy(item => item.CategoryName).ThenBy(item => item.ItemName)
            .Select(item => new ShoppingPurchaseItemView(item.CategoryName, item.ItemName)).ToList());
}
