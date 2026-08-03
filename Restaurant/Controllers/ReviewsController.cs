using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Controllers;
using Restaurant.Application.Features.Reviews.Commands.CreateReview;
using Restaurant.Application.Features.Reviews.Commands.DeleteReview;
using Restaurant.Application.Features.Reviews.Commands.UpdateReview;
using Restaurant.Application.Features.Reviews.Dtos.CreateReview;
using Restaurant.Application.Features.Reviews.Dtos.UpdateReview;
using Restaurant.Application.Features.Reviews.Queries.GetRestaurantReviews;
using Restaurant.Application.Features.Reviews.Queries.GetReviewById;
using Restaurant.Application.Features.Reviews.Queries.GetUserReviews;

namespace Restaurant.API.Controllers;

[Route("api/reviews")]
[Authorize]
public sealed class ReviewsController(ISender sender) : ApiController
{
    // ==========================================
    // CREATE
    // ==========================================
    [HttpPost]
    public async Task<IActionResult> CreateReview(
        [FromQuery] Guid userId,
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {

        var command = new CreateReviewCommand(userId, request);

        var result = await sender.Send(command, cancellationToken);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        return CreatedEnvelope(
            result.Value.Id,
            "Review created successfully");
    }

    // ==========================================
    // READ
    // ==========================================
    [HttpGet("{reviewId:guid}")]
    public async Task<IActionResult> GetReviewById(
        Guid reviewId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetReviewByIdQuery(reviewId),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(response, "Review retrieved successfully"),
            Problem);
    }

    [HttpGet("restaurant/{restaurantId:guid}")]
    public async Task<IActionResult> GetRestaurantReviews(
        Guid restaurantId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetRestaurantReviewsQuery(restaurantId, pageNumber, pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(response, "Restaurant reviews retrieved successfully"),
            Problem);
    }

    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserReviews(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetUserReviewsQuery(userId, pageNumber, pageSize),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(response, "User reviews retrieved successfully"),
            Problem);
    }

    // ==========================================
    // UPDATE
    // ==========================================
    [HttpPut("{reviewId:guid}")]
    public async Task<IActionResult> UpdateReview(
        Guid reviewId,
        [FromBody] UpdateReviewRequest request,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {

        var command = new UpdateReviewCommand(reviewId, userId, request);

        var result = await sender.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(response, "Review updated successfully"),
            Problem);
    }

    // ==========================================
    // DELETE
    // ==========================================
    [HttpDelete("{reviewId:guid}")]
    public async Task<IActionResult> DeleteReview(
        Guid reviewId,
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {

        var result = await sender.Send(
            new DeleteReviewCommand(reviewId, userId),
            cancellationToken);

        return result.Match<IActionResult>(
            _ => OkEnvelope((object?)null, "Review deleted successfully"),
            Problem);
    }

    
}