using AutoMapper;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DineClickAPI.Endpoints;

public static class AuthEndpoints
{
    public static void AddAuthEndpoints(this WebApplication app)
    {
        app.MapPost("api/v1/accessToken", async (UserManager<User> userManager, JwtService jwtService, [FromBody] RefreshAccessTokenDto refreshAccessTokenDto) =>
        {
            if (!jwtService.TryParseRefreshToken(refreshAccessTokenDto.RefreshToken, out var claims))
            {
                return Results.UnprocessableEntity(new { error = "Invalid refresh token" });
            }
            var user = await userManager.FindByIdAsync(claims!.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            if (user is null || user.IsBanned || user.TokenValidityThreshold.HasValue && 
                user.TokenValidityThreshold >= DateTimeOffset.FromUnixTimeSeconds(long.Parse(claims!.FindFirstValue(JwtRegisteredClaimNames.Iat)!)))
            {
                return Results.UnprocessableEntity(new { error = "Invalid refresh token." });
            }
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtService.CreateAccessToken(user.Id, user.UserName!, roles[0]);
            var refreshToken = jwtService.CreateRefreshToken(user.Id);
            return Results.Ok(new SuccessfullyLoginUserDto(accessToken, refreshToken));
        }).WithName("RefreshAccessToken")
          .Produces<SuccessfullyLoginUserDto>(200)
          .Produces(400)
          .Produces(422)
          .WithOpenApi();

        app.MapPost("api/v1/register", async (UserManager<User> userManager, IMapper mapper, IValidator<RegisterUserDto> validator, [FromBody] RegisterUserDto registerUserDto) =>
        {
            var validationResult = await validator.ValidateAsync(registerUserDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var newUser = mapper.Map<User>(registerUserDto);
            var createUserResult = await userManager.CreateAsync(newUser, registerUserDto.Password);
            if (!createUserResult.Succeeded)
            {
                return Results.UnprocessableEntity(createUserResult.Errors.Select(e => new { error = e.Description }));
            }
            await userManager.AddToRoleAsync(newUser, registerUserDto.Role.ToString());
            var newUserDto = mapper.Map<UserDto>(newUser, opt => opt.Items["Role"] = registerUserDto.Role);
            return Results.CreatedAtRoute("GetUser", new { userId = newUser.Id }, newUserDto);
        }).WithName("Register")
          .Produces<UserDto>(201)
          .Produces(400)
          .Produces(422)
          .WithOpenApi();

        app.MapPost("api/v1/login", async (UserManager<User> userManager, JwtService jwtService, [FromBody] LoginUserDto loginUserDto) =>
        {
            var user = await userManager.FindByNameAsync(loginUserDto.Username);
            if (user is null)
            {
                return Results.UnprocessableEntity(new { error = "Invalid username or password." });
            }
            var isPasswordValid = await userManager.CheckPasswordAsync(user, loginUserDto.Password);
            if (!isPasswordValid)
            {
                return Results.UnprocessableEntity(new { error = "Invalid username or password." });
            }
            DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
            if (user.IsBanned || user.TokenValidityThreshold.HasValue && user.TokenValidityThreshold >= DateTimeOffset.UtcNow)
            {
                return Results.Forbid();
            }
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = jwtService.CreateAccessToken(user.Id, user.UserName!, roles[0]);
            var refreshToken = jwtService.CreateRefreshToken(user.Id);
            return Results.Ok(new SuccessfullyLoginUserDto(accessToken, refreshToken));
        }).WithName("Login")
          .Produces<SuccessfullyLoginUserDto>(200)
          .Produces(400)
          .Produces(403)
          .Produces(422)
          .WithOpenApi();

        app.MapPost("api/v1/logout", [Authorize] async (HttpContext httpContext, UserManager<User> userManager) =>
        {
            var user = await userManager.FindByIdAsync(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
            if (user is not null)
            {
                DateTimeOffset currentUtc = DateTimeOffset.UtcNow;
                if (!user.TokenValidityThreshold.HasValue || user.TokenValidityThreshold < currentUtc)
                {
                    user.TokenValidityThreshold = currentUtc;
                    await userManager.UpdateAsync(user);
                }
            }
            return Results.NoContent();
        }).WithName("Logout")
          .Produces(204)
          .Produces(401)
          .WithOpenApi();
    }
}
