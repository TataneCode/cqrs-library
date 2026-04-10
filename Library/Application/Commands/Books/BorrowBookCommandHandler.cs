using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class BorrowBookCommandHandler(
    IBookRepository bookRepository,
    IRepository<Reader> readerRepository,
    IRepository<Notification> notificationRepository) : IRequestHandler<BorrowBookCommand, Unit>
{
    public async Task<Unit> Handle(BorrowBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.BookId} not found");

        var reader = await readerRepository.GetByIdAsync(request.ReaderId, cancellationToken);
        if (reader == null)
            throw new InvalidOperationException($"Reader with ID {request.ReaderId} not found");

        var currentlyBorrowedBooks = await bookRepository.FindAsync(
            b => b.BorrowedByReaderId == request.ReaderId, cancellationToken);
        if (currentlyBorrowedBooks.Count() >= Reader.MaxBorrowedBooks)
            throw new InvalidOperationException($"Reader has reached the maximum limit of {Reader.MaxBorrowedBooks} borrowed books");

        var pendingNotifications = await notificationRepository.FindAsync(
            n => n.BookId == request.BookId, cancellationToken);
        if (pendingNotifications.Any())
            throw new InvalidOperationException($"Book '{book.Title}' has pending return notifications and cannot be borrowed until they are resolved");

        book.Borrow(request.ReaderId);
        await bookRepository.UpdateAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
