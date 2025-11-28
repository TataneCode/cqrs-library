namespace Library.Domain.Entities;

public class Reader : BaseEntity
{
    public const int MaxBorrowedBooks = 3;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }

    private readonly List<Book> _borrowedBooks = new();
    public IReadOnlyCollection<Book> BorrowedBooks => _borrowedBooks.AsReadOnly();

    private readonly List<Notification> _notifications = new();
    public IReadOnlyCollection<Notification> Notifications => _notifications.AsReadOnly();

    private Reader() { } // For EF Core

    public Reader(string firstName, string lastName, string email, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public bool CanBorrowMoreBooks() => _borrowedBooks.Count < MaxBorrowedBooks;

    public int AvailableBorrowSlots() => MaxBorrowedBooks - _borrowedBooks.Count;

    public void UpdateDetails(string firstName, string lastName, string email, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        SetUpdatedAt();
    }

    public string GetFullName() => $"{FirstName} {LastName}";
}
