using Library.Domain.Entities;
using MediatR;

namespace Library.Application.Queries.Readers;

public record GetAllReadersQuery : IRequest<IEnumerable<Reader>>;
