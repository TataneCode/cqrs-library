using FluentAssertions;
using Library.Application.Queries.Books;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Queries.Books;

public class GetAvailableBooksQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly GetAvailableBooksQueryHandler _handler;

    public GetAvailableBooksQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _handler = new GetAvailableBooksQueryHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAvailableBooks()
    {
        // Arrange
        var query = new GetAvailableBooksQuery();
        var cancellationToken = CancellationToken.None;

        var availableBooks = new List<Book>
        {
            new Book("Available Book 1", "ISBN1", BookType.Novel, DateTime.UtcNow, Guid.NewGuid()),
            new Book("Available Book 2", "ISBN2", BookType.Comic, DateTime.UtcNow, Guid.NewGuid())
        };

        _bookRepositoryMock
            .Setup(x => x.GetAvailableBooksAsync(cancellationToken))
            .ReturnsAsync(availableBooks);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(availableBooks);

        _bookRepositoryMock.Verify(
            x => x.GetAvailableBooksAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoAvailableBooks_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAvailableBooksQuery();
        var cancellationToken = CancellationToken.None;

        _bookRepositoryMock
            .Setup(x => x.GetAvailableBooksAsync(cancellationToken))
            .ReturnsAsync(new List<Book>());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var query = new GetAvailableBooksQuery();
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _bookRepositoryMock
            .Setup(x => x.GetAvailableBooksAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
