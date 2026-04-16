using BookStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStorage.Infrastructure.Persistence.EntityTypeConfiguration;

public class LanguageConfiguration : IEntityTypeConfiguration<Language>
{
    public void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable("Languages");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Code).IsRequired().HasMaxLength(10);
        
        builder.HasIndex(x => x.Code).IsUnique();
    }
}