namespace BookStorage.Core.Services;

public class BookService
{
    public Task<bool> TryAddBook()
    {
        return Task.FromResult(true);
    }
}