using DainnProductEAVManagement.Entities;
using DainnProductEAVManagement.Repositories;

namespace DainnProductEAVManagement.Services;

/// <summary>
/// Service implementation for category operations.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryEntity?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetAllAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetRootCategoriesAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetCategoryTreeAsync(Guid? rootId = null, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.GetTreeAsync(rootId, cancellationToken);
    }

    public async Task<CategoryEntity> CreateCategoryAsync(string name, Guid? parentId = null, CancellationToken cancellationToken = default)
    {
        var category = new CategoryEntity
        {
            Name = name,
            ParentId = parentId,
            IsActive = true,
            UrlKey = GenerateUrlKey(name)
        };

        return await _categoryRepository.CreateAsync(category, cancellationToken);
    }

    public async Task<CategoryEntity> UpdateCategoryAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        return await _categoryRepository.UpdateAsync(category, cancellationToken);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _categoryRepository.DeleteAsync(id, cancellationToken);
    }

    public async Task MoveCategoryAsync(Guid categoryId, Guid newParentId, int position = 0, CancellationToken cancellationToken = default)
    {
        await _categoryRepository.MoveAsync(categoryId, newParentId, position, cancellationToken);
    }

    private static string GenerateUrlKey(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");
    }
}
