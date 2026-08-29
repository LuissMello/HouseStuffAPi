using System.Security.Claims;
using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HouseStuff.Infrastructure.Identity;

internal sealed class UserAccessService(
    UserManager<HouseStuffUser> userManager,
    SignInManager<HouseStuffUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    HouseStuffDbContext database,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptions,
    TimeProvider timeProvider) : IUserAccessService
{
    public async Task<AccessResult<bool>> SignInWithTokenAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim();
        var user = await userManager.FindByEmailAsync(normalizedEmail);
        if (user is null)
        {
            return AccessResult.Failure<bool>("invalid_credentials", "E-mail ou senha inválidos.");
        }

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return AccessResult.Failure<bool>("invalid_credentials", "E-mail ou senha inválidos.");
        }

        return AccessResult.Success(true);
    }

    public async Task<AccessResult<bool>> RefreshTokenAsync(
        string refreshToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tokenProtector = bearerTokenOptions.Get(IdentityConstants.BearerScheme).RefreshTokenProtector;
        var refreshTicket = tokenProtector.Unprotect(refreshToken);
        if (refreshTicket?.Properties.ExpiresUtc is not { } expiresUtc ||
            timeProvider.GetUtcNow() >= expiresUtc ||
            await signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            return AccessResult.Failure<bool>("invalid_refresh_token", "A sessão expirou. Entre novamente.");
        }

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var context = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Não foi possível acessar a requisição atual.");
        await context.SignInAsync(IdentityConstants.BearerScheme, principal);
        return AccessResult.Success(true);
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

    public async Task<AccessResult<UserSummary>> ChangeRoleAsync(
        ChangeUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        var currentId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (currentId is null)
        {
            return AccessResult.Failure<UserSummary>("current_user_not_found", "Não foi possível identificar o administrador atual.");
        }

        if (string.Equals(currentId, command.UserId, StringComparison.Ordinal))
        {
            return AccessResult.Failure<UserSummary>("own_role_change_not_allowed", "Altere somente o perfil de outra pessoa da casa.");
        }

        var current = await database.Users.SingleOrDefaultAsync(user => user.Id == currentId, cancellationToken);
        var target = await database.Users.SingleOrDefaultAsync(user => user.Id == command.UserId, cancellationToken);
        if (current?.ResidenceId is null || target?.ResidenceId is null || current.ResidenceId != target.ResidenceId)
        {
            return AccessResult.Failure<UserSummary>("user_role_change_not_allowed", "O perfil só pode ser alterado para outra pessoa da sua casa.");
        }

        var desiredRole = command.IsAdministrator ? HouseStuffRoles.Administrator : HouseStuffRoles.Member;
        var currentRoles = await userManager.GetRolesAsync(target);
        if (currentRoles.Count == 1 && string.Equals(currentRoles[0], desiredRole, StringComparison.Ordinal))
        {
            return AccessResult.Success(await ToSummaryAsync(target));
        }

        await using var transaction = await database.Database.BeginTransactionAsync(cancellationToken);
        var houseStuffRoles = currentRoles
            .Where(role => role is HouseStuffRoles.Administrator or HouseStuffRoles.Member)
            .ToArray();
        if (houseStuffRoles.Length > 0)
        {
            var removeResult = await userManager.RemoveFromRolesAsync(target, houseStuffRoles);
            if (!removeResult.Succeeded)
            {
                return AccessResult.Failure<UserSummary>("user_role_not_changed", "Não foi possível remover o perfil anterior.");
            }
        }

        var addResult = await userManager.AddToRoleAsync(target, desiredRole);
        if (!addResult.Succeeded)
        {
            return AccessResult.Failure<UserSummary>("user_role_not_changed", "Não foi possível atribuir o novo perfil.");
        }

        var stampResult = await userManager.UpdateSecurityStampAsync(target);
        if (!stampResult.Succeeded)
        {
            return AccessResult.Failure<UserSummary>("user_role_not_changed", "Não foi possível invalidar as sessões anteriores.");
        }

        await transaction.CommitAsync(cancellationToken);
        return AccessResult.Success(await ToSummaryAsync(target));
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
