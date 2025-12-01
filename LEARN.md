# Understanding CQRS and Domain-Centric Architecture

This document explains the architectural patterns used in this Library Management System: **CQRS (Command Query Responsibility Segregation)** and **Domain-Centric Architecture**.

## Table of Contents

1. [What is CQRS?](#what-is-cqrs)
2. [What is Domain-Centric Architecture?](#what-is-domain-centric-architecture)
3. [How They Work Together](#how-they-work-together)
4. [Implementation in This Project](#implementation-in-this-project)
5. [Benefits and Tradeoffs](#benefits-and-tradeoffs)
6. [Best Practices](#best-practices)
7. [Common Patterns](#common-patterns)

---

## What is CQRS?

**CQRS (Command Query Responsibility Segregation)** is a pattern that separates read operations (queries) from write operations (commands).

### The Core Principle

```
Traditional Approach:
┌─────────────────┐
│   Controller    │
│                 │
│  getAllBooks()  │
│  createBook()   │
│  borrowBook()   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│    Service      │
└─────────────────┘

CQRS Approach:
┌─────────────────┐
│   Controller    │
└────────┬────────┘
         │
    ┌────┴────┐
    │         │
    ▼         ▼
┌────────┐ ┌─────────┐
│ Query  │ │ Command │
│ (Read) │ │ (Write) │
└────────┘ └─────────┘
```

### Commands vs Queries

#### Commands (Write Operations)
- **Change state** - modify data in the system
- **Return minimal data** - typically just success/failure or an ID
- **Have side effects** - create, update, delete operations
- **Use imperative naming** - "CreateBook", "BorrowBook", "DeleteNotification"

#### Queries (Read Operations)
- **Don't change state** - only retrieve data
- **Return data** - DTOs, entities, or projections
- **No side effects** - safe to call multiple times
- **Use interrogative naming** - "GetAllBooks", "GetAvailableBooks"

### Example from This Project

**Command (Write):**
```csharp
// BorrowBookCommand.cs
public record BorrowBookCommand(
    Guid BookId,
    Guid ReaderId
) : IRequest<Unit>;  // Returns Unit (void equivalent)

// BorrowBookCommandHandler.cs
public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Unit>
{
    public async Task<Unit> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve entities
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        var reader = await readerRepository.GetByIdAsync(request.ReaderId, cancellationToken);

        // 2. Validate business rules
        if (!reader.CanBorrowMoreBooks())
            throw new InvalidOperationException("Reader has reached maximum limit");

        // 3. Execute domain logic
        book.Borrow(request.ReaderId);

        // 4. Persist changes
        await bookRepository.UpdateAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;  // No data returned
    }
}
```

**Query (Read):**
```csharp
// GetAvailableBooksQuery.cs
public record GetAvailableBooksQuery : IRequest<IEnumerable<Book>>;

// GetAvailableBooksQueryHandler.cs
public class GetAvailableBooksQueryHandler : IRequestHandler<GetAvailableBooksQuery, IEnumerable<Book>>
{
    public async Task<IEnumerable<Book>> Handle(GetAvailableBooksQuery request, CancellationToken cancellationToken)
    {
        // Simply retrieves and returns data - no state changes
        return await bookRepository.GetAvailableBooksAsync(cancellationToken);
    }
}
```

### Why CQRS?

1. **Separation of Concerns**: Read and write logic are independent
2. **Scalability**: Can optimize queries separately from commands
3. **Clarity**: Intent is clear from the operation type
4. **Security**: Different permissions for reads vs writes
5. **Performance**: Queries can use different data models optimized for reading

---

## What is Domain-Centric Architecture?

**Domain-Centric Architecture** (also called Domain-Driven Design or Clean Architecture) places the **domain model at the center** of your application.

### The Layered Structure

```
┌─────────────────────────────────────────────┐
│              API Layer                      │  ← HTTP Endpoints, Controllers
│  (AuthorsEndpoints, BooksEndpoints)         │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│          Application Layer                  │  ← CQRS Commands & Queries
│  (Commands, Queries, Handlers)              │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│   ┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┓     │
│   ┃       Domain Layer              ┃     │  ← Business Logic & Rules
│   ┃  (Entities, Value Objects,      ┃     │     (THE CORE)
│   ┃   Business Rules, Enums)        ┃     │
│   ┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┛     │
└──────────────────┬──────────────────────────┘
                   │
┌──────────────────▼──────────────────────────┐
│        Infrastructure Layer                 │  ← Data Access, External Services
│  (DbContext, Repositories, Services)        │
└─────────────────────────────────────────────┘
```

### Dependency Rule

**Dependencies point INWARD toward the domain:**
- Infrastructure depends on Domain
- Application depends on Domain
- API depends on Application
- **Domain depends on NOTHING** (pure business logic)

### Project Structure

```
Library/
├── Api/                          # HTTP layer
│   ├── AuthorsEndpoints.cs
│   ├── BooksEndpoints.cs
│   └── Requests/
├── Application/                  # Use cases
│   ├── Commands/
│   │   ├── Books/
│   │   │   ├── CreateBookCommand.cs
│   │   │   ├── CreateBookCommandHandler.cs
│   │   │   ├── BorrowBookCommand.cs
│   │   │   └── BorrowBookCommandHandler.cs
│   │   └── Authors/
│   └── Queries/
│       ├── Books/
│       │   ├── GetAllBooksQuery.cs
│       │   └── GetAllBooksQueryHandler.cs
│       └── Authors/
├── Domain/                       # ⭐ THE CORE ⭐
│   ├── Entities/
│   │   ├── Book.cs              # Rich domain models
│   │   ├── Author.cs
│   │   └── Reader.cs
│   └── Enums/
│       ├── BookType.cs
│       └── NotificationStatus.cs
└── Infrastructure/               # External concerns
    ├── Persistence/
    │   └── LibraryDbContext.cs
    ├── Repositories/
    │   └── BookRepository.cs
    └── Services/
        └── BookReturnNotificationService.cs
```

---

## How They Work Together

CQRS and Domain-Centric Architecture complement each other perfectly:

### Request Flow Example: Borrowing a Book

```
1. HTTP Request arrives
   POST /api/books/borrow
   { "bookId": "...", "readerId": "..." }
        │
        ▼
2. API Layer (BooksEndpoints.cs)
   Creates BorrowBookCommand from request
        │
        ▼
3. Application Layer (BorrowBookCommandHandler)
   Orchestrates the use case:
   - Retrieves Book and Reader entities
   - Calls domain methods
        │
        ▼
4. Domain Layer (Book.cs, Reader.cs)
   Contains business rules:
   - Reader.CanBorrowMoreBooks() checks limit
   - Book.Borrow() executes domain logic
   - Book.IsAvailable() validates state
        │
        ▼
5. Infrastructure Layer (BookRepository)
   Persists changes to database
        │
        ▼
6. Response returns
   201 Created or error message
```

---

## Implementation in This Project

### Domain Layer: Rich Domain Models

The domain entities contain **business logic**, not just data:

```csharp
// Domain/Entities/Reader.cs
public class Reader : BaseEntity
{
    public const int MaxBorrowedBooks = 3;  // Business rule as constant

    public string FirstName { get; private set; }  // Private setters!
    public string LastName { get; private set; }
    public string Email { get; private set; }

    private readonly List<Book> _borrowedBooks = new();
    public IReadOnlyCollection<Book> BorrowedBooks => _borrowedBooks.AsReadOnly();

    // Constructor with validation
    public Reader(string firstName, string lastName, string email)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");
        // ... more validation

        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }

    // Business logic methods
    public bool CanBorrowMoreBooks() => _borrowedBooks.Count < MaxBorrowedBooks;

    public int AvailableBorrowSlots() => MaxBorrowedBooks - _borrowedBooks.Count;
}
```

**Key Points:**
- **Private setters**: Properties can't be changed directly
- **Business rules**: `MaxBorrowedBooks` constant defines the rule
- **Validation**: Constructor ensures entity is always in valid state
- **Behavior methods**: `CanBorrowMoreBooks()` encapsulates business logic
- **Encapsulation**: Internal list is exposed as `IReadOnlyCollection`

### Domain Layer: Book Entity

```csharp
// Domain/Entities/Book.cs (simplified)
public class Book : BaseEntity
{
    public string Title { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid? BorrowedByReaderId { get; private set; }
    public DateTime? BorrowedAt { get; private set; }
    public DateTime? DueDate { get; private set; }

    // Domain method with business rules
    public void Borrow(Guid readerId, int borrowDurationDays = 14)
    {
        if (!IsAvailable())
            throw new InvalidOperationException($"Book '{Title}' is already borrowed");

        BorrowedByReaderId = readerId;
        BorrowedAt = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(borrowDurationDays);
        SetUpdatedAt();
    }

    public void Return()
    {
        if (IsAvailable())
            throw new InvalidOperationException($"Book '{Title}' is not currently borrowed");

        BorrowedByReaderId = null;
        BorrowedAt = null;
        DueDate = null;
        SetUpdatedAt();
    }

    public bool IsAvailable() => BorrowedByReaderId == null;
    public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}
```

**Business Rules Enforced:**
- A book can only be borrowed if it's available
- A book can only be returned if it's borrowed
- Due date is automatically calculated (14 days default)
- Overdue status is calculated, not stored

### Application Layer: MediatR for CQRS

This project uses **MediatR** library to implement CQRS:

#### Command Pattern
```csharp
// 1. Define the command (request)
public record CreateAuthorCommand(
    string FirstName,
    string LastName,
    string? Biography
) : IRequest<Guid>;  // Returns the created author's ID

// 2. Implement the handler
public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, Guid>
{
    private readonly IRepository<Author> _authorRepository;

    public CreateAuthorCommandHandler(IRepository<Author> authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        // Create domain entity (validation happens in constructor)
        var author = new Author(
            request.FirstName,
            request.LastName,
            request.Biography
        );

        // Persist using repository
        await _authorRepository.AddAsync(author, cancellationToken);
        await _authorRepository.SaveChangesAsync(cancellationToken);

        return author.Id;  // Return the ID
    }
}
```

#### Query Pattern
```csharp
// 1. Define the query (request)
public record GetAllAuthorsQuery : IRequest<IEnumerable<Author>>;

// 2. Implement the handler
public class GetAllAuthorsQueryHandler : IRequestHandler<GetAllAuthorsQuery, IEnumerable<Author>>
{
    private readonly IRepository<Author> _authorRepository;

    public GetAllAuthorsQueryHandler(IRepository<Author> authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<IEnumerable<Author>> Handle(GetAllAuthorsQuery request, CancellationToken cancellationToken)
    {
        return await _authorRepository.GetAllAsync(cancellationToken);
    }
}
```

### API Layer: Minimal APIs

```csharp
// Api/BooksEndpoints.cs
public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // QUERY endpoint
        group.MapGet("/available", async (IMediator mediator, CancellationToken ct) =>
        {
            var books = await mediator.Send(new GetAvailableBooksQuery(), ct);
            return Results.Ok(books);
        });

        // COMMAND endpoint
        group.MapPost("/borrow", async (
            BorrowBookRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new BorrowBookCommand(request.BookId, request.ReaderId);
            await mediator.Send(command, ct);
            return Results.Ok();
        });
    }
}
```

**Flow:**
1. HTTP request arrives
2. Endpoint creates Command/Query
3. MediatR routes to appropriate Handler
4. Handler executes business logic through Domain entities
5. Response is returned

---

## Benefits and Tradeoffs

### Benefits

#### CQRS Benefits
✅ **Clear Intent**: Command vs Query is immediately obvious
✅ **Optimized Operations**: Different strategies for reads and writes
✅ **Scalability**: Can scale read and write sides independently
✅ **Security**: Separate permissions for queries and commands
✅ **Audit Trail**: Easy to log all commands for compliance

#### Domain-Centric Architecture Benefits
✅ **Business Logic Centralization**: All rules in domain entities
✅ **Testability**: Domain logic has no external dependencies
✅ **Maintainability**: Changes to business rules happen in one place
✅ **Flexibility**: Can change database or UI without touching domain
✅ **Team Communication**: Domain model matches business language

### Tradeoffs

#### CQRS Tradeoffs
⚠️ **More Code**: Separate classes for commands and queries
⚠️ **Learning Curve**: Team needs to understand the pattern
⚠️ **Complexity**: Overkill for simple CRUD applications

#### Domain-Centric Architecture Tradeoffs
⚠️ **Initial Setup**: More files and structure upfront
⚠️ **Abstraction**: More layers to navigate
⚠️ **Overhead**: May be too much for small projects

### When to Use

**Use CQRS + Domain-Centric When:**
- Complex business rules
- Domain logic is core to your application
- Multiple teams working on the project
- Need to scale reads and writes differently
- Audit and compliance requirements
- Long-term maintainability is important

**Don't Use When:**
- Simple CRUD applications
- Prototypes or MVPs
- Very small team or solo project
- Time to market is critical
- Domain logic is minimal

---

## Best Practices

### 1. Keep Domain Pure

```csharp
// ❌ BAD: Domain entity depends on infrastructure
public class Book
{
    public void Borrow(Guid readerId)
    {
        // Don't do database calls in domain!
        var reader = _dbContext.Readers.Find(readerId);
        // ...
    }
}

// ✅ GOOD: Domain entity is pure
public class Book
{
    public void Borrow(Guid readerId)
    {
        // Just business logic
        if (!IsAvailable())
            throw new InvalidOperationException("Book is already borrowed");

        BorrowedByReaderId = readerId;
        BorrowedAt = DateTime.UtcNow;
    }
}
```

### 2. Commands Should Not Return Data

```csharp
// ❌ BAD: Command returns full entity
public record CreateBookCommand(...) : IRequest<Book>;

// ✅ GOOD: Command returns only ID or Unit
public record CreateBookCommand(...) : IRequest<Guid>;

// ✅ ALSO GOOD: For operations with no return value
public record BorrowBookCommand(...) : IRequest<Unit>;
```

### 3. Queries Should Not Modify State

```csharp
// ❌ BAD: Query has side effects
public async Task<IEnumerable<Book>> Handle(GetOverdueBooksQuery request, ...)
{
    var books = await _repository.GetOverdueBooks();

    // Don't modify state in a query!
    foreach (var book in books)
    {
        book.MarkAsOverdue();
        await _repository.UpdateAsync(book);
    }

    return books;
}

// ✅ GOOD: Query only reads
public async Task<IEnumerable<Book>> Handle(GetOverdueBooksQuery request, ...)
{
    return await _repository.GetOverdueBooks();
}
```

### 4. Use Private Setters in Domain Entities

```csharp
// ❌ BAD: Public setters allow bypassing validation
public class Author
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// Somewhere in code:
author.FirstName = "";  // Oops, breaks business rule!

// ✅ GOOD: Private setters force use of methods
public class Author
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }

    public void UpdateDetails(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty");

        FirstName = firstName;
        LastName = lastName;
    }
}
```

### 5. Validate in Domain Constructors

```csharp
// ✅ GOOD: Entity is always in valid state
public class Reader : BaseEntity
{
    public Reader(string firstName, string lastName, string email)
    {
        // Validation in constructor ensures invalid entities can't be created
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
    }
}
```

### 6. Use Value Objects for Complex Values

Value objects encapsulate related data and validation:

```csharp
// Example (not in this project, but a good practice):
public class Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (!IsValid(value))
            throw new ArgumentException("Invalid email format");

        Value = value;
    }

    private bool IsValid(string email)
    {
        // Email validation logic
        return email.Contains("@");
    }
}

public class Reader
{
    public Email Email { get; private set; }

    public Reader(string firstName, string lastName, Email email)
    {
        // Email is already validated!
        Email = email;
    }
}
```

---

## Common Patterns

### Pattern 1: Command Validation

```csharp
public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Unit>
{
    public async Task<Unit> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        // 1. Retrieve aggregates
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        var reader = await _readerRepository.GetByIdAsync(request.ReaderId, cancellationToken);

        // 2. Check existence
        if (book == null)
            throw new InvalidOperationException("Book not found");

        if (reader == null)
            throw new InvalidOperationException("Reader not found");

        // 3. Validate business rules (from domain)
        if (!reader.CanBorrowMoreBooks())
            throw new InvalidOperationException("Reader has reached maximum limit");

        // 4. Execute domain logic
        book.Borrow(request.ReaderId);

        // 5. Persist
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
```

### Pattern 2: Query Projections

For performance, queries can return DTOs instead of full entities:

```csharp
// Query returns a lightweight DTO
public record BookSummaryDto(Guid Id, string Title, string AuthorName, bool IsAvailable);

public class GetBookSummariesQuery : IRequest<IEnumerable<BookSummaryDto>>;

public class GetBookSummariesQueryHandler : IRequestHandler<GetBookSummariesQuery, IEnumerable<BookSummaryDto>>
{
    public async Task<IEnumerable<BookSummaryDto>> Handle(...)
    {
        // Use database projection for efficiency
        return await _dbContext.Books
            .Include(b => b.Author)
            .Select(b => new BookSummaryDto(
                b.Id,
                b.Title,
                b.Author.GetFullName(),
                b.IsAvailable()
            ))
            .ToListAsync(cancellationToken);
    }
}
```

### Pattern 3: Repository Pattern

Abstracts data access from domain logic:

```csharp
// Interface in Domain or Application layer
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task UpdateAsync(T entity, CancellationToken cancellationToken);
    Task DeleteAsync(T entity, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}

// Implementation in Infrastructure layer
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly LibraryDbContext _context;

    // Implementation using EF Core
}
```

---

## Conclusion

This Library Management System demonstrates:

1. **CQRS**: Separating reads (queries) from writes (commands) for clarity and scalability
2. **Domain-Centric Architecture**: Placing business logic in rich domain entities at the core
3. **Clean Separation**: Each layer has a clear responsibility
4. **Testability**: Domain logic is isolated and easy to test
5. **Maintainability**: Business rules are centralized in domain entities

The combination of these patterns creates a robust, maintainable application that clearly expresses business intent and can evolve with changing requirements.

---

## Further Reading

- **CQRS Pattern**: https://martinfowler.com/bliki/CQRS.html
- **Domain-Driven Design**: "Domain-Driven Design" by Eric Evans
- **Clean Architecture**: "Clean Architecture" by Robert C. Martin
- **MediatR Documentation**: https://github.com/jbogard/MediatR
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
