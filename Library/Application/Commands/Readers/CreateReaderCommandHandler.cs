using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Readers;

public class CreateReaderCommandHandler(IRepository<Reader> readerRepository) : IRequestHandler<CreateReaderCommand, Guid>
{
    public async Task<Guid> Handle(CreateReaderCommand request, CancellationToken cancellationToken)
    {
        var reader = new Reader(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber
        );

        await readerRepository.AddAsync(reader, cancellationToken);
        await readerRepository.SaveChangesAsync(cancellationToken);

        return reader.Id;
    }
}
