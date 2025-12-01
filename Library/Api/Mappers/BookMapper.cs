using Library.Api.Requests.Books;
using Library.Api.Responses.Books;
using Library.Application.Commands.Books;
using Library.Domain.Entities;

namespace Library.Api.Mappers;

public static class BookMapper
{
    public static CreateBookCommand ToCommand(this CreateBookRequest request)
    {
        return new CreateBookCommand(
            request.Title,
            request.ISBN,
            request.Type,
            request.PublishedDate,
            request.AuthorId,
            request.Description
        );
    }

    public static BorrowBookCommand ToBorrowCommand(this BorrowBookRequest request, Guid bookId)
    {
        return new BorrowBookCommand(bookId, request.ReaderId);
    }

    public static BookResponse ToResponse(this Book book)
    {
        return new BookResponse(
            book.Id,
            book.Title,
            book.Description,
            book.Type,
            book.PublishedDate,
            book.ISBN,
            book.AuthorId,
            book.Author?.GetFullName() ?? string.Empty,
            book.BorrowedByReaderId,
            book.BorrowedByReader?.GetFullName(),
            book.BorrowedAt,
            book.DueDate,
            book.IsAvailable(),
            book.CreatedAt,
            book.UpdatedAt
        );
    }

    public static IEnumerable<BookResponse> ToResponses(this IEnumerable<Book> books)
    {
        return books.Select(b => b.ToResponse());
    }
}
