using Patogh.Domain.Common;

namespace Patogh.Domain.Entities;

public class MenuItem : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid? ImageId { get; set; }

    // Navigation
    public Restaurant Restaurant { get; set; } = null!;
    public MediaAsset? Image { get; set; }
}