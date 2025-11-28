using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Queries.Readers;

public class GetAllReadersQueryHandler(IRepository<Reader> readerRepository) : IRequestHandler<GetAllReadersQuery, IEnumerable<Reader>>
{
    public async Task<IEnumerable<Reader>> Handle(GetAllReadersQuery request, CancellationToken cancellationToken)
    {
        return await readerRepository.GetAllAsync(cancellationToken);
    }
}
