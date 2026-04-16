using BookStorage.Core.Interfaces.Models;

namespace BookStorage.Core.Entities;

public class Publisher : IHasId
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public required string Name { get; set; }
}
