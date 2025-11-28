using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Books;

public class CreateBookCommandHandler(
    IRepository<Book> bookRepository,
    IRepository<Author> authorRepository) : IRequestHandler<CreateBookCommand, Guid>
{
    public async Task<Guid> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var author = await authorRepository.GetByIdAsync(request.AuthorId, cancellationToken);
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

        await bookRepository.AddAsync(book, cancellationToken);
        await bookRepository.SaveChangesAsync(cancellationToken);

        return book.Id;
    }
}
