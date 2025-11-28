using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using MediatR;

namespace Library.Application.Commands.Notifications;

public class DeleteNotificationCommandHandler(IRepository<Notification> notificationRepository) : IRequestHandler<DeleteNotificationCommand, Unit>
{
    public async Task<Unit> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification == null)
            throw new InvalidOperationException($"Notification with ID {request.NotificationId} not found");

        await notificationRepository.DeleteAsync(notification, cancellationToken);
        await notificationRepository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
