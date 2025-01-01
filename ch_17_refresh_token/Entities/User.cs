using Microsoft.AspNetCore.Identity;

namespace Entities;

public class User : IdentityUser
{
    public String? FirstName { get; set; }
    public String? LastName { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }
}