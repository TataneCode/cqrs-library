namespace Library.Api.Responses.Authors;

public record AuthorResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string? Biography,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
