using OrderProcessingService.Entities;

namespace OrderProcessingService.Exceptions;

public class InvalidOrderStatusTransitionException : Exception
{
    public OrderStatus CurrentStatus { get; }
    public OrderStatus RequestedStatus { get; }

    public InvalidOrderStatusTransitionException(OrderStatus currentStatus, OrderStatus requestedStatus)
        : base($"Invalid status transition from {currentStatus} to {requestedStatus}")
    {
        CurrentStatus = currentStatus;
        RequestedStatus = requestedStatus;
    }
}
