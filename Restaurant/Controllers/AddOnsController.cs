using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Features.AddOns.Commands.CreateAddOn;
using Restaurant.Application.Features.AddOns.Commands.DeleteAddOn;
using Restaurant.Application.Features.AddOns.Commands.UpdateAddOn;
using Restaurant.Application.Features.AddOns.Dtos.CreateAddOn;
using Restaurant.Application.Features.AddOns.Dtos.UpdateAddOn;
using Restaurant.Application.Features.AddOns.Queries.GetAddOn;
using Restaurant.Application.Features.AddOns.Queries.GetFoodAddOns;

namespace Restaurant.API.Controllers
{
    [Route("api/addons")]
    [Authorize]
    public sealed class AddOnsController(ISender sender) : ApiController
    {
        // ==========================================
        // CREATE
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> CreateAddOn(
            [FromForm] CreateAddOnRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateAddOnCommand(request),
                cancellationToken);

            if (result.IsError)
            {
                return Problem(result.Errors);
            }

            return CreatedEnvelope(
                result.Value,
                "Add-on created successfully");
        }

        // ==========================================
        // READ
        // ==========================================
        [HttpGet("{addOnId:guid}")]
        public async Task<IActionResult> GetAddOnById(
            Guid addOnId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetAddOnQuery(addOnId),
                cancellationToken);

            return result.Match<IActionResult>(
                response => OkEnvelope(
                    response,
                    "Add-on retrieved successfully"),
                Problem);
        }

        [HttpGet("~/api/foods/{foodId:guid}/addons")]
        public async Task<IActionResult> GetFoodAddOns(
            Guid foodId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(
                new GetFoodAddOnsQuery(foodId, pageNumber, pageSize),
                cancellationToken);

            return result.Match<IActionResult>(
                response => OkEnvelope(
                    response,
                    "Food add-ons retrieved successfully"),
                Problem);
        }

        // ==========================================
        // UPDATE (Partial Update)
        // ==========================================
        [HttpPatch("{addOnId:guid}")]
        public async Task<IActionResult> UpdateAddOn(
            Guid addOnId,
            [FromForm] UpdateAddOnRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateAddOnCommand(addOnId, request),
                cancellationToken);

            return result.Match<IActionResult>(
                response => OkEnvelope(
                    response,
                    "Add-on updated successfully"),
                Problem);
        }

        // ==========================================
        // DELETE
        // ==========================================
        [HttpDelete("{addOnId:guid}")]
        public async Task<IActionResult> DeleteAddOn(
            Guid addOnId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new DeleteAddOnCommand(addOnId),
                cancellationToken);

            return result.Match<IActionResult>(
                _ => OkEnvelope(
                    (object?)null,
                    "Add-on deleted successfully"),
                Problem);
        }
    }
}
