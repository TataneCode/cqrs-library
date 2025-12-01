using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Library.Domain.Entities;
using Library.Domain.Enums;

namespace Library.Infrastructure.Extensions;

public static class CsvImportExtensions
{
    /// <summary>
    /// Imports authors from a CSV file.
    /// </summary>
    /// <param name="csvPath">The path to the CSV file containing author data</param>
    /// <returns>A list of Author entities</returns>
    /// <exception cref="FileNotFoundException">Thrown when the CSV file is not found</exception>
    public static async Task<List<Author>> ImportAuthorsFromCsvAsync(string csvPath)
    {
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

        return await Task.FromResult(authors);
    }

    /// <summary>
    /// Imports readers from a CSV file.
    /// </summary>
    /// <param name="csvPath">The path to the CSV file containing reader data</param>
    /// <returns>A list of Reader entities</returns>
    /// <exception cref="FileNotFoundException">Thrown when the CSV file is not found</exception>
    public static async Task<List<Reader>> ImportReadersFromCsvAsync(string csvPath)
    {
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

        return await Task.FromResult(readers);
    }

    /// <summary>
    /// Imports book title templates from a CSV file.
    /// </summary>
    /// <param name="csvPath">The path to the CSV file containing book title data</param>
    /// <returns>A list of BookTitleCsvRecord objects</returns>
    /// <exception cref="FileNotFoundException">Thrown when the CSV file is not found</exception>
    public static async Task<List<BookTitleCsvRecord>> ImportBookTitlesFromCsvAsync(string csvPath)
    {
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

        var records = csv.GetRecords<BookTitleCsvRecord>().ToList();

        return await Task.FromResult(records);
    }

    /// <summary>
    /// Generates books from book title templates and authors.
    /// </summary>
    /// <param name="bookTemplates">List of book title templates from CSV</param>
    /// <param name="authors">List of authors to assign to books</param>
    /// <param name="count">Number of books to generate</param>
    /// <param name="randomSeed">Seed for random number generation (for reproducibility)</param>
    /// <param name="startIndex">Starting index for book generation (for batch processing)</param>
    /// <returns>A list of Book entities</returns>
    public static List<Book> GenerateBooksFromTemplates(
        List<BookTitleCsvRecord> bookTemplates,
        List<Author> authors,
        int count,
        int randomSeed = 42,
        int startIndex = 0)
    {
        var books = new List<Book>();
        var random = new Random(randomSeed);

        // Skip ahead in the random sequence to match the startIndex
        for (int skip = 0; skip < startIndex * 3; skip++)
        {
            random.Next();
        }

        for (int i = 0; i < count; i++)
        {
            var globalIndex = startIndex + i;
            var templateIndex = globalIndex % bookTemplates.Count;
            var template = bookTemplates[templateIndex];
            var author = authors[globalIndex % authors.Count];

            // Create variations of titles for uniqueness
            var titleSuffix = globalIndex / bookTemplates.Count;
            var title = titleSuffix == 0
                ? template.Title
                : $"{template.Title} - Edition {titleSuffix + 1}";

            // Generate ISBN-13
            var isbn = GenerateIsbn(globalIndex);

            // Parse BookType
            var bookType = Enum.Parse<BookType>(template.BookType);

            // Vary publication years slightly
            var yearVariation = random.Next(-2, 3);
            var publishedYear = template.Year + yearVariation;
            var publishedDate = new DateTime(
                Math.Max(1800, Math.Min(DateTime.UtcNow.Year, publishedYear)),
                random.Next(1, 13),
                random.Next(1, 28),
                0, 0, 0,
                DateTimeKind.Utc
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

        return books;
    }

    /// <summary>
    /// Generates a simple ISBN-13 for a book.
    /// Note: This does not generate a valid ISBN check digit, but ensures uniqueness.
    /// </summary>
    /// <param name="seed">Seed value for generating the ISBN</param>
    /// <returns>A 13-character ISBN string</returns>
    private static string GenerateIsbn(int seed)
    {
        var isbn = $"978{seed:D10}";
        return isbn.Length > 13 ? isbn[..13] : isbn.PadRight(13, '0');
    }
}

/// <summary>
/// CSV record for importing author data.
/// </summary>
public class AuthorCsvRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Biography { get; set; } = string.Empty;
}

/// <summary>
/// CSV record for importing reader data.
/// </summary>
public class ReaderCsvRecord
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

/// <summary>
/// CSV record for importing book title templates.
/// </summary>
public class BookTitleCsvRecord
{
    public string Title { get; set; } = string.Empty;
    public string BookType { get; set; } = string.Empty;
    public int Year { get; set; }
}
