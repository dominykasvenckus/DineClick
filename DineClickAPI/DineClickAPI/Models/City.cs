using System.ComponentModel.DataAnnotations.Schema;

namespace DineClickAPI.Models;

public class City
{
    public int CityId { get; set; }
    [Column(TypeName = "decimal(8,6)")]
    public required decimal Latitude { get; set; }
    [Column(TypeName = "decimal(9,6)")]
    public required decimal Longitude { get; set; }
    public required string Name { get; set; }
}
