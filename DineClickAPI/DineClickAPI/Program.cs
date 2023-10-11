using AutoMapper;
using DineClickAPI;
using DineClickAPI.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
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

app.UseExceptionHandler(c => c.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    if (exception is not null)
    {
        if (exception is BadHttpRequestException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "The request body contains invalid JSON." });
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new { error = "An internal server error occurred." });
        }
    }
}));

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
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    return Results.Ok(address);
}).WithName("GetAddress")
  .Produces<Address>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses", async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateAddressDto> validator, [FromBody] CrupdateAddressDto crupdateAddressDto) =>
{
    var validationResult = await validator.ValidateAsync(crupdateAddressDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
    }
    var address = mapper.Map<Address>(crupdateAddressDto);
    db.Addresses.Add(address);
    await db.SaveChangesAsync();
    return Results.CreatedAtRoute("GetAddress", new { addressId = address.AddressId }, address);
}).WithName("CreateAddress")
  .Produces<Address>(201)
  .Produces(400)
  .Produces(422)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateAddressDto> validator, int addressId, [FromBody] CrupdateAddressDto crupdateAddressDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var validationResult = await validator.ValidateAsync(crupdateAddressDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
    }
    mapper.Map<CrupdateAddressDto, Address>(crupdateAddressDto, address);
    db.Addresses.Update(address);
    await db.SaveChangesAsync();
    return Results.Ok(address);
}).WithName("UpdateAddress")
  .Produces<Address>(200)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}", async (ApplicationDbContext db, int addressId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    db.Addresses.Remove(address);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteAddress")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

// Endpoints for restaurants.
app.MapGet("api/addresses/{addressId:int}/restaurants", async (ApplicationDbContext db, IMapper mapper, int addressId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurants = await db.Restaurants
        .Include(r => r.Address)
        .Where(r => r.Address.AddressId == addressId)
        .ToListAsync();
    var restaurantDtos = mapper.Map<IEnumerable<RestaurantDto>>(restaurants);
    return Results.Ok(restaurantDtos);
}).WithName("GetRestaurantsByAddress")
  .Produces<IEnumerable<RestaurantDto>>(200)
  .Produces(404)
  .WithOpenApi();

app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, IMapper mapper, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
    return Results.Ok(restaurantDto);
}).WithName("GetRestaurantByAddress")
  .Produces<RestaurantDto>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses/{addressId:int}/restaurants", async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateRestaurantDto> validator, int addressId, [FromBody] CrupdateRestaurantDto crupdateRestaurantDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var validationResult = await validator.ValidateAsync(crupdateRestaurantDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
    }
    var restaurant = mapper.Map<Restaurant>(crupdateRestaurantDto, opt => opt.Items["Address"] = address);
    db.Restaurants.Add(restaurant);
    await db.SaveChangesAsync();
    var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
    return Results.CreatedAtRoute("GetRestaurantByAddress", new { addressId, restaurantId = restaurant.RestaurantId }, restaurantDto);
}).WithName("CreateRestaurantByAddress")
  .Produces<RestaurantDto>(201)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<CrupdateRestaurantDto> validator, int addressId, int restaurantId, [FromBody] CrupdateRestaurantDto crupdateRestaurantDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var validationResult = await validator.ValidateAsync(crupdateRestaurantDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
    }
    mapper.Map<CrupdateRestaurantDto, Restaurant>(crupdateRestaurantDto, restaurant);
    db.Restaurants.Update(restaurant);
    await db.SaveChangesAsync();
    var restaurantDto = mapper.Map<RestaurantDto>(restaurant);
    return Results.Ok(restaurantDto);
}).WithName("UpdateRestaurantByAddress")
  .Produces<RestaurantDto>(200)
  .Produces(400)
  .Produces(404)
  .Produces(422)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}/restaurants/{restaurantId:int}", async (ApplicationDbContext db, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    db.Restaurants.Remove(restaurant);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteRestaurantByAddress")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

