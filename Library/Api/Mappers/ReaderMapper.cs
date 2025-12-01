using Library.Api.Requests.Readers;
using Library.Api.Responses.Readers;
using Library.Application.Commands.Readers;
using Library.Domain.Entities;

namespace Library.Api.Mappers;

public static class ReaderMapper
{
    public static CreateReaderCommand ToCommand(this CreateReaderRequest request)
    {
        return new CreateReaderCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber
        );
    }

    public static ReaderResponse ToResponse(this Reader reader)
    {
        return new ReaderResponse(
            reader.Id,
            reader.FirstName,
            reader.LastName,
            reader.Email,
            reader.PhoneNumber,
            reader.BorrowedBooks.Count,
            reader.AvailableBorrowSlots(),
            reader.CreatedAt,
            reader.UpdatedAt
        );
    }

    public static IEnumerable<ReaderResponse> ToResponses(this IEnumerable<Reader> readers)
    {
        return readers.Select(r => r.ToResponse());
    }
}
