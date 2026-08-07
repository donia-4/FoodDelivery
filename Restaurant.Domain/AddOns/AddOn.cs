using Restaurant.Domain.Common;
using Restaurant.Domain.Results;

namespace Restaurant.Domain.AddOns;

public sealed class AddOn : AuditableEntity
{
    public Guid FoodId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public bool IsRequired { get; private set; } // FR-07: إجباري/اختياري
    public int? MaxQuantity { get; private set; }

    public Foods.Food Food { get; private set; } = null!;

    private AddOn() { }

    private AddOn(Guid id, Guid foodId, string name, decimal price, bool isRequired, int? maxQuantity) : base(id)
    {
        FoodId = foodId; Name = name; Price = price; IsRequired = isRequired; MaxQuantity = maxQuantity;
    }

    public static Result<AddOn> Create(Guid id, Guid foodId, string name, decimal price, bool isRequired = false, int? maxQuantity = null)
    {
        if (foodId == Guid.Empty) return AddOnErrors.InvalidFood;
        if (string.IsNullOrWhiteSpace(name)) return AddOnErrors.InvalidName;
        if (price < 0) return AddOnErrors.InvalidPrice;

        return new AddOn(id, foodId, name.Trim(), price, isRequired, maxQuantity);
    }

    public Result<Updated> Update(string name, decimal price, bool isRequired, int? maxQuantity)
    {
        if (string.IsNullOrWhiteSpace(name)) return AddOnErrors.InvalidName;
        if (price < 0) return AddOnErrors.InvalidPrice;

        Name = name.Trim(); Price = price; IsRequired = isRequired; MaxQuantity = maxQuantity;
        return Result.Updated;
    }
}