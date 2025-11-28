using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.Message)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(n => n.Reader)
            .WithMany(r => r.Notifications)
            .HasForeignKey(n => n.ReaderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Book)
            .WithMany(b => b.Notifications)
            .HasForeignKey(n => n.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.ReaderId);
    }
}
