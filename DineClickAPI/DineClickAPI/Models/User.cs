using Microsoft.AspNetCore.Identity;

namespace DineClickAPI.Models;

public class User : IdentityUser
{
    [ProtectedPersonalData]
    public required string FirstName { get; set; }
    [ProtectedPersonalData]
    public required string LastName { get; set; }
    public bool IsBanned { get; set; }
    public DateTimeOffset? TokenValidityThreshold { get; set; }
}

public enum UserRole
{
    RegisteredUser,
    RestaurantManager,
    Admin
}
