using Library.Api.Mappers;
using Library.Api.Requests.Readers;
using Library.Api.Responses.Common;
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
            var response = readers.ToResponses();
            return Results.Ok(response);
        })
        .WithName("GetAllReaders")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateReaderRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = request.ToCommand();
            var readerId = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/readers/{readerId}", new CreatedResourceResponse(readerId));
        })
        .WithName("CreateReader")
        .Produces(201);
    }
}
