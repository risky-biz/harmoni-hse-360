using Harmoni360.Domain.Entities.Waste;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Harmoni360.Infrastructure.Persistence.Configurations;

public class WasteCommentConfiguration : IEntityTypeConfiguration<WasteComment>
{
    public void Configure(EntityTypeBuilder<WasteComment> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(w => w.Type)
            .HasConversion<int>();

        builder.HasOne(w => w.CommentedBy)
            .WithMany()
            .HasForeignKey(w => w.CommentedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("WasteComments");
    }
}
