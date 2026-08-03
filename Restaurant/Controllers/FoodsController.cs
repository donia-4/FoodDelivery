using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.API.Dtos;
using Restaurant.Application.Common.Dtos;
using Restaurant.Application.Features.Foods.Commands.ChangeFoodAvailability;
using Restaurant.Application.Features.Foods.Commands.CreateFood;
using Restaurant.Application.Features.Foods.Commands.DeleteFood;
using Restaurant.Application.Features.Foods.Commands.HideFood;
using Restaurant.Application.Features.Foods.Commands.ShowFood;
using Restaurant.Application.Features.Foods.Commands.UpdateFood;
using Restaurant.Application.Features.Foods.Dtos.CreateFood;
using Restaurant.Application.Features.Foods.Dtos.SearchFoods;
using Restaurant.Application.Features.Foods.Dtos.UpdateFood;
using Restaurant.Application.Features.Foods.Queries.GetFoodById;
using Restaurant.Application.Features.Foods.Queries.SearchFoods;
using Restaurant.Domain.Results;

namespace Restaurant.API.Controllers
{
    [Route("api/foods")]
    [Authorize]
    public sealed class FoodsController(ISender sender) : ApiController
    {
        private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".jfif" };

        [HttpPost]
        public async Task<IActionResult> CreateFood([FromForm] CreateFoodApiRequest request, CancellationToken cancellationToken)
        {
            if (request.Image is not null && !IsValidImage(request.Image))
            {
                return Problem(new List<Error>
                {
                    Error.Validation("Image.Invalid", "Food image must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
                });
            }

            var appRequest = new CreateFoodRequest(
                request.RestaurantId,
                request.CategoryId,
                request.Name,
                request.Description,
                request.Price,
                ToFileUpload(request.Image),
                request.PreparationTimeMinutes,
                request.Calories);

            var result = await sender.Send(new CreateFoodCommand(appRequest), cancellationToken);
            if (result.IsError) return Problem(result.Errors);
            return CreatedEnvelope(result.Value, "Food item created successfully");
        }

        [HttpGet("{foodId:guid}")]
        public async Task<IActionResult> GetFoodById(Guid foodId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new GetFoodByIdQuery(foodId), cancellationToken);
            return result.Match<IActionResult>(r => OkEnvelope(r, "Food item retrieved successfully"), Problem);
        }

        [HttpPut("{foodId:guid}")]
        public async Task<IActionResult> UpdateFood(Guid foodId, [FromForm] UpdateFoodApiRequest request, CancellationToken cancellationToken)
        {
            if (request.Image is not null && !IsValidImage(request.Image))
            {
                return Problem(new List<Error>
                {
                    Error.Validation("Image.Invalid", "Food image must be ≤ 5 MB and of type: .jpg, .jpeg, .png, .jfif")
                });
            }

            var appRequest = new UpdateFoodRequest
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                CategoryId = request.CategoryId,
                Image = ToFileUpload(request.Image),
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                Calories = request.Calories
            };

            var result = await sender.Send(new UpdateFoodCommand(foodId, appRequest), cancellationToken);
            return result.Match<IActionResult>(r => OkEnvelope(r, "Food item updated successfully"), Problem);
        }

        [HttpDelete("{foodId:guid}")]
        public async Task<IActionResult> DeleteFood(Guid foodId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteFoodCommand(foodId), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Food item deleted successfully"), Problem);
        }

        [HttpPatch("{foodId:guid}/hide")]
        public async Task<IActionResult> HideFood(Guid foodId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new HideFoodCommand(foodId), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Food item hidden successfully"), Problem);
        }

        [HttpPatch("{foodId:guid}/show")]
        public async Task<IActionResult> ShowFood(Guid foodId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new ShowFoodCommand(foodId), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Food item shown successfully"), Problem);
        }

        [HttpPatch("{foodId:guid}/availability")]
        public async Task<IActionResult> ChangeAvailability(Guid foodId, [FromBody] bool isAvailable, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new ChangeFoodAvailabilityCommand(foodId, isAvailable), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Food availability changed successfully"), Problem);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchFoods([FromQuery] SearchFoodsRequest request, CancellationToken cancellationToken)
        {
            var query = new SearchFoodsQuery(
                request.RestaurantId,
                request.Name,
                request.CategoryName,
                request.MinPrice,
                request.MaxPrice,
                request.PageNumber,
                request.PageSize);

            var result = await sender.Send(query, cancellationToken);

            return result.Match<IActionResult>(
                r => OkEnvelope(r, "Foods retrieved successfully"),
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
}
