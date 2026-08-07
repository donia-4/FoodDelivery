using Restaurant.Domain.Common;

namespace Restaurant.Domain.Foods.Events;

public sealed record FoodCreatedEvent
    (Guid FoodId, Guid RestaurantId, Guid CategoryId, string Name, decimal Price) : DomainEvent;