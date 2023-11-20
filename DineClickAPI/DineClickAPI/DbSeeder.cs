using DineClickAPI.Models;
using Microsoft.AspNetCore.Identity;

namespace DineClickAPI;

public class DbSeeder
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbSeeder(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        await AddRoles();
        await AddAdminUser();
    }

    private async Task AddRoles()
    {
        var roles = (UserRole[])Enum.GetValues(typeof(UserRole));
        foreach (var role in roles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(role.ToString());
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole(role.ToString()));
            }
        }
    }

    private async Task AddAdminUser()
    {
        var newAdminUser = new User
        {
            UserName = "admin",
            Email = "admin@admin.com",
            FirstName = "John",
            LastName = "Doe"
        };
        var createAdminUserResult = await _userManager.CreateAsync(newAdminUser, "#Aa123456");
        if (createAdminUserResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(newAdminUser, UserRole.Admin.ToString());
        }
    }
}
