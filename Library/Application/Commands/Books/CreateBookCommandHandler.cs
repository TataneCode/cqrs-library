using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, Guid>
{
    private readonly IRepository<Book> _bookRepository;
    private readonly IRepository<Author> _authorRepository;

    public CreateBookCommandHandler(
        IRepository<Book> bookRepository,
        IRepository<Author> authorRepository)
    {
        _bookRepository = bookRepository;
        _authorRepository = authorRepository;
    }

    public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var author = await _authorRepository.GetByIdAsync(request.AuthorId, cancellationToken);
        if (author == null)
            throw new InvalidOperationException($"Author with ID {request.AuthorId} not found");

        var book = new Book(
            request.Title,
            request.ISBN,
            request.Type,
            request.PublishedDate,
            request.AuthorId,
            request.Description
        );

        await _bookRepository.AddAsync(book, cancellationToken);
        await _bookRepository.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}
