using Restaurant.Domain.Results;

namespace Restaurant.Domain.Restaurants;

public static class RestaurantImageErrors
{
    public static readonly Error InvalidRestaurant = Error.Validation("RestaurantImage.Restaurant.Required", "Restaurant is required.");
    public static readonly Error InvalidUrl = Error.Validation("RestaurantImage.Url.Required", "Image URL is required.");
    public static readonly Error NotFound = Error.NotFound("RestaurantImage.NotFound", "Image was not found.");
}