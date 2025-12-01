using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Library.Api.Requests.Authors;
using Library.Api.Responses.Common;
using Library.Domain.Entities;
using Library.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Library.IntegrationTests.Api;

public class AuthorsEndpointsTests : IntegrationTestBase
{
    public AuthorsEndpointsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task GetAllAuthors_WhenNoAuthorsExist_ReturnsEmptyList()
    {
        // Act
        var response = await HttpClient.GetAsync("/api/authors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authors = await response.Content.ReadFromJsonAsync<List<object>>();
        authors.Should().NotBeNull();
        authors.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAuthor_WithValidData_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateAuthorRequest(
            FirstName: "J.K.",
            LastName: "Rowling",
            Biography: "British author, best known for the Harry Potter series"
        );

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/authors", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CreatedResourceResponse>();
        result.Should().NotBeNull();
        result!.Id.Should().NotBeEmpty();

        // Verify in database
        var author = await DbContext.Authors
            .FirstOrDefaultAsync(a => a.Id == result.Id);
        author.Should().NotBeNull();
        author!.FirstName.Should().Be("J.K.");
        author.LastName.Should().Be("Rowling");
        author.Biography.Should().Be("British author, best known for the Harry Potter series");
    }

    [Fact]
    public async Task GetAllAuthors_WhenAuthorsExist_ReturnsAuthors()
    {
        // Arrange
        var author1 = new Author("George", "Orwell", "English novelist and essayist");
        var author2 = new Author("Jane", "Austen", "English novelist");

        DbContext.Authors.AddRange(author1, author2);
        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.GetAsync("/api/authors");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var authors = await response.Content.ReadFromJsonAsync<List<object>>();
        authors.Should().NotBeNull();
        authors.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAuthor_MultipleTimes_CreatesMultipleAuthors()
    {
        // Arrange
        var request1 = new CreateAuthorRequest("Ernest", "Hemingway", "American novelist");
        var request2 = new CreateAuthorRequest("F. Scott", "Fitzgerald", "American writer");

        // Act
        var response1 = await HttpClient.PostAsJsonAsync("/api/authors", request1);
        var response2 = await HttpClient.PostAsJsonAsync("/api/authors", request2);

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);

        var authorsInDb = await DbContext.Authors.CountAsync();
        authorsInDb.Should().Be(2);
    }
}
