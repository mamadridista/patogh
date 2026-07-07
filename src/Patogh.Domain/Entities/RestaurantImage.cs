using Patogh.Domain.Common;

namespace Patogh.Domain.Entities;

public class RestaurantImage : BaseEntity
{
    public Guid RestaurantId { get; set; }
    public Guid MediaAssetId { get; set; }

    // Navigation
    public Restaurant Restaurant { get; set; } = null!;
    public MediaAsset MediaAsset { get; set; } = null!;
}