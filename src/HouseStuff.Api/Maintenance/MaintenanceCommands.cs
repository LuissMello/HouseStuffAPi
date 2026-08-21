using HouseStuff.Application.Identity;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Api.Maintenance;

/// <summary>
/// Comandos administrativos executados fora do pipeline HTTP, para operar a base de qualquer
/// ambiente usando a connection string já presente na máquina (`fly ssh console -C "..."`).
/// </summary>
internal static class MaintenanceCommands
{
    private const string Verb = "maintenance";

    public static bool IsRequested(string[] args) =>
        args.Length > 0 && string.Equals(args[0], Verb, StringComparison.OrdinalIgnoreCase);

    public static async Task<int> RunAsync(IServiceProvider services, string[] args, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<HouseStuffUser>>();
        var database = scope.ServiceProvider.GetRequiredService<HouseStuffDbContext>();

        var command = args.Length > 1 ? args[1].ToLowerInvariant() : string.Empty;
        return command switch
        {
            "list-users" => await ListUsersAsync(users, database, cancellationToken),
            "reset-password" => await ResetPasswordAsync(users, args),
            _ => Usage(),
        };
    }

    private static async Task<int> ListUsersAsync(
        UserManager<HouseStuffUser> users,
        HouseStuffDbContext database,
        CancellationToken cancellationToken)
    {
        var all = await users.Users.OrderBy(user => user.Email).ToListAsync(cancellationToken);
        if (all.Count == 0)
        {
            Console.WriteLine("Nenhum usuário cadastrado.");
            return 0;
        }

        var residences = await database.Residences.ToDictionaryAsync(item => item.Id, item => item.Name, cancellationToken);
        foreach (var user in all)
        {
            var roles = string.Join(",", await users.GetRolesAsync(user));
            var residence = user.ResidenceId is Guid id && residences.TryGetValue(id, out var name) ? name : "-";
            var lockout = user.LockoutEnd is null ? "-" : user.LockoutEnd.Value.ToString("u");
            Console.WriteLine($"{user.Email} | nome={user.Name} | perfis={roles} | residência={residence} | falhas={user.AccessFailedCount} | bloqueio={lockout}");
        }

        return 0;
    }

    private static async Task<int> ResetPasswordAsync(UserManager<HouseStuffUser> users, string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Uso: maintenance reset-password <email> <nova-senha>");
            return 1;
        }

        var email = args[2].Trim();
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            Console.Error.WriteLine($"Usuário '{email}' não encontrado.");
            return 1;
        }

        var token = await users.GeneratePasswordResetTokenAsync(user);
        var result = await users.ResetPasswordAsync(user, token, args[3]);
        if (!result.Succeeded)
        {
            Console.Error.WriteLine(string.Join(" ", result.Errors.Select(error => error.Description)));
            return 1;
        }

        // Uma sequência de tentativas falhas anterior pode ter bloqueado a conta.
        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);
        Console.WriteLine($"Senha redefinida para '{email}'.");
        return 0;
    }

    private static int Usage()
    {
        Console.Error.WriteLine($"Uso: {Verb} <list-users|reset-password>");
        return 1;
    }
}
