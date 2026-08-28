using System.Security.Cryptography;
using HouseStuff.Application.Assignments;
using HouseStuff.Domain.Assignments;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Assignments;

internal sealed class TaskAssignmentService(HouseStuffDbContext database, ICurrentUserContext currentUser) : ITaskAssignmentService
{
    public async Task<AssignmentResult<ActiveAssignmentView?>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return AssignmentResult.Failure<ActiveAssignmentView?>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var assignment = await AssignmentQuery(session)
            .SingleOrDefaultAsync(cancellationToken);
        return AssignmentResult.Success(assignment);
    }

    public async Task<AssignmentResult<DrawProposalView>> DrawAsync(DrawTaskCommand command, CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return AssignmentResult.Failure<DrawProposalView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        if (await database.TaskAssignments.AnyAsync(item => item.AssignedToUserId == session.UserId && item.CompletedAt == null, cancellationToken))
        {
            return AssignmentResult.Failure<DrawProposalView>("assignment_already_active", "Conclua sua tarefa atual antes de sortear outra.");
        }

        var potExists = await database.Pots.AnyAsync(pot => pot.Id == command.PotId && pot.ResidenceId == session.ResidenceId && pot.IsActive, cancellationToken);
        if (!potExists)
        {
            return AssignmentResult.Failure<DrawProposalView>("pot_not_found", "Pote ativo não encontrado na sua casa.");
        }

        HouseholdTaskDifficulty? difficulty = null;
        if (!string.IsNullOrWhiteSpace(command.Difficulty))
        {
            if (!Enum.TryParse<HouseholdTaskDifficulty>(command.Difficulty, ignoreCase: true, out var parsedDifficulty) || !Enum.IsDefined(parsedDifficulty))
            {
                return AssignmentResult.Failure<DrawProposalView>("task_difficulty_invalid", "Selecione uma dificuldade válida.");
            }

            difficulty = parsedDifficulty;
        }

        var excluded = command.ExcludedTaskIds.Distinct().Take(100).ToArray();
        var now = DateTimeOffset.UtcNow;
        var candidates = await (from task in database.HouseholdTasks
                                join pot in database.Pots on task.PotId equals pot.Id
                                where task.ResidenceId == session.ResidenceId
                                    && task.PotId == command.PotId
                                    && task.IsActive
                                    && (difficulty == null || task.Difficulty == difficulty)
                                    && (task.IsAvailableToAllResidents || task.EligibleUsers.Any(user => user.UserId == session.UserId))
                                    && (task.NextAvailableAt == null || task.NextAvailableAt <= now)
                                    && pot.IsActive
                                    && !excluded.Contains(task.Id)
                                    && !database.TaskAssignments.Any(assignment => assignment.HouseholdTaskId == task.Id && assignment.CompletedAt == null)
                                select new DrawProposalView(
                                    task.Id,
                                    task.PotId,
                                    pot.Name,
                                    task.Name,
                                    task.Description,
                                    ToKind(task.Kind),
                                    task.RecurrenceDays,
                                    ToDifficulty(task.Difficulty)))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return AssignmentResult.Failure<DrawProposalView>("no_tasks_available", "Não há outra tarefa disponível neste pote agora.");
        }

        return AssignmentResult.Success(candidates[RandomNumberGenerator.GetInt32(candidates.Count)]);
    }

    public async Task<AssignmentResult<ActiveAssignmentView>> AcceptAsync(Guid taskId, CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return AssignmentResult.Failure<ActiveAssignmentView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        if (await database.TaskAssignments.AnyAsync(item => item.AssignedToUserId == session.UserId && item.CompletedAt == null, cancellationToken))
        {
            return AssignmentResult.Failure<ActiveAssignmentView>("assignment_already_active", "Você já possui uma tarefa ativa.");
        }

        var now = DateTimeOffset.UtcNow;
        var candidate = await (from task in database.HouseholdTasks
                               join pot in database.Pots on task.PotId equals pot.Id
                               where task.Id == taskId
                                   && task.ResidenceId == session.ResidenceId
                                   && task.IsActive
                                   && (task.IsAvailableToAllResidents || task.EligibleUsers.Any(user => user.UserId == session.UserId))
                                   && (task.NextAvailableAt == null || task.NextAvailableAt <= now)
                                   && pot.IsActive
                                   && !database.TaskAssignments.Any(assignment => assignment.HouseholdTaskId == task.Id && assignment.CompletedAt == null)
                               select new { Task = task, PotName = pot.Name })
            .SingleOrDefaultAsync(cancellationToken);
        if (candidate is null)
        {
            return AssignmentResult.Failure<ActiveAssignmentView>("task_unavailable", "Esta tarefa não está mais disponível. Sorteie outra.");
        }

        var acceptedAt = DateTimeOffset.UtcNow;
        var creation = TaskAssignment.Create(taskId, session.UserId, acceptedAt);
        if (!creation.Succeeded)
        {
            return AssignmentResult.Failure<ActiveAssignmentView>(creation.Code!, creation.Message!);
        }

        database.TaskAssignments.Add(creation.Assignment!);
        try
        {
            await database.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            database.ChangeTracker.Clear();
            return AssignmentResult.Failure<ActiveAssignmentView>("assignment_conflict", "A tarefa acabou de ser aceita. Sorteie outra.");
        }

        return AssignmentResult.Success(ToView(creation.Assignment!, candidate.Task, candidate.PotName));
    }

    public async Task<AssignmentResult<CompletedAssignmentView>> CompleteCurrentAsync(CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return AssignmentResult.Failure<CompletedAssignmentView>("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var current = await (from assignment in database.TaskAssignments
                             join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
                             where assignment.AssignedToUserId == session.UserId
                                 && assignment.CompletedAt == null
                                 && task.ResidenceId == session.ResidenceId
                             select new { Assignment = assignment, Task = task })
            .SingleOrDefaultAsync(cancellationToken);
        if (current is null)
        {
            return AssignmentResult.Failure<CompletedAssignmentView>("active_assignment_not_found", "Você não possui uma tarefa ativa para concluir.");
        }

        var completedAt = DateTimeOffset.UtcNow;
        var completion = current.Assignment.Complete(completedAt);
        if (!completion.Succeeded)
        {
            return AssignmentResult.Failure<CompletedAssignmentView>(completion.Code!, completion.Message!);
        }

        current.Task.RegisterCompletion(completedAt);
        await database.SaveChangesAsync(cancellationToken);

        return AssignmentResult.Success(new CompletedAssignmentView(
            current.Assignment.Id,
            current.Task.Id,
            current.Task.Name,
            ToKind(current.Task.Kind),
            completedAt,
            current.Task.NextAvailableAt,
            current.Task.Kind != HouseholdTaskKind.OneTime));
    }

    private IQueryable<ActiveAssignmentView> AssignmentQuery(CurrentUserSession session) =>
        from assignment in database.TaskAssignments
        join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
        join pot in database.Pots on task.PotId equals pot.Id
        where assignment.AssignedToUserId == session.UserId
            && assignment.CompletedAt == null
            && task.ResidenceId == session.ResidenceId
        select new ActiveAssignmentView(
            assignment.Id,
            task.Id,
            task.PotId,
            pot.Name,
            task.Name,
            task.Description,
            ToKind(task.Kind),
            task.RecurrenceDays,
            assignment.AcceptedAt,
            ToDifficulty(task.Difficulty));

    private static ActiveAssignmentView ToView(TaskAssignment assignment, Domain.Tasks.HouseholdTask task, string potName) => new(
        assignment.Id,
        task.Id,
        task.PotId,
        potName,
        task.Name,
        task.Description,
        ToKind(task.Kind),
        task.RecurrenceDays,
        assignment.AcceptedAt,
        ToDifficulty(task.Difficulty));

    private static string ToKind(Domain.Tasks.HouseholdTaskKind kind) => char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString()[1..];
    private static string ToDifficulty(HouseholdTaskDifficulty difficulty) => char.ToLowerInvariant(difficulty.ToString()[0]) + difficulty.ToString()[1..];
}
