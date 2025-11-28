using FluentAssertions;
using Library.Application.Commands.Authors;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Commands.Authors;

public class CreateAuthorCommandHandlerTests
{
    private readonly Mock<IRepository<Author>> _authorRepositoryMock;
    private readonly CreateAuthorCommandHandler _handler;

    public CreateAuthorCommandHandlerTests()
    {
        _authorRepositoryMock = new Mock<IRepository<Author>>();
        _handler = new CreateAuthorCommandHandler(_authorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAuthorAndReturnId()
    {
        // Arrange
        var command = new CreateAuthorCommand(
            "John",
            "Doe",
            "A famous author"
        );

        var cancellationToken = CancellationToken.None;

        _authorRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Author>(), cancellationToken))
            .ReturnsAsync((Author a, CancellationToken ct) => a);

        _authorRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _authorRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Author>(a =>
                a.FirstName == "John" &&
                a.LastName == "Doe" &&
                a.Biography == "A famous author"
            ), cancellationToken),
            Times.Once
        );

        _authorRepositoryMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CommandWithoutBiography_ShouldCreateAuthor()
    {
        // Arrange
        var command = new CreateAuthorCommand(
            "Jane",
            "Smith",
            null
        );

        var cancellationToken = CancellationToken.None;

        _authorRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Author>(), cancellationToken))
            .ReturnsAsync((Author a, CancellationToken ct) => a);

        _authorRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _authorRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Author>(a =>
                a.FirstName == "Jane" &&
                a.LastName == "Smith" &&
                a.Biography == null
            ), cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateAuthorCommand(
            "Error",
            "Test",
            null
        );

        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _authorRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Author>(), cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
