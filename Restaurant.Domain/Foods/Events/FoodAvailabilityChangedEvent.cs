using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restaurant.Domain.Common;

namespace Restaurant.Domain.Foods.Events
{
    public sealed record FoodAvailabilityChangedEvent(
        Guid FoodId,
        bool IsAvailable) : DomainEvent;
}
