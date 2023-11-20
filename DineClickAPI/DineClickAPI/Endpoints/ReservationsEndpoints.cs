using AutoMapper;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DineClickAPI.Endpoints;

public static class ReservationsEndpoints
{
    public static void AddReservationsEndpoints(this WebApplication app)
    {
        app.MapGet("api/v1/reservations", [Authorize(Roles = nameof(UserRole.RegisteredUser))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper) =>
        {
            var reservations = await db.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.ReservingUser)
                .Where(r => r.ReservingUser.Id == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                .ToListAsync();
            var registeredUserReservationDtos = mapper.Map<List<RegisteredUserReservationDto>>(reservations);
            return Results.Ok(registeredUserReservationDtos);
        }).WithName("GetReservations")
          .Produces<List<RegisteredUserReservationDto>>(200)
          .Produces(401)
          .Produces(403)
          .WithOpenApi();

        app.MapGet("api/v1/reservations/{reservationId:int}", [Authorize(Roles = nameof(UserRole.RegisteredUser))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, int reservationId) =>
        {
            var reservation = await db.Reservations
                .Include(r => r.Restaurant)
                .Include(r => r.ReservingUser)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
            if (reservation is null)
            {
                return Results.NotFound(new { error = "The requested reservation was not found." });
            }
            if (reservation.ReservingUser.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.Forbid();
            }
            var registeredUserReservationDto = mapper.Map<RegisteredUserReservationDto>(reservation);
            return Results.Ok(registeredUserReservationDto);
        }).WithName("GetReservation")
          .Produces<RegisteredUserReservationDto>(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapGet("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}/reservations", [Authorize(Roles = $"{nameof(UserRole.RegisteredUser)},{nameof(UserRole.RestaurantManager)}")] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, int cityId, int restaurantId) =>
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
            var reservations = new List<Reservation>();
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                reservations = await db.Reservations
                    .Include(r => r.Restaurant)
                        .ThenInclude(r => r.City)
                    .Include(r => r.Restaurant)
                        .ThenInclude(r => r.RestaurantManager)
                    .Include(r => r.ReservingUser)
                    .Where(r => r.Restaurant.RestaurantManager.Id == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) && 
                                r.Restaurant.City.CityId == cityId && 
                                r.Restaurant.RestaurantId == restaurant.RestaurantId)
                    .ToListAsync();
            }
            else
            {
                reservations = await db.Reservations
                    .Include(r => r.Restaurant)
                        .ThenInclude(r => r.City)
                    .Include(r => r.Restaurant)
                        .ThenInclude(r => r.RestaurantManager)
                    .Include(r => r.ReservingUser)
                    .Where(r => r.ReservingUser.Id == httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub) && 
                                r.Restaurant.City.CityId == cityId && 
                                r.Restaurant.RestaurantId == restaurant.RestaurantId)
                    .ToListAsync();
            }
            var reservationDtos = mapper.Map<List<ReservationDto>>(reservations);
            return Results.Ok(reservationDtos);
        }).WithName("GetReservationsByCityAndRestaurant")
          .Produces<List<ReservationDto>>(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapGet("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", [Authorize(Roles = $"{nameof(UserRole.RegisteredUser)},{nameof(UserRole.RestaurantManager)}")] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, int cityId, int restaurantId, int reservationId) =>
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
            var reservation = await db.Reservations
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.City)
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.RestaurantManager)
                .Include(r => r.ReservingUser)
                .FirstOrDefaultAsync(r => r.Restaurant.City.CityId == cityId &&
                                          r.Restaurant.RestaurantId == restaurant.RestaurantId && 
                                          r.ReservationId == reservationId);
            if (reservation is null)
            {
                return Results.NotFound(new { error = "The requested reservation was not found." });
            }
            if (httpContext.User.IsInRole(UserRole.RestaurantManager.ToString()))
            {
                if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                {
                    return Results.Forbid();
                }
            }
            else
            {
                if (reservation.ReservingUser.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
                {
                    return Results.Forbid();
                }
            }
            var reservationDto = mapper.Map<ReservationDto>(reservation);
            return Results.Ok(reservationDto);
        }).WithName("GetReservationByCityAndRestaurant")
          .Produces<ReservationDto>(200)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();

        app.MapPost("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}/reservations", [Authorize(Roles = nameof(UserRole.RegisteredUser))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, IValidator<CreateReservationDto> validator, int cityId, int restaurantId, [FromBody] CreateReservationDto createReservationDto) =>
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
            var validationResult = await validator.ValidateAsync(createReservationDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            var reservingUser = await db.Users.FindAsync(httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub));
            var reservation = mapper.Map<Reservation>(createReservationDto, opt =>
            {
                opt.Items["Restaurant"] = restaurant;
                opt.Items["ReservingUser"] = reservingUser;
            });
            db.Reservations.Add(reservation);
            await db.SaveChangesAsync();
            var reservationDto = mapper.Map<ReservationDto>(reservation);
            return Results.CreatedAtRoute("GetReservationByCityAndRestaurant", new { cityId, restaurantId = restaurant.RestaurantId, reservationId = reservation.ReservationId }, reservationDto);
        }).WithName("CreateReservationByCityAndRestaurant")
          .Produces<ReservationDto>(201)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapPut("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", [Authorize(Roles = nameof(UserRole.RestaurantManager))] async (HttpContext httpContext, ApplicationDbContext db, IMapper mapper, IValidator<UpdateReservationDto> validator, int cityId, int restaurantId, int reservationId, [FromBody] UpdateReservationDto updateReservationDto) =>
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
            var reservation = await db.Reservations
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.City)
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.RestaurantManager)
                .Include(r => r.ReservingUser)
                .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
            if (reservation is null)
            {
                return Results.NotFound(new { error = "The requested reservation was not found." });
            }
            if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.Forbid();
            }
            var validationResult = await validator.ValidateAsync(updateReservationDto);
            if (!validationResult.IsValid)
            {
                return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
            }
            mapper.Map<UpdateReservationDto, Reservation>(updateReservationDto, reservation);
            db.Reservations.Update(reservation);
            await db.SaveChangesAsync();
            var reservationDto = mapper.Map<ReservationDto>(reservation);
            return Results.Ok(reservationDto);
        }).WithName("UpdateReservationByCityAndRestaurant")
          .Produces<ReservationDto>(200)
          .Produces(400)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .Produces(422)
          .WithOpenApi();

        app.MapDelete("api/v1/cities/{cityId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", [Authorize(Roles = nameof(UserRole.RestaurantManager))] async (HttpContext httpContext, ApplicationDbContext db, int cityId, int restaurantId, int reservationId) =>
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
            var reservation = await db.Reservations
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.City)
                .Include(r => r.Restaurant)
                    .ThenInclude(r => r.RestaurantManager)
                .Include(r => r.ReservingUser)
                .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
            if (reservation is null)
            {
                return Results.NotFound(new { error = "The requested reservation was not found." });
            }
            if (restaurant.RestaurantManager.Id != httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub))
            {
                return Results.Forbid();
            }
            db.Reservations.Remove(reservation);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteReservationByCityAndRestaurant")
          .Produces(204)
          .Produces(401)
          .Produces(403)
          .Produces(404)
          .WithOpenApi();
    }
}
