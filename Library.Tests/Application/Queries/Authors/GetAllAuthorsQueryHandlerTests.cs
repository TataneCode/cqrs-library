using FluentAssertions;
using Library.Application.Queries.Authors;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Queries.Authors;

public class GetAllAuthorsQueryHandlerTests
{
    private readonly Mock<IRepository<Author>> _authorRepositoryMock;
    private readonly GetAllAuthorsQueryHandler _handler;

    public GetAllAuthorsQueryHandlerTests()
    {
        _authorRepositoryMock = new Mock<IRepository<Author>>();
        _handler = new GetAllAuthorsQueryHandler(_authorRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllAuthors()
    {
        // Arrange
        var query = new GetAllAuthorsQuery();
        var cancellationToken = CancellationToken.None;

        var authors = new List<Author>
        {
            new Author("John", "Doe", "Biography 1"),
            new Author("Jane", "Smith", "Biography 2"),
            new Author("Bob", "Johnson")
        };

        _authorRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(authors);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(authors);

        _authorRepositoryMock.Verify(
            x => x.GetAllAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoAuthors_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllAuthorsQuery();
        var cancellationToken = CancellationToken.None;

        _authorRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<Author>());

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
        var query = new GetAllAuthorsQuery();
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _authorRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
