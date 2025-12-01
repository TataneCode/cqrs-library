using FluentAssertions;
using Library.Application.Commands.Notifications;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;
using Moq;

namespace Library.Tests.Application.Commands.Notifications;

public class DeleteNotificationCommandHandlerTests
{
    private readonly Mock<IRepository<Notification>> _notificationRepositoryMock;
    private readonly DeleteNotificationCommandHandler _handler;

    public DeleteNotificationCommandHandlerTests()
    {
        _notificationRepositoryMock = new Mock<IRepository<Notification>>();
        _handler = new DeleteNotificationCommandHandler(_notificationRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldDeleteNotificationSuccessfully()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var command = new DeleteNotificationCommand(notificationId);
        var cancellationToken = CancellationToken.None;

        var notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Test notification"
        );
        typeof(Notification).GetProperty(nameof(Notification.Id))!
            .SetValue(notification, notificationId);

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(notificationId, cancellationToken))
            .ReturnsAsync(notification);

        _notificationRepositoryMock
            .Setup(x => x.DeleteAsync(notification, cancellationToken))
            .Returns(Task.CompletedTask);

        _notificationRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);

        _notificationRepositoryMock.Verify(
            x => x.GetByIdAsync(notificationId, cancellationToken),
            Times.Once
        );

        _notificationRepositoryMock.Verify(
            x => x.DeleteAsync(notification, cancellationToken),
            Times.Once
        );

        _notificationRepositoryMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_NotificationNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var command = new DeleteNotificationCommand(notificationId);
        var cancellationToken = CancellationToken.None;

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(notificationId, cancellationToken))
            .ReturnsAsync((Notification?)null);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Notification with ID {notificationId} not found");

        _notificationRepositoryMock.Verify(
            x => x.DeleteAsync(It.IsAny<Notification>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var command = new DeleteNotificationCommand(notificationId);
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _notificationRepositoryMock
            .Setup(x => x.GetByIdAsync(notificationId, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
