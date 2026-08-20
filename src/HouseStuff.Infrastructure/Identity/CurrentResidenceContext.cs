using System.Security.Claims;
using HouseStuff.Application.Pots;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace HouseStuff.Infrastructure.Identity;

internal sealed class CurrentResidenceContext(HouseStuffDbContext database, IHttpContextAccessor httpContextAccessor)
    : ICurrentResidenceContext
{
    public async Task<Guid?> GetResidenceIdAsync(CancellationToken cancellationToken)
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId is null
            ? null
            : await database.Users.Where(user => user.Id == userId).Select(user => user.ResidenceId).SingleOrDefaultAsync(cancellationToken);
    }
}
