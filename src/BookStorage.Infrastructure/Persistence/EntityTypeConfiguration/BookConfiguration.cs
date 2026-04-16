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
        
        builder.HasOne(x => x.Publisher)
            .WithMany()
            .HasForeignKey(x => x.PublisherId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.Language)
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(x => x.Country)
            .WithMany()
            .HasForeignKey(x => x.CountryId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasMany(x => x.Genres)
            .WithMany(g => g.Books)
            .UsingEntity("BookGenres");
        
        builder.HasMany(x => x.Authors)
            .WithMany(p => p.AuthoredBooks)
            .UsingEntity("BookAuthors");
        
        builder.HasMany(x => x.Translators)
            .WithMany(p => p.TranslatedBooks)
            .UsingEntity("BookTranslators");
        
        builder.HasMany(b => b.Files)
            .WithMany(f => f.Books)
            .UsingEntity("BookBookFiles");
    }
}