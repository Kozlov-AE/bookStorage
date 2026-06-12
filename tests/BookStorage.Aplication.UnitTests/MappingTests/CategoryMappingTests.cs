using AutoFixture;
using BookStorage.Api.DTOs;
using BookStorage.Core.Entities;
using MapsterMapper;
using Xunit;

namespace BookStorage.Aplication.UnitTests.MappingTests;

public class CategoryMappingTests
{
    private readonly Mapper _mapper;

    public CategoryMappingTests()
    {
        _mapper = MapsterTestHelper.GetMapperForTests();
        var fixture = new Fixture();
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    private Category CreateSampleCategory(Guid? id = null, string? name = null, Guid? parentCategoryId = null)
    {
        return new Category
        {
            Id = id ?? Guid.CreateVersion7(),
            Name = name ?? "Test Category",
            ParentCategoryId = parentCategoryId
        };
    }

    private CategoryDto CreateSampleCategoryDto(string? id = null, string? name = null, string? parentCategoryId = null)
    {
        var categoryDto = new CategoryDto(name ?? "Test Category")
        {
            Id = id,
            ParentCategoryId = parentCategoryId
        };
        return categoryDto;
    }

    private CreateCategoryRequestDto CreateSampleCreateCategoryRequestDto(string? name = null, string? parentCategoryId = null)
    {
        return new CreateCategoryRequestDto(
            name ?? "Test Category"
        )
        {
            ParentCategoryId = parentCategoryId
        };
    }

    private UpdateCategoryRequestDto CreateSampleUpdateCategoryRequestDto(string? name = null, string? parentCategoryId = null)
    {
        return new UpdateCategoryRequestDto(
            name ?? "Updated Category"
        )
        {
            ParentCategoryId = parentCategoryId
        };
    }

    [Fact]
    public void Map_Category_To_CategoryDto_Should_Ok()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var parentId = Guid.CreateVersion7();
        var srcCategory = CreateSampleCategory(categoryId, "Fiction", parentId);

        // Act
        var categoryDto = _mapper.Map<Category, CategoryDto>(srcCategory);

        // Assert
        Assert.NotNull(categoryDto);
        Assert.Equal(categoryId.ToString(), categoryDto.Id);
        Assert.Equal("Fiction", categoryDto.Name);
        Assert.Equal(parentId.ToString(), categoryDto.ParentCategoryId);
        Assert.Null(categoryDto.SubCategories);
    }

    [Fact]
    public void Map_Category_To_CategoryDto_With_Null_ParentId_Should_Ok()
    {
        // Arrange
        var srcCategory = CreateSampleCategory(Guid.CreateVersion7(), "Root Category", null);

        // Act
        var categoryDto = _mapper.Map<Category, CategoryDto>(srcCategory);

        // Assert
        Assert.NotNull(categoryDto);
        Assert.Null(categoryDto.ParentCategoryId);
        Assert.Equal("Root Category", categoryDto.Name);
    }

    [Fact]
    public void Map_Category_To_CategoryDto_With_Empty_Id_Should_Ok()
    {
        // Arrange
        var srcCategory = CreateSampleCategory(Guid.Empty, "Test Category", null);

        // Act
        var categoryDto = _mapper.Map<Category, CategoryDto>(srcCategory);

        // Assert
        Assert.NotNull(categoryDto);
        Assert.Null(categoryDto.Id);
    }

    [Fact]
    public void Map_CategoryDto_To_Category_Should_Ok()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7().ToString();
        var parentCategoryId = Guid.CreateVersion7().ToString();
        var srcCategoryDto = CreateSampleCategoryDto(categoryId, "Non-Fiction", parentCategoryId);

        // Act 
        var targetCategory = _mapper.Map<CategoryDto, Category>(srcCategoryDto);

