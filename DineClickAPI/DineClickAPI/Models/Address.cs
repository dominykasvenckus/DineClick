namespace DineClickAPI.Models
{
    public class Address
    {
        public int AddressId { get; set; }
        public required string Street { get; set; }
        public required string HouseNumber { get; set; }
        public required string City { get; set; }
    }
}
