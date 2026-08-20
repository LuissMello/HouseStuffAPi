using System.Security.Claims;
using HouseStuff.Application.Identity;
using HouseStuff.Application.Residences;
using HouseStuff.Domain.Residences;
using HouseStuff.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Residences;

internal sealed class ResidenceService(
    HouseStuffDbContext database,
    UserManager<HouseStuffUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IResidenceService
{
    public async Task<ResidenceResult<ResidenceView>> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user?.ResidenceId is null)
        {
            return ResidenceResult.Failure<ResidenceView>("residence_not_found", "Você ainda não está vinculado a uma casa.");
        }

        return ResidenceResult.Success(await BuildViewAsync(user.ResidenceId.Value, cancellationToken));
    }

    public async Task<ResidenceResult<ResidenceView>> CreateAsync(string name, CancellationToken cancellationToken)
    {
        var user = await GetCurrentUserAsync(cancellationToken);
        if (user is null)
        {
            return ResidenceResult.Failure<ResidenceView>("user_not_found", "Usuário autenticado não encontrado.");
        }

        if (user.ResidenceId is not null)
        {
            return ResidenceResult.Failure<ResidenceView>("user_already_has_residence", "Você já pertence a uma casa.");
        }

        var creation = Residence.Create(name, user.Id, DateTimeOffset.UtcNow);
        if (!creation.Succeeded)
        {
            return ResidenceResult.Failure<ResidenceView>(creation.Code!, creation.Message!);
        }

        database.Residences.Add(creation.Residence!);
        user.ResidenceId = creation.Residence!.Id;
        await database.SaveChangesAsync(cancellationToken);
        return ResidenceResult.Success(await BuildViewAsync(creation.Residence.Id, cancellationToken));
    }

    public async Task<ResidenceResult<ResidenceView>> AddMemberAsync(string userId, CancellationToken cancellationToken)
    {
        var administrator = await GetCurrentUserAsync(cancellationToken);
        if (administrator?.ResidenceId is null)
        {
            return ResidenceResult.Failure<ResidenceView>("administrator_without_residence", "Crie sua casa antes de associar moradores.");
        }

        var member = await database.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);
        if (member is null)
        {
            return ResidenceResult.Failure<ResidenceView>("user_not_found", "Usuário não encontrado.");
        }

        if (member.ResidenceId is not null)
        {
            return ResidenceResult.Failure<ResidenceView>("user_already_has_residence", "Este usuário já pertence a uma casa.");
        }

        member.ResidenceId = administrator.ResidenceId;
        await database.SaveChangesAsync(cancellationToken);
        return ResidenceResult.Success(await BuildViewAsync(administrator.ResidenceId.Value, cancellationToken));
    }

    private async Task<HouseStuffUser?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var id = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return id is null ? null : await database.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    private async Task<ResidenceView> BuildViewAsync(Guid residenceId, CancellationToken cancellationToken)
    {
        var residence = await database.Residences.SingleAsync(item => item.Id == residenceId, cancellationToken);
        var users = await database.Users.Where(user => user.ResidenceId == residenceId).OrderBy(user => user.Name).ToListAsync(cancellationToken);
        var members = new List<ResidenceMember>(users.Count);
        foreach (var user in users)
        {
            members.Add(new ResidenceMember(user.Id, user.Name, user.Email!, await userManager.IsInRoleAsync(user, HouseStuffRoles.Administrator)));
        }

        return new ResidenceView(residence.Id, residence.Name, members);
    }
}
