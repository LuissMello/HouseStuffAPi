using HouseStuff.Application.Assignments;
using HouseStuff.Application.Routine;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Routine;

internal sealed class RoutineOverviewService(HouseStuffDbContext database, ICurrentUserContext currentUser) : IRoutineOverviewService
{
    public async Task<RoutineOverviewResult> GetAsync(CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return RoutineOverviewResult.Failure("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var now = DateTimeOffset.UtcNow;
        var upcoming = await (from task in database.HouseholdTasks
                              join pot in database.Pots on task.PotId equals pot.Id
                              where task.ResidenceId == session.ResidenceId
                                  && task.Kind == HouseholdTaskKind.Recurring
                                  && task.IsActive
                                  && pot.IsActive
                                  && task.NextAvailableAt != null
                                  && task.NextAvailableAt > now
                              orderby task.NextAvailableAt, pot.Name, task.Name
                              select new UpcomingRecurringTaskView(
                                  task.Id,
                                  task.PotId,
                                  pot.Name,
                                  task.Name,
                                  task.Description,
                                  task.NextAvailableAt!.Value))
            .ToListAsync(cancellationToken);

        var history = await (from assignment in database.TaskAssignments
                             join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
                             join pot in database.Pots on task.PotId equals pot.Id
                             where assignment.AssignedToUserId == session.UserId
                                 && assignment.CompletedAt != null
                                 && task.ResidenceId == session.ResidenceId
                             orderby assignment.CompletedAt descending, assignment.Id
                             select new CompletionHistoryItemView(
                                 assignment.Id,
                                 task.Id,
                                 pot.Name,
                                 task.Name,
                                 ToKind(task.Kind),
                                 assignment.AcceptedAt,
                                 assignment.CompletedAt!.Value))
            .Take(50)
            .ToListAsync(cancellationToken);

        return RoutineOverviewResult.Success(new RoutineOverviewView(now, upcoming, history));
    }

    private static string ToKind(HouseholdTaskKind kind) => char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString()[1..];
}
