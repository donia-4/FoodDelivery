using Restaurant.Domain.Results;

namespace Restaurant.Domain.Restaurants;

public static class RestaurantErrors
{
    public static readonly Error InvalidOwner = Error.Validation("Restaurant.Owner.Invalid", "Restaurant owner is required.");
    public static readonly Error InvalidName = Error.Validation("Restaurant.Name.Required", "Restaurant name is required."); // BR-01
    public static readonly Error InvalidPhone = Error.Validation("Restaurant.Phone.Required", "Restaurant phone is required.");
    public static readonly Error InvalidEmail = Error.Validation("Restaurant.Email.Required", "Restaurant email is required.");
    public static readonly Error InvalidAddress = Error.Validation("Restaurant.Address.Required", "Restaurant address is required.");
    public static readonly Error AlreadyApproved = Error.Conflict("Restaurant.AlreadyApproved", "Restaurant is already approved.");
    public static readonly Error NotApproved = Error.Conflict("Restaurant.NotApproved", "Restaurant must be approved first.");
    public static readonly Error NotFound = Error.NotFound("Restaurant.NotFound", "Restaurant was not found.");
    public static readonly Error CannotRemoveLastBranch = Error.Conflict("Restaurant.Branch.LastBranch", "Cannot remove the last branch."); // BR-08
    public static readonly Error HasActiveOrders = Error.Conflict("Restaurant.Orders.Active", "Cannot delete restaurant with active orders."); // BR-03
    public static readonly Error Unauthorized = Error.Unauthorized("Restaurant.Unauthorized", "You are not authorized to manage this restaurant.");
    public static readonly Error InvalidReviewStatus = Error.Conflict(code: "Restaurant.Status.InvalidReviewStatus",description: "Restaurant cannot be reviewed in its current status.");
    public static readonly Error DuplicateName = Error.Conflict("Restaurant.Name.Duplicate", "A restaurant with this name already exists.");

}