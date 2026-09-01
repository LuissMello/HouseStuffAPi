using Microsoft.AspNetCore.Identity;
using HouseStuff.Application.Identity;

namespace HouseStuff.Infrastructure.Identity;

public sealed class HouseStuffUser : IdentityUser
{
    public required string Name { get; set; }
    public Guid? ResidenceId { get; set; }
    public string ProfileColor { get; set; } = ProfileColors.Default;
}
