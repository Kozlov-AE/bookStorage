using BookStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStorage.Infrastructure.Persistence.EntityTypeConfiguration;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");
        builder.HasKey(x => x.Id);

        
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.CreatedAt).IsRequired();
        
        builder.Property(x => x.Id).HasConversion<GuidConverter>();
        builder.Property(x => x.CategoryId).HasConversion<NullableGuidConverter>();
        
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(x => x.Authors)
            .WithMany(p => p.AuthoredBooks)
            .UsingEntity("BookAuthors");
        
        builder.HasMany(b => b.Files)
            .WithMany(f => f.Books)
            .UsingEntity("BookBookFiles");
    }
}