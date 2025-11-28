using Library.Application.Commands.Readers;
using Library.Application.Queries.Readers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public static class ReadersEndpoints
{
    public static void MapReadersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/readers").WithTags("Readers");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var readers = await mediator.Send(new GetAllReadersQuery());
            return Results.Ok(readers);
        })
        .WithName("GetAllReaders")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateReaderCommand command, IMediator mediator) =>
        {
            var readerId = await mediator.Send(command);
            return Results.Created($"/api/readers/{readerId}", new { id = readerId });
        })
        .WithName("CreateReader")
        .Produces(201);
    }
}
