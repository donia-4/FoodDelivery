using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Dtos;
using Restaurant.Application.Common.IntegrationEvents;
using Restaurant.Application.Common.Interfaces.Messaging;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Interfaces.Services;
using Restaurant.Application.Common.Messages;
using Restaurant.Application.Features.Restaurants.Commands.CreateRestaurant;
using Restaurant.Application.Features.Restaurants.Dtos.CreateRestaurant;
using Restaurant.Domain.Restaurants;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Restaurants.CreateRestaurant;

public sealed class CreateRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IFileService fileService,
    IOutbox outbox,
    ILogger<CreateRestaurantCommandHandler> logger)
    : IRequestHandler<
        CreateRestaurantCommand,
        Result<CreateRestaurantResponse>>
{
    public async Task<Result<CreateRestaurantResponse>> Handle(
        CreateRestaurantCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing CreateRestaurantCommand for Owner ID: {OwnerId}",
            command.Request.OwnerId);

        var request = command.Request;

        bool duplicateName = await restaurantRepository.ExistsWithTheGivenName(
            request.Name.ToLower(), cancellationToken);

        if (duplicateName)
            return RestaurantErrors.DuplicateName;

        UploadFileResponse? logo = null;
        if (request.Logo is not null)
        {
            logo = await fileService.UploadAsync(
                request.Logo,
                cancellationToken);
        }

        UploadFileResponse? cover = null;
        if (request.CoverImage is not null)
        {
            cover = await fileService.UploadAsync(
                request.CoverImage,
                cancellationToken);
        }

        var restaurantResult =
            Domain.Restaurants.Restaurant.Create(
                Guid.NewGuid(),
                request.OwnerId,
                request.Name,
                request.Description,
                request.Phone,
                request.Email,
                request.CuisineType,
                request.Address,
                logo?.Url,
                logo?.PublicId,
                cover?.Url,
                cover?.PublicId);

        if (restaurantResult.IsError)
        {
            return restaurantResult.Errors;
        }

        var restaurant = restaurantResult.Value;

        await restaurantRepository.AddAsync(
            restaurant,
            cancellationToken);

        // Write integration event to Outbox (same DB transaction)
        await outbox.AddAsync(
            new RestaurantRequestedIntegrationEvent(
                restaurant.Id,
                restaurant.OwnerId,
                restaurant.Name,
                DateTime.UtcNow),
            RoutingKeys.RestaurantRequested,
            cancellationToken);

        // Atomic commit: restaurant + outbox message
        await restaurantRepository.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Restaurant created successfully with ID: {RestaurantId}",
            restaurant.Id);

        return new CreateRestaurantResponse(
            restaurant.Id,
            restaurant.OwnerId,
            restaurant.Name,
            restaurant.Description,
            restaurant.Phone,
            restaurant.Email,
            restaurant.CuisineType,
            restaurant.Address,
            restaurant.Logo,
            restaurant.CoverImage,
            restaurant.Status.ToString(),
            restaurant.IsApproved);
    }
}