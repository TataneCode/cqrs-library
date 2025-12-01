namespace Library.Api.Requests.Readers;

public record CreateReaderRequest(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber
);
