using Restaurant.Domain.Results;

namespace Restaurant.Domain.AddOns;

public static class AddOnErrors
{
    public static readonly Error InvalidFood = Error.Validation("AddOn.Food.Required", "Food item is required.");
    public static readonly Error InvalidName = Error.Validation("AddOn.Name.Required", "Add-on name is required.");
    public static readonly Error InvalidPrice = Error.Validation("AddOn.Price.Invalid", "Add-on price must be zero or greater.");
    public static readonly Error NotFound = Error.NotFound("AddOn.NotFound", "Add-on was not found.");
}