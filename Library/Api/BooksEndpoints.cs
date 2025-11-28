using Library.Application.Commands.Books;
using Library.Application.Queries.Books;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public static class BooksEndpoints
{
    public static void MapBooksEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var books = await mediator.Send(new GetAllBooksQuery());
            return Results.Ok(books);
        })
        .WithName("GetAllBooks")
        .Produces(200);

        group.MapGet("/available", async (IMediator mediator) =>
        {
            var books = await mediator.Send(new GetAvailableBooksQuery());
            return Results.Ok(books);
        })
        .WithName("GetAvailableBooks")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateBookCommand command, IMediator mediator) =>
        {
            var bookId = await mediator.Send(command);
            return Results.Created($"/api/books/{bookId}", new { id = bookId });
        })
        .WithName("CreateBook")
        .Produces(201);

        group.MapPost("/{bookId:guid}/borrow", async (Guid bookId, [FromBody] BorrowRequest request, IMediator mediator) =>
        {
            await mediator.Send(new BorrowBookCommand(bookId, request.ReaderId));
            return Results.Ok();
        })
        .WithName("BorrowBook")
        .Produces(200);

        group.MapPost("/{bookId:guid}/return", async (Guid bookId, IMediator mediator) =>
        {
            await mediator.Send(new ReturnBookCommand(bookId));
            return Results.Ok();
        })
        .WithName("ReturnBook")
        .Produces(200);
    }

    private record BorrowRequest(Guid ReaderId);
}
