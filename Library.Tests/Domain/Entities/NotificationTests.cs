using FluentAssertions;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Tests.Domain.Entities;

public class NotificationTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateNotification()
    {
        // Arrange
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();
        var message = "Your book is due soon";

        // Act
        var notification = new Notification(readerId, bookId, message);

        // Assert
        notification.Should().NotBeNull();
        notification.ReaderId.Should().Be(readerId);
        notification.BookId.Should().Be(bookId);
        notification.Message.Should().Be(message);
        notification.Status.Should().Be(NotificationStatus.Pending);
        notification.SentAt.Should().BeNull();
        notification.Id.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyMessage_ShouldThrowArgumentException(string? message)
    {
        // Arrange
        var readerId = Guid.NewGuid();
        var bookId = Guid.NewGuid();

        // Act
        var act = () => new Notification(readerId, bookId, message!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Message cannot be empty*")
            .And.ParamName.Should().Be("message");
    }

    [Fact]
    public void MarkAsSent_PendingNotification_ShouldUpdateStatusAndSetSentAt()
    {
        // Arrange
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), "Test message");

        // Act
        notification.MarkAsSent();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Sent);
        notification.SentAt.Should().NotBeNull();
        notification.SentAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void MarkAsSent_AlreadySentNotification_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), "Test message");
        notification.MarkAsSent();

        // Act
        var act = () => notification.MarkAsSent();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Notification has already been sent");
    }

    [Fact]
    public void Dismiss_ShouldUpdateStatusToDismissed()
    {
        // Arrange
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), "Test message");

        // Act
        notification.Dismiss();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Dismissed);
    }

    [Fact]
    public void Dismiss_SentNotification_ShouldUpdateStatusToDismissed()
    {
        // Arrange
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), "Test message");
        notification.MarkAsSent();

        // Act
        notification.Dismiss();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Dismissed);
    }

    [Fact]
    public void Dismiss_AlreadyDismissedNotification_ShouldUpdateStatus()
    {
        // Arrange
        var notification = new Notification(Guid.NewGuid(), Guid.NewGuid(), "Test message");
        notification.Dismiss();

        // Act
        notification.Dismiss();

        // Assert
        notification.Status.Should().Be(NotificationStatus.Dismissed);
    }
}
