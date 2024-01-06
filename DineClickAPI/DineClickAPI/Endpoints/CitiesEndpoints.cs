using AutoMapper;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DineClickAPI.Endpoints;

public static class CitiesEndpoints
{
    public static void AddCitiesEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/cities", [Authorize] async (ApplicationDbContext db) =>
        {
            var cities = new List<City>();
            cities = await db.Cities.ToListAsync();
            return Results.Ok(cities);
        }).WithName("GetCities")
          .Produces<List<City>>(200)
          .Produces(401)
          .WithOpenApi();

        app.MapGet("api/v1/cities/{cityId:int}", [Authorize] async (HttpContext httpContext, ApplicationDbContext db, int cityId) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                var isManagerForRestaurantInCity = await db.Restaurants
                    .Include(r => r.City)
                    .Include(r => r.RestaurantManager)
                    .AnyAsync(r => r.RestaurantManager.Id == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) &&
                                   r.City.CityId == cityId);
                if (!isManagerForRestaurantInCity)
                {
                    return Results.Forbid();
                }
            }
            return Results.Ok(city);
        }).WithName("GetCity")
          .Produces<City>(200)
          .Produces(401)
          .Produces(404)
          .WithOpenApi();

        app.MapPost("api/v1/cities", [Authorize(Roles = nameof(UserRole.Admin))] async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateCityDto> validator, [FromBody] CrupdateCityDto crupdateCityDto) =>
        {
            var validationResult = await validator.ValidateAsync(crupdateCityDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var cityExists = await db.Cities
                .AnyAsync(c => c.Latitude == crupdateCityDto.Latitude &&
                               c.Longitude == crupdateCityDto.Longitude &&
                               c.Name == crupdateCityDto.Name);
            if (cityExists)
            {
                return Results.UnprocessableEntity(new { error = "A city with the same latitude, longitude and name already exists." });
            }
            var city = mapper.Map<City>(crupdateCityDto);
            db.Cities.Add(city);
            await db.SaveChangesAsync();
            return Results.CreatedAtRoute("GetCity", new { cityId = city.CityId }, city);
        }).WithName("CreateCity")
          .Produces<City>(201)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(422)
          .WithOpenApi();

        app.MapPut("api/v1/cities/{cityId:int}", [Authorize(Roles = nameof(UserRole.Admin))] async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateCityDto> validator, int cityId, [FromBody] CrupdateCityDto crupdateCityDto) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            var validationResult = await validator.ValidateAsync(crupdateCityDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var cityExists = await db.Cities
                .AnyAsync(c => c.Latitude == crupdateCityDto.Latitude &&
                               c.Longitude == crupdateCityDto.Longitude &&
                               c.Name == crupdateCityDto.Name);
            if (cityExists)
            {
                return Results.UnprocessableEntity(new { error = "A city with the same latitude, longitude and name already exists." });
            }
            mapper.Map<CrupdateCityDto, City>(crupdateCityDto, city);
            db.Cities.Update(city);
            await db.SaveChangesAsync();
            return Results.Ok(city);
        }).WithName("UpdateCity")
          .Produces<City>(200)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapDelete("api/v1/cities/{cityId:int}", [Authorize(Roles = nameof(UserRole.Admin))] async (ApplicationDbContext db, int cityId) =>
        {
            var city = await db.Cities.FirstOrDefaultAsync(c => c.CityId == cityId);
            if (city is null)
            {
                return Results.NotFound(new { error = "The requested city was not found." });
            }
            db.Cities.Remove(city);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteCity")
          .Produces(204)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();
    }
}
