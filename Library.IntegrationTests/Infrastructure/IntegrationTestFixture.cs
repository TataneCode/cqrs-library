using Microsoft.Extensions.DependencyInjection;

namespace Library.IntegrationTests.Infrastructure;

public class IntegrationTestFixture : IAsyncLifetime
{
    private DatabaseFixture _databaseFixture = null!;
    private IntegrationTestWebApplicationFactory _factory = null!;

    public HttpClient HttpClient { get; private set; } = null!;
    public IServiceProvider ServiceProvider => _factory.Services;
    public string ConnectionString => _databaseFixture.ConnectionString;

    public async Task InitializeAsync()
    {
        _databaseFixture = new DatabaseFixture();
        await _databaseFixture.InitializeAsync();

        _factory = new IntegrationTestWebApplicationFactory(_databaseFixture.ConnectionString);
        _factory.EnsureDatabaseCreated();
        HttpClient = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();
        if (_factory != null)
            await _factory.DisposeAsync();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }
}

[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<IntegrationTestFixture>
{
}
