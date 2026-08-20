using HouseStuff.Application.Pots;
using HouseStuff.Application.Tasks;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Tasks;

internal sealed class HouseholdTaskService(HouseStuffDbContext database, ICurrentResidenceContext residenceContext) : IHouseholdTaskService
{
    public async Task<HouseholdTaskResult<IReadOnlyList<HouseholdTaskView>>> ListAsync(Guid? potId, bool includeArchived, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return HouseholdTaskResult.Failure<IReadOnlyList<HouseholdTaskView>>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var query = from task in database.HouseholdTasks
                    join pot in database.Pots on task.PotId equals pot.Id
                    where task.ResidenceId == residenceId
                    select new { Task = task, PotName = pot.Name };
        if (potId is not null)
        {
            query = query.Where(item => item.Task.PotId == potId);
        }

        if (!includeArchived)
        {
            query = query.Where(item => item.Task.IsActive);
        }

        var tasks = await query.OrderBy(item => item.PotName).ThenBy(item => item.Task.Name)
            .Select(item => ToView(item.Task, item.PotName)).ToListAsync(cancellationToken);
        return HouseholdTaskResult.Success<IReadOnlyList<HouseholdTaskView>>(tasks);
    }

    public async Task<HouseholdTaskResult<HouseholdTaskView>> CreateAsync(SaveHouseholdTaskCommand command, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("residence_required", "Crie ou associe uma casa antes de cadastrar tarefas.");
        }

        var pot = await database.Pots.SingleOrDefaultAsync(item => item.Id == command.PotId && item.ResidenceId == residenceId, cancellationToken);
        if (pot is null)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("pot_not_found", "Pote não encontrado na sua casa.");
        }

        if (!pot.IsActive)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("pot_archived", "Reative o pote antes de adicionar tarefas.");
        }

        if (!TryParseKind(command.Kind, out var kind))
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("task_kind_invalid", "Selecione um tipo de tarefa válido.");
        }

        if (await HasDuplicateNameAsync(residenceId.Value, command.PotId, null, command.Name, cancellationToken))
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("task_name_duplicated", "Já existe uma tarefa com este nome neste pote.");
        }

        var creation = HouseholdTask.Create(residenceId.Value, command.PotId, command.Name, command.Description, kind, command.RecurrenceDays, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>(creation.Code!, creation.Message!);
        }

        database.HouseholdTasks.Add(creation.Task!);
        await database.SaveChangesAsync(cancellationToken);
        return HouseholdTaskResult.Success(ToView(creation.Task!, pot.Name));
    }

    public async Task<HouseholdTaskResult<HouseholdTaskView>> UpdateAsync(Guid id, SaveHouseholdTaskCommand command, CancellationToken cancellationToken)
    {
        var scoped = await GetScopedAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>(scoped.Code!, scoped.Message!);
        }

        var task = scoped.Value!;
        var pot = await database.Pots.SingleOrDefaultAsync(item => item.Id == command.PotId && item.ResidenceId == task.ResidenceId, cancellationToken);
        if (pot is null)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("pot_not_found", "Pote não encontrado na sua casa.");
        }

        if (!pot.IsActive)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("pot_archived", "Selecione um pote ativo.");
        }

        if (!TryParseKind(command.Kind, out var kind))
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("task_kind_invalid", "Selecione um tipo de tarefa válido.");
        }

        if (await HasDuplicateNameAsync(task.ResidenceId, command.PotId, id, command.Name, cancellationToken))
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("task_name_duplicated", "Já existe uma tarefa com este nome neste pote.");
        }

        var update = task.Update(command.PotId, command.Name, command.Description, kind, command.RecurrenceDays, DateTimeOffset.UtcNow);
        if (!update.Succeeded)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>(update.Code!, update.Message!);
        }

        await database.SaveChangesAsync(cancellationToken);
        return HouseholdTaskResult.Success(ToView(task, pot.Name));
    }

    public async Task<HouseholdTaskResult<HouseholdTaskView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var scoped = await GetScopedAsync(id, cancellationToken);
        if (!scoped.Succeeded)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>(scoped.Code!, scoped.Message!);
        }

        var pot = await database.Pots.SingleAsync(item => item.Id == scoped.Value!.PotId, cancellationToken);
        if (isActive && !pot.IsActive)
        {
            return HouseholdTaskResult.Failure<HouseholdTaskView>("pot_archived", "Reative o pote antes de reativar a tarefa.");
        }

        scoped.Value!.SetActive(isActive, DateTimeOffset.UtcNow);
        await database.SaveChangesAsync(cancellationToken);
        return HouseholdTaskResult.Success(ToView(scoped.Value, pot.Name));
    }

    private async Task<HouseholdTaskResult<HouseholdTask>> GetScopedAsync(Guid id, CancellationToken cancellationToken)
    {
        var residenceId = await residenceContext.GetResidenceIdAsync(cancellationToken);
        if (residenceId is null)
        {
            return HouseholdTaskResult.Failure<HouseholdTask>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var task = await database.HouseholdTasks.SingleOrDefaultAsync(item => item.Id == id && item.ResidenceId == residenceId, cancellationToken);
        return task is null
            ? HouseholdTaskResult.Failure<HouseholdTask>("task_not_found", "Tarefa não encontrada na sua casa.")
            : HouseholdTaskResult.Success(task);
    }

    private Task<bool> HasDuplicateNameAsync(Guid residenceId, Guid potId, Guid? excludedId, string name, CancellationToken cancellationToken)
    {
        var normalizedName = HouseholdTask.NormalizeName(name);
        return database.HouseholdTasks.AnyAsync(item => item.ResidenceId == residenceId && item.PotId == potId && item.Id != excludedId && item.NormalizedName == normalizedName, cancellationToken);
    }

    private static bool TryParseKind(string value, out HouseholdTaskKind kind) => Enum.TryParse(value, ignoreCase: true, out kind) && Enum.IsDefined(kind);

    private static HouseholdTaskView ToView(HouseholdTask task, string potName) => new(
        task.Id,
        task.PotId,
        potName,
        task.Name,
        task.Description,
        char.ToLowerInvariant(task.Kind.ToString()[0]) + task.Kind.ToString()[1..],
        task.RecurrenceDays,
        task.IsActive);
}
