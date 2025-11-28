using Library.Domain.Enums;

namespace Library.Domain.Entities;

public class Book : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public BookType Type { get; private set; }
    public DateTime PublishedDate { get; private set; }
    public string ISBN { get; private set; } = string.Empty;

    public Guid AuthorId { get; private set; }
    public Author Author { get; private set; } = null!;

    public Guid? BorrowedByReaderId { get; private set; }
    public Reader? BorrowedByReader { get; private set; }
    public DateTime? BorrowedAt { get; private set; }
    public DateTime? DueDate { get; private set; }

    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    private Book() { } // For EF Core

    public Book(string title, string isbn, BookType type, DateTime publishedDate, Guid authorId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be empty", nameof(isbn));

        Title = title;
        ISBN = isbn;
        Type = type;
        PublishedDate = publishedDate;
        AuthorId = authorId;
        Description = description;
    }

    public bool IsAvailable() => BorrowedByReaderId == null;

    public void Borrow(Guid readerId, int borrowDurationDays = 14)
    {
        if (!IsAvailable())
            throw new InvalidOperationException($"Book '{Title}' is already borrowed");

        BorrowedByReaderId = readerId;
        BorrowedAt = DateTime.UtcNow;
        DueDate = DateTime.UtcNow.AddDays(borrowDurationDays);
        SetUpdatedAt();
    }

    public void Return()
    {
        if (IsAvailable())
            throw new InvalidOperationException($"Book '{Title}' is not currently borrowed");

        BorrowedByReaderId = null;
        BorrowedByReader = null;
        BorrowedAt = null;
        DueDate = null;
        SetUpdatedAt();
    }

    public void UpdateDetails(string title, string? description, BookType type, DateTime publishedDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        Description = description;
        Type = type;
        PublishedDate = publishedDate;
        SetUpdatedAt();
    }

    public bool IsOverdue() => DueDate.HasValue && DueDate.Value < DateTime.UtcNow;
}
