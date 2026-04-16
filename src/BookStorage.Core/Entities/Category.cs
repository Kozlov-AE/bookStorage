using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Category : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
}
