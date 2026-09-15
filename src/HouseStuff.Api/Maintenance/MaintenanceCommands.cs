using HouseStuff.Application.Identity;
using HouseStuff.Domain.Tasks;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebPush;

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
            "list-tasks" => await ListTasksAsync(users, database, args, cancellationToken),
            "restrict-all-tasks" => await RestrictAllTasksAsync(users, database, args, cancellationToken),
            "generate-vapid-keys" => GenerateVapidKeys(),
            _ => Usage(),
        };
    }

    private static int GenerateVapidKeys()
    {
        var keys = VapidHelper.GenerateVapidKeys();
        Console.WriteLine($"Vapid__PublicKey={keys.PublicKey}");
        Console.WriteLine($"Vapid__PrivateKey={keys.PrivateKey}");
        return 0;
    }

    private static async Task<int> ListTasksAsync(UserManager<HouseStuffUser> users, HouseStuffDbContext database, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 3 || await ResolveResidenceIdAsync(users, args[2]) is not { } residenceId)
        {
            Console.Error.WriteLine("Uso: maintenance list-tasks <email-de-referencia-da-casa>");
            return 1;
        }

        var tasks = await database.HouseholdTasks
            .Include(task => task.EligibleUsers)
            .Where(task => task.ResidenceId == residenceId)
            .OrderBy(task => task.Name)
            .ToListAsync(cancellationToken);
        if (tasks.Count == 0)
        {
            Console.WriteLine("Nenhuma tarefa cadastrada nesta casa.");
            return 0;
        }

        var names = await database.Users.Where(user => user.ResidenceId == residenceId).ToDictionaryAsync(user => user.Id, user => user.Name, cancellationToken);
        foreach (var task in tasks)
        {
            var eligibility = task.IsAvailableToAllResidents
                ? "todos"
                : string.Join(", ", task.EligibleUsers.Select(user => names.GetValueOrDefault(user.UserId, user.UserId)));
            Console.WriteLine($"{task.Name} | ativa={task.IsActive} | elegível={eligibility}");
        }

        return 0;
    }

    private static async Task<int> RestrictAllTasksAsync(UserManager<HouseStuffUser> users, HouseStuffDbContext database, string[] args, CancellationToken cancellationToken)
    {
        if (args.Length < 4 || await ResolveResidenceIdAsync(users, args[2]) is not { } residenceId)
        {
            Console.Error.WriteLine("Uso: maintenance restrict-all-tasks <email-de-referencia-da-casa> <email1,email2,...>");
            return 1;
        }

        var memberEmails = args[3].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (memberEmails.Length == 0)
        {
            Console.Error.WriteLine("Informe ao menos um e-mail.");
            return 1;
        }

        var memberIds = new List<string>();
        foreach (var email in memberEmails)
        {
            var member = await users.FindByEmailAsync(email);
            if (member is null || member.ResidenceId != residenceId)
            {
                Console.Error.WriteLine($"Usuário '{email}' não encontrado nesta casa.");
                return 1;
            }

            memberIds.Add(member.Id);
        }

        var tasks = await database.HouseholdTasks
            .Include(task => task.EligibleUsers)
            .Where(task => task.ResidenceId == residenceId && task.IsAvailableToAllResidents)
            .ToListAsync(cancellationToken);
        if (tasks.Count == 0)
        {
            Console.WriteLine("Nenhuma tarefa \"Todos da casa\" encontrada.");
            return 0;
        }

        foreach (var task in tasks)
        {
            var result = task.Update(task.PotId, task.Name, task.Description, task.Kind, task.RecurrenceDays, DateTimeOffset.UtcNow, task.Difficulty, isAvailableToAllResidents: false, eligibleUserIds: memberIds);
            if (!result.Succeeded)
            {
                Console.Error.WriteLine($"Falha ao atualizar '{task.Name}': {result.Message}");
                return 1;
            }
        }

        await database.SaveChangesAsync(cancellationToken);
        Console.WriteLine($"{tasks.Count} tarefa(s) restrita(s) a {string.Join(", ", memberEmails)}: {string.Join(", ", tasks.Select(task => task.Name))}");
        return 0;
    }

    private static async Task<Guid?> ResolveResidenceIdAsync(UserManager<HouseStuffUser> users, string email)
    {
        var user = await users.FindByEmailAsync(email.Trim());
        return user?.ResidenceId;
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
        Console.Error.WriteLine($"Uso: {Verb} <list-users|reset-password|list-tasks|restrict-all-tasks|generate-vapid-keys>");
        return 1;
    }
}
