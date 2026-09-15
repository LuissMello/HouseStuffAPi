using HouseStuff.Application.Assignments;
using HouseStuff.Application.Identity;
using HouseStuff.Application.Routine;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Routine;

internal sealed class HouseholdDashboardService(HouseStuffDbContext database, ICurrentUserContext currentUser) : IHouseholdDashboardService
{
    public async Task<DashboardResult> GetAsync(CancellationToken cancellationToken)
    {
        var session = await currentUser.GetAsync(cancellationToken);
        if (session is null)
        {
            return DashboardResult.Failure("residence_required", "Você precisa estar vinculado a uma casa.");
        }

        var members = await database.Users
            .Where(user => user.ResidenceId == session.ResidenceId)
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

        var active = await (from assignment in database.TaskAssignments
                            join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
                            join pot in database.Pots on task.PotId equals pot.Id
                            where assignment.CompletedAt == null && task.ResidenceId == session.ResidenceId
                            orderby assignment.AcceptedAt descending
                            select new
                            {
                                assignment.AssignedToUserId,
                                View = new ActiveAssignmentView(
                                    assignment.Id, task.Id, task.PotId, pot.Name, task.Name, task.Description,
                                    ToKind(task.Kind), task.RecurrenceDays, assignment.AcceptedAt, ToDifficulty(task.Difficulty)),
                            })
            .ToListAsync(cancellationToken);

        var weekStart = StartOfWeekUtc(DateTimeOffset.UtcNow);
        var completedThisWeek = await (from assignment in database.TaskAssignments
                                       join task in database.HouseholdTasks on assignment.HouseholdTaskId equals task.Id
                                       where assignment.CompletedAt != null
                                           && assignment.CompletedAt >= weekStart
                                           && task.ResidenceId == session.ResidenceId
                                       group assignment by assignment.AssignedToUserId into byUser
                                       select new { UserId = byUser.Key, Count = byUser.Count() })
            .ToListAsync(cancellationToken);

        var administratorIds = await (from userRole in database.UserRoles
                                      join role in database.Roles on userRole.RoleId equals role.Id
                                      where role.Name == HouseStuffRoles.Administrator
                                      select userRole.UserId).ToListAsync(cancellationToken);
        var administratorSet = administratorIds.ToHashSet();

        var result = members.Select(user => new MemberDashboardView(
            user.Id,
            user.Name,
            user.ProfileColor,
            administratorSet.Contains(user.Id),
            user.HasLogin,
            active.Where(item => item.AssignedToUserId == user.Id).Select(item => item.View).ToList(),
            completedThisWeek.FirstOrDefault(item => item.UserId == user.Id)?.Count ?? 0))
            .ToList();

        return DashboardResult.Success(new HouseholdDashboardView(result));
    }

    private static DateTimeOffset StartOfWeekUtc(DateTimeOffset now)
    {
        var date = now.UtcDateTime.Date;
        var daysSinceMonday = ((int)date.DayOfWeek + 6) % 7;
        return new DateTimeOffset(date.AddDays(-daysSinceMonday), TimeSpan.Zero);
    }

    private static string ToKind(HouseholdTaskKind kind) => char.ToLowerInvariant(kind.ToString()[0]) + kind.ToString()[1..];
    private static string ToDifficulty(HouseholdTaskDifficulty difficulty) => char.ToLowerInvariant(difficulty.ToString()[0]) + difficulty.ToString()[1..];
}
