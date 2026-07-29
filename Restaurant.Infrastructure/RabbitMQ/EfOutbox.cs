using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Restaurant.Application.Common.Interfaces.Messaging;
using Restaurant.Domain.Outbox;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.RabbitMQ
{
    public sealed class EfOutbox : IOutbox
    {
        private readonly RestaurantDbContext _dbContext;

        public EfOutbox(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task AddAsync<TEvent>(
            TEvent @event,
            string routingKey,
            CancellationToken cancellationToken = default)
            where TEvent : class
        {
            var message = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = typeof(TEvent).AssemblyQualifiedName!,
                Content = JsonSerializer.Serialize(@event, @event.GetType()),
                RoutingKey = routingKey,
                OccurredOnUtc = DateTime.UtcNow
            };

            _dbContext.OutboxMessages.Add(message);
            return Task.CompletedTask;
        }
    }
}
