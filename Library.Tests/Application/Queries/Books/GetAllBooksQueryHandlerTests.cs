using FluentAssertions;
using Library.Application.Queries.Books;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Queries.Books;

public class GetAllBooksQueryHandlerTests
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock;
    private readonly GetAllBooksQueryHandler _handler;

    public GetAllBooksQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IRepository<Book>>();
        _handler = new GetAllBooksQueryHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllBooks()
    {
        // Arrange
        var query = new GetAllBooksQuery();
        var cancellationToken = CancellationToken.None;

        var books = new List<Book>
        {
            new Book("Book 1", "ISBN1", BookType.Novel, DateTime.UtcNow, Guid.NewGuid()),
            new Book("Book 2", "ISBN2", BookType.Comic, DateTime.UtcNow, Guid.NewGuid()),
            new Book("Book 3", "ISBN3", BookType.Manga, DateTime.UtcNow, Guid.NewGuid())
        };

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(books);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(books);

        _bookRepositoryMock.Verify(
            x => x.GetAllAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoBooks_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllBooksQuery();
        var cancellationToken = CancellationToken.None;

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
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
        var query = new GetAllBooksQuery();
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _bookRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
