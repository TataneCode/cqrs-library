using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Library.Api.Requests.Books;
using Library.Api.Responses.Common;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Library.IntegrationTests.Api;

public class BooksEndpointsTests : IntegrationTestBase
{
    public BooksEndpointsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAllBooks_WhenNoBooksExist_ReturnsEmptyList()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<object>>();
        books.Should().NotBeNull();
        books.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateBook_WithValidData_ReturnsCreatedResult()
    {
        // Arrange - Create an author first
        var author = new Author("J.R.R.", "Tolkien", "Author of The Lord of the Rings");
        DbContext.Authors.Add(author);
        await DbContext.SaveChangesAsync();

        var request = new CreateBookRequest(
            Title: "The Hobbit",
            ISBN: "978-0-547-92822-7",
            Type: BookType.Novel,
            PublishedDate: DateTime.SpecifyKind(new DateTime(1937, 9, 21), DateTimeKind.Utc),
            AuthorId: author.Id,
            Description: "A fantasy novel and children's book"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreatedResourceResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Verify in database
        var book = await DbContext.Books
            .FirstOrDefaultAsync(b => b.Id == result.Id);
        book.Should().NotBeNull();
        book!.Title.Should().Be("The Hobbit");
        book.AuthorId.Should().Be(author.Id);
    }

    [Fact]
    public async Task GetAllBooks_WhenBooksExist_ReturnsBooks()
    {
        // Arrange
        var author = new Author("Isaac", "Asimov", "Science fiction author");

        var book1 = new Book(
            title: "Foundation",
            isbn: "978-0-553-29335-0",
            type: BookType.Novel,
            publishedDate: DateTime.SpecifyKind(new DateTime(1951, 6, 1), DateTimeKind.Utc),
            authorId: author.Id
        );

        var book2 = new Book(
            title: "I, Robot",
            isbn: "978-0-553-38256-6",
            type: BookType.Novel,
            publishedDate: DateTime.SpecifyKind(new DateTime(1950, 12, 2), DateTimeKind.Utc),
            authorId: author.Id
        );

        DbContext.Authors.Add(author);
        DbContext.Books.AddRange(book1, book2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/books");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var books = await response.Content.ReadFromJsonAsync<List<object>>();
        books.Should().NotBeNull();
        books.Should().HaveCount(2);
    }
}
