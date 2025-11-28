using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Library.Domain.Entities;
using Library.Domain.Enums;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Data;

public class DatabaseSeeder
{
    private readonly LibraryDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(LibraryDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if already seeded
            if (await _context.Set<Author>().AnyAsync())
            {
                _logger.LogInformation("Database already seeded. Skipping...");
                return;
            }

            // Seed Authors
            var authors = await SeedAuthorsAsync();
            _logger.LogInformation("Seeded {Count} authors", authors.Count);

            // Seed Readers
            var readers = await SeedReadersAsync();
            _logger.LogInformation("Seeded {Count} readers", readers.Count);

            // Seed Books
            var bookCount = await SeedBooksAsync(authors);
            _logger.LogInformation("Seeded {Count} books", bookCount);

            _logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task<List<Author>> SeedAuthorsAsync()
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "authors.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Authors CSV file not found at {csvPath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<AuthorCsvRecord>().ToList();
        var authors = new List<Author>();

        foreach (var record in records)
        {
            var author = new Author(record.FirstName, record.LastName, record.Biography);
            authors.Add(author);
        }

        await _context.Set<Author>().AddRangeAsync(authors);
        await _context.SaveChangesAsync();

        return authors;
    }

    private async Task<List<Reader>> SeedReadersAsync()
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "readers.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Readers CSV file not found at {csvPath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<ReaderCsvRecord>().ToList();
        var readers = new List<Reader>();

        foreach (var record in records)
        {
            var readerEntity = new Reader(
                record.FirstName,
                record.LastName,
                record.Email,
                record.PhoneNumber
            );
            readers.Add(readerEntity);
        }

        await _context.Set<Reader>().AddRangeAsync(readers);
        await _context.SaveChangesAsync();

        return readers;
    }

    private async Task<int> SeedBooksAsync(List<Author> authors)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "book-titles.csv");

        if (!File.Exists(csvPath))
        {
            throw new FileNotFoundException($"Book titles CSV file not found at {csvPath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };

        using var reader = new StreamReader(csvPath);
        using var csv = new CsvReader(reader, config);

        var bookTemplates = csv.GetRecords<BookTitleCsvRecord>().ToList();
        var books = new List<Book>();
        var random = new Random(42); // Fixed seed for reproducibility

        const int targetBookCount = 10000;
        var booksPerBatch = 1000;
        var totalBooks = 0;

        // Generate books in batches
        for (int batch = 0; batch < targetBookCount / booksPerBatch; batch++)
        {
            books.Clear();

            for (int i = 0; i < booksPerBatch; i++)
            {
                var templateIndex = (batch * booksPerBatch + i) % bookTemplates.Count;
                var template = bookTemplates[templateIndex];
                var author = authors[(batch * booksPerBatch + i) % authors.Count];

                // Create variations of titles for uniqueness
                var titleSuffix = (batch * booksPerBatch + i) / bookTemplates.Count;
                var title = titleSuffix == 0
                    ? template.Title
                    : $"{template.Title} - Edition {titleSuffix + 1}";

                // Generate ISBN-13
                var isbn = GenerateIsbn(batch * booksPerBatch + i);

                // Parse BookType
                var bookType = Enum.Parse<BookType>(template.BookType);

                // Vary publication years slightly
                var yearVariation = random.Next(-2, 3);
                var publishedYear = template.Year + yearVariation;
                var publishedDate = new DateTime(
                    Math.Max(1800, Math.Min(DateTime.UtcNow.Year, publishedYear)),
                    random.Next(1, 13),
                    random.Next(1, 28)
                );

                var book = new Book(
                    title,
                    isbn,
                    bookType,
                    publishedDate,
                    author.Id
                );

                books.Add(book);
            }

            await _context.Set<Book>().AddRangeAsync(books);
            await _context.SaveChangesAsync();

            totalBooks += books.Count;
            _logger.LogInformation("Seeded batch {Batch}: {Count} books (Total: {Total})",
                batch + 1, books.Count, totalBooks);
        }

        return totalBooks;
    }

    private static string GenerateIsbn(int seed)
    {
        // Generate a simple ISBN-13 (not a valid check digit, but unique)
        var isbn = $"978{seed:D10}";
        return isbn.Length > 13 ? isbn[..13] : isbn.PadRight(13, '0');
    }
}

public class AuthorCsvRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
}

public class ReaderCsvRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class BookTitleCsvRecord
{
    public string Title { get; set; } = string.Empty;
    public string BookType { get; set; } = string.Empty;
    public int Year { get; set; }
}
