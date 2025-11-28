using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Readers;

public class CreateReaderCommandHandler : IRequestHandler<CreateReaderCommand, Guid>
{
    private readonly IRepository<Reader> _readerRepository;

    public CreateReaderCommandHandler(IRepository<Reader> readerRepository)
    {
        _readerRepository = readerRepository;
    }

    public async Task<Guid> Handle(CreateReaderCommand request, CancellationToken cancellationToken)
    {
        var reader = new Reader(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber
        );

        await _readerRepository.AddAsync(reader, cancellationToken);
        await _readerRepository.SaveChangesAsync(cancellationToken);

        return reader.Id;
    }
}
