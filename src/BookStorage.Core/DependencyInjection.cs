using BookStorage.Core.Interfaces.Application;
using BookStorage.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookStorage.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(
        this IServiceCollection services)
    {
        services.AddScoped<IBookService, BookService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IBookFileService, BookFileService>();
        return services;
    }
}