using Restaurant.Domain.Results;

namespace Restaurant.Domain.Reviews;

public static class ReviewErrors
{
    public static readonly Error InvalidRestaurant =
        Error.Validation(
            "Review.Restaurant.Required",
            "Restaurant is required.");

    public static readonly Error InvalidUser =
        Error.Validation(
            "Review.User.Required",
            "User is required.");

    public static readonly Error InvalidRating =
        Error.Validation(
            "Review.Rating.Invalid",
            "Rating must be between 1 and 5.");

    public static readonly Error AlreadyExists =
        Error.Conflict(
            "Review.AlreadyExists",
            "User has already reviewed this restaurant.");

    public static readonly Error NotFound =
        Error.NotFound(
            "Review.NotFound",
            "Review was not found.");
}