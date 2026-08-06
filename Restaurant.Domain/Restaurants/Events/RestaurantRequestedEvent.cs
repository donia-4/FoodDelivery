using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant.Domain.Common;

namespace Restaurant.Domain.Restaurants.Events
{
    public sealed record RestaurantRequestedEvent(
        Guid RestaurantId,
        Guid OwnerId,
        string Name) : DomainEvent;
}
