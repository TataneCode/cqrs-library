using FluentAssertions;
using Library.Domain.Entities;

namespace Library.Tests.Domain.Entities;

public class ReaderTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateReader()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john.doe@test.com";
        var phoneNumber = "1234567890";

        // Act
        var reader = new Reader(firstName, lastName, email, phoneNumber);

        // Assert
        reader.Should().NotBeNull();
        reader.FirstName.Should().Be(firstName);
        reader.LastName.Should().Be(lastName);
        reader.Email.Should().Be(email);
        reader.PhoneNumber.Should().Be(phoneNumber);
        reader.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutPhoneNumber_ShouldCreateReader()
    {
        // Arrange
        var firstName = "Jane";
        var lastName = "Smith";
        var email = "jane.smith@test.com";

        // Act
        var reader = new Reader(firstName, lastName, email);

        // Assert
        reader.Should().NotBeNull();
        reader.FirstName.Should().Be(firstName);
        reader.LastName.Should().Be(lastName);
        reader.Email.Should().Be(email);
        reader.PhoneNumber.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyFirstName_ShouldThrowArgumentException(string? firstName)
    {
        // Arrange
        var lastName = "Doe";
        var email = "test@test.com";

        // Act
        var act = () => new Reader(firstName!, lastName, email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("First name cannot be empty*")
            .And.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyLastName_ShouldThrowArgumentException(string? lastName)
    {
        // Arrange
        var firstName = "John";
        var email = "test@test.com";

        // Act
        var act = () => new Reader(firstName, lastName!, email);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be empty*")
            .And.ParamName.Should().Be("lastName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyEmail_ShouldThrowArgumentException(string? email)
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var act = () => new Reader(firstName, lastName, email!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*")
            .And.ParamName.Should().Be("email");
    }

    [Fact]
    public void CanBorrowMoreBooks_WhenNoBooks借Borrowed_ShouldReturnTrue()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var result = reader.CanBorrowMoreBooks();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanBorrowMoreBooks_WhenAtMaximumLimit_ShouldReturnFalse()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");
        AddBorrowedBooksToReader(reader, Reader.MaxBorrowedBooks);

        // Act
        var result = reader.CanBorrowMoreBooks();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CanBorrowMoreBooks_WhenBelowMaximumLimit_ShouldReturnTrue()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");
        AddBorrowedBooksToReader(reader, 2);

        // Act
        var result = reader.CanBorrowMoreBooks();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AvailableBorrowSlots_WhenNoBorrowedBooks_ShouldReturnMaximum()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var result = reader.AvailableBorrowSlots();

        // Assert
        result.Should().Be(Reader.MaxBorrowedBooks);
    }

    [Fact]
    public void AvailableBorrowSlots_WhenSomeBorrowedBooks_ShouldReturnRemainingSlots()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");
        AddBorrowedBooksToReader(reader, 2);

        // Act
        var result = reader.AvailableBorrowSlots();

        // Assert
        result.Should().Be(1);
    }

    [Fact]
    public void AvailableBorrowSlots_WhenAllSlotsUsed_ShouldReturnZero()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");
        AddBorrowedBooksToReader(reader, Reader.MaxBorrowedBooks);

        // Act
        var result = reader.AvailableBorrowSlots();

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void UpdateDetails_ValidParameters_ShouldUpdateReaderDetails()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "old@test.com", "1111111111");
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newEmail = "new@test.com";
        var newPhoneNumber = "2222222222";

        // Act
        reader.UpdateDetails(newFirstName, newLastName, newEmail, newPhoneNumber);

        // Assert
        reader.FirstName.Should().Be(newFirstName);
        reader.LastName.Should().Be(newLastName);
        reader.Email.Should().Be(newEmail);
        reader.PhoneNumber.Should().Be(newPhoneNumber);
    }

    [Fact]
    public void UpdateDetails_WithNullPhoneNumber_ShouldUpdateDetails()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "old@test.com", "1111111111");
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newEmail = "new@test.com";

        // Act
        reader.UpdateDetails(newFirstName, newLastName, newEmail, null);

        // Assert
        reader.FirstName.Should().Be(newFirstName);
        reader.LastName.Should().Be(newLastName);
        reader.Email.Should().Be(newEmail);
        reader.PhoneNumber.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_EmptyFirstName_ShouldThrowArgumentException(string? firstName)
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var act = () => reader.UpdateDetails(firstName!, "Smith", "new@test.com", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("First name cannot be empty*")
            .And.ParamName.Should().Be("firstName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_EmptyLastName_ShouldThrowArgumentException(string? lastName)
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var act = () => reader.UpdateDetails("Jane", lastName!, "new@test.com", null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be empty*")
            .And.ParamName.Should().Be("lastName");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_EmptyEmail_ShouldThrowArgumentException(string? email)
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var act = () => reader.UpdateDetails("Jane", "Smith", email!, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*")
            .And.ParamName.Should().Be("email");
    }

    [Fact]
    public void GetFullName_ShouldReturnFormattedFullName()
    {
        // Arrange
        var reader = new Reader("John", "Doe", "test@test.com");

        // Act
        var fullName = reader.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void BorrowedBooks_InitialState_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var reader = new Reader("John", "Doe", "test@test.com");

        // Assert
        reader.BorrowedBooks.Should().NotBeNull();
        reader.BorrowedBooks.Should().BeEmpty();
    }

    [Fact]
    public void Notifications_InitialState_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var reader = new Reader("John", "Doe", "test@test.com");

        // Assert
        reader.Notifications.Should().NotBeNull();
        reader.Notifications.Should().BeEmpty();
    }

    private static void AddBorrowedBooksToReader(Reader reader, int count)
    {
        var borrowedBooksField = typeof(Reader)
            .GetField("_borrowedBooks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (borrowedBooksField != null)
        {
            var borrowedBooks = (List<Book>)borrowedBooksField.GetValue(reader)!;
            for (int i = 0; i < count; i++)
            {
                var book = new Book(
                    $"Book {i}",
                    $"ISBN{i:D13}",
                    Library.Domain.Enums.BookType.Novel,
                    DateTime.UtcNow.AddYears(-1),
                    Guid.NewGuid()
                );
                borrowedBooks.Add(book);
            }
        }
    }
}
