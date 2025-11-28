using Library.Domain.Entities;
using MediatR;

namespace Library.Application.Queries.Authors;

public record GetAllAuthorsQuery : IRequest<IEnumerable<Author>>;
