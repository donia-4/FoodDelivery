using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Features.Branches.Commands.CreateBranch;
using Restaurant.Application.Features.Branches.Commands.DeleteBranch;
using Restaurant.Application.Features.Branches.Commands.ToggleBranch;
using Restaurant.Application.Features.Branches.Commands.UpdateBranch;
using Restaurant.Application.Features.Branches.Dtos.CreateBranch;
using Restaurant.Application.Features.Branches.Dtos.UpdateBranch;
using Restaurant.Application.Features.Branches.Queries.GetBranchById;

namespace Restaurant.API.Controllers;

[Route("api/branches")]
[Authorize]
public sealed class BranchesController(ISender sender) : ApiController
{
    [HttpPost]
    public async Task<IActionResult> CreateBranch(
        [FromBody] CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBranchCommand(request);

        var result = await sender.Send(
            command,
            cancellationToken);

        if (result.IsError)
        {
            return Problem(result.Errors);
        }

        return CreatedEnvelope(
            result.Value,
            "Branch created successfully");
    }

    // ==========================================
    // READ
    // ==========================================
    [HttpGet("{branchId:guid}")]
    public async Task<IActionResult> GetBranchById(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetBranchByIdQuery(branchId),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Branch retrieved successfully"),
            Problem);
    }

    // ==========================================
    // UPDATE
    // ==========================================
    [HttpPut("{branchId:guid}")]
    public async Task<IActionResult> UpdateBranch(
        Guid branchId,
        [FromForm] UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateBranchCommand(branchId, request);

        var result = await sender.Send(
            command,
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Branch updated successfully"),
            Problem);
    }

    // ==========================================
    // DELETE
    // ==========================================
    [HttpDelete("{branchId:guid}")]
    public async Task<IActionResult> DeleteBranch(
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new DeleteBranchCommand(branchId),
            cancellationToken);

        return result.Match<IActionResult>(
            _ => OkEnvelope((object?)null, "Branch deleted successfully"),Problem);
    }

    // ==========================================
    // ACTIVATE / DEACTIVATE
    // ==========================================

    [HttpPatch("{branchId:guid}/toggle-active")]
    public async Task<IActionResult> ToggleBranchActive(
    Guid branchId,
    CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new ToggleBranchActiveCommand(branchId),
            cancellationToken);

        return result.Match<IActionResult>(
            response => OkEnvelope(
                response,
                "Branch active status toggled successfully"),
            Problem);
    }
}