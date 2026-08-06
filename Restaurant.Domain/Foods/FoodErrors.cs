using Restaurant.Domain.Results;

namespace Restaurant.Domain.Foods;

public static class FoodErrors
{
    public static readonly Error InvalidRestaurant = Error.Validation("Food.Restaurant.Invalid", "Restaurant is required.");
    public static readonly Error InvalidName = Error.Validation("Food.Name.Required", "Food name is required.");
    public static readonly Error InvalidPrice = Error.Validation("Food.Price.Invalid", "Food price must be zero or greater."); // BR-06
    public static readonly Error InvalidCategory = Error.Validation("Food.Category.Required", "Food category is required."); // BR-07
    public static readonly Error InActiveOrder = Error.Conflict("Food.Order.Active", "Cannot delete item used in active order."); // BR-05
    public static readonly Error NotFound = Error.NotFound("Food.NotFound", "Food was not found.");
    public static readonly Error NotAvailable = Error.Conflict("Food.NotAvailable", "This item is currently not available."); // BR-10
    public static readonly Error DuplicateName = Error.Conflict("Food.Name.Duplicate", "A menu item with this name already exists for this branch.");

}