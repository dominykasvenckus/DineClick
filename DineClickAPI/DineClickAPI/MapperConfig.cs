using AutoMapper;
using DineClickAPI.Models;

namespace DineClickAPI
{
    public class MapperConfig : Profile
    {
        public MapperConfig() 
        {
            CreateMap<AddressDto, Address>();
            CreateMap<RestaurantDto, Restaurant>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom((src, dest, destMember, context) =>
                {
                    if (context.TryGetItems(out var items) && items.ContainsKey("Address"))
                    {
                        return context.Items["Address"];
                    }
                    return destMember;
                }));
            CreateMap<CreateReservationDto, Reservation>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ReservationStatus.Pending))
                .ForMember(dest => dest.Restaurant, opt => opt.MapFrom((src, dest, destMember, context) => context.Items["Restaurant"]));
            CreateMap<UpdateReservationDto, Reservation>();
        }
    }
}
