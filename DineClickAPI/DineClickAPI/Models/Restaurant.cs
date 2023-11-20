namespace DineClickAPI.Models;

public class Restaurant
{
    public int RestaurantId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string StreetAddress { get; set; }
    public required string WebsiteUrl { get; set; }
    public required City City { get; set; }
    public required User RestaurantManager { get; set; }
}
