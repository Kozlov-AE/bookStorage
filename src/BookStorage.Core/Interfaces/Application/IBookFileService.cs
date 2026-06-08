using BookStorage.Core.Entities;

namespace BookStorage.Core.Interfaces.Application;

public interface IBookFileService
{
    Task<(Stream? Content, BookFile? Metadata)> GetByIdAsync(Guid id, CancellationToken ct = default);
}
