using MediatR;

namespace HarmoniHSE360.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}