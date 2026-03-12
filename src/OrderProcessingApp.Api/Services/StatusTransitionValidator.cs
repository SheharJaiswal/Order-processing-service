using OrderProcessingService.Entities;
using OrderProcessingService.Exceptions;

namespace OrderProcessingService.Services;

public static class StatusTransitionValidator
{
    // Define valid state transitions
    private static readonly Dictionary<OrderStatus, HashSet<OrderStatus>> ValidTransitions = new()
    {
        { OrderStatus.Pending, new HashSet<OrderStatus> { OrderStatus.Confirmed, OrderStatus.Cancelled } },
        { OrderStatus.Confirmed, new HashSet<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
        { OrderStatus.Processing, new HashSet<OrderStatus> { OrderStatus.Shipped, OrderStatus.Cancelled } },
        { OrderStatus.Shipped, new HashSet<OrderStatus> { OrderStatus.Delivered, OrderStatus.Cancelled } },
        { OrderStatus.Delivered, new HashSet<OrderStatus>() }, // Terminal state - no transitions allowed
        { OrderStatus.Cancelled, new HashSet<OrderStatus>() }  // Terminal state - no transitions allowed
    };

    public static bool IsValidTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (!ValidTransitions.ContainsKey(currentStatus))
        {
            return false;
        }

        return ValidTransitions[currentStatus].Contains(newStatus);
    }

    public static void ValidateTransition(OrderStatus currentStatus, OrderStatus newStatus)
    {
        if (!IsValidTransition(currentStatus, newStatus))
        {
            throw new InvalidOrderStatusTransitionException(currentStatus, newStatus);
        }
    }

    public static bool IsTerminalStatus(OrderStatus status)
    {
        return status == OrderStatus.Delivered || status == OrderStatus.Cancelled;
    }
}
