using FluentAssertions;
using Library.Application.Queries.Readers;
using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Moq;

namespace Library.Tests.Application.Queries.Readers;

public class GetAllReadersQueryHandlerTests
{
    private readonly Mock<IRepository<Reader>> _readerRepositoryMock;
    private readonly GetAllReadersQueryHandler _handler;

    public GetAllReadersQueryHandlerTests()
    {
        _readerRepositoryMock = new Mock<IRepository<Reader>>();
        _handler = new GetAllReadersQueryHandler(_readerRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllReaders()
    {
        // Arrange
        var query = new GetAllReadersQuery();
        var cancellationToken = CancellationToken.None;

        var readers = new List<Reader>
        {
            new Reader("John", "Doe", "john@test.com", "1234567890"),
            new Reader("Jane", "Smith", "jane@test.com", "0987654321"),
            new Reader("Bob", "Johnson", "bob@test.com")
        };

        _readerRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(readers);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(readers);

        _readerRepositoryMock.Verify(
            x => x.GetAllAsync(cancellationToken),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_WhenNoReaders_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllReadersQuery();
        var cancellationToken = CancellationToken.None;

        _readerRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ReturnsAsync(new List<Reader>());

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
        var query = new GetAllReadersQuery();
        var cancellationToken = CancellationToken.None;
        var expectedException = new InvalidOperationException("Database error");

        _readerRepositoryMock
            .Setup(x => x.GetAllAsync(cancellationToken))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }
}
