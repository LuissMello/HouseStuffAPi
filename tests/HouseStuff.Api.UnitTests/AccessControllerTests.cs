using HouseStuff.Api.Controllers;
using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HouseStuff.Api.UnitTests;

public sealed class AccessControllerTests
{
    [Fact]
    public async Task LoginLetsBearerHandlerWriteTokensWhenCredentialsAreValid()
    {
        var service = new StubUserAccessService { SignInResult = AccessResult.Success(true) };
        var controller = new AuthController(service);

        var result = await controller.Login(new LoginRequest("admin@house.local", "Secret#123", false), CancellationToken.None);

        Assert.IsType<EmptyResult>(result);
    }

    [Fact]
    public async Task LoginReturnsUnauthorizedForInvalidCredentials()
    {
        var service = new StubUserAccessService
        {
            SignInResult = AccessResult.Failure<bool>("invalid_credentials", "E-mail ou senha inválidos."),
        };
        var controller = new AuthController(service);

        var result = await controller.Login(new LoginRequest("x@house.local", "wrong", false), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, problem.StatusCode);
    }

    [Fact]
    public async Task RefreshReturnsUnauthorizedForExpiredToken()
    {
        var service = new StubUserAccessService
        {
            RefreshResult = AccessResult.Failure<bool>("invalid_refresh_token", "A sessão expirou. Entre novamente."),
        };
        var controller = new AuthController(service);

        var result = await controller.Refresh(new RefreshTokenRequest("expired"), CancellationToken.None);

        var problem = Assert.IsType<ObjectResult>(result);
        Assert.Equal(401, problem.StatusCode);
    }

    [Fact]
    public async Task CreateReturnsCreatedUser()
    {
        var expected = new UserSummary("2", "luis@house.local", "Luis", false);
        var service = new StubUserAccessService { CreateResult = AccessResult.Success(expected) };
        var controller = new UsersController(service);

        var result = await controller.Create(
            new CreateUserRequest(expected.Email, expected.Name, "Secret#123", false),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(result);
        Assert.Same(expected, created.Value);
    }

    [Fact]
    public async Task ChangeRoleReturnsUpdatedUser()
    {
        var expected = new UserSummary("2", "luis@house.local", "Luis", true);
        var service = new StubUserAccessService { ChangeRoleResult = AccessResult.Success(expected) };
        var controller = new UsersController(service);

        var result = await controller.ChangeRole(expected.Id, new ChangeUserRoleRequest(true), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    private sealed class StubUserAccessService : IUserAccessService
    {
        public AccessResult<bool> SignInResult { get; init; } = AccessResult.Failure<bool>("missing", "missing");
        public AccessResult<bool> RefreshResult { get; init; } = AccessResult.Failure<bool>("missing", "missing");
        public AccessResult<UserSummary> CreateResult { get; init; } = AccessResult.Failure<UserSummary>("missing", "missing");
        public AccessResult<UserSummary> ChangeRoleResult { get; init; } = AccessResult.Failure<UserSummary>("missing", "missing");

        public Task<AccessResult<bool>> SignInWithTokenAsync(string email, string password, CancellationToken cancellationToken) =>
            Task.FromResult(SignInResult);

        public Task<AccessResult<bool>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken) =>
            Task.FromResult(RefreshResult);

        public Task SignOutAsync() => Task.CompletedTask;

        public Task<CurrentUser?> GetCurrentAsync(CancellationToken cancellationToken) => Task.FromResult<CurrentUser?>(null);

        public Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<UserSummary>>([]);

        public Task<AccessResult<UserSummary>> CreateAsync(CreateUserCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(CreateResult);

        public Task<AccessResult<UserSummary>> ChangeRoleAsync(ChangeUserRoleCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(ChangeRoleResult);
    }
}
