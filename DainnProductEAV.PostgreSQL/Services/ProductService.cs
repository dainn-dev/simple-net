using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Entities;
using DainnProductEAV.PostgreSQL.Repositories;
using Microsoft.EntityFrameworkCore;
using DainnCommon.Exceptions;

namespace DainnProductEAV.PostgreSQL.Services;

/// <summary>
/// Service implementation for product operations with business logic.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ProductCatalogDbContext _context;

    public ProductService(IProductRepository productRepository, ProductCatalogDbContext context)
    {
        _productRepository = productRepository;
        _context = context;
    }

    #region CRUD

    public async Task<ProductEntity?> GetProductAsync(Guid id, int storeId = 0, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetByIdAsync(id, storeId, cancellationToken);
    }

    public async Task<ProductEntity?> GetProductBySkuAsync(string sku, int storeId = 0, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetBySkuAsync(sku, storeId, cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetProductsAsync(int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAllAsync(storeId, page, pageSize, cancellationToken);
    }

    public async Task<ProductEntity> CreateProductAsync(string sku, string typeId = "simple", Guid attributeSetId = default, CancellationToken cancellationToken = default)
    {
        // Validate SKU uniqueness
        var existing = await _productRepository.GetBySkuAsync(sku, cancellationToken: cancellationToken);
        if (existing != null)
        {
            throw new BusinessRuleException($"Product with SKU '{sku}' already exists.");
        }

        var product = new ProductEntity
        {
            Sku = sku,
            TypeId = typeId,
            AttributeSetId = attributeSetId
        };

        return await _productRepository.CreateAsync(product, cancellationToken);
    }

    public async Task<ProductEntity> UpdateProductAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        return await _productRepository.UpdateAsync(product, cancellationToken);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _productRepository.DeleteAsync(id, cancellationToken);
    }

    #endregion

    #region Attribute Operations

    public async Task<T?> GetAttributeAsync<T>(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetAttributeValueAsync<T>(productId, attributeCode, storeId, cancellationToken);
    }

    public async Task SetAttributeAsync(Guid productId, string attributeCode, object? value, int storeId = 0, CancellationToken cancellationToken = default)
    {
        await _productRepository.SetAttributeValueAsync(productId, attributeCode, value, storeId, cancellationToken);
    }

    public async Task SetAttributesAsync(Guid productId, Dictionary<string, object?> attributes, int storeId = 0, CancellationToken cancellationToken = default)
    {
        foreach (var kvp in attributes)
        {
            await _productRepository.SetAttributeValueAsync(productId, kvp.Key, kvp.Value, storeId, cancellationToken);
        }
    }

    #endregion

    #region Category Operations

    public async Task<IEnumerable<ProductEntity>> GetProductsByCategoryAsync(Guid categoryId, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _productRepository.GetByCategoryAsync(categoryId, storeId, page, pageSize, cancellationToken);
    }

    public async Task AssignProductToCategoryAsync(Guid productId, Guid categoryId, int position = 0, CancellationToken cancellationToken = default)
    {
        await _productRepository.AssignToCategoryAsync(productId, categoryId, position, cancellationToken);
    }

    public async Task RemoveProductFromCategoryAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        await _productRepository.RemoveFromCategoryAsync(productId, categoryId, cancellationToken);
    }

    #endregion

    #region Inventory

    public async Task UpdateInventoryAsync(Guid productId, int qty, bool isInStock, CancellationToken cancellationToken = default)
    {
        var inventory = await _context.ProductInventories.FindAsync(new object[] { productId }, cancellationToken);
        
        if (inventory != null)
        {
            inventory.Qty = qty;
            inventory.IsInStock = isInStock;
        }
        else
        {
            _context.ProductInventories.Add(new ProductInventory
            {
                ProductId = productId,
                Qty = qty,
                IsInStock = isInStock
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProductInventory?> GetInventoryAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.ProductInventories.FindAsync(new object[] { productId }, cancellationToken);
    }

    #endregion

    #region Search

    public async Task<IEnumerable<ProductEntity>> SearchProductsAsync(string query, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _productRepository.SearchAsync(query, storeId, page, pageSize, cancellationToken);
    }

    #endregion
}
