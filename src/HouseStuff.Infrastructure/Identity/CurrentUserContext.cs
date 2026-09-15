using System.Security.Claims;
using HouseStuff.Application.Assignments;
using HouseStuff.Application.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Identity;

internal sealed class CurrentUserContext(HouseStuffDbContext database, IHttpContextAccessor httpContextAccessor) : ICurrentUserContext
{
    public async Task<CurrentUserSession?> GetAsync(CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return null;
        }

        var user = await database.Users.Where(user => user.Id == userId && user.ResidenceId != null)
            .Select(user => new { user.Id, ResidenceId = user.ResidenceId!.Value })
            .SingleOrDefaultAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        var isAdministrator = await (from userRole in database.UserRoles
                                      join role in database.Roles on userRole.RoleId equals role.Id
                                      where userRole.UserId == user.Id && role.Name == HouseStuffRoles.Administrator
                                      select userRole).AnyAsync(cancellationToken);

        return new CurrentUserSession(user.Id, user.ResidenceId, isAdministrator);
    }
}
