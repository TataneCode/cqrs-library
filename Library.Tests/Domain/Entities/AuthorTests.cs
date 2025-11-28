using FluentAssertions;
using Library.Domain.Entities;

namespace Library.Tests.Domain.Entities;

public class AuthorTests
{
    [Fact]
    public void Constructor_ValidParameters_ShouldCreateAuthor()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var biography = "A famous author";

        // Act
        var author = new Author(firstName, lastName, biography);

        // Assert
        author.Should().NotBeNull();
        author.FirstName.Should().Be(firstName);
        author.LastName.Should().Be(lastName);
        author.Biography.Should().Be(biography);
        author.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_WithoutBiography_ShouldCreateAuthor()
    {
        // Arrange
        var firstName = "Jane";
        var lastName = "Smith";

        // Act
        var author = new Author(firstName, lastName);

        // Assert
        author.Should().NotBeNull();
        author.FirstName.Should().Be(firstName);
        author.LastName.Should().Be(lastName);
        author.Biography.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_EmptyFirstName_ShouldThrowArgumentException(string? firstName)
    {
        // Arrange
        var lastName = "Doe";

        // Act
        var act = () => new Author(firstName!, lastName);

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

        // Act
        var act = () => new Author(firstName, lastName!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be empty*")
            .And.ParamName.Should().Be("lastName");
    }

    [Fact]
    public void UpdateDetails_ValidParameters_ShouldUpdateAuthorDetails()
    {
        // Arrange
        var author = new Author("John", "Doe", "Old biography");
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newBiography = "New biography";

        // Act
        author.UpdateDetails(newFirstName, newLastName, newBiography);

        // Assert
        author.FirstName.Should().Be(newFirstName);
        author.LastName.Should().Be(newLastName);
        author.Biography.Should().Be(newBiography);
    }

    [Fact]
    public void UpdateDetails_WithNullBiography_ShouldUpdateAuthorDetails()
    {
        // Arrange
        var author = new Author("John", "Doe", "Old biography");
        var newFirstName = "Jane";
        var newLastName = "Smith";

        // Act
        author.UpdateDetails(newFirstName, newLastName, null);

        // Assert
        author.FirstName.Should().Be(newFirstName);
        author.LastName.Should().Be(newLastName);
        author.Biography.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDetails_EmptyFirstName_ShouldThrowArgumentException(string? firstName)
    {
        // Arrange
        var author = new Author("John", "Doe");

        // Act
        var act = () => author.UpdateDetails(firstName!, "Smith", null);

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
        var author = new Author("John", "Doe");

        // Act
        var act = () => author.UpdateDetails("Jane", lastName!, null);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("Last name cannot be empty*")
            .And.ParamName.Should().Be("lastName");
    }

    [Fact]
    public void GetFullName_ShouldReturnFormattedFullName()
    {
        // Arrange
        var author = new Author("John", "Doe");

        // Act
        var fullName = author.GetFullName();

        // Assert
        fullName.Should().Be("John Doe");
    }

    [Fact]
    public void Books_InitialState_ShouldBeEmptyCollection()
    {
        // Arrange & Act
        var author = new Author("John", "Doe");

        // Assert
        author.Books.Should().NotBeNull();
        author.Books.Should().BeEmpty();
    }
}
