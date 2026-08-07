using Restaurant.Domain.Common;
using Restaurant.Domain.Results;
using Restaurant.Domian.Restaurants.Enums;

namespace Restaurant.Domain.Restaurants;

public sealed class RestaurantImage : AuditableEntity
{
    public Guid RestaurantId { get; private set; }
    public ImageType ImageType { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public int? DisplayOrder { get; private set; }

    public Restaurant Restaurant { get; private set; } = null!;

    private RestaurantImage() { }

    private RestaurantImage(Guid id, Guid restaurantId, ImageType imageType, string imageUrl, int? displayOrder) : base(id)
    {
        RestaurantId = restaurantId; ImageType = imageType; ImageUrl = imageUrl; DisplayOrder = displayOrder;
    }

    public static Result<RestaurantImage> Create(Guid id, Guid restaurantId, ImageType imageType, string imageUrl, int? displayOrder = null)
    {
        if (restaurantId == Guid.Empty) return RestaurantImageErrors.InvalidRestaurant;
        if (string.IsNullOrWhiteSpace(imageUrl)) return RestaurantImageErrors.InvalidUrl;

        return new RestaurantImage(id, restaurantId, imageType, imageUrl.Trim(), displayOrder);
    }
}
