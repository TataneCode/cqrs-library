using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure.Services;

public class BookReturnNotificationService(
    IServiceProvider serviceProvider,
    ILogger<BookReturnNotificationService> logger) : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Book Return Notification Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCreateNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while checking for overdue books.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        logger.LogInformation("Book Return Notification Service is stopping.");
    }

    private async Task CheckAndCreateNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();

        var overdueBooks = await bookRepository.GetOverdueBooksAsync(cancellationToken);
        var overdueBooksList = overdueBooks.ToList();

        if (!overdueBooksList.Any())
        {
            logger.LogInformation("No overdue books found.");
            return;
        }

        logger.LogInformation("Found {Count} overdue books.", overdueBooksList.Count);

        foreach (var book in overdueBooksList)
        {
            if (book.BorrowedByReaderId == null || book.DueDate == null)
                continue;

            var existingNotifications = await notificationRepository.FindAsync(
                n => n.BookId == book.Id && n.ReaderId == book.BorrowedByReaderId.Value,
                cancellationToken);

            if (existingNotifications.Any())
            {
                logger.LogDebug("Notification already exists for book {BookId}", book.Id);
                continue;
            }

            var daysOverdue = (DateTime.UtcNow - book.DueDate.Value).Days;
            var message = $"The book '{book.Title}' is {daysOverdue} day(s) overdue. Please return it to the library.";

            var notification = new Notification(
                book.BorrowedByReaderId.Value,
                book.Id,
                message
            );

            await notificationRepository.AddAsync(notification, cancellationToken);
            logger.LogInformation("Created notification for overdue book: {BookTitle} (Reader: {ReaderId})",
                book.Title, book.BorrowedByReaderId.Value);
        }

        await notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
