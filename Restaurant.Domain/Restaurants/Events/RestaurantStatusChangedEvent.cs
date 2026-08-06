using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant.Domain.Common;
using Restaurant.Domain.Restaurants.Enums;

namespace Restaurant.Domian.Restaurants.Events
{
    public sealed record RestaurantStatusChangedEvent(
        Guid RestaurantId,
        RestaurantStatus NewStatus,
        RestaurantStatus OldStatus) : DomainEvent;
}
