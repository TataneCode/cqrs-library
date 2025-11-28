using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Queries.Authors;

public class GetAllAuthorsQueryHandler(IRepository<Author> authorRepository) : IRequestHandler<GetAllAuthorsQuery, IEnumerable<Author>>
{
    public async Task<IEnumerable<Author>> Handle(GetAllAuthorsQuery request, CancellationToken cancellationToken)
    {
        return await authorRepository.GetAllAsync(cancellationToken);
    }
}
