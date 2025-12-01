namespace Library.Api.Requests.Authors;

public record CreateAuthorRequest(
    string FirstName,
    string LastName,
    string? Biography
);
