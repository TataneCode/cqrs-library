using FluentAssertions;
using Library.Application.Commands.Readers;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Commands.Readers;

public class CreateReaderCommandHandlerTests
{
    private readonly Mock<IRepository<Reader>> _readerRepositoryMock;
    private readonly CreateReaderCommandHandler _handler;

    public CreateReaderCommandHandlerTests()
    {
        _readerRepositoryMock = new Mock<IRepository<Reader>>();
        _handler = new CreateReaderCommandHandler(_readerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateReaderAndReturnId()
    {
        // Arrange
        var command = new CreateReaderCommand(
            "John",
            "Doe",
            "john.doe@test.com",
            "1234567890"
        );

        var cancellationToken = CancellationToken.None;

        _readerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Reader>(), cancellationToken))
            .ReturnsAsync((Reader r, CancellationToken ct) => r);

        _readerRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _readerRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Reader>(r =>
                r.FirstName == "John" &&
                r.LastName == "Doe" &&
                r.Email == "john.doe@test.com" &&
                r.PhoneNumber == "1234567890"
            ), cancellationToken),
            Times.Once
        );

        _readerRepositoryMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CommandWithoutPhoneNumber_ShouldCreateReader()
    {
        // Arrange
        var command = new CreateReaderCommand(
            "Jane",
            "Smith",
            "jane.smith@test.com",
            null
        );

        var cancellationToken = CancellationToken.None;

        _readerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Reader>(), cancellationToken))
            .ReturnsAsync((Reader r, CancellationToken ct) => r);

        _readerRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _readerRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Reader>(r =>
                r.FirstName == "Jane" &&
                r.LastName == "Smith" &&
                r.Email == "jane.smith@test.com" &&
                r.PhoneNumber == null
            ), cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateReaderCommand(
            "Error",
            "Test",
            "error@test.com",
            null
        );

        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _readerRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Reader>(), cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
