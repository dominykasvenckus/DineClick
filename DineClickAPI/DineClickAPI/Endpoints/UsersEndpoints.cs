using AutoMapper;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DineClickAPI.Endpoints;

public static class UsersEndpoints
{
    public static void AddUsersEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/users", [Authorize(Roles = nameof(UserRole.Admin))] async (UserManager<User> userManager, IMapper mapper) =>
        {
            var users = await userManager.Users.ToListAsync();
            var adminUserDtos = new List<AdminUserDto>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                var adminUserDto = mapper.Map<AdminUserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
                adminUserDtos.Add(adminUserDto);
            }
            return Results.Ok(adminUserDtos);
        }).WithName("GetUsers")
          .Produces<List<AdminUserDto>>(200)
          .Produces(401)
          .Produces(403)
          .WithOpenApi();

        app.MapGet("api/v1/users/{userId}", [Authorize] async (HttpContext httpContext, UserManager<User> userManager, IMapper mapper, string userId) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "The requested user was not found." });
            }
            var roles = await userManager.GetRolesAsync(user);
            if (!httpContext.User.IsInRole(UserRole.Admin.ToString()))
            {
                if (userId != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                {
                    return Results.Forbid();
                }
                var userDto = mapper.Map<UserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
                return Results.Ok(userDto);
            }
            var adminUserDto = mapper.Map<AdminUserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
            return Results.Ok(adminUserDto);
        }).WithName("GetUser")
          .Produces<UserDto>(200)
          .Produces<AdminUserDto>(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapPut("api/v1/users/{userId}", [Authorize] async (HttpContext httpContext, UserManager<User> userManager, IMapper mapper, IValidator<UpdateUserDto> validator, string userId, [FromBody] UpdateUserDto updateUserDto) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "The requested user was not found." });
            }
            if (httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) != userId)
            {
                return Results.Forbid();
            }
            var validationResult = await validator.ValidateAsync(updateUserDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var roles = await userManager.GetRolesAsync(user);
            mapper.Map<UpdateUserDto, User>(updateUserDto, user);
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return Results.UnprocessableEntity(result.Errors.Select(e => new { error = e.Description }));
            }
            if (!httpContext.User.IsInRole(UserRole.Admin.ToString()))
            {
                var userDto = mapper.Map<UserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
                return Results.Ok(userDto);
            }
            var adminUserDto = mapper.Map<AdminUserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
            return Results.Ok(adminUserDto);
        }).WithName("UpdateUser")
          .Produces<UserDto>(200)
          .Produces<AdminUserDto>(200)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();


        app.MapDelete("api/v1/users/{userId}", [Authorize(Roles = nameof(UserRole.Admin))] async (UserManager<User> userManager, string userId) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "The requested user was not found." });
            }
            await userManager.DeleteAsync(user);
            return Results.NoContent();
        }).WithName("DeleteUser")
          .Produces(204)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapPut("api/v1/users/{userId}/ban", [Authorize(Roles = nameof(UserRole.Admin))] async (HttpContext httpContext, UserManager<User> userManager, IMapper mapper, string userId) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "The requested user was not found." });
            }
            if (userId == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.UnprocessableEntity(new { error = "Cannot ban the currently authenticated user." });
            }
            user.IsBanned = true;
            await userManager.UpdateAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            var adminUserDto = mapper.Map<AdminUserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
            return Results.Ok(adminUserDto);
        }).WithName("BanUser")
          .Produces(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapPut("api/v1/users/{userId}/unban", [Authorize(Roles = nameof(UserRole.Admin))] async (HttpContext httpContext, UserManager<User> userManager, IMapper mapper, string userId) =>
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                return Results.NotFound(new { error = "The requested user was not found." });
            }
            if (userId == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.UnprocessableEntity(new { error = "Cannot unban the currently authenticated user." });
            }
            user.IsBanned = false;
            await userManager.UpdateAsync(user);
            var roles = await userManager.GetRolesAsync(user);
            var adminUserDto = mapper.Map<AdminUserDto>(user, opt => opt.Items["Role"] = Enum.Parse<UserRole>(roles[0]));
            return Results.Ok(adminUserDto);
        }).WithName("UnbanUser")
          .Produces(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();
    }
}
