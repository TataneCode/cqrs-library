using FluentAssertions;
using Library.Application.Commands.Books;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;
using Moq;

namespace Library.Tests.Application.Commands.Books;

public class BorrowBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<IRepository<Reader>> _readerRepositoryMock;
    private readonly Mock<IRepository<Notification>> _notificationRepositoryMock;
    private readonly BorrowBookCommandHandler _handler;

    public BorrowBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _readerRepositoryMock = new Mock<IRepository<Reader>>();
        _notificationRepositoryMock = new Mock<IRepository<Notification>>();
        _handler = new BorrowBookCommandHandler(
            _bookRepositoryMock.Object,
            _readerRepositoryMock.Object,
            _notificationRepositoryMock.Object
        );
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldBorrowBookSuccessfully()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, readerId);
        var cancellationToken = CancellationToken.None;

        var book = CreateMockBook(bookId);
        var reader = CreateMockReader(readerId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        _readerRepositoryMock
            .Setup(x => x.GetByIdAsync(readerId, cancellationToken))
            .ReturnsAsync(reader);

        _bookRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<Book>());

        _notificationRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<Notification>());

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

        _bookRepositoryMock.Verify(
            x => x.GetByIdAsync(bookId, cancellationToken),
            Times.Once
        );

        _readerRepositoryMock.Verify(
            x => x.GetByIdAsync(readerId, cancellationToken),
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
        var readerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, readerId);
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
    public async Task Handle_ReaderNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, readerId);
        var cancellationToken = CancellationToken.None;

        var book = CreateMockBook(bookId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        _readerRepositoryMock
            .Setup(x => x.GetByIdAsync(readerId, cancellationToken))
            .ReturnsAsync((Reader?)null);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Reader with ID {readerId} not found");

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ReaderHasMaximumBorrowedBooks_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, readerId);
        var cancellationToken = CancellationToken.None;

        var book = CreateMockBook(bookId);
        var reader = CreateMockReader(readerId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        _readerRepositoryMock
            .Setup(x => x.GetByIdAsync(readerId, cancellationToken))
            .ReturnsAsync(reader);

        var borrowedBooks = Enumerable.Range(0, Reader.MaxBorrowedBooks)
            .Select(_ => CreateMockBook(Guid.NewGuid()));
        _bookRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>(), cancellationToken))
            .ReturnsAsync(borrowedBooks);

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Reader has reached the maximum limit of {Reader.MaxBorrowedBooks} borrowed books");

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_BookHasPendingNotification_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var readerId = Guid.NewGuid();
        var command = new BorrowBookCommand(bookId, readerId);
        var cancellationToken = CancellationToken.None;

        var book = CreateMockBook(bookId);
        var reader = CreateMockReader(readerId);

        _bookRepositoryMock
            .Setup(x => x.GetByIdAsync(bookId, cancellationToken))
            .ReturnsAsync(book);

        _readerRepositoryMock
            .Setup(x => x.GetByIdAsync(readerId, cancellationToken))
            .ReturnsAsync(reader);

        _bookRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Book, bool>>>(), cancellationToken))
            .ReturnsAsync(Enumerable.Empty<Book>());

        var notification = new Notification(readerId, bookId, "Overdue book notification");
        _notificationRepositoryMock
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Notification, bool>>>(), cancellationToken))
            .ReturnsAsync(new[] { notification });

        // Act
        var act = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Book '{book.Title}' has pending return notifications and cannot be borrowed until they are resolved");

        _bookRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static Book CreateMockBook(Guid bookId)
    {
        var authorId = Guid.NewGuid();
        var book = new Book(
            "Test Book",
            "1234567890123",
            Library.Domain.Enums.BookType.Novel,
            DateTime.UtcNow.AddYears(-1),
            authorId
        );
        typeof(Book).GetProperty(nameof(Book.Id))!
            .SetValue(book, bookId);
        return book;
    }

    private static Reader CreateMockReader(Guid readerId)
    {
        var reader = new Reader(
            "Test",
            "Reader",
            $"reader{readerId}@test.com",
            "1234567890"
        );

        typeof(Reader).GetProperty(nameof(Reader.Id))!
            .SetValue(reader, readerId);

        return reader;
    }
}
