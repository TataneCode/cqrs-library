using Library.Domain.Enums;
using MediatR;

namespace Library.Application.Commands.Books;

public record CreateBookCommand(
    string Title,
    string ISBN,
    BookType Type,
    DateTime PublishedDate,
    Guid AuthorId,
    string? Description
) : IRequest<Guid>;
