using FluentAssertions;
using OrderProcessingService.Entities;
using OrderProcessingService.Exceptions;
using OrderProcessingService.Services;

namespace OrderProcessingApp.Tests;

public class StatusTransitionValidatorTests
{
    [Fact]
    public void IsValidTransition_PendingToConfirmed_ReturnsTrue()
    {
        // Arrange
        var currentStatus = OrderStatus.Pending;
        var newStatus = OrderStatus.Confirmed;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTransition_ConfirmedToProcessing_ReturnsTrue()
    {
        // Arrange
        var currentStatus = OrderStatus.Confirmed;
        var newStatus = OrderStatus.Processing;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTransition_ProcessingToShipped_ReturnsTrue()
    {
        // Arrange
        var currentStatus = OrderStatus.Processing;
        var newStatus = OrderStatus.Shipped;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTransition_ShippedToDelivered_ReturnsTrue()
    {
        // Arrange
        var currentStatus = OrderStatus.Shipped;
        var newStatus = OrderStatus.Delivered;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidTransition_BackwardsTransition_ReturnsFalse()
    {
        // Arrange
        var currentStatus = OrderStatus.Confirmed;
        var newStatus = OrderStatus.Pending;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTransition_SkipAhead_ReturnsFalse()
    {
        // Arrange
        var currentStatus = OrderStatus.Pending;
        var newStatus = OrderStatus.Processing;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTransition_FromTerminalState_ReturnsFalse()
    {
        // Arrange
        var currentStatus = OrderStatus.Delivered;
        var newStatus = OrderStatus.Shipped;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidTransition_PendingToCancelled_ReturnsTrue()
    {
        // Arrange
        var currentStatus = OrderStatus.Pending;
        var newStatus = OrderStatus.Cancelled;

        // Act
        var result = StatusTransitionValidator.IsValidTransition(currentStatus, newStatus);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateTransition_InvalidTransition_ThrowsException()
    {
        // Arrange
        var currentStatus = OrderStatus.Delivered;
        var newStatus = OrderStatus.Processing;

        // Act & Assert
        Assert.Throws<InvalidOrderStatusTransitionException>(() =>
        {
            StatusTransitionValidator.ValidateTransition(currentStatus, newStatus);
        });
    }

    [Fact]
    public void IsTerminalStatus_Delivered_ReturnsTrue()
    {
        // Arrange
        var status = OrderStatus.Delivered;

        // Act
        var result = StatusTransitionValidator.IsTerminalStatus(status);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTerminalStatus_Cancelled_ReturnsTrue()
    {
        // Arrange
        var status = OrderStatus.Cancelled;

        // Act
        var result = StatusTransitionValidator.IsTerminalStatus(status);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsTerminalStatus_Pending_ReturnsFalse()
    {
        // Arrange
        var status = OrderStatus.Pending;

        // Act
        var result = StatusTransitionValidator.IsTerminalStatus(status);

        // Assert
        result.Should().BeFalse();
    }
}