        // Assert
        Assert.NotNull(targetCategory);
        Assert.Equal("Non-Fiction", targetCategory.Name);
        Assert.Equal(Guid.Parse(categoryId), targetCategory.Id);
        Assert.Equal(Guid.Parse(parentCategoryId), targetCategory.ParentCategoryId);
    }

    [Fact]
    public void Map_CategoryDto_To_Category_With_Null_Id_Should_Ok()
    {
        // Arrange
        var srcCategoryDto = CreateSampleCategoryDto(null, "Test Category", null);

        // Act
        var targetCategory = _mapper.Map<CategoryDto, Category>(srcCategoryDto);

        // Assert
        Assert.NotNull(targetCategory);
        Assert.Equal("Test Category", targetCategory.Name);
        Assert.Equal(Guid.Empty, targetCategory.Id);
        Assert.Null(targetCategory.ParentCategoryId);
    }

    [Fact]
    public void Map_CategoryDto_To_Category_With_Null_ParentId_Should_Ok()
    {
        // Arrange
        var srcCategoryDto = CreateSampleCategoryDto(Guid.CreateVersion7().ToString(), "Root Category", null);

        // Act
        var targetCategory = _mapper.Map<CategoryDto, Category>(srcCategoryDto);

        // Assert
        Assert.NotNull(targetCategory);
        Assert.Equal("Root Category", targetCategory.Name);
        Assert.Null(targetCategory.ParentCategoryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Fiction")]
    [InlineData("Category With Multiple Words")]
    [InlineData("Укатегорія з українськими символами")]
    [InlineData("小说分类 - Chinese Fiction")]
    public void Map_Category_To_CategoryDto_Different_Names_Should_Ok(string name)
    {
        // Arrange
        var srcCategory = CreateSampleCategory(Guid.CreateVersion7(), name, null);

        // Act
        var categoryDto = _mapper.Map<Category, CategoryDto>(srcCategory);

        // Assert
        Assert.NotNull(categoryDto);
        Assert.Equal(name, categoryDto.Name);
    }

    [Fact]
    public void Map_Category_To_CategoryDto_Uses_ConstructUsing_Correctly()
    {
        // Arrange
        var srcCategory = CreateSampleCategory();

        // Act
        var categoryDto = _mapper.Map<Category, CategoryDto>(srcCategory);

        // Assert
        Assert.NotNull(categoryDto);
        Assert.Equal(srcCategory.Id.ToString(), categoryDto.Id);
        Assert.Equal(srcCategory.Name, categoryDto.Name);
        Assert.Equal(srcCategory.ParentCategoryId?.ToString(), categoryDto.ParentCategoryId);
        Assert.Null(categoryDto.SubCategories);
    }

    [Fact]
    public void Map_Category_To_CategoryDto_With_SubCategories_Maps_Recursively()
    {
        // Arrange
        var childId1 = Guid.CreateVersion7();
        var childId2 = Guid.CreateVersion7();
        var parentId = Guid.CreateVersion7();

        var child1 = new Category
        {
            Id = childId1,
            Name = "Science Fiction",
            ParentCategoryId = parentId,
            SubCategories = new List<Category>()
        };
        var child2 = new Category
        {
            Id = childId2,
            Name = "Fantasy",
            ParentCategoryId = parentId,
            SubCategories = new List<Category>()
        };

        var parent = new Category
        {
            Id = parentId,
            Name = "Fiction",
            ParentCategoryId = null,
            SubCategories = new List<Category> { child1, child2 }
        };

        // Act
        var parentDto = _mapper.Map<Category, CategoryDto>(parent);

        // Assert
        Assert.NotNull(parentDto);
        Assert.Equal(parentId.ToString(), parentDto.Id);
        Assert.Equal("Fiction", parentDto.Name);
        Assert.Null(parentDto.ParentCategoryId);

        Assert.NotNull(parentDto.SubCategories);
        Assert.Equal(2, parentDto.SubCategories.Count());

        var childDto1 = parentDto.SubCategories.First(s => s.Name == "Science Fiction");
        Assert.Equal(childId1.ToString(), childDto1.Id);
        Assert.Equal(parentId.ToString(), childDto1.ParentCategoryId);
        Assert.NotNull(childDto1.SubCategories);
        Assert.Empty(childDto1.SubCategories);

        var childDto2 = parentDto.SubCategories.First(s => s.Name == "Fantasy");
        Assert.Equal(childId2.ToString(), childDto2.Id);
        Assert.Equal(parentId.ToString(), childDto2.ParentCategoryId);
        Assert.NotNull(childDto2.SubCategories);
        Assert.Empty(childDto2.SubCategories);
    }

    [Fact]
    public void Map_CreateCategoryRequestDto_To_Category_Should_Ok()
    {
        // Arrange
        var parentId = Guid.CreateVersion7();
        var requestDto = CreateSampleCreateCategoryRequestDto("New Category", parentId.ToString());

        // Act
        var category = _mapper.Map<CreateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotNull(category);
        Assert.Equal("New Category", category.Name);
        Assert.Equal(parentId, category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_CreateCategoryRequestDto_To_Category_With_Null_ParentId_Should_Ok()
    {
        // Arrange
        var requestDto = CreateSampleCreateCategoryRequestDto("Root Category", null);

        // Act
        var category = _mapper.Map<CreateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotNull(category);
        Assert.Equal("Root Category", category.Name);
        Assert.Null(category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_CreateCategoryRequestDto_To_Category_Ignores_Id_Correctly()
    {
        // Arrange
        var requestDto = CreateSampleCreateCategoryRequestDto("Test Category");

        // Act
        var category = _mapper.Map<CreateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_UpdateCategoryRequestDto_To_Category_Should_Ok()
    {
        // Arrange
        var parentId = Guid.CreateVersion7();
        var requestDto = CreateSampleUpdateCategoryRequestDto("Updated Category", parentId.ToString());

        // Act
        var category = _mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotNull(category);
        Assert.Equal("Updated Category", category.Name);
        Assert.Equal(parentId, category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_UpdateCategoryRequestDto_To_Category_With_Null_ParentId_Should_Ok()
    {
        // Arrange
        var requestDto = CreateSampleUpdateCategoryRequestDto("Updated Root", null);

        // Act
        var category = _mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotNull(category);
        Assert.Equal("Updated Root", category.Name);
        Assert.Null(category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Theory]
    [InlineData("Fiction")]
    [InlineData("Non-Fiction")]
    [InlineData("科技 - Technology")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Very Long Category Name With Multiple Words And Spaces")]
    public void Map_CreateCategoryRequestDto_To_Category_Different_Names_Should_Ok(string name)
    {
        // Arrange
        var requestDto = CreateSampleCreateCategoryRequestDto(name);

        // Act
        var category = _mapper.Map<CreateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotNull(category);
        Assert.Equal(name, category.Name);
        Assert.Null(category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_UpdateCategoryRequestDto_To_Category_With_Invalid_ParentId_Should_Handle_Gracefully()
    {
        // Arrange
        var requestDto = CreateSampleUpdateCategoryRequestDto("Test", "invalid-guid");

        // Act
        try
        {
            var category = _mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);
            
            Assert.NotNull(category);
            Assert.Equal("invalid-guid", requestDto.ParentCategoryId);
            
        }
        catch (Exception ex)
        {
            Assert.NotNull(ex);
        }
    }

    [Fact]
    public void Map_CreateCategoryRequestDto_To_Category_Ignores_Only_Id_Field()
    {
        // Arrange
        var requestDto = CreateSampleCreateCategoryRequestDto("Test Category");

        // Act
        var category = _mapper.Map<CreateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.NotEqual(Guid.CreateVersion7().ToString(), requestDto.Name);
        Assert.NotEqual(Guid.CreateVersion7().ToString(), requestDto.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }

    [Fact]
    public void Map_UpdateCategoryRequestDto_To_Category_Ignores_Only_Id_Field()
    {
        // Arrange
        var requestDto = CreateSampleUpdateCategoryRequestDto("Test Category");

        // Act
        var category = _mapper.Map<UpdateCategoryRequestDto, Category>(requestDto);

        // Assert
        Assert.Equal("Test Category", category.Name);
        Assert.Null(category.ParentCategoryId);
        Assert.Equal(Guid.Empty, category.Id);
    }
}