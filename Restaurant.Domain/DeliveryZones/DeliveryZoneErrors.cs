using Restaurant.Domain.Results;

namespace Restaurant.Domain.DeliveryZones;

public static class DeliveryZoneErrors
{
    public static readonly Error InvalidBranch = Error.Validation("DeliveryZone.Branch.Required", "Branch is required.");
    public static readonly Error InvalidName = Error.Validation("DeliveryZone.Name.Required", "Zone name is required.");
    public static readonly Error InvalidDeliveryFee = Error.Validation("DeliveryZone.Fee.Invalid", "Delivery fee must be zero or greater.");
    public static readonly Error InvalidMinimumOrder = Error.Validation("DeliveryZone.MinimumOrder.Invalid", "Minimum order must be zero or greater.");
    public static readonly Error NotFound = Error.NotFound("DeliveryZone.NotFound", "Delivery zone was not found.");
    public static readonly Error DuplicateName = Error.Conflict("DeliveryZone.Name.Duplicate", "A delivery zone with this name already exists for this branch.");
}