using AutoMapper;
using DineClickAPI;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(option => {
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSqlConnection"), x => x.UseDateOnlyTimeOnly());
});
builder.Services.AddAutoMapper(typeof(MapperConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoints for addresses.
app.MapGet("api/addresses", async (ApplicationDbContext db) =>
{
    var addresses = await db.Addresses.ToListAsync();
    return Results.Ok(addresses);
}).WithName("GetAddresses")
  .Produces<IEnumerable<Address>>(200)
  .WithOpenApi();

app.MapGet("api/addresses/{addressId:int}", async (ApplicationDbContext db, int addressId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    return Results.Ok(address);
}).WithName("GetAddress")
  .Produces<Address>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses", async (ApplicationDbContext db, IMapper mapper, IValidator<AddressDto> validator, [FromBody] AddressDto addressDto) =>
{
    var validationResult = await validator.ValidateAsync(addressDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    var address = mapper.Map<Address>(addressDto);
    db.Addresses.Add(address);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetAddress", new { addressId = address.AddressId }, address);
}).WithName("CreateAddress")
  .Produces<Address>(201)
  .Produces(400)
  .Produces(422)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<AddressDto> validator, int addressId, [FromBody] AddressDto addressDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var validationResult = await validator.ValidateAsync(addressDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    mapper.Map<AddressDto, Address>(addressDto, address);
    db.Addresses.Update(address);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("UpdateAddress")
  .Produces(204)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}", async (ApplicationDbContext db, int addressId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    db.Addresses.Remove(address);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteAddress")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

// Endpoints for restaurants.
app.MapGet("api/addresses/{addressId:int}/restaurants", async (ApplicationDbContext db, int addressId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurants = await db.Restaurants
        .Include(r => r.Address)
        .Where(r => r.Address.AddressId == addressId)
        .ToListAsync();
    return Results.Ok(restaurants);
}).WithName("GetRestaurantsByAddress")
  .Produces<IEnumerable<Restaurant>>(200)
  .Produces(404)
  .WithOpenApi();

app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    return Results.Ok(restaurant);
}).WithName("GetRestaurantByAddress")
  .Produces<Restaurant>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses/{addressId:int}/restaurants", async (ApplicationDbContext db, IMapper mapper, IValidator<RestaurantDto> validator, int addressId, [FromBody] RestaurantDto restaurantDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var validationResult = await validator.ValidateAsync(restaurantDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    var restaurant = mapper.Map<Restaurant>(restaurantDto, opt => opt.Items["Address"] = address);
    db.Restaurants.Add(restaurant);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetRestaurantByAddress", new { addressId, restaurantId = restaurant.RestaurantId }, restaurant);
}).WithName("CreateRestaurantByAddress")
  .Produces<Restaurant>(201)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<RestaurantDto> validator, int addressId, int restaurantId, [FromBody] RestaurantDto restaurantDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var validationResult = await validator.ValidateAsync(restaurantDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    mapper.Map<RestaurantDto, Restaurant>(restaurantDto, restaurant);
    db.Restaurants.Update(restaurant);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("UpdateRestaurantByAddress")
  .Produces(204)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    db.Restaurants.Remove(restaurant);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteRestaurantByAddress")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

// Endpoints for reservations.
app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations", async (ApplicationDbContext db, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var reservations = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .Where(r => r.Restaurant.RestaurantId == restaurant.RestaurantId)
        .ToListAsync();
    return Results.Ok(reservations);
}).WithName("GetReservationsByAddressAndRestaurant")
  .Produces<IEnumerable<Reservation>>(200)
  .Produces(404)
  .WithOpenApi();

app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, int addressId, int restaurantId, int reservationId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation == null)
    {
        return Results.NotFound("Error: The requested reservation was not found.");
    }
    return Results.Ok(reservation);
}).WithName("GetReservationByAddressAndRestaurant")
  .Produces<Reservation>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations", async (ApplicationDbContext db, IMapper mapper, IValidator<CreateReservationDto> validator, int addressId, int restaurantId, [FromBody] CreateReservationDto createReservationDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var validationResult = await validator.ValidateAsync(createReservationDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    var reservation = mapper.Map<Reservation>(createReservationDto, opt => opt.Items["Restaurant"] = restaurant);
    db.Reservations.Add(reservation);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetReservationByAddressAndRestaurant", new { addressId, restaurantId = restaurant.RestaurantId, reservationId = reservation.ReservationId }, reservation);
}).WithName("CreateReservationByAddressAndRestaurant")
  .Produces<Reservation>(201)
  .Produces(400)
  .Produces(404)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<UpdateReservationDto> validator, int addressId, int restaurantId, int reservationId, [FromBody] UpdateReservationDto updateReservationDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation == null)
    {
        return Results.NotFound("Error: The requested reservation was not found.");
    }
    var validationResult = await validator.ValidateAsync(updateReservationDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => $"Error: {e.ErrorMessage}"));
    }
    mapper.Map<UpdateReservationDto, Reservation>(updateReservationDto, reservation);
    db.Reservations.Update(reservation);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("UpdateReservationByAddressAndRestaurant")
  .Produces(204)
  .Produces(400)
  .Produces(404)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, int addressId, int restaurantId, int reservationId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address == null)
    {
        return Results.NotFound("Error: The requested address was not found.");
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant == null)
    {
        return Results.NotFound("Error: The requested restaurant was not found.");
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation == null)
    {
        return Results.NotFound("Error: The requested reservation was not found.");
    }
    db.Reservations.Remove(reservation);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteReservationByAddressAndRestaurant")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

app.Run();

public record AddressDto(string Street, string HouseNumber, string City);
public record RestaurantDto(string Name, string Description, string WebsiteUrl);
public record CreateReservationDto(DateOnly Date, TimeOnly Time, int PartySize);
public record UpdateReservationDto(DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus ReservationStatus);
