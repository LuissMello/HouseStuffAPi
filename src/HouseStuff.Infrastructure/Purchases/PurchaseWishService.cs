using HouseStuff.Application.Pots;
using HouseStuff.Application.Purchases;
using HouseStuff.Domain.Purchases;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Purchases;

internal sealed class PurchaseWishService(HouseStuffDbContext database, ICurrentResidenceContext residenceContext) : IPurchaseWishService
{
    public async Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> GetAsync(CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        return residenceId is null
            ? PurchaseWishResult.Failure<IReadOnlyList<PurchaseWishView>>("residence_required", "Você precisa estar vinculado a uma casa.")
            : PurchaseWishResult.Success(await GetOrderedAsync(residenceId.Value, cancellationToken));
    }

    public async Task<PurchaseWishResult<PurchaseWishView>> CreateAsync(SavePurchaseWishCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PurchaseWishResult.Failure<PurchaseWishView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var nextPriority = (await database.PurchaseWishes
            .Where(wish => wish.ResidenceId == residenceId)
            .MaxAsync(wish => (int?)wish.Priority, cancellationToken) ?? -1) + 1;
        var creation = PurchaseWish.Create(residenceId.Value, command.Name, command.StoreUrl, nextPriority, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return PurchaseWishResult.Failure<PurchaseWishView>(creation.Code!, creation.Message!);
        }

        database.PurchaseWishes.Add(creation.Wish!);
        await database.SaveChangesAsync(cancellationToken);
        return PurchaseWishResult.Success(ToView(creation.Wish!));
    }

    public async Task<PurchaseWishResult<PurchaseWishView>> UpdateAsync(Guid id, SavePurchaseWishCommand command, CancellationToken cancellationToken)
    {
        var scoped = await GetWishAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return PurchaseWishResult.Failure<PurchaseWishView>(scoped.Code!, scoped.Message!);
        }

        var update = scoped.Value!.Update(command.Name, command.StoreUrl, DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return PurchaseWishResult.Failure<PurchaseWishView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        return PurchaseWishResult.Success(ToView(scoped.Value));
    }

    public async Task<PurchaseWishResult<bool>> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PurchaseWishResult.Failure<bool>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var ordered = await GetOrderedEntitiesAsync(residenceId.Value, cancellationToken);
        var wish = ordered.SingleOrDefault(stored => stored.Id == id);
        if (wish is null)
        {
            return PurchaseWishResult.Failure<bool>("purchase_wish_not_found", "Desejo não encontrado na sua casa.");
        }

        database.PurchaseWishes.Remove(wish);
        var now = DateTimeOffset.UtcNow;
        var remaining = ordered.Where(stored => stored.Id != id).ToList();
        for (var index = 0; index < remaining.Count; index++)
        {
            remaining[index].SetPriority(index, now);
        }

        await database.SaveChangesAsync(cancellationToken);
        return PurchaseWishResult.Success(true);
    }

    public async Task<PurchaseWishResult<IReadOnlyList<PurchaseWishView>>> ReorderAsync(ReorderPurchaseWishesCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PurchaseWishResult.Failure<IReadOnlyList<PurchaseWishView>>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var stored = await GetOrderedEntitiesAsync(residenceId.Value, cancellationToken);
        var submitted = command.OrderedIds ?? [];
        if (submitted.Count != stored.Count || submitted.Distinct().Count() != submitted.Count ||
            !submitted.ToHashSet().SetEquals(stored.Select(wish => wish.Id)))
        {
            return PurchaseWishResult.Failure<IReadOnlyList<PurchaseWishView>>("purchase_wish_order_invalid", "Envie todos os desejos da casa uma única vez na ordem desejada.");
        }

        var byId = stored.ToDictionary(wish => wish.Id);
        var now = DateTimeOffset.UtcNow;
        for (var index = 0; index < submitted.Count; index++)
        {
            byId[submitted[index]].SetPriority(index, now);
        }

        await database.SaveChangesAsync(cancellationToken);
        return PurchaseWishResult.Success<IReadOnlyList<PurchaseWishView>>(submitted.Select(id => ToView(byId[id])).ToList());
    }

    private async Task<PurchaseWishResult<PurchaseWish>> GetWishAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PurchaseWishResult.Failure<PurchaseWish>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var wish = await database.PurchaseWishes.SingleOrDefaultAsync(stored => stored.Id == id && stored.ResidenceId == residenceId, cancellationToken);
        return wish is null
            ? PurchaseWishResult.Failure<PurchaseWish>("purchase_wish_not_found", "Desejo não encontrado na sua casa.")
            : PurchaseWishResult.Success(wish);
    }

    private async Task<IReadOnlyList<PurchaseWishView>> GetOrderedAsync(Guid residenceId, CancellationToken cancellationToken) =>
        (await GetOrderedEntitiesAsync(residenceId, cancellationToken)).Select(ToView).ToList();

    private async Task<List<PurchaseWish>> GetOrderedEntitiesAsync(Guid residenceId, CancellationToken cancellationToken) =>
        await database.PurchaseWishes.Where(wish => wish.ResidenceId == residenceId)
            .OrderBy(wish => wish.Priority).ThenBy(wish => wish.CreatedAt).ThenBy(wish => wish.Id)
            .ToListAsync(cancellationToken);

    private static PurchaseWishView ToView(PurchaseWish wish) => new(wish.Id, wish.Name, wish.StoreUrl, wish.Priority);
}
