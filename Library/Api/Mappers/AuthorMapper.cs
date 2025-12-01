using Library.Api.Requests.Authors;
using Library.Api.Responses.Authors;
using Library.Application.Commands.Authors;
using Library.Domain.Entities;

namespace Library.Api.Mappers;

public static class AuthorMapper
{
    public static CreateAuthorCommand ToCommand(this CreateAuthorRequest request)
    {
        return new CreateAuthorCommand(
            request.FirstName,
            request.LastName,
            request.Biography
        );
    }

    public static AuthorResponse ToResponse(this Author author)
    {
        return new AuthorResponse(
            author.Id,
            author.FirstName,
            author.LastName,
            author.Biography,
            author.CreatedAt,
            author.UpdatedAt
        );
    }

    public static IEnumerable<AuthorResponse> ToResponses(this IEnumerable<Author> authors)
    {
        return authors.Select(a => a.ToResponse());
    }
}
