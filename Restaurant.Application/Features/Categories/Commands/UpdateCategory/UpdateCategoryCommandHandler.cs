using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Interfaces.Services;
using Restaurant.Application.Features.Categories.Dtos.UpdateCategory;
using Restaurant.Domain.Categories;
using Restaurant.Domain.Foods;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Categories.Commands.UpdateCategory
{
    public sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, 
        ICacheService cacheService, ILogger<UpdateCategoryCommandHandler> logger) : IRequestHandler<UpdateCategoryCommand, Result<UpdateCategoryResponse>>
    {
        public async Task<Result<UpdateCategoryResponse>> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Processing UpdateCategoryCommand for Category ID: {CategoryId}", command.CategoryId);

            var category = await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);
            if (category is null) return CategoryErrors.NotFound;

            bool duplicateName = await categoryRepository.ExistsWithTheGivenName(command.Request.Name, cancellationToken);
            if (duplicateName) return CategoryErrors.DuplicateName;

            var result = category.Rename(command.Request.Name);
            if (result.IsError) return result.TopError;

            await categoryRepository.SaveChangesAsync(cancellationToken);
            await cacheService.RemoveByTagAsync($"category:{category.Id}", cancellationToken);
            await cacheService.RemoveByTagAsync($"restaurant:{category.RestaurantId}:categories", cancellationToken);
            await cacheService.RemoveByTagAsync("categories", cancellationToken);

            logger.LogInformation("Category ID: {CategoryId} updated successfully", category.Id);

            return new UpdateCategoryResponse(category.Id);
        }
    }
}
