using MediatR;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Features.Restaurants.Dtos.GetRestaurantById;
using Restaurant.Domain.Restaurants;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Restaurants.Queries.GetRestaurantById;

public sealed class GetRestaurantByIdQueryHandler(IRestaurantRepository repository)
    : IRequestHandler<GetRestaurantByIdQuery, Result<RestaurantDto>>
{
    public async Task<Result<RestaurantDto>> Handle(
        GetRestaurantByIdQuery request, CancellationToken ct)
    {
        var restaurant = await repository.GetByIdAsync(request.Id, ct);

        if (restaurant is null)
            return RestaurantErrors.NotFound;

        return new RestaurantDto(
            restaurant.Id,
            restaurant.Name,
            restaurant.Address,
            restaurant.CuisineType.ToString(),
            restaurant.Status.ToString(),
            restaurant.AverageRating);
    }
}