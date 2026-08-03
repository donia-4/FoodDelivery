using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Features.Categories.Commands.CreateCategory;
using Restaurant.Application.Features.Categories.Commands.DeleteCategory;
using Restaurant.Application.Features.Categories.Commands.ReorderCategories;
using Restaurant.Application.Features.Categories.Commands.UpdateCategory;
using Restaurant.Application.Features.Categories.Dtos.CreateCategory;
using Restaurant.Application.Features.Categories.Dtos.ReorderCategories;
using Restaurant.Application.Features.Categories.Dtos.UpdateCategory;

namespace Restaurant.API.Controllers
{
    [Route("api/categories")]
    [Authorize]
    public sealed class CategoriesController(ISender sender) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new CreateCategoryCommand(request), cancellationToken);
            if (result.IsError) return Problem(result.Errors);
            return CreatedEnvelope(result.Value, "Category created successfully");
        }

        [HttpPut("{categoryId:guid}")]
        public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new UpdateCategoryCommand(categoryId, request), cancellationToken);
            return result.Match<IActionResult>(r => OkEnvelope(r, "Category updated successfully"), Problem);
        }

        [HttpDelete("{categoryId:guid}")]
        public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new DeleteCategoryCommand(categoryId), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Category deleted successfully"), Problem);
        }

        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderCategories([FromBody] IReadOnlyCollection<ReorderCategoryRequest> request, CancellationToken cancellationToken)
        {
            var result = await sender.Send(new ReorderCategoriesCommand(request), cancellationToken);
            return result.Match<IActionResult>(_ => OkEnvelope((object?)null, "Categories reordered successfully"), Problem);
        }
    }
}
