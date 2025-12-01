using Library.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace Library.IntegrationTests.Infrastructure;

[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceScope _scope;
    private Respawner _respawner = null!;
    private NpgsqlConnection _dbConnection = null!;

    protected HttpClient HttpClient { get; }
    protected LibraryDbContext DbContext { get; }
    protected IServiceProvider Services { get; }

    protected IntegrationTestBase(IntegrationTestFixture fixture)
    {
        HttpClient = fixture.HttpClient;
        _scope = fixture.ServiceProvider.CreateScope();
        Services = _scope.ServiceProvider;
        DbContext = Services.GetRequiredService<LibraryDbContext>();
        _dbConnection = new NpgsqlConnection(fixture.ConnectionString);
    }

    public async Task InitializeAsync()
    {
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = ["public"],
            TablesToIgnore = ["__EFMigrationsHistory"]
        });
    }

    public async Task DisposeAsync()
    {
        await ResetDatabaseAsync();
        await _dbConnection.CloseAsync();
        await _dbConnection.DisposeAsync();
        _scope.Dispose();
    }

    protected async Task ResetDatabaseAsync()
    {
        await _respawner.ResetAsync(_dbConnection);
    }
}
