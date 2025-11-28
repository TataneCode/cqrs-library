using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class ReturnBookCommandHandler(IRepository<Book> bookRepository) : IRequestHandler<ReturnBookCommand, Unit>
{
    public async Task<Unit> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.BookId} not found");

        book.Return();
        await bookRepository.UpdateAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
