using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Queries.Books;

public class GetAvailableBooksQueryHandler(IBookRepository bookRepository) : IRequestHandler<GetAvailableBooksQuery, IEnumerable<Book>>
{
    public async Task<IEnumerable<Book>> Handle(GetAvailableBooksQuery request, CancellationToken cancellationToken)
    {
        return await bookRepository.GetAvailableBooksAsync(cancellationToken);
    }
}
