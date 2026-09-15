using HouseStuff.Application.Assignments;

namespace HouseStuff.Application.Routine;

public sealed record MemberDashboardView(
    string UserId,
    string Name,
    string ProfileColor,
    bool IsAdministrator,
    bool HasLogin,
    IReadOnlyList<ActiveAssignmentView> ActiveAssignments,
    int CompletedThisWeek);

public sealed record HouseholdDashboardView(IReadOnlyList<MemberDashboardView> Members);

public sealed record DashboardResult(bool Succeeded, HouseholdDashboardView? Value, string? Code, string? Message)
{
    public static DashboardResult Success(HouseholdDashboardView value) => new(true, value, null, null);
    public static DashboardResult Failure(string code, string message) => new(false, null, code, message);
}

public interface IHouseholdDashboardService
{
    Task<DashboardResult> GetAsync(CancellationToken cancellationToken);
}
