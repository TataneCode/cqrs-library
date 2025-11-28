# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 10 API project implementing a library management system using CQRS (Command Query Responsibility Segregation) pattern. The application manages authors, books (novels, comics, mangas, newspapers), readers, and notifications for a library back-office system.

### Core Business Rules

- Books are written by authors and can be of different types (novels, comics, mangas, newspapers)
- Readers can borrow a maximum of 3 books simultaneously
- A background service registers notifications when books need to be returned
- Books can only be borrowed again after their return notification is deleted

## Architecture

The solution follows a **domain-centric architecture** with the following directory structure:

- **Api**: Web API layer, endpoints, controllers
- **Application**: Application services, CQRS commands/queries, handlers
- **Domain**: Domain models, entities, business logic, domain events
- **Infrastructure**: Data access, PostgreSQL integration, external services

All code is contained within a single project (`Library`) but organized by these architectural layers.

## Technology Stack

- .NET 10
- PostgreSQL (latest version)
- Docker Compose + Dockerfile for containerization
- Central Package Management (for NuGet packages)

## Development Commands

### Building and Running

```bash
# Build the project
dotnet build Library/Library.csproj

# Run the application
dotnet run --project Library/Library.csproj

# Run with hot reload
dotnet watch --project Library/Library.csproj
```

### Testing

```bash
# Run all tests (once test project is created)
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedName~TestClassName.TestMethodName"

# Run tests with verbosity
dotnet test --verbosity detailed
```

### Docker

```bash
# Build and run with Docker Compose
docker-compose up --build

# Run in detached mode
docker-compose up -d

# Stop services
docker-compose down

# View logs
docker-compose logs -f
```

### Database

The application uses PostgreSQL. Connection strings are configured in `appsettings.json` and `appsettings.Development.json`.

### API Testing

```bash
# Use the request.http file in the root directory
# Install REST Client extension in VS Code
# Open request.http and click "Send Request" above any endpoint

# Or use curl for testing
curl -X GET http://localhost:5000/api/authors
curl -X POST http://localhost:5000/api/authors \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Test","lastName":"Author","biography":"Test bio"}'
```

## Code Organization Notes

- The domain-centric architecture (Api, Application, Domain, Infrastructure) is fully implemented as directories within the Library project
- A unit test project (Library.Tests) exists with xUnit, Moq, and FluentAssertions
- Central package management is configured using Directory.Packages.props
- Database seeding is available via the DatabaseSeeder service (see SEEDING.md)

## CQRS Implementation

When implementing CQRS:

- **Commands**: Mutation operations (creating, updating, deleting) go in Application/Commands
- **Queries**: Read operations go in Application/Queries
- **Handlers**: Each command/query should have a corresponding handler
- **Domain Events**: Business events should be raised from the domain layer
- Consider using MediatR or similar library for mediating commands/queries to handlers
