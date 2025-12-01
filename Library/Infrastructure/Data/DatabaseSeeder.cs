using Library.Domain.Entities;
using Library.Infrastructure.Extensions;
using Library.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Library.Infrastructure.Data;

public class DatabaseSeeder(LibraryDbContext context, ILogger<DatabaseSeeder> logger)
{
    private const int TargetBookCount = 10000;
    private const int BooksPerBatch = 1000;

    public async Task SeedAsync()
    {
        try
        {
            logger.LogInformation("Starting database seeding...");

            // Check if already seeded
            if (await context.Set<Author>().AnyAsync())
            {
                logger.LogInformation("Database already seeded. Skipping...");
                return;
            }

            // Seed Authors
            var authors = await SeedAuthorsAsync();
            logger.LogInformation("Seeded {Count} authors", authors.Count);

            // Seed Readers
            var readers = await SeedReadersAsync();
            logger.LogInformation("Seeded {Count} readers", readers.Count);

            // Seed Books
            var bookCount = await SeedBooksAsync(authors);
            logger.LogInformation("Seeded {Count} books", bookCount);

            logger.LogInformation("Database seeding completed successfully!");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while seeding database");
            throw;
        }
    }

    private async Task<List<Author>> SeedAuthorsAsync()
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "authors.csv");
        var authors = await CsvImportExtensions.ImportAuthorsFromCsvAsync(csvPath);

        await context.Set<Author>().AddRangeAsync(authors);
        await context.SaveChangesAsync();

        return authors;
    }

    private async Task<List<Reader>> SeedReadersAsync()
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "readers.csv");
        var readers = await CsvImportExtensions.ImportReadersFromCsvAsync(csvPath);

        await context.Set<Reader>().AddRangeAsync(readers);
        await context.SaveChangesAsync();

        return readers;
    }

    private async Task<int> SeedBooksAsync(List<Author> authors)
    {
        var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seeds", "book-titles.csv");
        var bookTemplates = await CsvImportExtensions.ImportBookTitlesFromCsvAsync(csvPath);

        var totalBooks = 0;

        // Generate and seed books in batches
        for (int batch = 0; batch < TargetBookCount / BooksPerBatch; batch++)
        {
            var startIndex = batch * BooksPerBatch;

            // Generate only the books for this batch
            var booksInBatch = CsvImportExtensions.GenerateBooksFromTemplates(
                bookTemplates,
                authors,
                BooksPerBatch,
                randomSeed: 42,
                startIndex: startIndex
            );

            await context.Set<Book>().AddRangeAsync(booksInBatch);
            await context.SaveChangesAsync();

            totalBooks += booksInBatch.Count;
            logger.LogInformation("Seeded batch {Batch}: {Count} books (Total: {Total})",
                batch + 1, booksInBatch.Count, totalBooks);
        }

        return totalBooks;
    }
}
