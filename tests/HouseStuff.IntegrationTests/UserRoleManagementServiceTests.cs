using System.Security.Claims;
using HouseStuff.Application.Identity;
using HouseStuff.Domain.Residences;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace HouseStuff.IntegrationTests;

public sealed class UserRoleManagementServiceTests
{
    private const string AdminConnection = "Host=localhost;Port=54329;Database=postgres;Username=housestuff;Password=housestuff_local";
    private const string TestConnection = "Host=localhost;Port=54329;Database=housestuff_role_management_integration_tests;Username=housestuff;Password=housestuff_local";

    [Fact]
    public async Task AdministratorChangesOnlyAnotherResidentRole()
    {
        await CreateTestDatabaseAsync();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<HouseStuffDbContext>(options => options.UseNpgsql(TestConnection));
        services.AddIdentityCore<HouseStuffUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<HouseStuffDbContext>();
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<HouseStuffDbContext>();
        await database.Database.EnsureDeletedAsync();
        await database.Database.EnsureCreatedAsync();

        var users = scope.ServiceProvider.GetRequiredService<UserManager<HouseStuffUser>>();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await roles.CreateAsync(new IdentityRole(HouseStuffRoles.Administrator));
        await roles.CreateAsync(new IdentityRole(HouseStuffRoles.Member));

        var residence = Residence.Create("Casa Um", "admin", DateTimeOffset.UtcNow).Residence!;
        var otherResidence = Residence.Create("Casa Dois", "outsider", DateTimeOffset.UtcNow).Residence!;
        database.Residences.AddRange(residence, otherResidence);
        await database.SaveChangesAsync();

        var admin = User("admin", "Administrador", residence.Id);
        var member = User("member", "Morador", residence.Id);
        var pending = User("pending", "Pendente", null);
        var outsider = User("outsider", "Outra casa", otherResidence.Id);
        foreach (var user in new[] { admin, member, pending, outsider })
        {
            Assert.True((await users.CreateAsync(user)).Succeeded);
        }

        await users.AddToRoleAsync(admin, HouseStuffRoles.Administrator);
        await users.AddToRoleAsync(member, HouseStuffRoles.Member);
        await users.AddToRoleAsync(pending, HouseStuffRoles.Member);
        await users.AddToRoleAsync(outsider, HouseStuffRoles.Member);

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    [new Claim(ClaimTypes.NameIdentifier, admin.Id)],
                    "Test")),
            },
        };
        var service = new UserAccessService(users, null!, accessor, database, null!, TimeProvider.System);
        var previousStamp = member.SecurityStamp;

        var promoted = await service.ChangeRoleAsync(new ChangeUserRoleCommand(member.Id, true), CancellationToken.None);

        Assert.True(promoted.Succeeded);
        Assert.True(promoted.Value!.IsAdministrator);
        Assert.True(await users.IsInRoleAsync(member, HouseStuffRoles.Administrator));
        Assert.False(await users.IsInRoleAsync(member, HouseStuffRoles.Member));
        Assert.NotEqual(previousStamp, member.SecurityStamp);

        var demoted = await service.ChangeRoleAsync(new ChangeUserRoleCommand(member.Id, false), CancellationToken.None);
        var ownRole = await service.ChangeRoleAsync(new ChangeUserRoleCommand(admin.Id, false), CancellationToken.None);
        var pendingRole = await service.ChangeRoleAsync(new ChangeUserRoleCommand(pending.Id, true), CancellationToken.None);
        var outsiderRole = await service.ChangeRoleAsync(new ChangeUserRoleCommand(outsider.Id, true), CancellationToken.None);

        Assert.True(demoted.Succeeded);
        Assert.False(demoted.Value!.IsAdministrator);
        Assert.True(await users.IsInRoleAsync(member, HouseStuffRoles.Member));
        Assert.False(await users.IsInRoleAsync(member, HouseStuffRoles.Administrator));
        Assert.Equal("own_role_change_not_allowed", ownRole.Code);
        Assert.Equal("user_role_change_not_allowed", pendingRole.Code);
        Assert.Equal("user_role_change_not_allowed", outsiderRole.Code);

        var colored = await service.UpdateProfileColorAsync("#51469b", CancellationToken.None);
        var customColor = await service.UpdateProfileColorAsync("#ffffff", CancellationToken.None);
        var invalidColor = await service.UpdateProfileColorAsync("branco", CancellationToken.None);
        Assert.Equal("#51469B", colored.Value!.ProfileColor);
        Assert.Equal("#FFFFFF", customColor.Value!.ProfileColor);
        Assert.Equal("#FFFFFF", admin.ProfileColor);
        Assert.Equal("profile_color_invalid", invalidColor.Code);

        await database.Database.EnsureDeletedAsync();
    }

    private static HouseStuffUser User(string id, string name, Guid? residenceId) => new()
    {
        Id = id,
        Name = name,
        Email = $"{id}@house.local",
        UserName = $"{id}@house.local",
        ResidenceId = residenceId,
    };

    private static async Task CreateTestDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(AdminConnection);
        await connection.OpenAsync();
        await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = 'housestuff_role_management_integration_tests'", connection);
        if (await exists.ExecuteScalarAsync() is null)
        {
            await using var create = new NpgsqlCommand("CREATE DATABASE housestuff_role_management_integration_tests", connection);
            await create.ExecuteNonQueryAsync();
        }
    }
}
