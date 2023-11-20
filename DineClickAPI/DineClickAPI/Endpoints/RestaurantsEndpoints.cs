using AutoMapper;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DineClickAPI.Endpoints;

public static class RestaurantsEndpoints
{
    public static void AddRestaurantsEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/cities/{cityId:int}/restaurants", [Authorize] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, int cityId) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var restaurants = new List<Restaurant>();
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                restaurants = await db.Restaurants
                    .Include(r => r.City)
                    .Include(r => r.RestaurantManager)
                    .Where(r => r.RestaurantManager.Id == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) && 
                                r.City.CityId == cityId)
                    .ToListAsync();
            }
            else
            {
                restaurants = await db.Restaurants
                    .Include(r => r.City)
                    .Include(r => r.RestaurantManager)
                    .Where(r => r.City.CityId == cityId)
                    .ToListAsync();
            }
            var restaurantDtos = mapper.Map<List<RestaurantDto>>(restaurants);
            return Results.Ok(restaurantDtos);
        }).WithName("GetRestaurantsByCity")
          .Produces<List<RestaurantDto>>(200)
          .Produces(401)
          .Produces(404)
          .WithOpenApi();

        app.MapGet("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}", [Authorize] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, int cityId, int restaurantId) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var restaurant = await db.Restaurants
                .Include(r => r.City)
                .Include(r => r.RestaurantManager)
                .FirstOrDefaultAsync(r => r.City.CityId == cityId && r.RestaurantId == restaurantId);
            if (restaurant is null)
            {
                return Results.NotFound(new { error = "The requested restaurant was not found." });
            }
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                {
                    return Results.Forbid();
                }
            }
            var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
            return Results.Ok(restaurantDto);
        }).WithName("GetRestaurantByCity")
          .Produces<RestaurantDto>(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapPost("api/v1/cities/{cityId:int}/restaurants", [Authorize(Roles = nameof(UserRole.RestaurantManager))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, IValidator<CrupdateRestaurantDto> validator, int cityId, [FromBody] CrupdateRestaurantDto crupdateRestaurantDto) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var validationResult = await validator.ValidateAsync(crupdateRestaurantDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var restaurantExists = await db.Restaurants
                .Include(r => r.City)
                .AnyAsync(r => r.Name == crupdateRestaurantDto.Name &&
                               r.StreetAddress == crupdateRestaurantDto.StreetAddress &&
                               r.City.CityId == cityId);
            if (restaurantExists)
            {
                return Results.UnprocessableEntity(new { error = "A restaurant with the same name, street address and city already exists." });
            }
            var restaurantManager = await db.Users.FindAsync(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var restaurant = mapper.Map<Restaurant>(crupdateRestaurantDto, opt =>
            {
                opt.Items["City"] = city;
                opt.Items["RestaurantManager"] = restaurantManager;
            });
            db.Restaurants.Add(restaurant);
            await db.SaveChangesAsync();
            var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
            return Results.CreatedAtRoute("GetRestaurantByCity", new { cityId, restaurantId = restaurant.RestaurantId }, restaurantDto);
        }).WithName("CreateRestaurantByCity")
          .Produces<RestaurantDto>(201)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapPut("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}", [Authorize(Roles = nameof(UserRole.RestaurantManager))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, IValidator<CrupdateRestaurantDto> validator, int cityId, int restaurantId, [FromBody] CrupdateRestaurantDto crupdateRestaurantDto) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var restaurant = await db.Restaurants
                .Include(r => r.City)
                .Include(r => r.RestaurantManager)
                .FirstOrDefaultAsync(r => r.City.CityId == cityId && r.RestaurantId == restaurantId);
            if (restaurant is null)
            {
                return Results.NotFound(new { error = "The requested restaurant was not found." });
            }
            if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.Forbid();
            }
            var validationResult = await validator.ValidateAsync(crupdateRestaurantDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var restaurantExists = await db.Restaurants
                .Include(r => r.City)
                .AnyAsync(r => r.Name == crupdateRestaurantDto.Name &&
                               r.StreetAddress == crupdateRestaurantDto.StreetAddress &&
                               r.City.CityId == cityId);
            if (restaurantExists)
            {
                return Results.UnprocessableEntity(new { error = "A restaurant with the same name, street address and city already exists." });
            }
            mapper.Map<CrupdateRestaurantDto, Restaurant>(crupdateRestaurantDto, restaurant);
            db.Restaurants.Update(restaurant);
            await db.SaveChangesAsync();
            var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
            return Results.Ok(restaurantDto);
        }).WithName("UpdateRestaurantByCity")
          .Produces<RestaurantDto>(200)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapDelete("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}", [Authorize(Roles = $"{nameof(UserRole.RestaurantManager)},{nameof(UserRole.Admin)}")] async (HttpContext httpContext, ApplicationDbContext db, int cityId, int restaurantId) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var restaurant = await db.Restaurants
                .Include(r => r.City)
                .Include(r => r.RestaurantManager)
                .FirstOrDefaultAsync(r => r.City.CityId == cityId && r.RestaurantId == restaurantId);
            if (restaurant is null)
            {
                return Results.NotFound(new { error = "The requested restaurant was not found." });
            }
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                {
                    return Results.Forbid();
                }
            }
            db.Restaurants.Remove(restaurant);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteRestaurantByCity")
          .Produces(204)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();
    }
}
