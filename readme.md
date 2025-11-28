# CQRS Library Management System

A production-ready .NET 10 API implementing the CQRS (Command Query Responsibility Segregation) pattern for managing a library back-office system.

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
- [Docker Setup](#docker-setup)
- [API Documentation](#api-documentation)
- [Database](#database)
- [Testing](#testing)
- [Configuration](#configuration)
- [Development Workflow](#development-workflow)
- [Troubleshooting](#troubleshooting)
- [Project Structure](#project-structure)
- [Additional Resources](#additional-resources)

## 🎯 Overview

This project demonstrates a complete CQRS implementation for a library management system. The application manages authors, books (novels, comics, mangas, newspapers), readers, and return notifications for a library back-office.

### Key Features

- ✅ **CQRS Pattern** - Separation of read and write operations
- ✅ **Domain-Driven Design** - Rich domain models with business logic
- ✅ **Repository Pattern** - Abstraction over data access
- ✅ **MediatR** - In-process messaging for commands and queries
- ✅ **Entity Framework Core** - ORM with PostgreSQL
- ✅ **Minimal APIs** - Modern endpoint routing
- ✅ **Unit Testing** - Comprehensive test coverage with xUnit and Moq
- ✅ **Database Seeding** - Automated data population with 10,000+ records
- ✅ **Docker Support** - Full containerization with Docker Compose
- ✅ **Central Package Management** - Consistent NuGet versioning

### Business Rules

- **Books** are written by authors and can be of different types (Novel, Comic, Manga, Newspaper)
- **Readers** can borrow a maximum of **3 books** simultaneously
- A **background service** registers notifications when books need to be returned
- **Books** can only be borrowed again after their return notification is deleted
- Each book has a unique **ISBN-13** identifier
- Books track their borrowed status, due date, and borrowing reader

## 🏗️ Architecture

### Domain-Centric Architecture

The solution follows a **Clean Architecture** approach with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│  (HTTP Endpoints, Request/Response DTOs, Routing)           │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                   Application Layer                          │
│  (Commands, Queries, Handlers, Validation, DTOs)            │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────────┐
│                     Domain Layer                             │
│  (Entities, Value Objects, Domain Events, Business Logic)   │
└─────────────────────────────────────────────────────────────┘
                     ▲
┌────────────────────┴────────────────────────────────────────┐
│                 Infrastructure Layer                         │
│  (DbContext, Repositories, External Services, Persistence)  │
└─────────────────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### 🌐 API Layer (`Library/Api/`)
- **Minimal API endpoints** using ASP.NET Core
- HTTP request/response handling
- Endpoint routing and grouping
- OpenAPI documentation
- **No business logic** - delegates to Application layer

**Example:**
```csharp
group.MapPost("/", async ([FromBody] CreateAuthorCommand command, IMediator mediator) =>
{
    var authorId = await mediator.Send(command);
    return Results.Created($"/api/authors/{authorId}", new { id = authorId });
});
```

#### 📋 Application Layer (`Library/Application/`)
- **Commands** - Write operations (Create, Update, Delete)
- **Queries** - Read operations (Get, List, Filter)
- **Handlers** - Process commands and queries via MediatR
- **Validation** - FluentValidation for input validation
- **DTOs** - Data transfer objects for API responses

**CQRS Pattern:**
```
Command → CommandHandler → Domain Logic → Repository → Database
Query   → QueryHandler   → Repository   → Database  → DTO
```

**Example Command:**
```csharp
public record CreateAuthorCommand(
    string FirstName,
    string LastName,
    string? Biography
) : IRequest<Guid>;
```

#### 🎯 Domain Layer (`Library/Domain/`)
- **Entities** - Rich domain models with business logic
- **Value Objects** - Immutable objects (e.g., ISBN)
- **Enums** - Domain enumerations (BookType)
- **Domain Events** - Business events
- **Business Rules** - Encapsulated in entities

**Example Entity:**
```csharp
public class Reader : BaseEntity
{
    public const int MaxBorrowedBooks = 3;

    public bool CanBorrowMoreBooks() => _borrowedBooks.Count < MaxBorrowedBooks;
}
```

#### 🔧 Infrastructure Layer (`Library/Infrastructure/`)
- **DbContext** - Entity Framework Core database context
- **Repositories** - Data access implementations
- **Configurations** - EF Core entity configurations
- **External Services** - Third-party integrations
- **Database Seeding** - CSV-based data seeding

### CQRS Flow

**Write Operation (Command):**
```
1. API receives POST request
2. Deserializes to Command object
3. MediatR sends command to CommandHandler
4. Handler validates and processes business logic
5. Updates domain entities
6. Repository persists changes to database
7. Returns result (typically ID of created entity)
```

**Read Operation (Query):**
```
1. API receives GET request
2. Creates Query object
3. MediatR sends query to QueryHandler
4. Handler retrieves data via repository
5. Maps entities to DTOs
6. Returns DTOs to client
```

## 💻 Technology Stack

### Core Technologies

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 10.0 | Runtime framework |
| **C#** | 12.0 | Programming language |
| **ASP.NET Core** | 10.0 | Web framework |
| **PostgreSQL** | 17-alpine | Database |
| **Entity Framework Core** | 10.0 | ORM |
| **Npgsql** | 10.0 | PostgreSQL provider |

### Libraries & Packages

| Package | Purpose |
|---------|---------|
| **MediatR** (12.4.1) | CQRS mediator pattern |
| **FluentValidation** (11.11.0) | Input validation |
| **CsvHelper** (33.0.1) | CSV parsing for seeding |
| **xUnit** (2.9.2) | Testing framework |
| **Moq** (4.20.72) | Mocking library |
| **FluentAssertions** (6.12.2) | Assertion library |

### Infrastructure

- **Docker** - Container runtime
- **Docker Compose** - Multi-container orchestration
- **GitHub** - Version control
- **REST Client** - API testing (VS Code extension)

## 🚀 Getting Started

### Prerequisites

Ensure you have the following installed:

- **[.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)** (Required)
- **[Docker Desktop](https://www.docker.com/products/docker-desktop)** (Recommended)
- **[Git](https://git-scm.com/)** (For cloning)
- **[VS Code](https://code.visualstudio.com/)** or Visual Studio 2022+ (Optional)
- **PostgreSQL 17** (Only if running without Docker)

### Quick Start

#### Option 1: Using Docker (Recommended)

```bash
# 1. Clone the repository
git clone <repository-url>
cd cqrs-library

# 2. Start all services (API + PostgreSQL)
docker-compose up --build

# 3. Access the API
# API: http://localhost:8080
# Database: localhost:5432
```

The API will be available at **http://localhost:8080**

#### Option 2: Running Locally

```bash
# 1. Clone the repository
git clone <repository-url>
cd cqrs-library

# 2. Start PostgreSQL (if not using Docker)
# Ensure PostgreSQL is running on localhost:5432

# 3. Update connection string in appsettings.Development.json
# "DefaultConnection": "Host=localhost;Port=5432;Database=librarydb;Username=youruser;Password=yourpass"

# 4. Restore NuGet packages
dotnet restore

# 5. Build the solution
dotnet build

# 6. Run the application
dotnet run --project Library/Library.csproj

# 7. Access the API at http://localhost:5000
```

### Verify Installation

Test the API is running:

```bash
# Using curl
curl http://localhost:8080/api/authors

# Expected: JSON response with authors data
```

## 🐳 Docker Setup

The project includes **separate Docker configurations** for development and production environments, optimized for debugging and deployment.

### Docker Files Overview

| File | Purpose | Environment |
|------|---------|-------------|
| `Dockerfile.dev` | Development container with hot reload & debugging | Development |
| `Dockerfile.prod` | Optimized production build | Production |
| `docker-compose.yml` | Base configuration | Both |
| `docker-compose.override.yml` | Development overrides (auto-loaded) | Development |
| `docker-compose.prod.yml` | Production configuration | Production |
| `.env.example` | Environment variables template | Both |

### Development Environment

**Features:**
- ✅ Hot reload - changes reflected instantly
- ✅ Remote debugging with VS Code
- ✅ Source code volume mounting
- ✅ Verbose logging
- ✅ Fast iteration

**Start Development:**

```bash
# Automatically uses docker-compose.override.yml
docker-compose up

# With build
docker-compose up --build

# Detached mode
docker-compose up -d
```

**Development Features:**
- Based on `dotnet/sdk:10.0` (~1.2GB)
- Includes VS debugger (vsdbg)
- Uses `dotnet watch run` for hot reload
- PostgreSQL query logging enabled
- Interactive terminal support

**Hot Reload:**
Make changes to any `.cs` file and the application automatically restarts!

### Production Environment

**Features:**
- ✅ Multi-stage optimized build
- ✅ Minimal image size (~105MB)
- ✅ Non-root user security
- ✅ Health checks
- ✅ Resource limits
- ✅ Traefik reverse proxy with Let's Encrypt

**Start Production:**

```bash
# Using production configuration
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# With build
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up --build -d

# View logs
docker-compose -f docker-compose.yml -f docker-compose.prod.yml logs -f
```

**Production Features:**
- Based on `dotnet/aspnet:10.0-alpine` (~105MB)
- Non-root user (appuser:1000)
- Read-only filesystem
- CPU/Memory limits
- Automated restarts on failure
- Structured logging with rotation

### Environment Variables

**Create .env file:**

```bash
cp .env.example .env
```

**Edit .env:**

```env
POSTGRES_DB=librarydb
POSTGRES_USER=libraryuser
POSTGRES_PASSWORD=your-secure-password
POSTGRES_PORT=5432
API_PORT=8080
ASPNETCORE_ENVIRONMENT=Production
NEED_SEED=false
```

### Docker Commands

**Development:**
```bash
# Start
docker-compose up

# Stop
docker-compose down

# Rebuild
docker-compose build

# View logs
docker-compose logs -f library-api

# Shell access
docker-compose exec library-api /bin/bash
```

**Production:**
```bash
# Start (using shorthand)
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d

# Stop
docker-compose -f docker-compose.yml -f docker-compose.prod.yml down

# Update and restart
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

### Enable Database Seeding

**Development:**
Set environment variable in `docker-compose.override.yml`:
```yaml
environment:
  - NeedSeed=true
```

**Production:**
Add to `.env` file:
```env
NEED_SEED=true
```

Then restart:
```bash
docker-compose up -d  # Development
# or
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d  # Production
```

### Debugging with VS Code

The development container includes remote debugging support:

1. Start containers: `docker-compose up`
2. Install "Remote - Containers" extension in VS Code
3. Attach to running container
4. Set breakpoints and debug!

See [DOCKER.md](DOCKER.md) for detailed debugging configuration.

### Traefik Reverse Proxy (Production)

The production setup includes Traefik v3.0 as a modern reverse proxy with:
- ✅ Automatic service discovery via Docker labels
- ✅ Let's Encrypt SSL/TLS with automatic certificate management
- ✅ HTTP to HTTPS redirect
- ✅ Rate limiting (100 req/s average, 50 burst)
- ✅ Gzip compression
- ✅ Security headers (HSTS, XSS protection)
- ✅ Dashboard with authentication
- ✅ Prometheus metrics

Configure your domain in `.env`:
```bash
DOMAIN=yourdomain.com
LETSENCRYPT_EMAIL=admin@yourdomain.com
```

Traefik automatically discovers services and manages SSL certificates. See [DOCKER.md](DOCKER.md) for full configuration details.

### Image Size Comparison

| Environment | Image Size | Build Time | Features |
|-------------|------------|------------|----------|
| Development | ~1.2GB | Fast | Debugging, hot reload |
| Production | ~105MB | Optimized | Minimal, secure |

### Docker Troubleshooting

**Port already in use:**
```bash
# Change port in docker-compose.yml or .env
API_PORT=9090
```

**Database connection issues:**
```bash
docker-compose ps postgres
docker-compose logs postgres
docker-compose restart postgres
```

**Clear everything and restart:**
```bash
docker-compose down -v
docker system prune -a
docker-compose up --build
```

**View database:**
```bash
docker-compose exec postgres psql -U libraryuser -d librarydb
```

For complete Docker documentation, see **[DOCKER.md](DOCKER.md)**.

## 📚 API Documentation

### API Architecture

The API follows **RESTful** principles with resource-based URLs:

- **Resources:** Authors, Books, Readers, Notifications
- **HTTP Verbs:** GET (read), POST (create), DELETE (delete)
- **Status Codes:** 200 OK, 201 Created, 204 No Content, 400 Bad Request, 404 Not Found
- **Content Type:** `application/json`

### Endpoints Overview

#### Authors API

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/authors` | Get all authors | - | `200 OK` + Author list |
| POST | `/api/authors` | Create author | `CreateAuthorCommand` | `201 Created` + Author ID |

**Create Author Request:**
```json
{
  "firstName": "George",
  "lastName": "Orwell",
  "biography": "English novelist and essayist"
}
```

**Response:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

#### Books API

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/books` | Get all books | - | `200 OK` + Book list |
| GET | `/api/books/available` | Get available books | - | `200 OK` + Book list |
| POST | `/api/books` | Create book | `CreateBookCommand` | `201 Created` + Book ID |
| POST | `/api/books/{id}/borrow` | Borrow book | `{ "readerId": "guid" }` | `200 OK` |
| POST | `/api/books/{id}/return` | Return book | - | `200 OK` |

**Create Book Request:**
```json
{
  "title": "1984",
  "isbn": "9780451524935",
  "type": "Novel",
  "publishedDate": "1949-06-08T00:00:00Z",
  "authorId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "description": "Dystopian social science fiction novel"
}
```

**Book Types:** `Novel`, `Comic`, `Manga`, `Newspaper`

#### Readers API

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| GET | `/api/readers` | Get all readers | - | `200 OK` + Reader list |
| POST | `/api/readers` | Create reader | `CreateReaderCommand` | `201 Created` + Reader ID |

**Create Reader Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "phoneNumber": "555-0123"
}
```

#### Notifications API

| Method | Endpoint | Description | Request Body | Response |
|--------|----------|-------------|--------------|----------|
| DELETE | `/api/notifications/{id}` | Delete notification | - | `204 No Content` |

### Using the API

#### REST Client (VS Code)

1. Install [REST Client extension](https://marketplace.visualstudio.com/items?itemName=humao.rest-client)
2. Open `request.http` in the project root
3. Click "Send Request" above any endpoint

The `request.http` file includes:
- ✅ All API endpoints
- ✅ Example requests
- ✅ Complete workflows
- ✅ Error scenarios
- ✅ Environment variables

#### cURL Examples

```bash
# Get all authors
curl -X GET http://localhost:8080/api/authors

# Create an author
curl -X POST http://localhost:8080/api/authors \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Austen",
    "biography": "English novelist"
  }'

# Get available books
curl -X GET http://localhost:8080/api/books/available

# Borrow a book
curl -X POST http://localhost:8080/api/books/{bookId}/borrow \
  -H "Content-Type: application/json" \
  -d '{"readerId": "{readerId}"}'
```

#### Postman

Import the endpoints from `request.http` or use the following collection structure:

1. Create a collection: "Library API"
2. Add requests for each endpoint
3. Set base URL variable: `{{baseUrl}}` = `http://localhost:8080`

### API Response Format

**Success Response (200 OK):**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "George",
    "lastName": "Orwell",
    "biography": "English novelist and essayist",
    "createdAt": "2024-11-28T10:00:00Z",
    "updatedAt": null
  }
]
```

**Created Response (201 Created):**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Error Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FirstName": ["First name cannot be empty"]
  }
}
```

## 🗄️ Database

### PostgreSQL Configuration

**Connection String:**
```
Host=localhost;Port=5432;Database=librarydb;Username=libraryuser;Password=librarypass
```

**Database Name:** `librarydb`
**Port:** `5432`
**User:** `libraryuser`
**Password:** `librarypass`

### Entity Relationship Diagram

```
┌─────────────┐
│   Author    │
│─────────────│
│ Id (PK)     │
│ FirstName   │
│ LastName    │
│ Biography   │
└──────┬──────┘
       │ 1
       │
       │ N
┌──────▼──────┐          ┌──────────────┐
│    Book     │          │    Reader    │
│─────────────│          │──────────────│
│ Id (PK)     │          │ Id (PK)      │
│ Title       │          │ FirstName    │
│ ISBN        │          │ LastName     │
│ Type        │          │ Email        │
│ AuthorId(FK)│          │ PhoneNumber  │
│ ReaderId(FK)├─────N:1─▶│              │
│ BorrowedAt  │          └──────┬───────┘
│ DueDate     │                 │ 1
└──────┬──────┘                 │
       │ 1                      │ N
       │                        │
       │ N                ┌─────▼──────────┐
       └─────────────────▶│ Notification   │
                          │────────────────│
                          │ Id (PK)        │
                          │ Message        │
                          │ BookId (FK)    │
                          │ ReaderId (FK)  │
                          │ CreatedAt      │
                          └────────────────┘
```

### Database Entities

**Authors:**
- Stores author information
- One-to-many relationship with Books

**Books:**
- Central entity with book details
- References Author (many-to-one)
- References Reader when borrowed (many-to-one, nullable)
- Tracks borrowing state and due date

**Readers:**
- Library members who borrow books
- One-to-many relationship with borrowed Books
- Maximum of 3 concurrent borrows enforced in domain logic

**Notifications:**
- Return reminders for overdue books
- References Book and Reader
- Created by background service

### Database Seeding

The application supports automated database seeding via CSV files.

**Seed Data:**
- **107 Authors** from `Library/Data/Seeds/authors.csv`
- **100 Readers** from `Library/Data/Seeds/readers.csv`
- **10,000 Books** generated from `Library/Data/Seeds/book-titles.csv`

**Enable Seeding:**

1. **Environment Variable:**
   ```bash
   export NeedSeed=true
   dotnet run --project Library/Library.csproj
   ```

2. **Docker Compose:**
   ```yaml
   environment:
     NeedSeed: "true"
   ```

3. **Configuration File:**
   ```json
   {
     "NeedSeed": true
   }
   ```

**Seeding Process:**
1. Checks if database is already seeded
2. Creates authors from CSV
3. Creates readers from CSV
4. Generates 10,000 books in batches
5. Books are variations of titles with unique ISBNs

For detailed seeding documentation, see **[SEEDING.md](SEEDING.md)**.

### Migrations

Entity Framework Core migrations track database schema changes.

```bash
# Create a new migration
dotnet ef migrations add MigrationName --project Library

# Apply migrations
dotnet ef database update --project Library

# Remove last migration
dotnet ef migrations remove --project Library

# Generate SQL script
dotnet ef migrations script --project Library
```

### Direct Database Access

**Using Docker:**
```bash
# Connect to PostgreSQL
docker-compose exec postgres psql -U libraryuser -d librarydb

# Run queries
SELECT * FROM "Authors" LIMIT 10;
SELECT COUNT(*) FROM "Books";
SELECT * FROM "Books" WHERE "BorrowedByReaderId" IS NOT NULL;
```

**Using psql locally:**
```bash
psql -h localhost -p 5432 -U libraryuser -d librarydb
```

## ✅ Testing

### Test Architecture

The solution includes comprehensive unit tests following the **AAA pattern** (Arrange-Act-Assert):

- **Framework:** xUnit 2.9.2
- **Mocking:** Moq 4.20.72
- **Assertions:** FluentAssertions 6.12.2
- **Coverage:** Application and Domain layers

### Test Structure

```
Library.Tests/
├── Application/
│   ├── Commands/
│   │   ├── Authors/
│   │   │   └── CreateAuthorCommandHandlerTests.cs
│   │   └── Books/
│   │       └── BorrowBookCommandHandlerTests.cs
│   └── Queries/
└── Domain/
    └── Entities/
        └── AuthorTests.cs
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity detailed

# Run specific test class
dotnet test --filter "FullyQualifiedName~CreateAuthorCommandHandlerTests"

# Run specific test method
dotnet test --filter "FullyQualifiedName~CreateAuthorCommandHandlerTests.Handle_ValidCommand_ShouldCreateAuthorAndReturnId"

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Test Examples

**Command Handler Test:**
```csharp
[Fact]
public async Task Handle_ValidCommand_ShouldCreateAuthorAndReturnId()
{
    // Arrange
    var command = new CreateAuthorCommand("John", "Doe", "Author bio");
    _authorRepositoryMock
        .Setup(x => x.AddAsync(It.IsAny<Author>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((Author a, CancellationToken ct) => a);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeEmpty();
    _authorRepositoryMock.Verify(
        x => x.AddAsync(It.Is<Author>(a => a.FirstName == "John"), It.IsAny<CancellationToken>()),
        Times.Once
    );
}
```

**Domain Entity Test:**
```csharp
[Fact]
public void Constructor_ValidParameters_ShouldCreateAuthor()
{
    // Arrange & Act
    var author = new Author("Jane", "Doe", "Biography");

    // Assert
    author.Should().NotBeNull();
    author.FirstName.Should().Be("Jane");
    author.LastName.Should().Be("Doe");
    author.Biography.Should().Be("Biography");
}
```

### Test Coverage

Current test coverage includes:

- ✅ **Command Handlers** - CreateAuthor, BorrowBook
- ✅ **Domain Entities** - Author validation and business logic
- ✅ **Error Scenarios** - Invalid inputs, business rule violations
- ✅ **Repository Mocking** - Isolated unit tests

**Total Tests:** 25 tests passing

## ⚙️ Configuration

### Application Settings

Configuration is managed through `appsettings.json` and environment-specific files.

**appsettings.json (Production):**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=librarydb;Username=libraryuser;Password=librarypass"
  }
}
```

**appsettings.Development.json:**
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=librarydb;Username=libraryuser;Password=librarypass"
  }
}
```

### Environment Variables

| Variable | Description | Default | Example |
|----------|-------------|---------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` | `Development` |
| `ASPNETCORE_URLS` | Binding URLs | `http://+:5000` | `http://+:8080` |
| `ConnectionStrings__DefaultConnection` | Database connection | - | `Host=postgres;Database=librarydb;...` |
| `NeedSeed` | Enable database seeding | `false` | `true` |

**Setting Environment Variables:**

Linux/macOS:
```bash
export NeedSeed=true
export ASPNETCORE_ENVIRONMENT=Development
```

Windows PowerShell:
```powershell
$env:NeedSeed="true"
$env:ASPNETCORE_ENVIRONMENT="Development"
```

### Connection Strings

**Format:**
```
Host={server};Port={port};Database={database};Username={user};Password={password}
```

**Docker:**
```
Host=postgres;Database=librarydb;Username=libraryuser;Password=librarypass
```

**Local:**
```
Host=localhost;Port=5432;Database=librarydb;Username=libraryuser;Password=librarypass
```

### Logging

Logging is configured via `appsettings.json`:

**Log Levels:**
- `Trace` - Most detailed
- `Debug` - Debugging information
- `Information` - General information
- `Warning` - Warning messages
- `Error` - Error messages
- `Critical` - Critical failures

**Configuration:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore": "Warning",
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

## 🔧 Development Workflow

### Local Development

```bash
# 1. Clone repository
git clone <repository-url>
cd cqrs-library

# 2. Install dependencies
dotnet restore

# 3. Start PostgreSQL (Docker or local)
docker-compose up postgres -d

# 4. Run migrations (if needed)
dotnet ef database update --project Library

# 5. Start the application with hot reload
dotnet watch --project Library/Library.csproj

# 6. Make changes - application will auto-reload
```

### Adding a New Feature

**Example: Adding a new command**

1. **Create Command** in `Application/Commands/`:
   ```csharp
   public record UpdateAuthorCommand(Guid Id, string FirstName, string LastName) : IRequest<Unit>;
   ```

2. **Create Handler**:
   ```csharp
   public class UpdateAuthorCommandHandler : IRequestHandler<UpdateAuthorCommand, Unit>
   {
       // Implementation
   }
   ```

3. **Add Endpoint** in `Api/`:
   ```csharp
   group.MapPut("/{id}", async (Guid id, UpdateAuthorCommand command, IMediator mediator) =>
   {
       await mediator.Send(command);
       return Results.Ok();
   });
   ```

4. **Write Tests** in `Library.Tests/`:
   ```csharp
   public class UpdateAuthorCommandHandlerTests
   {
       // Tests following AAA pattern
   }
   ```

5. **Update API documentation** in `request.http`

### Code Style Guidelines

- Follow C# naming conventions
- Use `record` for DTOs and commands
- Keep domain logic in entities
- Use async/await for I/O operations
- Write unit tests for new features
- Use nullable reference types
- Document public APIs with XML comments

### Git Workflow

```bash
# Create feature branch
git checkout -b feature/add-book-ratings

# Make changes and commit
git add .
git commit -m "Add book rating feature"

# Push to remote
git push origin feature/add-book-ratings

# Create pull request
```

### Building for Production

```bash
# Build in Release mode
dotnet build -c Release

# Publish application
dotnet publish Library/Library.csproj -c Release -o ./publish

# Create Docker image
docker build -t library-api:latest .

# Run container
docker run -p 8080:8080 library-api:latest
```

## 🔍 Troubleshooting

### Common Issues

#### Port Already in Use

**Error:** `Address already in use`

**Solution:**
```bash
# Change port in docker-compose.yml
ports:
  - "9090:8080"  # Use different port

# Or kill process using port 8080
lsof -ti:8080 | xargs kill -9  # macOS/Linux
netstat -ano | findstr :8080   # Windows
```

#### Database Connection Failed

**Error:** `Could not connect to PostgreSQL`

**Solutions:**
1. Check PostgreSQL is running:
   ```bash
   docker-compose ps postgres
   ```

2. Verify connection string in `appsettings.json`

3. Check PostgreSQL logs:
   ```bash
   docker-compose logs postgres
   ```

4. Restart PostgreSQL:
   ```bash
   docker-compose restart postgres
   ```

#### Migrations Not Applied

**Error:** `Table does not exist`

**Solution:**
```bash
# Apply migrations manually
dotnet ef database update --project Library

# Or recreate database
dotnet ef database drop --project Library --force
dotnet ef database update --project Library
```

#### Seeding Fails

**Error:** `CSV file not found`

**Solution:**
```bash
# Ensure CSV files are copied to output
dotnet build Library/Library.csproj

# Check files exist
ls Library/bin/Debug/net10.0/Data/Seeds/
```

#### Tests Failing

**Error:** `System.NotSupportedException: Unsupported expression`

**Solution:**
- Ensure mocking only virtual/interface members
- Check test setup and mocks are correctly configured

#### Docker Build Issues

**Error:** `Unable to resolve service`

**Solution:**
```bash
# Clear Docker cache
docker-compose down -v
docker system prune -a

# Rebuild from scratch
docker-compose build --no-cache
docker-compose up
```

### Debugging

**Enable detailed logging:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft": "Debug"
  }
}
```

**Attach debugger in VS Code:**

Create `.vscode/launch.json`:
```json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": ".NET Core Launch (web)",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build",
      "program": "${workspaceFolder}/Library/bin/Debug/net10.0/Library.dll",
      "args": [],
      "cwd": "${workspaceFolder}/Library",
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ]
}
```

### Getting Help

If you encounter issues:

1. Check this troubleshooting section
2. Review logs: `docker-compose logs -f`
3. Consult [CLAUDE.md](CLAUDE.md) for architecture details
4. Check [SEEDING.md](SEEDING.md) for seeding issues
5. Open an issue on GitHub

## 📁 Project Structure

```
cqrs-library/
├── .git/                           # Git repository
├── .gitignore                      # Git ignore rules
├── Library.sln                     # Solution file
├── Library.slnx                    # XML solution file
├── Directory.Packages.props        # Central package management
├── docker-compose.yml              # Docker Compose configuration
├── Dockerfile                      # Docker image definition
├── .dockerignore                   # Docker ignore rules
├── readme.md                       # This file
├── CLAUDE.md                       # Claude Code guidance
├── SEEDING.md                      # Seeding documentation
├── request.http                    # API test requests
│
├── Library/                        # Main API project
│   ├── Library.csproj             # Project file
│   ├── Program.cs                 # Application entry point
│   ├── appsettings.json           # Configuration (production)
│   ├── appsettings.Development.json # Configuration (development)
│   │
│   ├── Api/                       # API Layer
│   │   ├── AuthorsEndpoints.cs   # Author endpoints
│   │   ├── BooksEndpoints.cs     # Book endpoints
│   │   ├── ReadersEndpoints.cs   # Reader endpoints
│   │   └── NotificationsEndpoints.cs # Notification endpoints
│   │
│   ├── Application/               # Application Layer
│   │   ├── Commands/             # Write operations
│   │   │   ├── Authors/
│   │   │   │   ├── CreateAuthorCommand.cs
│   │   │   │   └── CreateAuthorCommandHandler.cs
│   │   │   ├── Books/
│   │   │   │   ├── CreateBookCommand.cs
│   │   │   │   ├── CreateBookCommandHandler.cs
│   │   │   │   ├── BorrowBookCommand.cs
│   │   │   │   ├── BorrowBookCommandHandler.cs
│   │   │   │   ├── ReturnBookCommand.cs
│   │   │   │   └── ReturnBookCommandHandler.cs
│   │   │   ├── Readers/
│   │   │   │   ├── CreateReaderCommand.cs
│   │   │   │   └── CreateReaderCommandHandler.cs
│   │   │   └── Notifications/
│   │   │       ├── DeleteNotificationCommand.cs
│   │   │       └── DeleteNotificationCommandHandler.cs
│   │   │
│   │   └── Queries/              # Read operations
│   │       ├── Authors/
│   │       │   ├── GetAllAuthorsQuery.cs
│   │       │   └── GetAllAuthorsQueryHandler.cs
│   │       ├── Books/
│   │       │   ├── GetAllBooksQuery.cs
│   │       │   ├── GetAllBooksQueryHandler.cs
│   │       │   ├── GetAvailableBooksQuery.cs
│   │       │   └── GetAvailableBooksQueryHandler.cs
│   │       └── Readers/
│   │           ├── GetAllReadersQuery.cs
│   │           └── GetAllReadersQueryHandler.cs
│   │
│   ├── Domain/                    # Domain Layer
│   │   ├── Entities/             # Domain entities
│   │   │   ├── BaseEntity.cs    # Base entity class
│   │   │   ├── Author.cs
│   │   │   ├── Book.cs
│   │   │   ├── Reader.cs
│   │   │   └── Notification.cs
│   │   └── Enums/               # Domain enumerations
│   │       └── BookType.cs
│   │
│   ├── Infrastructure/            # Infrastructure Layer
│   │   ├── Data/                # Data seeding
│   │   │   └── DatabaseSeeder.cs
│   │   ├── Persistence/         # Database context
│   │   │   ├── LibraryDbContext.cs
│   │   │   └── Configurations/
│   │   │       ├── AuthorConfiguration.cs
│   │   │       ├── BookConfiguration.cs
│   │   │       ├── ReaderConfiguration.cs
│   │   │       └── NotificationConfiguration.cs
│   │   ├── Repositories/        # Repository pattern
│   │   │   ├── IRepository.cs
│   │   │   ├── Repository.cs
│   │   │   └── BookRepository.cs
│   │   └── Services/           # Background services
│   │       └── BookReturnNotificationService.cs
│   │
│   └── Data/                     # Seed data
│       └── Seeds/
│           ├── authors.csv
│           ├── readers.csv
│           └── book-titles.csv
│
└── Library.Tests/                # Unit test project
    ├── Library.Tests.csproj     # Test project file
    ├── Application/             # Application layer tests
    │   └── Commands/
    │       ├── Authors/
    │       │   └── CreateAuthorCommandHandlerTests.cs
    │       └── Books/
    │           └── BorrowBookCommandHandlerTests.cs
    └── Domain/                  # Domain layer tests
        └── Entities/
            └── AuthorTests.cs
```

## 📚 Additional Resources

### Documentation

- **[CLAUDE.md](CLAUDE.md)** - Development guidance and architecture details
- **[SEEDING.md](SEEDING.md)** - Database seeding documentation
- **[DOCKER.md](DOCKER.md)** - Docker configuration and deployment guide
- **[request.http](request.http)** - API endpoint examples

### External Links

- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MediatR](https://github.com/jbogard/MediatR)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Docker Documentation](https://docs.docker.com/)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)

### Learning Resources

- **CQRS Pattern:** Understanding command-query separation
- **Domain-Driven Design:** Rich domain models and business logic
- **Clean Architecture:** Layered architecture principles
- **Repository Pattern:** Data access abstraction
- **Unit Testing:** AAA pattern with xUnit and Moq

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Contribution Guidelines

- Follow existing code style and patterns
- Write unit tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR
- Keep commits atomic and well-described

---

**Built with ❤️ using .NET 10 and CQRS Pattern**
