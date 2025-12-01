using FluentAssertions;
using Library.Application.Commands.Books;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Commands.Books;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock;
    private readonly Mock<IRepository<Author>> _authorRepositoryMock;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IRepository<Book>>();
        _authorRepositoryMock = new Mock<IRepository<Author>>();
        _handler = new CreateBookCommandHandler(
            _bookRepositoryMock.Object,
            _authorRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateBookAndReturnId()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateBookCommand(
            "The Great Gatsby",
            "9780743273565",
            BookType.Novel,
            new DateTime(1925, 4, 10),
            authorId,
            "A classic American novel"
        );

        var cancellationToken = CancellationToken.None;
        var author = new Author("F. Scott", "Fitzgerald", "American novelist");
        typeof(Author).GetProperty(nameof(Author.Id))!.SetValue(author, authorId);

        _authorRepositoryMock
            .Setup(x => x.GetByIdAsync(authorId, cancellationToken))
            .ReturnsAsync(author);

        _bookRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Book>(), cancellationToken))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _bookRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _authorRepositoryMock.Verify(
            x => x.GetByIdAsync(authorId, cancellationToken),
            Times.Once
        );

        _bookRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Book>(b =>
                b.Title == "The Great Gatsby" &&
                b.ISBN == "9780743273565" &&
                b.Type == BookType.Novel &&
                b.AuthorId == authorId &&
                b.Description == "A classic American novel"
            ), cancellationToken),
            Times.Once
        );

        _bookRepositoryMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_CommandWithoutDescription_ShouldCreateBook()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateBookCommand(
            "Test Book",
            "9780123456789",
            BookType.Comic,
            DateTime.UtcNow,
            authorId,
            null
        );

        var cancellationToken = CancellationToken.None;
        var author = new Author("Test", "Author");
        typeof(Author).GetProperty(nameof(Author.Id))!.SetValue(author, authorId);

        _authorRepositoryMock
            .Setup(x => x.GetByIdAsync(authorId, cancellationToken))
            .ReturnsAsync(author);

        _bookRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Book>(), cancellationToken))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _bookRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeEmpty();

        _bookRepositoryMock.Verify(
            x => x.AddAsync(It.Is<Book>(b =>
                b.Title == "Test Book" &&
                b.Description == null
            ), cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_AuthorNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateBookCommand(
            "Test Book",
            "9780123456789",
            BookType.Novel,
            DateTime.UtcNow,
            authorId,
            null
        );

        var cancellationToken = CancellationToken.None;

        _authorRepositoryMock
            .Setup(x => x.GetByIdAsync(authorId, cancellationToken))
            .ReturnsAsync((Author?)null);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Author with ID {authorId} not found");

        _bookRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var command = new CreateBookCommand(
            "Test Book",
            "9780123456789",
            BookType.Novel,
            DateTime.UtcNow,
            authorId,
            null
        );

        var cancellationToken = CancellationToken.None;
        var author = new Author("Test", "Author");
        var expectedException = new InvalidOperationException("Database error");

        _authorRepositoryMock
            .Setup(x => x.GetByIdAsync(authorId, cancellationToken))
            .ReturnsAsync(author);

        _bookRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Book>(), cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
