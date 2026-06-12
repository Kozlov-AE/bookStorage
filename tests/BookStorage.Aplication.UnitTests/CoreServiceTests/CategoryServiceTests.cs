using AutoFixture;
using BookStorage.Core.Entities;
using BookStorage.Core.Interfaces.Persistence;
using BookStorage.Core.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookStorage.Aplication.UnitTests.CoreServiceTests;

public class CategoryServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICategoryRepository> _catRepoMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
            .ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _uowMock = new Mock<IUnitOfWork>(MockBehavior.Strict);
        _catRepoMock = new Mock<ICategoryRepository>(MockBehavior.Strict);
        var loggerMock = new Mock<ILogger<CategoryService>>(MockBehavior.Strict);

        _uowMock.Setup(u => u.Categories).Returns(_catRepoMock.Object);

        loggerMock.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        _service = new CategoryService(_uowMock.Object, loggerMock.Object);
    }

    #region GetAllAsync
    
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = _fixture.CreateMany<Category>(3).ToList();

        _catRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(3, result.Count());
        _catRepoMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmpty()
    {
        // Arrange
        _catRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Empty(result);
    }

    #endregion GetAllAsync
    
    #region GetByIdAsync
    
    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCategory()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var category = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .Create();

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(categoryId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.GetByIdAsync(categoryId);

        // Assert
        Assert.Null(result);
    }
    #endregion GetByIdAsync

    #region GetByNameAsync

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMatching()
    {
        // Arrange
        var categories = _fixture.Build<Category>()
            .With(c => c.Name, "Fiction")
            .CreateMany(2)
            .ToList();

        _catRepoMock
            .Setup(r => r.GetByName("Fiction", It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetByNameAsync("Fiction");

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetByNameAsync_NoMatch_ReturnsEmpty()
    {
        // Arrange
        _catRepoMock
            .Setup(r => r.GetByName("NonExistent", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetByNameAsync("NonExistent");

        // Assert
        Assert.Empty(result);
    }

    #endregion GetByNameAsync

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_NewCategory_SavesAndReturnsSuccess()
    {
        // Arrange
        var category = new Category { Name = "Science Fiction" };
        var savedCategory = new Category
        {
            Id = Guid.CreateVersion7(),
            Name = "Science Fiction"
        };

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Science Fiction", null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _catRepoMock
            .Setup(r => r.AddAsync(It.Is<Category>(c => c.Name == "Science Fiction"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedCategory);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(category);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Science Fiction", result.Value!.Name);
        _catRepoMock.Verify(r => r.CheckCategorySameNameAsync("Science Fiction", null,
            It.IsAny<CancellationToken>()), Times.Once);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameSameParent_ReturnsFailure()
    {
        // Arrange
        var parentId = Guid.CreateVersion7();

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Fiction", parentId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateAsync(new Category
        {
            Name = "Fiction",
            ParentCategoryId = parentId
        });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.CategorySameName, result.ResultCode);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameSameParentNull_ReturnsFailure()
    {
        // Arrange
        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Fiction", null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateAsync(new Category { Name = "Fiction" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.CategorySameName, result.ResultCode);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DuplicateNameDifferentParent_Succeeds()
    {
        // Arrange
        var parentB = Guid.CreateVersion7();

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Fiction", parentB,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var savedCategory = new Category
        {
            Id = Guid.CreateVersion7(),
            Name = "Fiction",
            ParentCategoryId = parentB
        };

        _catRepoMock
            .Setup(r => r.AddAsync(It.Is<Category>(c => c.ParentCategoryId == parentB),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedCategory);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(new Category
        {
            Name = "Fiction",
            ParentCategoryId = parentB
        });

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(parentB, result.Value!.ParentCategoryId);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ReturnsSuccess()
    {
        // Arrange
        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("", null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var savedCat = new Category { Id = Guid.CreateVersion7(), Name = "" };
        _catRepoMock
            .Setup(r => r.AddAsync(It.Is<Category>(c => c.Name == ""),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedCat);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(new Category { Name = "" });

        // Assert
        Assert.True(result.IsSuccess);
        _catRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhitespaceName_ReturnsSuccess()
    {
        // Arrange
        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("   ", null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var savedCat = new Category { Id = Guid.CreateVersion7(), Name = "   " };
        _catRepoMock
            .Setup(r => r.AddAsync(It.Is<Category>(c => c.Name == "   "),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedCat);

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(new Category { Name = "   " });

        // Assert
        Assert.True(result.IsSuccess);
    }

    #endregion CreateAsync

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_EmptyName_ReturnsInvalidInput()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var updateDto = new Category { Name = "" };

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.InvalidInput, result.ResultCode);
    }

    [Fact]
    public async Task UpdateAsync_WhitespaceName_ReturnsInvalidInput()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var updateDto = new Category { Name = "   " };

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.InvalidInput, result.ResultCode);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCategory_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var existing = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.Name, "Old Name")
            .With(c => c.ParentCategoryId, (Guid?)null)
            .With(c => c.ParentCategory, (Category?)null)
            .With(c => c.SubCategories, new List<Category>())
            .Create();

        var updateDto = new Category
        {
            Name = "Updated Name",
            ParentCategoryId = null
        };

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Updated Name", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _catRepoMock
            .Setup(r => r.Update(It.Is<Category>(c => c.Name == "Updated Name")))
            .Verifiable();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value!.Name);
        _catRepoMock.Verify(r => r.Update(It.IsAny<Category>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UnchangedName_StillCallsSameNameCheckAndSucceeds()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var existing = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.Name, "Unchanged")
            .With(c => c.ParentCategoryId, (Guid?)null)
            .With(c => c.ParentCategory, (Category?)null)
            .With(c => c.SubCategories, new List<Category>())
            .Create();

        var updateDto = new Category
        {
            Name = "Unchanged",
            ParentCategoryId = null
        };

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("Unchanged", null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _catRepoMock
            .Setup(r => r.Update(It.IsAny<Category>()))
            .Verifiable();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(categoryId, updateDto);

        // Assert
        Assert.True(result.IsSuccess);
        _catRepoMock.Verify(r => r.CheckCategorySameNameAsync(
            It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.UpdateAsync(categoryId, new Category { Name = "Anything" });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.NotFound, result.ResultCode);
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ReturnsSameNameFailure()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var existing = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.Name, "Old Name")
            .With(c => c.ParentCategoryId, (Guid?)null)
            .With(c => c.ParentCategory, (Category?)null)
            .With(c => c.SubCategories, new List<Category>())
            .Create();

        _catRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _catRepoMock
            .Setup(r => r.CheckCategorySameNameAsync("New Name", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateAsync(categoryId, new Category
        {
            Name = "New Name",
            ParentCategoryId = null
        });

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.CategorySameName, result.ResultCode);
    }

    #endregion UpdateAsync

    #region DeleteAsync

    [Fact]
    public async Task DeleteAsync_ExistingCategoryWithoutChildren_DeletesAndReturnsSuccess()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var category = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.Name, "Deletable")
            .With(c => c.SubCategories, new List<Category>())
            .With(c => c.ParentCategory, (Category?)null)
            .Create();

        _catRepoMock
            .Setup(r => r.GetByIdWithChildsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _catRepoMock
            .Setup(r => r.Delete(It.Is<Category>(c => c.Id == categoryId)))
            .Verifiable();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Deletable", result.Value!.Name);
        _catRepoMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingCategory_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();

        _catRepoMock
            .Setup(r => r.GetByIdWithChildsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.NotFound, result.ResultCode);
    }

    [Fact]
    public async Task DeleteAsync_CategoryWithSubCategories_ReturnsFailure()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var category = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.Name, "Parent")
            .With(c => c.SubCategories, new List<Category>
            {
                new Category { Id = Guid.CreateVersion7(), Name = "Child" }
            })
            .With(c => c.ParentCategory, (Category?)null)
            .Create();

        _catRepoMock
            .Setup(r => r.GetByIdWithChildsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.CategoryRemovingNoAllowedBySubCategories, result.ResultCode);
        _catRepoMock.Verify(r => r.Delete(It.IsAny<Category>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeleteThrows_ReturnsInnerError()
    {
        // Arrange
        var categoryId = Guid.CreateVersion7();
        var category = _fixture.Build<Category>()
            .With(c => c.Id, categoryId)
            .With(c => c.SubCategories, new List<Category>())
            .With(c => c.ParentCategory, (Category?)null)
            .Create();

        _catRepoMock
            .Setup(r => r.GetByIdWithChildsAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _catRepoMock
            .Setup(r => r.Delete(It.IsAny<Category>()))
            .Throws(new InvalidOperationException("DB error"));

        // Act
        var result = await _service.DeleteAsync(categoryId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultCodes.InnerError, result.ResultCode);
    }

    #endregion DeleteAsync
}
