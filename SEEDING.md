# Database Seeding

This project includes an automated database seeding feature to populate the library database with test data.

## Overview

The seeding process will create:
- **107 Authors** - Famous authors from various literary genres
- **100 Readers** - Users with realistic names and contact information
- **10,000 Books** - Books of different types (novels, comics, mangas, newspapers) from various authors

## How to Enable Seeding

Seeding is controlled by the `NeedSeed` environment variable or configuration setting. When set to `true`, the application will:
1. Ensure the database exists
2. Apply any pending migrations
3. Seed the database with initial data (if not already seeded)

### Option 1: Using Environment Variable

Set the environment variable before running the application:

**Linux/macOS:**
```bash
export NeedSeed=true
dotnet run --project Library/Library.csproj
```

**Windows (PowerShell):**
```powershell
$env:NeedSeed="true"
dotnet run --project Library/Library.csproj
```

**Windows (CMD):**
```cmd
set NeedSeed=true
dotnet run --project Library/Library.csproj
```

### Option 2: Using appsettings.json

Add the following to your `appsettings.json` or `appsettings.Development.json`:

```json
{
  "NeedSeed": true
}
```

Then run the application normally:
```bash
dotnet run --project Library/Library.csproj
```

### Option 3: Using Docker Compose

Set the environment variable in your `docker-compose.yml`:

```yaml
services:
  library-api:
    environment:
      - NeedSeed=true
```

## Seed Data Sources

The seed data is loaded from CSV files located in `Library/Data/Seeds/`:

- **authors.csv** - Contains 107 famous authors with biographies
- **readers.csv** - Contains 100 readers with realistic names and contact details
- **book-titles.csv** - Contains book title templates used to generate 10,000 books

## How Seeding Works

1. **Authors**: All authors from `authors.csv` are created first
2. **Readers**: All 100 readers from `readers.csv` are created
3. **Books**: The seeder creates 10,000 books by:
   - Cycling through book titles from `book-titles.csv`
   - Assigning authors in rotation
   - Creating variations of titles (e.g., "Pride and Prejudice - Edition 2")
   - Generating unique ISBN-13 numbers
   - Varying publication years slightly for realism

Books are seeded in batches of 1,000 to optimize database performance.

## Database Safety

- The seeder checks if data already exists before seeding
- If authors already exist in the database, seeding is skipped
- This prevents duplicate data on multiple application starts

## Performance

Seeding 10,000 books takes approximately 30-60 seconds depending on your database configuration and hardware.

## Customizing Seed Data

To customize the seed data:

1. Edit the CSV files in `Library/Data/Seeds/`
2. Maintain the CSV header format
3. For BookType, use: `Novel`, `Comic`, `Manga`, or `Newspaper`
4. Rebuild the application to copy updated CSV files

## Example Output

When seeding runs successfully, you'll see logs like:

```
info: Program[0]
      NeedSeed is set to true. Preparing database...
info: Program[0]
      Database ensured to exist.
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Starting database seeding...
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded 107 authors
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded 100 readers
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded batch 1: 1000 books (Total: 1000)
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded batch 2: 1000 books (Total: 2000)
...
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded batch 10: 1000 books (Total: 10000)
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Seeded 10000 books
info: Library.Infrastructure.Data.DatabaseSeeder[0]
      Database seeding completed successfully!
```

## Troubleshooting

**Seeding doesn't run:**
- Verify the `NeedSeed` environment variable is set to `true` (case-insensitive)
- Check application logs for errors
- Ensure PostgreSQL is running and accessible

**CSV files not found:**
- Verify CSV files exist in `Library/Data/Seeds/`
- Ensure the build copies CSV files to output directory (check Library.csproj)

**Database connection errors:**
- Check connection string in `appsettings.json`
- Verify PostgreSQL is running on the expected port
- Ensure database user has create/write permissions
