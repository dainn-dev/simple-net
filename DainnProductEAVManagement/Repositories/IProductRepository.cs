using DainnProductEAVManagement.Entities;

namespace DainnProductEAVManagement.Repositories;

/// <summary>
/// Repository interface for product operations with EAV support.
/// </summary>
public interface IProductRepository
{
    // ========== Basic CRUD ==========
    Task<ProductEntity?> GetByIdAsync(Guid id, int storeId = 0, CancellationToken cancellationToken = default);
    Task<ProductEntity?> GetBySkuAsync(string sku, int storeId = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetAllAsync(int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ProductEntity> CreateAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task<ProductEntity> UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // ========== Dynamic Attribute Access ==========
    Task<object?> GetAttributeValueAsync(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default);
    Task<T?> GetAttributeValueAsync<T>(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default);
    Task SetAttributeValueAsync(Guid productId, string attributeCode, object? value, int storeId = 0, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object?>> GetAllAttributeValuesAsync(Guid productId, int storeId = 0, CancellationToken cancellationToken = default);

    // ========== Category Operations ==========
    Task<IEnumerable<ProductEntity>> GetByCategoryAsync(Guid categoryId, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task AssignToCategoryAsync(Guid productId, Guid categoryId, int position = 0, CancellationToken cancellationToken = default);
    Task RemoveFromCategoryAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default);

    // ========== Search & Filter ==========
    Task<IEnumerable<ProductEntity>> SearchAsync(string query, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> FilterByAttributeAsync(string attributeCode, object value, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
