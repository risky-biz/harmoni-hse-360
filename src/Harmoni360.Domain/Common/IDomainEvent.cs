using MediatR;

namespace Harmoni360.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}