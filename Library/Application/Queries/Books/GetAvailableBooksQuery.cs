using Library.Domain.Entities;
using MediatR;

namespace Library.Application.Queries.Books;

public record GetAvailableBooksQuery : IRequest<IEnumerable<Book>>;
