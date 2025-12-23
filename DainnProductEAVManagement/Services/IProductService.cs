using DainnProductEAVManagement.Entities;

namespace DainnProductEAVManagement.Services;

/// <summary>
/// Service interface for product operations with business logic.
/// </summary>
public interface IProductService
{
    // CRUD
    Task<ProductEntity?> GetProductAsync(Guid id, int storeId = 0, CancellationToken cancellationToken = default);
    Task<ProductEntity?> GetProductBySkuAsync(string sku, int storeId = 0, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductEntity>> GetProductsAsync(int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<ProductEntity> CreateProductAsync(string sku, string typeId = "simple", Guid attributeSetId = default, CancellationToken cancellationToken = default);
    Task<ProductEntity> UpdateProductAsync(ProductEntity product, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);

    // Attribute operations
    Task<T?> GetAttributeAsync<T>(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default);
    Task SetAttributeAsync(Guid productId, string attributeCode, object? value, int storeId = 0, CancellationToken cancellationToken = default);
    Task SetAttributesAsync(Guid productId, Dictionary<string, object?> attributes, int storeId = 0, CancellationToken cancellationToken = default);

    // Category operations
    Task<IEnumerable<ProductEntity>> GetProductsByCategoryAsync(Guid categoryId, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task AssignProductToCategoryAsync(Guid productId, Guid categoryId, int position = 0, CancellationToken cancellationToken = default);
    Task RemoveProductFromCategoryAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default);

    // Inventory
    Task UpdateInventoryAsync(Guid productId, int qty, bool isInStock, CancellationToken cancellationToken = default);
    Task<ProductInventory?> GetInventoryAsync(Guid productId, CancellationToken cancellationToken = default);

    // Search
    Task<IEnumerable<ProductEntity>> SearchProductsAsync(string query, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default);
}
