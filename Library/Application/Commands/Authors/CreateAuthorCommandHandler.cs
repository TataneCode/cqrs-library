using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Authors;

public class CreateAuthorCommandHandler(IRepository<Author> authorRepository) : IRequestHandler<CreateAuthorCommand, Guid>
{
    public async Task<Guid> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = new Author(
            request.FirstName,
            request.LastName,
            request.Biography
        );

        await authorRepository.AddAsync(author, cancellationToken);
        await authorRepository.SaveChangesAsync(cancellationToken);

        return author.Id;
    }
}
