using Library.Api.Mappers;
using Library.Api.Requests.Authors;
using Library.Api.Responses.Common;
using Library.Application.Queries.Authors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public static class AuthorsEndpoints
{
    public static void MapAuthorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
        {
            var authors = await mediator.Send(new GetAllAuthorsQuery(), cancellationToken);
            var response = authors.ToResponses();
            return Results.Ok(response);
        })
        .WithName("GetAllAuthors")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateAuthorRequest request, IMediator mediator, CancellationToken cancellationToken) =>
        {
            var command = request.ToCommand();
            var authorId = await mediator.Send(command, cancellationToken);
            return Results.Created($"/api/authors/{authorId}", new CreatedResourceResponse(authorId));
        })
        .WithName("CreateAuthor")
        .Produces(201);
    }
}
