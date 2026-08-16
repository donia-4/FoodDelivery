using MediatR;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Interfaces.Repositories;
using Restaurant.Application.Common.Interfaces.Services;
using Restaurant.Application.Features.Branches.Dtos.UpdateBranch;
using Restaurant.Domain.Branches;
using Restaurant.Domain.Results;
using Restaurant.Domain.WorkingHours;

namespace Restaurant.Application.Features.Branches.Commands.UpdateBranch;

public sealed class UpdateBranchCommandHandler(
    IBranchRepository branchRepository,
    ILogger<UpdateBranchCommandHandler> logger,
    ICacheService cacheService)
    : IRequestHandler<UpdateBranchCommand, Result<UpdateBranchResponse>>
{
    private readonly IBranchRepository _branchRepository = branchRepository;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<Result<UpdateBranchResponse>> Handle(
        UpdateBranchCommand command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Processing UpdateBranchCommand for Branch ID: {BranchId}",
            command.BranchId);

        var request = command.Request;

        var branch = await _branchRepository.GetByIdWithWorkingHoursAsync(
            command.BranchId,
            cancellationToken);

        if (branch is null)
        {
            return BranchErrors.NotFound;
        }

        if (command.Request.Name != null)
        {
            bool duplicateName = await _branchRepository.ExistsWithTheGivenName(command.Request.Name, cancellationToken);
            if (duplicateName) return BranchErrors.DuplicateName;
        }

        var updateResult = branch.Update(
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.Phone);

        if (updateResult.IsError)
        {
            return updateResult.TopError;
        }

        if (request.WorkingHours is not null)
        {
            var workingHours = new List<WorkingHour>();

            foreach (var item in request.WorkingHours)
            {
                var workingHourResult = WorkingHour.Create(
                    Guid.NewGuid(),
                    branch.Id,
                    item.DayOfWeek,
                    item.OpenTime,
                    item.CloseTime,
                    item.IsClosed);

                if (workingHourResult.IsError)
                {
                    return workingHourResult.TopError;
                }

                workingHours.Add(workingHourResult.Value);
            }

            var replaceWorkingHoursResult =
                branch.ReplaceWorkingHours(workingHours);

            if (replaceWorkingHoursResult.IsError)
            {
                return replaceWorkingHoursResult.TopError;
            }
        }

        await _branchRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByTagAsync(
            $"branch:{branch.Id}",
            cancellationToken);

        await _cacheService.RemoveByTagAsync(
            $"restaurant:{branch.RestaurantId}:branches",
            cancellationToken);

        await _cacheService.RemoveByTagAsync(
            "branches",
            cancellationToken);

        logger.LogInformation("Successfully updated Branch ID: {BranchId}",
            branch.Id);

        return new UpdateBranchResponse(branch.Id);
    }
}