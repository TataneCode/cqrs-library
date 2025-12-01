using Library.Api.Mappers;
using Library.Api.Requests.Books;
using Library.Api.Responses.Common;
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

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var books = await mediator.Send(new GetAllBooksQuery(), cancellationToken);
            var response = books.ToResponses();
            return Results.Ok(response);
        })
        .WithName("GetAllBooks")
        .Produces(200);

        group.MapGet("/available", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var books = await mediator.Send(new GetAvailableBooksQuery(), cancellationToken);
            var response = books.ToResponses();
            return Results.Ok(response);
        })
        .WithName("GetAvailableBooks")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateBookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = request.ToCommand();
            var bookId = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/books/{bookId}", new CreatedResourceResponse(bookId));
        })
        .WithName("CreateBook")
        .Produces(201);

        group.MapPost("/{bookId:guid}/borrow", async (Guid bookId, [FromBody] BorrowBookRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = request.ToBorrowCommand(bookId);
            await mediator.Send(command, cancellationToken);
            return Results.Ok();
        })
        .WithName("BorrowBook")
        .Produces(200);

        group.MapPost("/{bookId:guid}/return", async (Guid bookId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ReturnBookCommand(bookId), cancellationToken);
            return Results.Ok();
        })
        .WithName("ReturnBook")
        .Produces(200);
    }
}
