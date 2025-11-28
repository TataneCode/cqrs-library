using Library.Domain.Entities;
using Library.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Library.Infrastructure.Services;

public class BookReturnNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookReturnNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1);

    public BookReturnNotificationService(
        IServiceProvider serviceProvider,
        ILogger<BookReturnNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Book Return Notification Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndCreateNotificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for overdue books.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Book Return Notification Service is stopping.");
    }

    private async Task CheckAndCreateNotificationsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var bookRepository = scope.ServiceProvider.GetRequiredService<IBookRepository>();
        var notificationRepository = scope.ServiceProvider.GetRequiredService<IRepository<Notification>>();

        var overdueBooks = await bookRepository.GetOverdueBooksAsync(cancellationToken);
        var overdueBooksList = overdueBooks.ToList();

        if (!overdueBooksList.Any())
        {
            _logger.LogInformation("No overdue books found.");
            return;
        }

        _logger.LogInformation("Found {Count} overdue books.", overdueBooksList.Count);

        foreach (var book in overdueBooksList)
        {
            if (book.BorrowedByReaderId == null || book.DueDate == null)
                continue;

            var existingNotifications = await notificationRepository.FindAsync(
                n => n.BookId == book.Id && n.ReaderId == book.BorrowedByReaderId.Value,
                cancellationToken);

            if (existingNotifications.Any())
            {
                _logger.LogDebug("Notification already exists for book {BookId}", book.Id);
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
            _logger.LogInformation("Created notification for overdue book: {BookTitle} (Reader: {ReaderId})",
                book.Title, book.BorrowedByReaderId.Value);
        }

        await notificationRepository.SaveChangesAsync(cancellationToken);
    }
}
