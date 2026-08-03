using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Application.Features.DeliveryZones.Command.CreateDeliveryZone;
using Restaurant.Application.Features.DeliveryZones.Command.DeleteDeliveryZone;
using Restaurant.Application.Features.DeliveryZones.Command.UpdateDeliveryZone;
using Restaurant.Application.Features.DeliveryZones.Commands.CreateDeliveryZone;
using Restaurant.Application.Features.DeliveryZones.Dtos.UpdateDeliveryZone;
using Restaurant.Application.Features.DeliveryZones.Queries.GetBranchDeliveryZones;
using Restaurant.Application.Features.DeliveryZones.Queries.GetDeliveryZone;

namespace Restaurant.API.Controllers
{
    [Route("api/delivery-zones")]
    [Authorize]
    public sealed class DeliveryZonesController(ISender sender) : ApiController
    {
        [HttpPost]
        public async Task<IActionResult> CreateDeliveryZone(
            [FromForm] CreateDeliveryZoneRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new CreateDeliveryZoneCommand(request),
                cancellationToken);

            if (result.IsError) return Problem(result.Errors);
            return CreatedEnvelope(result.Value, "Delivery zone created successfully");
        }

        [HttpGet("{zoneId:guid}")]
        public async Task<IActionResult> GetDeliveryZone(
            Guid zoneId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new GetDeliveryZoneQuery(zoneId),
                cancellationToken);

            return result.Match<IActionResult>(
                r => OkEnvelope(r, "Delivery zone retrieved successfully"),
                Problem);
        }

        [HttpGet("~/api/branches/{branchId:guid}/delivery-zones")]
        public async Task<IActionResult> GetBranchDeliveryZones(
            Guid branchId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(
                new GetBranchDeliveryZonesQuery(
                    branchId,
                    pageNumber,
                    pageSize),
                cancellationToken);

            return result.Match<IActionResult>(
                response => OkEnvelope(
                    response,
                    "Branch delivery zones retrieved successfully"),
                Problem);
        }

        [HttpPatch("{zoneId:guid}")]
        public async Task<IActionResult> UpdateDeliveryZone(
            Guid zoneId,
            [FromForm] UpdateDeliveryZoneRequest request,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new UpdateDeliveryZoneCommand(zoneId, request),
                cancellationToken);

            return result.Match<IActionResult>(
                r => OkEnvelope(r, "Delivery zone updated successfully"),
                Problem);
        }

        [HttpDelete("{zoneId:guid}")]
        public async Task<IActionResult> DeleteDeliveryZone(
            Guid zoneId,
            CancellationToken cancellationToken)
        {
            var result = await sender.Send(
                new DeleteDeliveryZoneCommand(zoneId),
                cancellationToken);

            return result.Match<IActionResult>(
                _ => OkEnvelope((object?)null, "Delivery zone deleted successfully"),
                Problem);
        }
    }
}
