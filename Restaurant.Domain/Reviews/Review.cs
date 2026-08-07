using Restaurant.Domain.Common;
using Restaurant.Domain.Results;

namespace Restaurant.Domain.Reviews;

public sealed class Review : AuditableEntity
{
    public Guid RestaurantId { get; private set; }
    public Guid UserId { get; private set; }

    public int Rating { get; private set; }
    public string? Comment { get; private set; }

    public Restaurants.Restaurant Restaurant { get; private set; } = null!;

    private Review()
    {
    }

    private Review(
        Guid id,
        Guid restaurantId,
        Guid userId,
        int rating,
        string? comment)
        : base(id)
    {
        RestaurantId = restaurantId;
        UserId = userId;
        Rating = rating;
        Comment = comment;
    }

    public static Result<Review> Create(
        Guid id,
        Guid restaurantId,
        Guid userId,
        int rating,
        string? comment = null)
    {
        if (restaurantId == Guid.Empty)
            return ReviewErrors.InvalidRestaurant;

        if (userId == Guid.Empty)
            return ReviewErrors.InvalidUser;

        if (rating is < 1 or > 5)
            return ReviewErrors.InvalidRating;

        return new Review(
            id,
            restaurantId,
            userId,
            rating,
            comment?.Trim());
    }

    public Result<Updated> Update(
        int? rating = null,
        string? comment = null)
    {
        if (rating.HasValue)
        {
            if (rating.Value is < 1 or > 5)
                return ReviewErrors.InvalidRating;

            Rating = rating.Value;
        }

        if (comment is not null)
        {
            Comment = comment.Trim();
        }

        return Result.Updated;
    }
}