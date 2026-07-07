using Patogh.Domain.Common;

namespace Patogh.Domain.Entities;

public class Restaurant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string PriceRange { get; set; } = string.Empty;
    public string FoodType { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsApproved { get; set; } = false;

    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? CoverImageId { get; set; }
    public MediaAsset? CoverImage { get; set; }

    public ICollection<MenuItem> MenuItems { get; set; }
        = new List<MenuItem>();
    public ICollection<RestaurantTable> Tables { get; set; }
        = new List<RestaurantTable>();
    public ICollection<RestaurantImage> Images { get; set; }
        = new List<RestaurantImage>();
}