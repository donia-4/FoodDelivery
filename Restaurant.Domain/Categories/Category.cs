using Restaurant.Domain.Common;
using Restaurant.Domain.Foods;
using Restaurant.Domain.Results;

namespace Restaurant.Domain.Categories;

public sealed class Category : AuditableEntity
{
    private readonly List<Food> _foods = [];

    public Guid RestaurantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; } // FR-05: إعادة الترتيب

    public Restaurants.Restaurant Restaurant { get; private set; } = null!;
    public IReadOnlyCollection<Food> Foods => _foods;

    private Category() { }

    private Category(Guid id, Guid restaurantId, string name, int displayOrder) : base(id)
    {
        RestaurantId = restaurantId; Name = name; DisplayOrder = displayOrder;
    }

    public static Result<Category> Create(Guid id, Guid restaurantId, string name, int displayOrder = 0)
    {
        if (restaurantId == Guid.Empty) return CategoryErrors.InvalidRestaurant;
        if (string.IsNullOrWhiteSpace(name)) return CategoryErrors.InvalidName;
        return new Category(id, restaurantId, name.Trim(), displayOrder);
    }

    public Result<Updated> Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return CategoryErrors.InvalidName;
        Name = name.Trim(); return Result.Updated;
    }
    public Result<Deleted> CanDelete()
    {
        if (_foods.Count > 0)
            return CategoryErrors.HasItems;

        return Result.Deleted;
    }
    public Result<Updated> Reorder(int displayOrder) { DisplayOrder = displayOrder; return Result.Updated; }
}