using BookStorage.Api.DTOs;
using BookStorage.Api.Extensions;
using BookStorage.Core.Entities;

namespace BookStorage.Api.Mapping.MapsterProfiles;

public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        PersonToPersonDto(config);
        PersonDtoToPerson(config);
        
        CategoryToCategoryDto(config);
        CategoryDtoToCategory(config);
        CreateCategoryRequestDtoToCategory(config);
        UpdateCategoryRequestDtoToCategory(config);

        BookToBookDto(config);
        BookToBookListItemDto(config);
        CreateBookRequestToBook(config);
        
        BookFileToBookFileDto(config);
    }
    
    #region Person Mappings
    
    private static void PersonToPersonDto(TypeAdapterConfig config)
    {
        config.NewConfig<Person, PersonDto>()
            .ConstructUsing(src => new PersonDto(src.Id.ToNullableString(), src.FullName, src.Birthday));
    }
    
    private static void PersonDtoToPerson(TypeAdapterConfig config)
    {
        config.NewConfig<PersonDto, Person>()
            .ConstructUsing(src => new Person { Id = src.Id.ToGuid(), FullName = src.FullName, Birthday = src.Birthday });
    }
    
    #endregion
    
    #region Category Mappings
    
    private static void CategoryToCategoryDto(TypeAdapterConfig config)
    {
        config.NewConfig<Category, CategoryDto>()
            .ConstructUsing(src => new CategoryDto(src.Name))
            .Map(dest => dest.Id, src => src.Id.ToNullableString())
            .Map(d => d.ParentCategoryId, src => src.ParentCategoryId.ToNullableString())
            .Map(d => d.SubCategories, src => src.SubCategories.Count > 0 ? src.SubCategories.Adapt<IEnumerable<CategoryDto>>() : null);
    }
    
    private static void CategoryDtoToCategory(TypeAdapterConfig config)
    {
        config.NewConfig<CategoryDto, Category>()
            .ConstructUsing(src => new Category { Id = src.Id.ToGuid(), Name = src.Name, ParentCategoryId = src.ParentCategoryId.ToNullableGuid() });
    }

    private static void CreateCategoryRequestDtoToCategory(TypeAdapterConfig config)
    {
        config.NewConfig<CreateCategoryRequestDto, Category>()
            .Ignore(dest => dest.Id)
            .Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId.ToNullableGuid());
    }

    private static void UpdateCategoryRequestDtoToCategory(TypeAdapterConfig config)
    {
        config.NewConfig<UpdateCategoryRequestDto, Category>()
            .Ignore(dest => dest.Id)
            .Map(dest => dest.ParentCategoryId, src => src.ParentCategoryId.ToNullableGuid());
    }


    #endregion

    #region Book Mappings

    private static void BookToBookDto(TypeAdapterConfig config)
    {
        config.NewConfig<Book, BookDto>()
            .Map(dest => dest.Id, src => src.Id.ToNullableString())
            .Map(dest => dest.Category, src => src.Category != null ? src.Category.Adapt<CategoryDto>() : null);
    }
    
    private static void BookToBookListItemDto(TypeAdapterConfig config)
    {
        config.NewConfig<Book, BookListItemDto>()
            .Map(dest => dest.Id, src => src.Id.ToNullableString())
            .Map(dest => dest.CategoryId, src => src.CategoryId.ToNullableString());
    }
    
    private static void CreateBookRequestToBook(TypeAdapterConfig config)
    {
        config.NewConfig<CreateBookRequestDto, Book>()
            .ConstructUsing(src => new Book { Title = src.Title })
            .Ignore(dest => dest.Id)
            .Map(dest => dest.CategoryId, src => src.Category != null ? src.Category.Id.ToNullableGuid() : null)
            .Map(dest => dest.Authors, src => src.Authors != null ? src.Authors.Adapt<IEnumerable<Person>>() : new List<Person>(0))
            .AfterMapping((dest) => {
                dest.CreatedAt = DateTime.UtcNow;
            });
    }
    
    #endregion
    
    #region BookFile Mappings
    
    private static void BookFileToBookFileDto(TypeAdapterConfig config)
    {
        config.NewConfig<BookFile, BookFileDto>()
            .Map(dest => dest.Id, src => src.Id.ToNullableString())
            .Map(dest => dest.FileType, src => src.FileType);
    }
    
    #endregion
}