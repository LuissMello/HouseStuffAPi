using System.Security.Cryptography;
using HouseStuff.Application.Assignments;
using HouseStuff.Domain.Assignments;
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

        var excluded = command.ExcludedTaskIds.Distinct().Take(100).ToArray();
        var candidates = await (from task in database.HouseholdTasks
                                join pot in database.Pots on task.PotId equals pot.Id
                                where task.ResidenceId == session.ResidenceId
                                    && task.PotId == command.PotId
                                    && task.IsActive
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
                                    task.RecurrenceDays))
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

        var candidate = await (from task in database.HouseholdTasks
                               join pot in database.Pots on task.PotId equals pot.Id
                               where task.Id == taskId
                                   && task.ResidenceId == session.ResidenceId
                                   && task.IsActive
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
            assignment.AcceptedAt);

    private static ActiveAssignmentView ToView(TaskAssignment assignment, Domain.Tasks.HouseholdTask task, string potName) => new(
        assignment.Id,
        task.Id,
        task.PotId,
        potName,
        task.Name,
        task.Description,
        ToKind(task.Kind),
        task.RecurrenceDays,
        assignment.AcceptedAt);

    private static string ToKind(Domain.Tasks.HouseholdTaskKind kind) => char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString()[1..];
}
