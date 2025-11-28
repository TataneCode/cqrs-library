using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class BorrowBookCommandHandler(
    IRepository<Book> bookRepository,
    IRepository<Reader> readerRepository) : IRequestHandler<BorrowBookCommand, Unit>
{
    public async Task<Unit> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.BookId} not found");

        var reader = await readerRepository.GetByIdAsync(request.ReaderId, cancellationToken);
        if (reader == null)
            throw new InvalidOperationException($"Reader with ID {request.ReaderId} not found");

        if (!reader.CanBorrowMoreBooks())
            throw new InvalidOperationException($"Reader has reached the maximum limit of {Reader.MaxBorrowedBooks} borrowed books");

        book.Borrow(request.ReaderId);
        await bookRepository.UpdateAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
