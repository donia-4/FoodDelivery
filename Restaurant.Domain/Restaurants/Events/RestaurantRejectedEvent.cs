using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant.Domain.Common;

namespace Restaurant.Domian.Restaurants.Events
{
    public sealed record RestaurantRejectedEvent(
        Guid RestaurantId,
        string? Reason) : DomainEvent;
}
