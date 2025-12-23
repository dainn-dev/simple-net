using DainnProductEAVManagement.Entities;

namespace DainnProductEAVManagement.Repositories;

/// <summary>
/// Repository interface for category operations.
/// </summary>
public interface ICategoryRepository
{
    Task<CategoryEntity?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryEntity>> GetTreeAsync(Guid? rootId = null, CancellationToken cancellationToken = default);
    Task<CategoryEntity> CreateAsync(CategoryEntity category, CancellationToken cancellationToken = default);
    Task<CategoryEntity> UpdateAsync(CategoryEntity category, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task MoveAsync(Guid categoryId, Guid newParentId, int position = 0, CancellationToken cancellationToken = default);
}
