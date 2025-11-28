using MediatR;

namespace Library.Application.Commands.Books;

public record ReturnBookCommand(Guid BookId) : IRequest<Unit>;
