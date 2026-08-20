using HouseStuff.Api.Controllers;
using HouseStuff.Application.Residences;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.UnitTests;

public sealed class ResidencesControllerTests
{
    [Fact]
    public async Task CurrentReturnsOnlyServiceResolvedResidence()
    {
        var expected = new ResidenceView(Guid.NewGuid(), "Casa Um", []);
        var controller = new ResidencesController(new StubResidenceService { CurrentResult = ResidenceResult.Success(expected) });

        var result = await controller.Current(CancellationToken.None);

        var ok = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task CurrentReturnsNotFoundWhenUserHasNoResidence()
    {
        var controller = new ResidencesController(new StubResidenceService
        {
            CurrentResult = ResidenceResult.Failure<ResidenceView>("residence_not_found", "Sem casa."),
        });

        var result = await controller.Current(CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(404, problem.StatusCode);
    }

    [Fact]
    public async Task CreateRejectsSecondResidence()
    {
        var controller = new ResidencesController(new StubResidenceService
        {
            CreateResult = ResidenceResult.Failure<ResidenceView>("user_already_has_residence", "Você já pertence a uma casa."),
        });

        var result = await controller.Create(new CreateResidenceRequest("Casa Dois"), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, problem.StatusCode);
    }

    private sealed class StubResidenceService : IResidenceService
    {
        public ResidenceResult<ResidenceView> CurrentResult { get; init; } = ResidenceResult.Failure<ResidenceView>("missing", "missing");
        public ResidenceResult<ResidenceView> CreateResult { get; init; } = ResidenceResult.Failure<ResidenceView>("missing", "missing");

        public Task<ResidenceResult<ResidenceView>> GetCurrentAsync(CancellationToken cancellationToken) => Task.FromResult(CurrentResult);
        public Task<ResidenceResult<ResidenceView>> CreateAsync(string name, CancellationToken cancellationToken) => Task.FromResult(CreateResult);
        public Task<ResidenceResult<ResidenceView>> AddMemberAsync(string userId, CancellationToken cancellationToken) => Task.FromResult(CurrentResult);
    }
}
