using Library.Domain.Enums;

namespace Library.Api.Responses.Books;

public record BookResponse(
    Guid Id,
    string Title,
    string? Description,
    BookType Type,
    DateTime PublishedDate,
    string ISBN,
    Guid AuthorId,
    string AuthorName,
    Guid? BorrowedByReaderId,
    string? BorrowedByReaderName,
    DateTime? BorrowedAt,
    DateTime? DueDate,
    bool IsAvailable,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
