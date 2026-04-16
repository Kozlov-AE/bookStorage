using BookStorage.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookStorage.Infrastructure.Persistence.EntityTypeConfiguration;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");
        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.FirstName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.LastName).IsRequired().HasMaxLength(200);
    }
}