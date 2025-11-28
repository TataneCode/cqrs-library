using Library.Application.Commands.Notifications;
using MediatR;

namespace Library.Api;

public static class NotificationsEndpoints
{
    public static void MapNotificationsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").WithTags("Notifications");

        group.MapDelete("/{notificationId:guid}", async (Guid notificationId, IMediator mediator, CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteNotificationCommand(notificationId), cancellationToken);
            return Results.NoContent();
        })
        .WithName("DeleteNotification")
        .Produces(204);
    }
}
