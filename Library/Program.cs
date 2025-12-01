using Library.Api;
using Library.Domain.Entities;
using Library.Infrastructure.Data;
using Library.Infrastructure.Extensions;
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

// Seed database if needed (checks NeedSeed configuration/environment variable)
await app.SeedDatabaseAsync();

await app.RunAsync();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
