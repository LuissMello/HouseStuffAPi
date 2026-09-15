using HouseStuff.Api.Controllers;
using HouseStuff.Application.Routine;

namespace HouseStuff.Api.UnitTests;

public sealed class RoutineControllerTests
{
    [Fact]
    public async Task GetReturnsRoutineOverview()
    {
        var expected = new RoutineOverviewView(DateTimeOffset.UtcNow, [], []);
        var controller = new RoutineController(new StubRoutineService(RoutineOverviewResult.Success(expected)), new StubDashboardService(DashboardResult.Failure("missing", "missing")));

        var result = await controller.Get(CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task GetReturnsProblemWhenResidenceIsMissing()
    {
        var controller = new RoutineController(new StubRoutineService(RoutineOverviewResult.Failure("residence_required", "Vínculo necessário.")), new StubDashboardService(DashboardResult.Failure("missing", "missing")));

        var result = await controller.Get(CancellationToken.None);

        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task GetDashboardReturnsHouseholdDashboard()
    {
        var expected = new HouseholdDashboardView([new MemberDashboardView("1", "Luis", "#2F6B50", true, true, [], 3)]);
        var controller = new RoutineController(new StubRoutineService(RoutineOverviewResult.Failure("missing", "missing")), new StubDashboardService(DashboardResult.Success(expected)));

        var result = await controller.GetDashboard(CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    private sealed class StubRoutineService(RoutineOverviewResult result) : IRoutineOverviewService
    {
        public Task<RoutineOverviewResult> GetAsync(CancellationToken cancellationToken) => Task.FromResult(result);
    }

    private sealed class StubDashboardService(DashboardResult result) : IHouseholdDashboardService
    {
        public Task<DashboardResult> GetAsync(CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
