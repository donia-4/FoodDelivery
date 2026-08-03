using MediatR;
using Restaurant.Application.Features.Restaurants.Dtos.GetRestaurantById;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Restaurants.Queries.GetRestaurantById;

public record GetRestaurantByIdQuery(Guid Id) : IRequest<Result<RestaurantDto>>;