using Restaurant.Domain.Common;
using Restaurant.Domain.DeliveryZones;
using Restaurant.Domain.Results;
using Restaurant.Domain.WorkingHours;

namespace Restaurant.Domain.Branches;

public sealed class Branch : AuditableEntity
{
    private readonly List<WorkingHour> _workingHours = [];
    private readonly List<DeliveryZone> _deliveryZones = [];

    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public string Phone { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    public Restaurants.Restaurant Restaurant { get; private set; } = null!;
    public IReadOnlyCollection<WorkingHour> WorkingHours => _workingHours;
    public IReadOnlyCollection<DeliveryZone> DeliveryZones => _deliveryZones;

    private Branch() { }

    private Branch(Guid id, Guid restaurantId, string name, string address,
        decimal latitude, decimal longitude, string phone) : base(id)
    {
        RestaurantId = restaurantId; Name = name; Address = address;
        Latitude = latitude; Longitude = longitude; Phone = phone;
        IsActive = true;
    }

    public static Result<Branch> Create(Guid id, Guid restaurantId, string name,
        string address, decimal latitude, decimal longitude, string phone)
    {
        if (restaurantId == Guid.Empty) return BranchErrors.InvalidRestaurant;
        if (string.IsNullOrWhiteSpace(name)) return BranchErrors.InvalidName;
        if (string.IsNullOrWhiteSpace(address)) return BranchErrors.InvalidAddress;
        if (string.IsNullOrWhiteSpace(phone)) return BranchErrors.InvalidPhone;

        return new Branch(id, restaurantId, name.Trim(), address.Trim(), latitude, longitude, phone.Trim());
    }

    public Result<Updated> Update(string? name,string? address,decimal? latitude,decimal? longitude,string? phone)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BranchErrors.InvalidName;

            Name = name.Trim();
        }

        if (address is not null)
        {
            if (string.IsNullOrWhiteSpace(address))
                return BranchErrors.InvalidAddress;

            Address = address.Trim();
        }

        if (latitude.HasValue)
        {
            Latitude = latitude.Value;
        }

        if (longitude.HasValue)
        {
            Longitude = longitude.Value;
        }

        if (phone is not null)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return BranchErrors.InvalidPhone;

            Phone = phone.Trim();
        }

        return Result.Updated;
    }

    public Result<Updated> Activate() { IsActive = true; return Result.Updated; }
    public Result<Updated> Deactivate() { IsActive = false; return Result.Updated; }

    public Result<Updated> AddWorkingHour(WorkingHour wh) { _workingHours.Add(wh); return Result.Updated; }
    public Result<Updated> ReplaceWorkingHours(IReadOnlyCollection<WorkingHour> workingHours)
    {
        ArgumentNullException.ThrowIfNull(workingHours);

        _workingHours.Clear();
        _workingHours.AddRange(workingHours);

        return Result.Updated;
    }
    public Result<Updated> AddDeliveryZone(DeliveryZone zone) { _deliveryZones.Add(zone); return Result.Updated; }
}