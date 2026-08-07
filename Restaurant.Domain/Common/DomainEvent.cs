using MediatR;

namespace Restaurant.Domain.Common;

public abstract record DomainEvent : INotification;