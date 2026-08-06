using Restaurant.Domain.Results;

namespace Restaurant.Domain.Categories;

public static class CategoryErrors
{
    public static readonly Error InvalidRestaurant = Error.Validation("Category.Restaurant.Required", "Restaurant is required.");
    public static readonly Error InvalidName = Error.Validation("Category.Name.Required", "Category name is required.");
    public static readonly Error HasItems = Error.Conflict("Category.Items.Exists", "Cannot delete category containing items."); // BR-04
    public static readonly Error NotFound = Error.NotFound("Category.NotFound", "Category was not found.");
    public static readonly Error DuplicateName = Error.Conflict("Category.Name.Duplicate", "A category with this name already exists for this restaurant.");

}