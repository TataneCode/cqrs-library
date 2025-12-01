namespace Library.Api.Responses.Readers;

public record ReaderResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    int BorrowedBooksCount,
    int AvailableBorrowSlots,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
