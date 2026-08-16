using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Dtos;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Interfaces.Services;
using Restaurant.Application.Features.Foods.Commands.UpdateFood;
using Restaurant.Application.Features.Foods.Dtos.UpdateFood;
using Restaurant.Domain.Foods;
using Restaurant.Domain.Results;

namespace Restaurant.Application.Features.Foods.Commands.UpdateFood;

public sealed class UpdateFoodCommandHandler(
    IFoodRepository foodRepository,
    ICacheService cacheService,
    IFileService fileService,
    ILogger<UpdateFoodCommandHandler> logger)
    : IRequestHandler<UpdateFoodCommand, Result<UpdateFoodResponse>>
{
    private readonly IFoodRepository _foodRepository = foodRepository;
    private readonly ICacheService _cacheService = cacheService;
    private readonly IFileService _fileService = fileService;

    public async Task<Result<UpdateFoodResponse>> Handle(
        UpdateFoodCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing UpdateFoodCommand for Food ID: {FoodId}",
            command.FoodId);

        var request = command.Request;

        var food = await _foodRepository.GetByIdAsync(
            command.FoodId,
            cancellationToken);

        if (food is null)
        {
            return FoodErrors.NotFound;
        }

        if(command.Request.Name != null)
        {
            bool duplicateName = await _foodRepository.ExistsWithTheGivenName(
            request.Name?.ToLower() ?? food.Name.ToLower(),
            cancellationToken);

            if (duplicateName && request.Name != food.Name)
                return FoodErrors.DuplicateName;
        }

        // ─── Step 1: Upload new image first (safest order) ───
        UploadFileResponse? newImage = null;
        string? oldImagePublicId = food.ImagePublicId;

        if (request.Image is not null)
        {
            logger.LogInformation(
                "Uploading new image for Food ID: {FoodId}",
                command.FoodId);

            newImage = await _fileService.UploadAsync(
                request.Image,
                cancellationToken);

            logger.LogInformation(
                "New image uploaded successfully. PublicId: {PublicId}",
                newImage.PublicId);
        }

        // ─── Step 2: Update entity in database ───
        var result = food.Update(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryId,
            newImage?.Url,
            newImage?.PublicId,
            request.PreparationTimeMinutes,
            request.Calories);

        if (result.IsError)
        {
            // Rollback: delete newly uploaded image if upload succeeded
            if (newImage is not null)
            {
                logger.LogWarning(
                    "Domain update failed. Rolling back new image: {PublicId}",
                    newImage.PublicId);

                await _fileService.DeleteAsync(
                    newImage.PublicId,
                    cancellationToken);
            }

            return result.TopError;
        }

        try
        {
            await _foodRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Rollback: delete newly uploaded image if DB save failed
            if (newImage is not null)
            {
                logger.LogError(
                    ex,
                    "Database save failed. Rolling back new image: {PublicId}",
                    newImage.PublicId);

                await _fileService.DeleteAsync(
                    newImage.PublicId,
                    cancellationToken);
            }

            throw;
        }

        // ─── Step 3: Delete old image from Cloudinary (after successful DB update) ───
        if (newImage is not null && !string.IsNullOrWhiteSpace(oldImagePublicId))
        {
            logger.LogInformation(
                "Deleting old image from Cloudinary: {PublicId}",
                oldImagePublicId);

            var deleted = await _fileService.DeleteAsync(
                oldImagePublicId,
                cancellationToken);

            if (!deleted)
            {
                logger.LogWarning(
                    "Failed to delete old image from Cloudinary: {PublicId}",
                    oldImagePublicId);
            }
        }

        // ─── Invalidate cache ───
        await _cacheService.RemoveByTagAsync(
            $"food:{food.Id}",
            cancellationToken);

        await _cacheService.RemoveByTagAsync(
            $"restaurant:{food.RestaurantId}:menu",
            cancellationToken);

        await _cacheService.RemoveByTagAsync(
            "foods",
            cancellationToken);

        logger.LogInformation(
            "Successfully updated Food ID: {FoodId}",
            food.Id);

        return new UpdateFoodResponse(food.Id);
    }
}