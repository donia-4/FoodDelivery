using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Restaurant.Application.Common.Interfaces.Messaging;
using Restaurant.Domain.Outbox;
using Restaurant.Infrastructure.Data;

namespace Restaurant.Infrastructure.BackgroundJobs;

public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;
    private const int BatchSize = 100;

    public OutboxProcessor(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in Outbox Processor");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<RestaurantDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

        await using var transaction = await dbContext.Database
            .BeginTransactionAsync(cancellationToken);

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType is null)
                {
                    _logger.LogWarning("Could not resolve type {Type}", message.Type);
                    message.RetryCount++;
                    continue;
                }

                var @event = JsonSerializer.Deserialize(message.Content, eventType);
                if (@event is null)
                {
                    _logger.LogWarning("Failed to deserialize message {Id}", message.Id);
                    message.RetryCount++;
                    continue;
                }

                // === HERE: actually publishes to RabbitMQ ===
                await publisher.PublishAsync(@event, message.RoutingKey, cancellationToken);

                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;

                _logger.LogDebug(
                    "Published outbox message {Id} to {RoutingKey}",
                    message.Id, message.RoutingKey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to publish message {Id}, retry {RetryCount}",
                    message.Id, message.RetryCount + 1);

                message.RetryCount++;
                message.Error = ex.ToString();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}