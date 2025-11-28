using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Queries.Books;

public class GetAllBooksQueryHandler(IRepository<Book> bookRepository) : IRequestHandler<GetAllBooksQuery, IEnumerable<Book>>
{
    public async Task<IEnumerable<Book>> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        return await bookRepository.GetAllAsync(cancellationToken);
    }
}
