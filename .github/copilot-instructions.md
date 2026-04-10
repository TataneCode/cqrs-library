# Copilot Instructions

## Project Overview

.NET 10 library management back-office API using CQRS + MediatR. Manages authors, books (Novel/Comic/Manga/Newspaper), readers, and return notifications. All code lives in a single project (`Library/`) organized into domain-centric layers.

### Core Business Rules

- Readers can borrow at most **3 books simultaneously** (`Reader.MaxBorrowedBooks = 3`)
- Default borrow duration is **14 days**; overdue books trigger notifications
- A book can only be borrowed again **after its return notification is deleted**
- `BookReturnNotificationService` (background service) runs every hour to create overdue notifications

## Commands

```bash
# Build
dotnet build Library/Library.csproj

# Run
dotnet run --project Library/Library.csproj

# Run all tests
dotnet test

# Run unit tests only
dotnet test Library.Tests/Library.Tests.csproj

# Run integration tests only
dotnet test Library.IntegrationTests/Library.IntegrationTests.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName~BorrowBookCommandHandlerTests.Handle_ValidCommand_ShouldBorrowBookSuccessfully"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings --results-directory ./TestResults
```

## Architecture

```
Library/
  Api/                   Minimal API endpoint registration + Requests/ Responses/ Mappers/
  Application/
    Commands/            MediatR commands (records) + handlers, grouped by aggregate
    Queries/             MediatR queries (records) + handlers, grouped by aggregate
  Domain/
    Entities/            Rich domain entities (private setters, business logic methods)
    Enums/               BookType, NotificationStatus
  Infrastructure/
    Persistence/         EF Core DbContext + Configurations/ (IEntityTypeConfiguration)
    Repositories/        Generic IRepository<T> + specialized IBookRepository
    Services/            BookReturnNotificationService (BackgroundService)
    Data/                DatabaseSeeder
    Extensions/          IHost extension for conditional seeding
```

## Key Conventions

### CQRS with MediatR

- **Commands** are `record`s implementing `IRequest<T>` (use `IRequest<Unit>` for void operations)
- **Queries** are `record`s implementing `IRequest<IEnumerable<TResponse>>` or similar
- Each command/query has exactly **one handler** class in the same folder
- Handlers use constructor injection (primary constructor syntax)

### Domain Entities

- All entities inherit `BaseEntity` (provides `Id`, `CreatedAt`, `UpdatedAt`, `SetUpdatedAt()`)
- Properties have `private set` — mutation happens only through domain methods
- Each entity has a `private` parameterless constructor for EF Core
- Business rules are enforced inside entity methods with `InvalidOperationException` or `ArgumentException`

### API Layer

- Endpoints use **Minimal API** registered via static extension methods (`Map*Endpoints`)
- `Api/Mappers/` contains static mapper classes with extension methods for: `Request → Command`, `Entity → Response`
- No AutoMapper — all mappings are manual extension methods
- Endpoint groups use `.WithTags(...)` and `.Produces(...)` for OpenAPI

### Repository Pattern

- `IRepository<T>` provides `GetByIdAsync`, `GetAllAsync`, `FindAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `SaveChangesAsync`
- Specialized repositories extend the interface (e.g., `IBookRepository` adds `GetAvailableBooksAsync`, `GetOverdueBooksAsync`, `GetByISBNAsync`)
- `BookRepository` is registered as **both** `IRepository<Book>` and `IBookRepository`
- Handlers always call `SaveChangesAsync` explicitly after mutations

### Testing

**Unit tests** (`Library.Tests/`):
- Pattern: Moq for repositories, FluentAssertions for assertions, Arrange/Act/Assert
- Handler tests instantiate the handler directly with mocked `IRepository<T>` dependencies

**Integration tests** (`Library.IntegrationTests/`):
- Use `Testcontainers` (PostgreSQL) + `Respawn` to reset the DB between tests
- Inherit from `IntegrationTestBase` which provides `HttpClient`, `DbContext`, and `ResetDatabaseAsync()`
- All tests share `[Collection("Integration Tests")]` to reuse the container
- `IntegrationTestWebApplicationFactory` removes `IHostedService` registrations (background workers disabled during tests)

### NuGet Packages

Managed via **Central Package Management** (`Directory.Packages.props`). Add package references to `Directory.Packages.props` first, then reference without version in `.csproj`.
