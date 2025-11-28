using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class ReaderConfiguration : IEntityTypeConfiguration<Reader>
{
    public void Configure(EntityTypeBuilder<Reader> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.PhoneNumber)
            .HasMaxLength(20);

        builder.HasMany(r => r.BorrowedBooks)
            .WithOne(b => b.BorrowedByReader)
            .HasForeignKey(b => b.BorrowedByReaderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Notifications)
            .WithOne(n => n.Reader)
            .HasForeignKey(n => n.ReaderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Email)
            .IsUnique();
    }
}
