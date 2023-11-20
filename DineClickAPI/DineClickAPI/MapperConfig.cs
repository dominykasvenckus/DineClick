using AutoMapper;
using DineClickAPI.Models;

namespace DineClickAPI;

public class MapperConfig : Profile
{
    public MapperConfig() 
    {
        CreateMap<CrupdateCityDto, City>();
        CreateMap<CrupdateRestaurantDto, Restaurant>()
            .ForMember(dest => dest.City, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (context.TryGetItems(out var items) && items.ContainsKey("City"))
                {
                    return context.Items["City"];
                }
                return destMember;
            }))
            .ForMember(dest => dest.RestaurantManager, opt => opt.MapFrom((src, dest, destMember, context) =>
            {
                if (context.TryGetItems(out var items) && items.ContainsKey("RestaurantManager"))
                {
                    return context.Items["RestaurantManager"];
                }
                return destMember;
            }));
        CreateMap<Restaurant, RestaurantDto>()
            .ForMember(dest => dest.RestaurantManagerId, opt => opt.MapFrom(src => src.RestaurantManager.Id));
        CreateMap<CreateReservationDto, Reservation>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReservationStatus.Pending))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
            .ForMember(dest => dest.Restaurant, opt => opt.MapFrom((src, dest, destMember, context) => context.Items["Restaurant"]))
            .ForMember(dest => dest.ReservingUser, opt => opt.MapFrom((src, dest, destMember, context) => context.Items["ReservingUser"]));
        CreateMap<UpdateReservationDto, Reservation>();
        CreateMap<Reservation, ReservationDto>()
            .ForMember(dest => dest.ReservingUserId, opt => opt.MapFrom(src => src.ReservingUser.Id));
        CreateMap<Reservation, RegisteredUserReservationDto>()
            .ForCtorParam("RestaurantId", opt => opt.MapFrom(src => src.Restaurant.RestaurantId))
            .ForMember(dest => dest.ReservingUserId, opt => opt.MapFrom(src => src.ReservingUser.Id));
        CreateMap<RegisterUserDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));
        CreateMap<User, UserDto>()
            .ForCtorParam("UserId", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Role", opt => opt.MapFrom((src, context) => context.Items["Role"]));
        CreateMap<User, AdminUserDto>()
            .ForCtorParam("UserId", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("Role", opt => opt.MapFrom((src, context) => context.Items["Role"]));
    }
}
