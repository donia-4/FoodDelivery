using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Restaurant.Application.Common.Interfaces.Messaging
{
    public interface IOutbox
    {
        Task AddAsync<TEvent>(
            TEvent @event,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TEvent : class;
    }
}
