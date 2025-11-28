using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class ReturnBookCommandHandler : IRequestHandler<ReturnBookCommand, Unit>
{
    private readonly IRepository<Book> _bookRepository;

    public ReturnBookCommandHandler(IRepository<Book> bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Unit> Handle(ReturnBookCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book == null)
            throw new InvalidOperationException($"Book with ID {request.BookId} not found");

        book.Return();
        await _bookRepository.UpdateAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
