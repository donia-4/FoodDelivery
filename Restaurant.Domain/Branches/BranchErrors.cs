using Restaurant.Domain.Results;

namespace Restaurant.Domain.Branches;

public static class BranchErrors
{
    public static readonly Error InvalidRestaurant = Error.Validation("Branch.Restaurant.Required", "Restaurant is required.");
    public static readonly Error InvalidName = Error.Validation("Branch.Name.Required", "Branch name is required.");
    public static readonly Error InvalidAddress = Error.Validation("Branch.Address.Required", "Branch address is required.");
    public static readonly Error InvalidPhone = Error.Validation("Branch.Phone.Required", "Branch phone is required.");
    public static readonly Error NotFound = Error.NotFound("Branch.NotFound", "Branch was not found.");
    public static readonly Error DuplicateName = Error.Conflict("Branch.Name.Duplicate", "A branch with this name already exists for this restaurant.");
}