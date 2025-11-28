using MediatR;

namespace Library.Application.Commands.Notifications;

public record DeleteNotificationCommand(Guid NotificationId) : IRequest<Unit>;
