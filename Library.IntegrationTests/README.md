# Library Integration Tests

This project contains integration tests for the Library API using xUnit, Testcontainers, and Respawn.

## Overview

The integration tests verify the API endpoints and database interactions using:

- **xUnit**: Testing framework
- **Testcontainers**: Provides isolated PostgreSQL containers for each test run
- **Respawn**: Resets the database state between tests to ensure test isolation
- **FluentAssertions**: Provides readable assertions
- **Microsoft.AspNetCore.Mvc.Testing**: WebApplicationFactory for testing ASP.NET Core applications

## Test Infrastructure

### Key Components

- **DatabaseFixture**: Manages the PostgreSQL Testcontainer lifecycle
- **IntegrationTestWebApplicationFactory**: Custom WebApplicationFactory that configures the test environment
- **IntegrationTestFixture**: Combines the database and application factory, managing their initialization and cleanup
- **IntegrationTestBase**: Base class for all integration tests, provides access to HttpClient, DbContext, and Respawn functionality

### Test Collection

All integration tests are part of the "Integration Tests" collection, which ensures:
- A single database container is shared across all tests in the collection
- Database state is reset between tests using Respawn
- Tests can run in parallel while maintaining isolation

## Running the Tests

### Prerequisites

- Docker must be running (required for Testcontainers)
- .NET 10 SDK

### Run All Tests

```bash
dotnet test Library.IntegrationTests/Library.IntegrationTests.csproj
```

### Run with Detailed Output

```bash
dotnet test Library.IntegrationTests/Library.IntegrationTests.csproj --verbosity detailed
```

### Run a Specific Test

```bash
dotnet test --filter "FullyQualifiedName~AuthorsEndpointsTests.CreateAuthor_WithValidData_ReturnsCreatedResult"
```

## Writing New Integration Tests

### Basic Structure

```csharp
using Library.IntegrationTests.Infrastructure;

namespace Library.IntegrationTests.Api;

public class MyEndpointsTests : IntegrationTestBase
{
    public MyEndpointsTests(IntegrationTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task MyTest_Scenario_ExpectedBehavior()
    {
        // Arrange
        // Set up test data using DbContext

        // Act
        // Make HTTP requests using HttpClient

        // Assert
        // Verify results using FluentAssertions
    }
}
```

### Important Notes

1. **DateTime Values**: When creating entities with DateTime properties, use UTC DateTimes:
   ```csharp
   DateTime.SpecifyKind(new DateTime(2024, 1, 1), DateTimeKind.Utc)
   ```

2. **Database Access**: Use the `DbContext` property to access the database directly:
   ```csharp
   DbContext.Authors.Add(author);
   await DbContext.SaveChangesAsync();
   ```

3. **HTTP Requests**: Use the `HttpClient` property to make API requests:
   ```csharp
   var response = await HttpClient.GetAsync("/api/authors");
   ```

4. **Database Cleanup**: The database is automatically reset between tests using Respawn, so you don't need to manually clean up test data.

## Example Tests

See the following files for examples:
- `Api/AuthorsEndpointsTests.cs`: Tests for author endpoints
- `Api/BooksEndpointsTests.cs`: Tests for book endpoints

## Troubleshooting

### Docker Connection Issues

If tests fail with Docker connection errors, ensure:
- Docker Desktop is running
- Your user has permissions to access Docker (on Linux, add user to docker group)

### Test Isolation Issues

If tests interfere with each other:
- Ensure you're inheriting from `IntegrationTestBase`
- Verify the test class is part of the "Integration Tests" collection
- Check that Respawn is properly resetting the database

### Performance

- First test run may be slow due to Docker image download
- Subsequent runs are faster as the image is cached
- Consider running tests in parallel for better performance (xUnit does this by default within a collection)
