using HouseStuff.Api.Controllers;
using HouseStuff.Application.Pots;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.UnitTests;

public sealed class PotsControllerTests
{
    [Fact]
    public async Task MemberListRequestsOnlyActivePots()
    {
        var expected = new[] { new PotView(Guid.NewGuid(), "Mensal", null, 0, true) };
        var service = new StubPotService { ListResult = PotResult.Success<IReadOnlyList<PotView>>(expected) };

        var result = await new PotsController(service).List(CancellationToken.None);

        Assert.False(service.IncludeArchivedRequested);
        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task ResidentCanCreatePot()
    {
        var expected = new PotView(Guid.NewGuid(), "Semanal", "Toda semana", 1, true);
        var service = new StubPotService { CreateResult = PotResult.Success(expected) };

        var result = await new AdminPotsController(service).Create(new SavePotRequest("Semanal", "Toda semana"), CancellationToken.None);

        Assert.Equal(201, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public void ManagementRequiresAuthenticationWithoutAdministratorRole()
    {
        var authorization = Assert.Single(typeof(AdminPotsController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>());

        Assert.Null(authorization.Roles);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundForPotOutsideCurrentResidence()
    {
        var service = new StubPotService { UpdateResult = PotResult.Failure<PotView>("pot_not_found", "Pote não encontrado.") };

        var result = await new AdminPotsController(service).Update(Guid.NewGuid(), new SavePotRequest("Mensal", null), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
    }

    private sealed class StubPotService : IPotService
    {
        public bool IncludeArchivedRequested { get; private set; }
        public PotResult<IReadOnlyList<PotView>> ListResult { get; init; } = PotResult.Success<IReadOnlyList<PotView>>([]);
        public PotResult<PotView> CreateResult { get; init; } = PotResult.Failure<PotView>("missing", "missing");
        public PotResult<PotView> UpdateResult { get; init; } = PotResult.Failure<PotView>("missing", "missing");

        public Task<PotResult<IReadOnlyList<PotView>>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
        {
            IncludeArchivedRequested = includeArchived;
            return Task.FromResult(ListResult);
        }

        public Task<PotResult<PotView>> CreateAsync(SavePotCommand command, CancellationToken cancellationToken) => Task.FromResult(CreateResult);
        public Task<PotResult<PotView>> UpdateAsync(Guid id, SavePotCommand command, CancellationToken cancellationToken) => Task.FromResult(UpdateResult);
        public Task<PotResult<PotView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken) => Task.FromResult(UpdateResult);
        public Task<PotResult<IReadOnlyList<PotView>>> MoveAsync(Guid id, int offset, CancellationToken cancellationToken) => Task.FromResult(ListResult);
    }
}
