namespace Restaurant.Application.Features.Restaurants.Dtos.GetRestaurantById;

public record RestaurantDto(
    Guid Id,
    string Name,
    string Address,
    string CuisineType,
    string Status,
    decimal AverageRating);