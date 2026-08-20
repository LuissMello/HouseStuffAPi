using HouseStuff.Application.Pots;
using HouseStuff.Domain.Pots;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Pots;

internal sealed class PotService(HouseStuffDbContext database, ICurrentResidenceContext residenceContext) : IPotService
{
    public async Task<PotResult<IReadOnlyList<PotView>>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PotResult.Failure<IReadOnlyList<PotView>>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var query = database.Pots.Where(pot => pot.ResidenceId == residenceId);
        if (!includeArchived)
        {
            query = query.Where(pot => pot.IsActive);
        }

        var pots = await query.OrderBy(pot => pot.DisplayOrder).ThenBy(pot => pot.Name).Select(pot => ToView(pot)).ToListAsync(cancellationToken);
        return PotResult.Success<IReadOnlyList<PotView>>(pots);
    }

    public async Task<PotResult<PotView>> CreateAsync(SavePotCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PotResult.Failure<PotView>("residence_required", "Crie ou associe uma casa antes de cadastrar potes.");
        }

        var normalizedName = Pot.NormalizeName(command.Name);
        if (await database.Pots.AnyAsync(pot => pot.ResidenceId == residenceId && pot.NormalizedName == normalizedName, cancellationToken))
        {
            return PotResult.Failure<PotView>("pot_name_duplicated", "Já existe um pote com este nome na sua casa.");
        }

        var nextOrder = (await database.Pots.Where(pot => pot.ResidenceId == residenceId).MaxAsync(pot => (int?)pot.DisplayOrder, cancellationToken) ?? -1) + 1;
        var creation = Pot.Create(residenceId.Value, command.Name, command.Description, nextOrder, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return PotResult.Failure<PotView>(creation.Code!, creation.Message!);
        }

        database.Pots.Add(creation.Pot!);
        await database.SaveChangesAsync(cancellationToken);
        return PotResult.Success(ToView(creation.Pot!));
    }

    public async Task<PotResult<PotView>> UpdateAsync(Guid id, SavePotCommand command, CancellationToken cancellationToken)
    {
        var scoped = await GetScopedAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return PotResult.Failure<PotView>(scoped.Code!, scoped.Message!);
        }

        var pot = scoped.Value!;
        var normalizedName = Pot.NormalizeName(command.Name);
        if (await database.Pots.AnyAsync(item => item.ResidenceId == pot.ResidenceId && item.Id != id && item.NormalizedName == normalizedName, cancellationToken))
        {
            return PotResult.Failure<PotView>("pot_name_duplicated", "Já existe um pote com este nome na sua casa.");
        }

        var update = pot.Update(command.Name, command.Description, DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return PotResult.Failure<PotView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        return PotResult.Success(ToView(pot));
    }

    public async Task<PotResult<PotView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var scoped = await GetScopedAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return PotResult.Failure<PotView>(scoped.Code!, scoped.Message!);
        }

        scoped.Value!.SetActive(isActive, DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(cancellationToken);
        return PotResult.Success(ToView(scoped.Value));
    }

    public async Task<PotResult<IReadOnlyList<PotView>>> MoveAsync(Guid id, int offset, CancellationToken cancellationToken)
    {
        if (offset is not (-1 or 1))
        {
            return PotResult.Failure<IReadOnlyList<PotView>>("move_invalid", "O deslocamento deve ser -1 ou 1.");
        }

        var scoped = await GetScopedAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return PotResult.Failure<IReadOnlyList<PotView>>(scoped.Code!, scoped.Message!);
        }

        var ordered = await database.Pots.Where(pot => pot.ResidenceId == scoped.Value!.ResidenceId)
            .OrderBy(pot => pot.DisplayOrder).ThenBy(pot => pot.Name).ToListAsync(cancellationToken);
        var index = ordered.FindIndex(pot => pot.Id == id);
        var targetIndex = index + offset;
        if (targetIndex >= 0 && targetIndex < ordered.Count)
        {
            var target = ordered[targetIndex];
            var currentOrder = scoped.Value!.DisplayOrder;
            scoped.Value.SetDisplayOrder(target.DisplayOrder, DateTimeOffset.UtcNow);
            target.SetDisplayOrder(currentOrder, DateTimeOffset.UtcNow);
            await database.SaveChangesAsync(cancellationToken);
        }

        return PotResult.Success<IReadOnlyList<PotView>>(ordered.OrderBy(pot => pot.DisplayOrder).ThenBy(pot => pot.Name).Select(ToView).ToList());
    }

    private async Task<PotResult<Pot>> GetScopedAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return PotResult.Failure<Pot>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var pot = await database.Pots.SingleOrDefaultAsync(item => item.Id == id && item.ResidenceId == residenceId, cancellationToken);
        return pot is null
            ? PotResult.Failure<Pot>("pot_not_found", "Pote não encontrado na sua casa.")
            : PotResult.Success(pot);
    }

    private static PotView ToView(Pot pot) => new(pot.Id, pot.Name, pot.Description, pot.DisplayOrder, pot.IsActive);
}