// Endpoints for reservations.
app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations", async (ApplicationDbContext db, IMapper mapper, int addressId, int restaurantId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var reservations = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .Where(r => r.Restaurant.RestaurantId == restaurant.RestaurantId)
        .ToListAsync();
    var reservationDtos = mapper.Map<IEnumerable<ReservationDto>>(reservations);
    return Results.Ok(reservationDtos);
}).WithName("GetReservationsByAddressAndRestaurant")
  .Produces<IEnumerable<ReservationDto>>(200)
  .Produces(404)
  .WithOpenApi();

app.MapGet("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, IMapper mapper, int addressId, int restaurantId, int reservationId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation is null)
    {
        return Results.NotFound(new { error = "The requested reservation was not found." });
    }
    var reservationDto = mapper.Map<ReservationDto>(reservation);
    return Results.Ok(reservationDto);
}).WithName("GetReservationByAddressAndRestaurant")
  .Produces<ReservationDto>(200)
  .Produces(404)
  .WithOpenApi();

app.MapPost("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations", async (ApplicationDbContext db, IMapper mapper, IValidator<CreateReservationDto> validator, int addressId, int restaurantId, [FromBody] CreateReservationDto createReservationDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var validationResult = await validator.ValidateAsync(createReservationDto);
    if (!validationResult.IsValid)
    {
        return Results.UnprocessableEntity(validationResult.Errors.Select(e => new { error = e.ErrorMessage }));
    }
    var reservation = mapper.Map<Reservation>(createReservationDto, opt => opt.Items["Restaurant"] = restaurant);
    db.Reservations.Add(reservation);
    await db.SaveChangesAsync();
    var reservationDto = mapper.Map<ReservationDto>(reservation);
    return Results.CreatedAtRoute("GetReservationByAddressAndRestaurant", new { addressId, restaurantId = restaurant.RestaurantId, reservationId = reservation.ReservationId }, reservationDto);
}).WithName("CreateReservationByAddressAndRestaurant")
  .Produces<ReservationDto>(201)
  .Produces(400)
  .Produces(404)
  .WithOpenApi();

app.MapPut("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, IMapper mapper, IValidator<UpdateReservationDto> validator, int addressId, int restaurantId, int reservationId, [FromBody] UpdateReservationDto updateReservationDto) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation is null)
    {
        return Results.NotFound(new { error = "The requested reservation was not found." });
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
}).WithName("UpdateReservationByAddressAndRestaurant")
  .Produces<ReservationDto>(200)
  .Produces(400)
  .Produces(404)
  .WithOpenApi();

app.MapDelete("api/addresses/{addressId:int}/restaurants/{restaurantId:int}/reservations/{reservationId:int}", async (ApplicationDbContext db, int addressId, int restaurantId, int reservationId) =>
{
    var address = await db.Addresses.FindAsync(addressId);
    if (address is null)
    {
        return Results.NotFound(new { error = "The requested address was not found." });
    }
    var restaurant = await db.Restaurants
        .Include(r => r.Address)
        .FirstOrDefaultAsync(r => r.Address.AddressId == addressId && r.RestaurantId == restaurantId);
    if (restaurant is null)
    {
        return Results.NotFound(new { error = "The requested restaurant was not found." });
    }
    var reservation = await db.Reservations
        .Include(r => r.Restaurant)
            .ThenInclude(r => r.Address)
        .FirstOrDefaultAsync(r => r.Restaurant.RestaurantId == restaurant.RestaurantId && r.ReservationId == reservationId);
    if (reservation is null)
    {
        return Results.NotFound(new { error = "The requested reservation was not found." });
    }
    db.Reservations.Remove(reservation);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).WithName("DeleteReservationByAddressAndRestaurant")
  .Produces(204)
  .Produces(404)
  .WithOpenApi();

app.Run();

public record CrupdateAddressDto(string Street, string HouseNumber, string City);
public record CrupdateRestaurantDto(string Name, string Description, string WebsiteUrl);
public record RestaurantDto(int RestaurantId, string Name, string Description, string WebsiteUrl);
public record CreateReservationDto(DateOnly Date, TimeOnly Time, int PartySize);
public record UpdateReservationDto(DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus Status);
public record ReservationDto(int ReservationId, DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus Status);
