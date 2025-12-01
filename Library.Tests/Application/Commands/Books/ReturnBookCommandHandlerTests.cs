using FluentAssertions;
using Library.Application.Commands.Books;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Repositories;
using MediatR;
using Moq;

namespace Library.Tests.Application.Commands.Books;

public class ReturnBookCommandHandlerTests
{
    private readonly Mock<IRepository<Book>> _bookRepositoryMock;
    private readonly ReturnBookCommandHandler _handler;

    public ReturnBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IRepository<Book>>();
        _handler = new ReturnBookCommandHandler(_bookRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnBookSuccessfully()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId);
        var cancellationToken = CancellationToken.None;

        var book = CreateBorrowedBook(bookId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        _bookRepositoryMock
            .Setup(x => x.UpdateAsync(book, cancellationToken))
            .Returns(Task.CompletedTask);

        _bookRepositoryMock
            .Setup(x => x.SaveChangesAsync(cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().Be(Unit.Value);
        book.IsAvailable().Should().BeTrue();

        _bookRepositoryMock.Verify(
            x => x.GetByIdAsync(bookId, cancellationToken),
            Times.Once
        );

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(book, cancellationToken),
            Times.Once
        );

        _bookRepositoryMock.Verify(
            x => x.SaveChangesAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_BookNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId);
        var cancellationToken = CancellationToken.None;

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync((Book?)null);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Book with ID {bookId} not found");

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_BookNotBorrowed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId);
        var cancellationToken = CancellationToken.None;

        var book = CreateAvailableBook(bookId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*is not currently borrowed");

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var command = new ReturnBookCommand(bookId);
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    private static Book CreateBorrowedBook(Guid bookId)
    {
        var book = new Book(
            "Test Book",
            "1234567890123",
            BookType.Novel,
            DateTime.UtcNow.AddYears(-1),
            Guid.NewGuid()
        );
        typeof(Book).GetProperty(nameof(Book.Id))!
            .SetValue(book, bookId);

        // Borrow the book
        book.Borrow(Guid.NewGuid());

        return book;
    }

    private static Book CreateAvailableBook(Guid bookId)
    {
        var book = new Book(
            "Test Book",
            "1234567890123",
            BookType.Novel,
            DateTime.UtcNow.AddYears(-1),
            Guid.NewGuid()
        );
        typeof(Book).GetProperty(nameof(Book.Id))!
            .SetValue(book, bookId);

        return book;
    }
}
