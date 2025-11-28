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

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var readers = await mediator.Send(new GetAllReadersQuery(), cancellationToken);
            return Results.Ok(readers);
        })
        .WithName("GetAllReaders")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateReaderCommand command, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var readerId = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/readers/{readerId}", new { id = readerId });
        })
        .WithName("CreateReader")
        .Produces(201);
    }
}
