using System.ComponentModel.DataAnnotations;
using OrderProcessingApp.Api.Entities;

namespace OrderProcessingApp.Api.Dtos;

public class StatusTransitionRequest
{
    [Required]
    public OrderStatus NewStatus { get; set; }

    public string? CancelReason { get; set; }
}
