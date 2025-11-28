using MediatR;

namespace Library.Application.Commands.Readers;

public record CreateReaderCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
) : IRequest<Guid>;
