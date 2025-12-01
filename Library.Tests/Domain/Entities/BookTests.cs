using FluentAssertions;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Tests.Domain.Entities;

public class BookTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateBook()
    {
        // Arrange
        var title = "The Great Gatsby";
        var isbn = "9780743273565";
        var type = BookType.Novel;
        var publishedDate = new DateTime(1925, 4, 10);
        var authorId = Guid.NewGuid();
        var description = "A classic American novel";

        // Act
        var book = new Book(title, isbn, type, publishedDate, authorId, description);

        // Assert
        book.Should().NotBeNull();
        book.Title.Should().Be(title);
        book.ISBN.Should().Be(isbn);
        book.Type.Should().Be(type);
        book.PublishedDate.Should().Be(publishedDate);
        book.AuthorId.Should().Be(authorId);
        book.Description.Should().Be(description);
        book.Id.Should().NotBeEmpty();
        book.IsAvailable().Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithoutDescription_ShouldCreateBook()
    {
        // Arrange
        var title = "The Great Gatsby";
        var isbn = "9780743273565";
        var type = BookType.Novel;
        var publishedDate = new DateTime(1925, 4, 10);
        var authorId = Guid.NewGuid();

        // Act
        var book = new Book(title, isbn, type, publishedDate, authorId);

        // Assert
        book.Should().NotBeNull();
        book.Title.Should().Be(title);
        book.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyTitle_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var isbn = "9780743273565";
        var type = BookType.Novel;
        var publishedDate = DateTime.UtcNow;
        var authorId = Guid.NewGuid();

        // Act
        var act = () => new Book(title!, isbn, type, publishedDate, authorId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty*")
            .And.ParamName.Should().Be("title");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyISBN_ShouldThrowArgumentException(string? isbn)
    {
        // Arrange
        var title = "The Great Gatsby";
        var type = BookType.Novel;
        var publishedDate = DateTime.UtcNow;
        var authorId = Guid.NewGuid();

        // Act
        var act = () => new Book(title, isbn!, type, publishedDate, authorId);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("ISBN cannot be empty*")
            .And.ParamName.Should().Be("isbn");
    }

    [Fact]
    public void IsAvailable_WhenNotBorrowed_ShouldReturnTrue()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var result = book.IsAvailable();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsAvailable_WhenBorrowed_ShouldReturnFalse()
    {
        // Arrange
        var book = CreateTestBook();
        var readerId = Guid.NewGuid();
        book.Borrow(readerId);

        // Act
        var result = book.IsAvailable();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Borrow_AvailableBook_ShouldSetBorrowDetails()
    {
        // Arrange
        var book = CreateTestBook();
        var readerId = Guid.NewGuid();
        var borrowDurationDays = 14;

        // Act
        book.Borrow(readerId, borrowDurationDays);

        // Assert
        book.BorrowedByReaderId.Should().Be(readerId);
        book.BorrowedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        book.DueDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(borrowDurationDays), TimeSpan.FromSeconds(5));
        book.IsAvailable().Should().BeFalse();
    }

    [Fact]
    public void Borrow_AlreadyBorrowedBook_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var book = CreateTestBook();
        var firstReaderId = Guid.NewGuid();
        var secondReaderId = Guid.NewGuid();
        book.Borrow(firstReaderId);

        // Act
        var act = () => book.Borrow(secondReaderId);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*is already borrowed");
    }

    [Fact]
    public void Return_BorrowedBook_ShouldClearBorrowDetails()
    {
        // Arrange
        var book = CreateTestBook();
        var readerId = Guid.NewGuid();
        book.Borrow(readerId);

        // Act
        book.Return();

        // Assert
        book.BorrowedByReaderId.Should().BeNull();
        book.BorrowedByReader.Should().BeNull();
        book.BorrowedAt.Should().BeNull();
        book.DueDate.Should().BeNull();
        book.IsAvailable().Should().BeTrue();
    }

    [Fact]
    public void Return_NotBorrowedBook_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var act = () => book.Return();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*is not currently borrowed");
    }

    [Fact]
    public void UpdateDetails_ValidParameters_ShouldUpdateBookDetails()
    {
        // Arrange
        var book = CreateTestBook();
        var newTitle = "Updated Title";
        var newDescription = "Updated description";
        var newType = BookType.Comic;
        var newPublishedDate = DateTime.UtcNow.AddYears(-2);

        // Act
        book.UpdateDetails(newTitle, newDescription, newType, newPublishedDate);

        // Assert
        book.Title.Should().Be(newTitle);
        book.Description.Should().Be(newDescription);
        book.Type.Should().Be(newType);
        book.PublishedDate.Should().Be(newPublishedDate);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_EmptyTitle_ShouldThrowArgumentException(string? title)
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var act = () => book.UpdateDetails(title!, "Description", BookType.Novel, DateTime.UtcNow);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty*")
            .And.ParamName.Should().Be("title");
    }

    [Fact]
    public void IsOverdue_BookNotBorrowed_ShouldReturnFalse()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var result = book.IsOverdue();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_BookDueDateInFuture_ShouldReturnFalse()
    {
        // Arrange
        var book = CreateTestBook();
        var readerId = Guid.NewGuid();
        book.Borrow(readerId, 14);

        // Act
        var result = book.IsOverdue();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_BookDueDateInPast_ShouldReturnTrue()
    {
        // Arrange
        var book = CreateTestBook();
        var readerId = Guid.NewGuid();
        book.Borrow(readerId, 0);

        // Wait a bit to ensure the due date is in the past
        Thread.Sleep(10);

        // Act
        var result = book.IsOverdue();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Notifications_InitialState_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var book = CreateTestBook();

        // Assert
        book.Notifications.Should().NotBeNull();
        book.Notifications.Should().BeEmpty();
    }

    private static Book CreateTestBook()
    {
        return new Book(
            "Test Book",
            "9780123456789",
            BookType.Novel,
            DateTime.UtcNow.AddYears(-1),
            Guid.NewGuid(),
            "Test description"
        );
    }
}
