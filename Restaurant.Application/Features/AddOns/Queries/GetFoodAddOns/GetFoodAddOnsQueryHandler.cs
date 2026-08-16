using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Models;
using Restaurant.Application.Features.AddOns.Dtos.GetFoodAddOns;
using Restaurant.Domain.Foods;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.AddOns.Queries.GetFoodAddOns;

public sealed class GetFoodAddOnsQueryHandler(
    IFoodRepository foodRepository,
    ILogger<GetFoodAddOnsQueryHandler> logger)
    : IRequestHandler<GetFoodAddOnsQuery, Result<PaginatedList<GetFoodAddOnsResponse>>>
{
    public async Task<Result<PaginatedList<GetFoodAddOnsResponse>>> Handle(
        GetFoodAddOnsQuery request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Retrieving paginated add-ons for Food {FoodId}. PageNumber: {PageNumber}, PageSize: {PageSize}",
            request.FoodId,
            request.PageNumber,
            request.PageSize);

        var food = await foodRepository.GetByIdWithAddOnsAsync(
            request.FoodId,
            cancellationToken);

        if (food is null)
        {
            logger.LogWarning(
                "Food with ID {FoodId} was not found",
                request.FoodId);

            return FoodErrors.NotFound;
        }

        var query = foodRepository
            .GetAddOnsByFoodId(request.FoodId)
            .Select(addOn => new GetFoodAddOnsResponse(
                addOn.Id,
                addOn.Name,
                addOn.Price,
                addOn.IsRequired,
                addOn.MaxQuantity));

        var paginatedResult = await PaginatedList<GetFoodAddOnsResponse>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        logger.LogInformation(
            "Retrieved {ReturnedCount} add-ons out of {TotalCount} for Food {FoodId}",
            paginatedResult.Items.Count,
            paginatedResult.TotalCount,
            request.FoodId);

        return paginatedResult;
    }
}