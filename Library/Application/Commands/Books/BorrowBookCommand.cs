using MediatR;

namespace Library.Application.Commands.Books;

public record BorrowBookCommand(
    Guid BookId,
    Guid ReaderId
) : IRequest<Unit>;
