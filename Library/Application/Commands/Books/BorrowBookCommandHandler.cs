using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class BorrowBookCommandHandler : IRequestHandler<BorrowBookCommand, Unit>
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<Reader> _readerRepository;

    public BorrowBookCommandHandler(
        IRepository<Book> bookRepository,
        IRepository<Reader> readerRepository)
    {
        _bookRepository = bookRepository;
        _readerRepository = readerRepository;
    }

    public async Task<Unit> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.BookId} not found");

        var reader = await _readerRepository.GetByIdAsync(request.ReaderId, cancellationToken);
        if (reader == null)
            throw new InvalidOperationException($"Reader with ID {request.ReaderId} not found");

        if (!reader.CanBorrowMoreBooks())
            throw new InvalidOperationException($"Reader has reached the maximum limit of {Reader.MaxBorrowedBooks} borrowed books");

        book.Borrow(request.ReaderId);
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
