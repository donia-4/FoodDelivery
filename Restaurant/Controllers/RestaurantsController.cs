using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Dtos;
using Restaurant.Application.Common.Dtos;
using Restaurant.Application.Features.Branches.Queries.GetRestaurantBranches;
using Restaurant.Application.Features.Categories.Queries.GetRestaurantCategories;
using Restaurant.Application.Features.Foods.Queries.GetRestaurantMenu;
using Restaurant.Application.Features.Restaurants.Commands.ChangeRestaurantAvailability;
using Restaurant.Application.Features.Restaurants.Commands.CreateRestaurant;
using Restaurant.Application.Features.Restaurants.Commands.ReviewRestaurant;
using Restaurant.Application.Features.Restaurants.Commands.UpdateRestaurant;
using Restaurant.Application.Features.Restaurants.CreateRestaurant;
using Restaurant.Application.Features.Restaurants.Dtos.ChangeRestaurantAvailability;
using Restaurant.Application.Features.Restaurants.Dtos.CreateRestaurant;
using Restaurant.Application.Features.Restaurants.Dtos.ReviewRestaurant;
using Restaurant.Application.Features.Restaurants.Dtos.SearchRestaurants;
using Restaurant.Application.Features.Restaurants.Dtos.UpdateRestaurant;
using Restaurant.Application.Features.Restaurants.Queries.GetAllRestaurants;
using Restaurant.Application.Features.Restaurants.Queries.SearchRestaurants;
using Restaurant.Domain.Results;

namespace Restaurant.API.Controllers;

[Route("api/restaurants")]
[Authorize]
public sealed class RestaurantsController(ISender sender)
    : ApiController
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".jfif" };

    [HttpPost("request")]
    public async Task<IActionResult> CreateRestaurant(
        [FromForm] CreateRestaurantApiRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Logo is not null && !IsValidImage(request.Logo))
        {
            return Problem(new List<Error>
            {
                Error.Validation("Logo.Invalid", "Logo must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
            });
        }

        if (request.CoverImage is not null && !IsValidImage(request.CoverImage))
        {
            return Problem(new List<Error>
            {
                Error.Validation("CoverImage.Invalid", "Cover image must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
            });
        }

        var appRequest = new CreateRestaurantRequest(
            request.OwnerId,
            request.Name,
            request.Description,
            ToFileUpload(request.Logo),
            ToFileUpload(request.CoverImage),
            request.Phone,
            request.Email,
            request.CuisineType,
            request.Address);

        var command = new CreateRestaurantCommand(appRequest);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        return CreatedEnvelope(
            result.Value,
            "Restaurant request submitted successfully");
    }

    [HttpGet]
    public async Task<IActionResult> GetAllRestaurants(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetAllRestaurantsQuery(
                pageNumber,
                pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurants retrieved successfully"),
            Problem);
    }

    [HttpPatch("{restaurantId:guid}")]
    public async Task<IActionResult> UpdateRestaurant(
        Guid restaurantId,
        [FromForm] UpdateRestaurantApiRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Logo is not null && !IsValidImage(request.Logo))
        {
            return Problem(new List<Error>
            {
                Error.Validation("Logo.Invalid", "Logo must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
            });
        }

        if (request.CoverImage is not null && !IsValidImage(request.CoverImage))
        {
            return Problem(new List<Error>
            {
                Error.Validation("CoverImage.Invalid", "Cover image must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
            });
        }

        var command = new UpdateRestaurantCommand(
            restaurantId,
            request.Name,
            request.Description,
            request.Phone,
            request.Email,
            request.CuisineType,
            request.Address,
            ToFileUpload(request.Logo),
            ToFileUpload(request.CoverImage));

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurant updated successfully"),
            Problem);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetRestaurantBranches(
        Guid restaurantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetRestaurantBranchesQuery(
                restaurantId,
                pageNumber,
                pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurant branches retrieved successfully"),
            Problem);
    }

    [HttpGet("{restaurantId:guid}/menu")]
    public async Task<IActionResult> GetRestaurantMenu(
        Guid restaurantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetRestaurantMenuQuery(
                restaurantId,
                pageNumber,
                pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurant menu retrieved successfully"),
            Problem);
    }

    [HttpGet("{restaurantId:guid}/categories")]
    public async Task<IActionResult> GetRestaurantCategories(
        Guid restaurantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetRestaurantCategoriesQuery(
                restaurantId,
                pageNumber,
                pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Categories retrieved successfully"),
            Problem);
    }

    [HttpPut("{restaurantId:guid}/review")]
    public async Task<IActionResult> ReviewRestaurant(
        Guid restaurantId,
        [FromForm] ReviewRestaurantRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ReviewRestaurantCommand(restaurantId, request),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurant reviewed successfully"),
            Problem);
    }

    [HttpPut("{restaurantId:guid}/availability")]
    public async Task<IActionResult> ChangeRestaurantAvailability(
        Guid restaurantId,
        [FromForm] ChangeRestaurantAvailabilityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ChangeRestaurantAvailabilityCommand(restaurantId, request),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Restaurant availability changed successfully"),
            Problem);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchRestaurants(
        [FromQuery] string? name,
        [FromQuery] string? city,
        [FromQuery] Restaurant.Domain.Restaurants.Enums.CuisineType? cuisineType,
        [FromQuery] Guid? categoryId,
        [FromQuery] Restaurant.Domain.Restaurants.Enums.RestaurantStatus? status,
        [FromQuery] decimal? minRating,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var request = new SearchRestaurantsRequest
        {
            Name = name,
            City = city,
            CuisineType = cuisineType,
            CategoryId = categoryId,
            Status = status,
            MinRating = minRating,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await sender.Send(
            new SearchRestaurantsQuery(request),
            cancellationToken);

        return result.Match<IActionResult>(
            r => OkEnvelope(r, "Restaurants searched successfully"),
            Problem);
    }

    private static bool IsValidImage(IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            return false;

        var extension = Path.GetExtension(file.FileName);
        return AllowedExtensions.Contains(extension);
    }

    private static FileUpload? ToFileUpload(IFormFile? file)
    {
        if (file is null) return null;

        return new FileUpload(
            file.OpenReadStream(),
            file.FileName,
            file.ContentType);
    }
}