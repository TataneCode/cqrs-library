using Library.Domain.Enums;

namespace Library.Api.Requests.Books;

public record CreateBookRequest(
    string Title,
    string ISBN,
    BookType Type,
    DateTime PublishedDate,
    Guid AuthorId,
    string? Description
);
