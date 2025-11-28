using MediatR;

namespace Library.Application.Commands.Authors;

public record CreateAuthorCommand(
    string FirstName,
    string LastName,
    string? Biography
) : IRequest<Guid>;
