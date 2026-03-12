using System.ComponentModel.DataAnnotations;
using OrderProcessingService.Entities;

namespace OrderProcessingService.Dtos;

public class StatusTransitionRequest
{
    [Required]
    public OrderStatus NewStatus { get; set; }

    public string? CancelReason { get; set; }
}
