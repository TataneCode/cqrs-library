using Library.Application.Commands.Authors;
using Library.Application.Queries.Authors;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api;

public static class AuthorsEndpoints
{
    public static void MapAuthorsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/authors").WithTags("Authors");

        group.MapGet("/", async (IMediator mediator) =>
        {
            var authors = await mediator.Send(new GetAllAuthorsQuery());
            return Results.Ok(authors);
        })
        .WithName("GetAllAuthors")
        .Produces(200);

        group.MapPost("/", async ([FromBody] CreateAuthorCommand command, IMediator mediator) =>
        {
            var authorId = await mediator.Send(command);
            return Results.Created($"/api/authors/{authorId}", new { id = authorId });
        })
        .WithName("CreateAuthor")
        .Produces(201);
    }
}
