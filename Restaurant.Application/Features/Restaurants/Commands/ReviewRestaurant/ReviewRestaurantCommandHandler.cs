using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.IntegrationEvents;
using Restaurant.Application.Common.Interfaces.Messaging;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Messages;
using Restaurant.Application.Features.Restaurants.Dtos.ReviewRestaurant;
using Restaurant.Domain.Restaurants;
using Restaurant.Domain.Restaurants.Enums;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Restaurants.Commands.ReviewRestaurant;

public sealed class ReviewRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    IOutbox outbox,
    ILogger<ReviewRestaurantCommandHandler> logger)
    : IRequestHandler<ReviewRestaurantCommand, Result<ReviewRestaurantResponse>>
{
    public async Task<Result<ReviewRestaurantResponse>> Handle(
        ReviewRestaurantCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing ReviewRestaurantCommand for Restaurant ID: {RestaurantId}",
            command.RestaurantId);

        var request = command.Request;

        var restaurant = await restaurantRepository.GetByIdAsync(
            command.RestaurantId,
            cancellationToken);

        if (restaurant is null)
            return RestaurantErrors.NotFound;

        var previousStatus = restaurant.Status.ToString();

        Result<Updated> result = request.Status switch
        {
            RestaurantStatus.Approved => restaurant.Approve(),
            RestaurantStatus.Rejected => restaurant.Reject(request.Reason),
            RestaurantStatus.Pending => restaurant.RequestModification(),
            _ => RestaurantErrors.InvalidReviewStatus
        };

        if (result.IsError)
            return result.Errors;

        // Write integration events to Outbox (same DB transaction)
        if (request.Status == RestaurantStatus.Approved)
        {
            await outbox.AddAsync(
                new RestaurantApprovedIntegrationEvent(
                    restaurant.Id,
                    restaurant.OwnerId,
                    restaurant.Name,
                    DateTime.UtcNow),
                RoutingKeys.RestaurantApproved,
                cancellationToken);
        }
        else if (request.Status == RestaurantStatus.Rejected)
        {
            await outbox.AddAsync(
                new RestaurantRejectedIntegrationEvent(
                    restaurant.Id,
                    restaurant.OwnerId,
                    restaurant.Name,
                    request.Reason ?? string.Empty,
                    DateTime.UtcNow),
                RoutingKeys.RestaurantRejected,
                cancellationToken);
        }

        if (previousStatus != restaurant.Status.ToString())
        {
            await outbox.AddAsync(
                new RestaurantStatusChangedIntegrationEvent(
                    restaurant.Id,
                    previousStatus,
                    restaurant.Status.ToString(),
                    DateTime.UtcNow),
                RoutingKeys.RestaurantStatusChanged,
                cancellationToken);
        }

        // Atomic commit: restaurant state + outbox messages
        await restaurantRepository.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Restaurant ID: {RestaurantId} has been {Status}",
            restaurant.Id,
            request.Status.ToString().ToLower());

        return new ReviewRestaurantResponse(
            restaurant.Id,
            request.Status.ToString(),
            $"Restaurant has been {request.Status.ToString().ToLower()}.");
    }
}