using System.Security.Claims;
using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Identity;

internal sealed class UserAccessService(
    UserManager<HouseStuffUser> userManager,
    SignInManager<HouseStuffUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    HouseStuffDbContext database) : IUserAccessService
{
    public async Task<AccessResult<CurrentUser>> SignInAsync(
        string email,
        string password,
        bool rememberMe,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return AccessResult.Failure<CurrentUser>("invalid_credentials", "E-mail ou senha inválidos.");
        }

        var result = await signInManager.PasswordSignInAsync(user, password, rememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return AccessResult.Failure<CurrentUser>("invalid_credentials", "E-mail ou senha inválidos.");
        }

        return AccessResult.Success(await ToCurrentUserAsync(user));
    }

    public Task SignOutAsync() => signInManager.SignOutAsync();

    public async Task<CurrentUser?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var principal = httpContextAccessor.HttpContext?.User;
        var id = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (id is null)
        {
            return null;
        }

        var user = await userManager.Users.SingleOrDefaultAsync(item => item.Id == id, cancellationToken);
        return user is null ? null : await ToCurrentUserAsync(user);
    }

    public async Task<IReadOnlyList<UserSummary>> ListAsync(CancellationToken cancellationToken)
    {
        var currentId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var current = currentId is null
            ? null
            : await database.Users.SingleOrDefaultAsync(user => user.Id == currentId, cancellationToken);
        if (current is null)
        {
            return [];
        }

        var users = await database.Users
            .Where(user => user.ResidenceId == current.ResidenceId || user.ResidenceId == null)
            .OrderBy(item => item.Name)
            .ToListAsync(cancellationToken);
        var result = new List<UserSummary>(users.Count);
        foreach (var user in users)
        {
            result.Add(await ToSummaryAsync(user));
        }

        return result;
    }

    public async Task<AccessResult<UserSummary>> CreateAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken)
    {
        var email = command.Email.Trim();
        var name = command.Name.Trim();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(name))
        {
            return AccessResult.Failure<UserSummary>("invalid_user", "Nome e e-mail são obrigatórios.");
        }

        var currentId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var current = currentId is null
            ? null
            : await database.Users.SingleOrDefaultAsync(item => item.Id == currentId, cancellationToken);
        var user = new HouseStuffUser { UserName = email, Email = email, Name = name, ResidenceId = current?.ResidenceId };
        var result = await userManager.CreateAsync(user, command.TemporaryPassword);
        if (!result.Succeeded)
        {
            var message = string.Join(" ", result.Errors.Select(error => error.Description));
            return AccessResult.Failure<UserSummary>("user_not_created", message);
        }

        var role = command.IsAdministrator ? HouseStuffRoles.Administrator : HouseStuffRoles.Member;
        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return AccessResult.Failure<UserSummary>("role_not_assigned", "Não foi possível atribuir o perfil do usuário.");
        }

        return AccessResult.Success(await ToSummaryAsync(user));
    }

    private async Task<CurrentUser> ToCurrentUserAsync(HouseStuffUser user)
    {
        var residenceName = user.ResidenceId is null
            ? null
            : await database.Residences.Where(item => item.Id == user.ResidenceId).Select(item => item.Name).SingleAsync();
        return new(user.Id, user.Email!, user.Name, await userManager.IsInRoleAsync(user, HouseStuffRoles.Administrator), user.ResidenceId, residenceName);
    }

    private async Task<UserSummary> ToSummaryAsync(HouseStuffUser user)
    {
        var residenceName = user.ResidenceId is null
            ? null
            : await database.Residences.Where(item => item.Id == user.ResidenceId).Select(item => item.Name).SingleAsync();
        return new(user.Id, user.Email!, user.Name, await userManager.IsInRoleAsync(user, HouseStuffRoles.Administrator), user.ResidenceId, residenceName);
    }
}
