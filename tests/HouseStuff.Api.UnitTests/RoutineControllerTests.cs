using HouseStuff.Api.Controllers;
using HouseStuff.Application.Routine;

namespace HouseStuff.Api.UnitTests;

public sealed class RoutineControllerTests
{
    [Fact]
    public async Task GetReturnsRoutineOverview()
    {
        var expected = new RoutineOverviewView(DateTimeOffset.UtcNow, [], []);
        var controller = new RoutineController(new StubRoutineService(RoutineOverviewResult.Success(expected)));

        var result = await controller.Get(CancellationToken.None);

        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task GetReturnsProblemWhenResidenceIsMissing()
    {
        var controller = new RoutineController(new StubRoutineService(RoutineOverviewResult.Failure("residence_required", "Vínculo necessário.")));

        var result = await controller.Get(CancellationToken.None);

        Assert.Equal(400, result.StatusCode);
    }

    private sealed class StubRoutineService(RoutineOverviewResult result) : IRoutineOverviewService
    {
        public Task<RoutineOverviewResult> GetAsync(CancellationToken cancellationToken) => Task.FromResult(result);
    }
}
