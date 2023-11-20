using DineClickAPI.Models;

namespace DineClickAPI;

public record CrupdateCityDto(decimal Latitude, decimal Longitude, string Name);
public record CrupdateRestaurantDto(string Name, string Description, string StreetAddress, string WebsiteUrl);
public record RestaurantDto(int RestaurantId, string Name, string Description, string StreetAddress, string WebsiteUrl, string RestaurantManagerId);
public record CreateReservationDto(DateOnly Date, TimeOnly Time, int PartySize);
public record UpdateReservationDto(DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus Status);
public record ReservationDto(int ReservationId, DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus Status, DateTimeOffset CreatedAt, string ReservingUserId);
public record RegisteredUserReservationDto(int ReservationId, DateOnly Date, TimeOnly Time, int PartySize, ReservationStatus Status, DateTimeOffset CreatedAt, int RestaurantId, string ReservingUserId);
public record RefreshAccessTokenDto(string RefreshToken);
public record RegisterUserDto(string Username, string Password, string Email, string FirstName, string LastName, UserRole Role);
public record LoginUserDto(string Username, string Password);
public record SuccessfullyLoginUserDto(string AccessToken, string RefreshToken);
public record UpdateUserDto(string Username, string Email, string FirstName, string LastName);
public record UserDto(string UserId, string Username, string Email, string FirstName, string LastName, UserRole Role);
public record AdminUserDto(string UserId, string Username, string Email, string FirstName, string LastName, UserRole Role, bool IsBanned, DateTimeOffset TokenValidityThreshold);
