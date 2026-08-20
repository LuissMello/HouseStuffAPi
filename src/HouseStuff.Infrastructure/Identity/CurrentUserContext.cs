using System.Security.Claims;
using HouseStuff.Application.Assignments;
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

        return await database.Users.Where(user => user.Id == userId && user.ResidenceId != null)
            .Select(user => new CurrentUserSession(user.Id, user.ResidenceId!.Value))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
