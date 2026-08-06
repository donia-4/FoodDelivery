using Restaurant.Domain.Common;
using Restaurant.Domain.Results;

namespace Restaurant.Domain.DeliveryZones;

public sealed class DeliveryZone : AuditableEntity
{
    public Guid BranchId { get; private set; }
    public string ZoneName { get; private set; } = string.Empty;
    public decimal DeliveryFee { get; private set; }
    public decimal MinimumOrder { get; private set; }
    public string? PolygonGeoJson { get; private set; } // حدود المنطقة

    public Branches.Branch Branch { get; private set; } = null!;

    private DeliveryZone() { }

    private DeliveryZone(Guid id, Guid branchId, string zoneName,
        decimal deliveryFee, decimal minimumOrder, string? polygonGeoJson) : base(id)
    {
        BranchId = branchId; ZoneName = zoneName; DeliveryFee = deliveryFee;
        MinimumOrder = minimumOrder; PolygonGeoJson = polygonGeoJson;
    }

    public static Result<DeliveryZone> Create(Guid id, Guid branchId, string zoneName,
        decimal deliveryFee, decimal minimumOrder, string? polygonGeoJson = null)
    {
        if (branchId == Guid.Empty) return DeliveryZoneErrors.InvalidBranch;
        if (string.IsNullOrWhiteSpace(zoneName)) return DeliveryZoneErrors.InvalidName;
        if (deliveryFee < 0) return DeliveryZoneErrors.InvalidDeliveryFee;
        if (minimumOrder < 0) return DeliveryZoneErrors.InvalidMinimumOrder;

        return new DeliveryZone(id, branchId, zoneName.Trim(), deliveryFee, minimumOrder, polygonGeoJson);
    }

    public Result<Updated> Update(string zoneName, decimal deliveryFee, decimal minimumOrder, string? polygonGeoJson)
    {
        if (string.IsNullOrWhiteSpace(zoneName)) return DeliveryZoneErrors.InvalidName;
        if (deliveryFee < 0) return DeliveryZoneErrors.InvalidDeliveryFee;
        if (minimumOrder < 0) return DeliveryZoneErrors.InvalidMinimumOrder;

        ZoneName = zoneName.Trim(); DeliveryFee = deliveryFee;
        MinimumOrder = minimumOrder; PolygonGeoJson = polygonGeoJson;
        return Result.Updated;
    }
}