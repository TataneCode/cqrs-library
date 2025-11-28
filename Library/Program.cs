using Library.Api;
using Library.Domain.Entities;
using Library.Infrastructure.Data;
using Library.Infrastructure.Persistence;
using Library.Infrastructure.Repositories;
using Library.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// Add DbContext
builder.Services.AddDbContext<LibraryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// Add Repositories
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IRepository<Book>, BookRepository>();
builder.Services.AddScoped<IRepository<Author>, Repository<Author>>();
builder.Services.AddScoped<IRepository<Reader>, Repository<Reader>>();
builder.Services.AddScoped<IRepository<Notification>, Repository<Notification>>();

// Add Background Services
builder.Services.AddHostedService<BookReturnNotificationService>();

// Add DatabaseSeeder
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Map API endpoints
app.MapAuthorsEndpoints();
app.MapBooksEndpoints();
app.MapReadersEndpoints();
app.MapNotificationsEndpoints();

// Check for database seeding
var needSeed =
    builder.Configuration.GetValue<bool>("NeedSeed", false)
    || Environment.GetEnvironmentVariable("NeedSeed")?.ToLower() == "true";

if (needSeed)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("NeedSeed is set to true. Preparing database...");

        var context = services.GetRequiredService<LibraryDbContext>();

        // Run migrations
        logger.LogInformation("Applying migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Migrations applied successfully.");

        // Seed database
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
        throw;
    }
}

await app.RunAsync();
