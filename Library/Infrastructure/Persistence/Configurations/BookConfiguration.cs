using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(1000);

        builder.Property(b => b.ISBN)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(b => b.Author)
            .WithMany(a => a.Books)
            .HasForeignKey(b => b.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.BorrowedByReader)
            .WithMany(r => r.BorrowedBooks)
            .HasForeignKey(b => b.BorrowedByReaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Notifications)
            .WithOne(n => n.Book)
            .HasForeignKey(n => n.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => b.ISBN)
            .IsUnique();

        builder.HasIndex(b => b.BorrowedByReaderId);
        builder.HasIndex(b => b.DueDate);
    }
}
