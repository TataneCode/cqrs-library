using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Queries.Readers;

public class GetAllReadersQueryHandler : IRequestHandler<GetAllReadersQuery, IEnumerable<Reader>>
{
    private readonly IRepository<Reader> _readerRepository;

    public GetAllReadersQueryHandler(IRepository<Reader> readerRepository)
    {
        _readerRepository = readerRepository;
    }

    public async Task<IEnumerable<Reader>> Handle(GetAllReadersQuery request, CancellationToken cancellationToken)
    {
        return await _readerRepository.GetAllAsync(cancellationToken);
    }
}
