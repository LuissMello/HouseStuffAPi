using Microsoft.AspNetCore.Identity;

namespace HouseStuff.Infrastructure.Identity;

public sealed class HouseStuffUser : IdentityUser
{
    public required string Name { get; set; }
    public Guid? ResidenceId { get; set; }
}
