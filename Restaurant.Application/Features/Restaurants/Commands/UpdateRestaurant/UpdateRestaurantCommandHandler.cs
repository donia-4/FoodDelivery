using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Dtos;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Interfaces.Services;
using Restaurant.Application.Features.Restaurants.Commands.UpdateRestaurant;
using Restaurant.Application.Features.Restaurants.Dtos.UpdateRestaurant;
using Restaurant.Domain.Restaurants;
using Restaurant.Domain.Results;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Restaurant.Application.Features.Restaurants.UpdateRestaurant;

public sealed class UpdateRestaurantCommandHandler(
    IRestaurantRepository restaurantRepository,
    ICacheService cacheService,
    IFileService fileService,
    ILogger<UpdateRestaurantCommandHandler> logger)
    : IRequestHandler<
        UpdateRestaurantCommand,
        Result<UpdateRestaurantResponse>>
{
    private readonly IRestaurantRepository _restaurantRepository =
        restaurantRepository;

    private readonly ICacheService _cacheService =
        cacheService;

    private readonly IFileService _fileService =
        fileService;

    public async Task<Result<UpdateRestaurantResponse>> Handle(
        UpdateRestaurantCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing UpdateRestaurantCommand for Restaurant ID: {RestaurantId}",
            request.RestaurantId);

        var restaurant = await _restaurantRepository.GetByIdAsync(
            request.RestaurantId,
            cancellationToken);

        if (restaurant is null)
        {
            return RestaurantErrors.NotFound;
        }

        if (request.Name != null)
        {
            bool duplicateName = await _restaurantRepository.ExistsWithTheGivenName(
                request.Name?.ToLower() ?? restaurant.Name.ToLower(),
                cancellationToken);

            if (duplicateName && request.Name != restaurant.Name)
                return RestaurantErrors.DuplicateName;
        }

        // ─── Step 1: Upload new images first (safest order) ───
        UploadFileResponse? newLogo = null;
        UploadFileResponse? newCover = null;
        string? oldLogoPublicId = restaurant.LogoPublicId;
        string? oldCoverPublicId = restaurant.CoverImagePublicId;

        if (request.Logo is not null)
        {
            logger.LogInformation(
                "Uploading new logo for Restaurant ID: {RestaurantId}",
                request.RestaurantId);

            newLogo = await _fileService.UploadAsync(
                request.Logo,
                cancellationToken);

            logger.LogInformation(
                "New logo uploaded successfully. PublicId: {PublicId}",
                newLogo.PublicId);
        }

        if (request.CoverImage is not null)
        {
            logger.LogInformation(
                "Uploading new cover image for Restaurant ID: {RestaurantId}",
                request.RestaurantId);

            newCover = await _fileService.UploadAsync(
                request.CoverImage,
                cancellationToken);

            logger.LogInformation(
                "New cover image uploaded successfully. PublicId: {PublicId}",
                newCover.PublicId);
        }

        // ─── Step 2: Update entity in database ───
        var result = restaurant.UpdateDetails(
            request.Name,
            request.Description,
            request.Phone,
            request.Email,
            request.CuisineType,
            request.Address,
            newLogo?.Url,
            newLogo?.PublicId,
            newCover?.Url,
            newCover?.PublicId);

        if (result.IsError)
        {
            // Rollback: delete newly uploaded images
            if (newLogo is not null)
            {
                logger.LogWarning(
                    "Domain update failed. Rolling back new logo: {PublicId}",
                    newLogo.PublicId);

                await _fileService.DeleteAsync(
                    newLogo.PublicId,
                    cancellationToken);
            }

            if (newCover is not null)
            {
                logger.LogWarning(
                    "Domain update failed. Rolling back new cover: {PublicId}",
                    newCover.PublicId);

                await _fileService.DeleteAsync(
                    newCover.PublicId,
                    cancellationToken);
            }

            return result.Errors;
        }

        try
        {
            await _restaurantRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Rollback: delete newly uploaded images if DB save failed
            if (newLogo is not null)
            {
                logger.LogError(
                    ex,
                    "Database save failed. Rolling back new logo: {PublicId}",
                    newLogo.PublicId);

                await _fileService.DeleteAsync(
                    newLogo.PublicId,
                    cancellationToken);
            }

            if (newCover is not null)
            {
                logger.LogError(
                    ex,
                    "Database save failed. Rolling back new cover: {PublicId}",
                    newCover.PublicId);

                await _fileService.DeleteAsync(
                    newCover.PublicId,
                    cancellationToken);
            }

            throw;
        }

        // ─── Step 3: Delete old images from Cloudinary (after successful DB update) ───
        if (newLogo is not null && !string.IsNullOrWhiteSpace(oldLogoPublicId))
        {
            logger.LogInformation(
                "Deleting old logo from Cloudinary: {PublicId}",
                oldLogoPublicId);

            var deleted = await _fileService.DeleteAsync(
                oldLogoPublicId,
                cancellationToken);

            if (!deleted)
            {
                logger.LogWarning(
                    "Failed to delete old logo from Cloudinary: {PublicId}",
                    oldLogoPublicId);
            }
        }

        if (newCover is not null && !string.IsNullOrWhiteSpace(oldCoverPublicId))
        {
            logger.LogInformation(
                "Deleting old cover image from Cloudinary: {PublicId}",
                oldCoverPublicId);

            var deleted = await _fileService.DeleteAsync(
                oldCoverPublicId,
                cancellationToken);

            if (!deleted)
            {
                logger.LogWarning(
                    "Failed to delete old cover image from Cloudinary: {PublicId}",
                    oldCoverPublicId);
            }
        }

        // ─── Invalidate cache ───
        await _cacheService.RemoveByTagAsync(
            "restaurants",
            cancellationToken);

        logger.LogInformation(
            "Successfully updated Restaurant ID: {RestaurantId}",
            request.RestaurantId);

        return new UpdateRestaurantResponse(
            restaurant.Id,
            restaurant.Name,
            restaurant.Description,
            restaurant.Phone,
            restaurant.Email,
            restaurant.CuisineType,
            restaurant.Address,
            restaurant.Logo,
            restaurant.CoverImage);
    }
}