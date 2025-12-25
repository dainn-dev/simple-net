using DainnProductEAV.PostgreSQL.Entities;

namespace DainnProductEAV.PostgreSQL.Services;

/// <summary>
/// Service interface for category operations.
/// </summary>
public interface ICategoryService
{
    Task<CategoryEntity?> GetCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetCategoryTreeAsync(Guid? rootId = null, CancellationToken cancellationToken = default);
    Task<CategoryEntity> CreateCategoryAsync(string name, Guid? parentId = null, CancellationToken cancellationToken = default);
    Task<CategoryEntity> UpdateCategoryAsync(CategoryEntity category, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task MoveCategoryAsync(Guid categoryId, Guid newParentId, int position = 0, CancellationToken cancellationToken = default);
}
