using Library.Domain.Enums;

namespace Library.Domain.Entities;

public class Notification : BaseEntity
{
    public string Message { get; private set; } = string.Empty;
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }

    public Guid ReaderId { get; private set; }
    public Reader Reader { get; private set; } = null!;

    public Guid BookId { get; private set; }
    public Book Book { get; private set; } = null!;

    private Notification() { } // For EF Core

    public Notification(Guid readerId, Guid bookId, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        ReaderId = readerId;
        BookId = bookId;
        Message = message;
        Status = NotificationStatus.Pending;
    }

    public void MarkAsSent()
    {
        if (Status == NotificationStatus.Sent)
            throw new InvalidOperationException("Notification has already been sent");

        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Dismiss()
    {
        Status = NotificationStatus.Dismissed;
        SetUpdatedAt();
    }
}
