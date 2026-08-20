using HouseStuff.Api.Controllers;
using HouseStuff.Application.Tasks;

namespace HouseStuff.Api.UnitTests;

public sealed class HouseholdTasksControllerTests
{
    [Fact]
    public async Task ListIncludesArchivedForAdministrator()
    {
        var expected = new[] { new HouseholdTaskView(Guid.NewGuid(), Guid.NewGuid(), "Mensal", "Limpar geladeira", null, "recurring", 30, true) };
        var service = new StubHouseholdTaskService { ListResult = HouseholdTaskResult.Success<IReadOnlyList<HouseholdTaskView>>(expected) };

        var result = await new HouseholdTasksController(service).List(null, CancellationToken.None);

        Assert.True(service.IncludeArchivedRequested);
        Assert.Equal(200, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task CreateReturnsCreatedTask()
    {
        var expected = new HouseholdTaskView(Guid.NewGuid(), Guid.NewGuid(), "Diário", "Lavar louça", null, "reusable", null, true);
        var service = new StubHouseholdTaskService { CreateResult = HouseholdTaskResult.Success(expected) };

        var result = await new HouseholdTasksController(service).Create(new SaveHouseholdTaskRequest(expected.PotId, expected.Name, null, "reusable", null), CancellationToken.None);

        Assert.Equal(201, result.StatusCode);
        Assert.Same(expected, result.Value);
    }

    [Fact]
    public async Task UpdateReturnsNotFoundOutsideCurrentResidence()
    {
        var service = new StubHouseholdTaskService { UpdateResult = HouseholdTaskResult.Failure<HouseholdTaskView>("task_not_found", "Não encontrada.") };

        var result = await new HouseholdTasksController(service).Update(Guid.NewGuid(), new SaveHouseholdTaskRequest(Guid.NewGuid(), "Tarefa", null, "oneTime", null), CancellationToken.None);

        Assert.Equal(404, result.StatusCode);
    }

    private sealed class StubHouseholdTaskService : IHouseholdTaskService
    {
        public bool IncludeArchivedRequested { get; private set; }
        public HouseholdTaskResult<IReadOnlyList<HouseholdTaskView>> ListResult { get; init; } = HouseholdTaskResult.Success<IReadOnlyList<HouseholdTaskView>>([]);
        public HouseholdTaskResult<HouseholdTaskView> CreateResult { get; init; } = HouseholdTaskResult.Failure<HouseholdTaskView>("missing", "missing");
        public HouseholdTaskResult<HouseholdTaskView> UpdateResult { get; init; } = HouseholdTaskResult.Failure<HouseholdTaskView>("missing", "missing");

        public Task<HouseholdTaskResult<IReadOnlyList<HouseholdTaskView>>> ListAsync(Guid? potId, bool includeArchived, CancellationToken cancellationToken)
        {
            IncludeArchivedRequested = includeArchived;
            return Task.FromResult(ListResult);
        }

        public Task<HouseholdTaskResult<HouseholdTaskView>> CreateAsync(SaveHouseholdTaskCommand command, CancellationToken cancellationToken) => Task.FromResult(CreateResult);
        public Task<HouseholdTaskResult<HouseholdTaskView>> UpdateAsync(Guid id, SaveHouseholdTaskCommand command, CancellationToken cancellationToken) => Task.FromResult(UpdateResult);
        public Task<HouseholdTaskResult<HouseholdTaskView>> SetActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken) => Task.FromResult(UpdateResult);
    }
}
