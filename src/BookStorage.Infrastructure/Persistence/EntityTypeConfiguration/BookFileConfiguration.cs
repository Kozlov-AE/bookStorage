using BookStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStorage.Infrastructure.Persistence.EntityTypeConfiguration;

public class BookFileConfiguration : IEntityTypeConfiguration<BookFile>
{
    public void Configure(EntityTypeBuilder<BookFile> builder)
    {
        builder.ToTable("BookFiles");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id).HasConversion<GuidConverter>();
        builder.Property(x => x.FileName).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FileSizeBytes).IsRequired();
    }
}