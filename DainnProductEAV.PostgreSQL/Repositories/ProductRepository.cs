using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Entities;
using DainnProductEAV.PostgreSQL.ValueEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DainnProductEAV.PostgreSQL.Repositories;

/// <summary>
/// Repository implementation for product operations with EAV support.
/// Includes caching for attribute metadata.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly ProductCatalogDbContext _context;
    private readonly IMemoryCache _cache;
    private const string AttributeCachePrefix = "attr_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public ProductRepository(ProductCatalogDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    #region Basic CRUD

    public async Task<ProductEntity?> GetByIdAsync(Guid id, int storeId = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.AttributeSet)
            .Include(p => p.Inventory)
            .Include(p => p.CategoryRelations)
                .ThenInclude(cr => cr.Category)
            .FirstOrDefaultAsync(p => p.EntityId == id, cancellationToken);
    }

    public async Task<ProductEntity?> GetBySkuAsync(string sku, int storeId = 0, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.AttributeSet)
            .Include(p => p.Inventory)
            .FirstOrDefaultAsync(p => p.Sku == sku, cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> GetAllAsync(int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Include(p => p.AttributeSet)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductEntity> CreateAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        
        return product;
    }

    public async Task<ProductEntity> UpdateAsync(ProductEntity product, CancellationToken cancellationToken = default)
    {
        product.UpdatedAt = DateTime.UtcNow;
        
        _context.Products.Update(product);
        await _context.SaveChangesAsync(cancellationToken);
        
        return product;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.Products.FindAsync(new object[] { id }, cancellationToken);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Dynamic Attribute Access

    public async Task<object?> GetAttributeValueAsync(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeByCodeAsync(attributeCode, cancellationToken);
        if (attribute == null) return null;

        return attribute.BackendType switch
        {
            "varchar" => await GetVarcharValueAsync(productId, attribute.AttributeId, storeId, cancellationToken),
            "int" => await GetIntValueAsync(productId, attribute.AttributeId, storeId, cancellationToken),
            "decimal" => await GetDecimalValueAsync(productId, attribute.AttributeId, storeId, cancellationToken),
            "text" => await GetTextValueAsync(productId, attribute.AttributeId, storeId, cancellationToken),
            "datetime" => await GetDatetimeValueAsync(productId, attribute.AttributeId, storeId, cancellationToken),
            _ => null
        };
    }

    public async Task<T?> GetAttributeValueAsync<T>(Guid productId, string attributeCode, int storeId = 0, CancellationToken cancellationToken = default)
    {
        var value = await GetAttributeValueAsync(productId, attributeCode, storeId, cancellationToken);
        if (value == null) return default;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAttributeValueAsync(Guid productId, string attributeCode, object? value, int storeId = 0, CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeByCodeAsync(attributeCode, cancellationToken);
        if (attribute == null)
            throw new ArgumentException($"Attribute '{attributeCode}' not found.");

        switch (attribute.BackendType)
        {
            case "varchar":
                await SetVarcharValueAsync(productId, attribute.AttributeId, value?.ToString(), storeId, cancellationToken);
                break;
            case "int":
                await SetIntValueAsync(productId, attribute.AttributeId, value != null ? Convert.ToInt32(value) : null, storeId, cancellationToken);
                break;
            case "decimal":
                await SetDecimalValueAsync(productId, attribute.AttributeId, value != null ? Convert.ToDecimal(value) : null, storeId, cancellationToken);
                break;
            case "text":
                await SetTextValueAsync(productId, attribute.AttributeId, value?.ToString(), storeId, cancellationToken);
                break;
            case "datetime":
                await SetDatetimeValueAsync(productId, attribute.AttributeId, value != null ? Convert.ToDateTime(value) : null, storeId, cancellationToken);
                break;
            default:
                throw new ArgumentException($"Unknown backend type: {attribute.BackendType}");
        }
    }

    public async Task<Dictionary<string, object?>> GetAllAttributeValuesAsync(Guid productId, int storeId = 0, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, object?>();
        var attributes = await _context.Attributes.ToListAsync(cancellationToken);

        foreach (var attr in attributes)
        {
            var value = await GetAttributeValueAsync(productId, attr.AttributeCode, storeId, cancellationToken);
            result[attr.AttributeCode] = value;
        }

        return result;
    }

    #endregion

    #region Category Operations

    public async Task<IEnumerable<ProductEntity>> GetByCategoryAsync(Guid categoryId, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        return await _context.ProductCategoryRelations
            .Where(pcr => pcr.CategoryId == categoryId)
            .OrderBy(pcr => pcr.Position)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(pcr => pcr.Product)
            .ToListAsync(cancellationToken);
    }

    public async Task AssignToCategoryAsync(Guid productId, Guid categoryId, int position = 0, CancellationToken cancellationToken = default)
    {
        var existing = await _context.ProductCategoryRelations
            .FirstOrDefaultAsync(pcr => pcr.ProductId == productId && pcr.CategoryId == categoryId, cancellationToken);

        if (existing != null)
        {
            existing.Position = position;
        }
        else
        {
            _context.ProductCategoryRelations.Add(new ProductCategoryRelation
            {
                ProductId = productId,
                CategoryId = categoryId,
                Position = position
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveFromCategoryAsync(Guid productId, Guid categoryId, CancellationToken cancellationToken = default)
    {
        var relation = await _context.ProductCategoryRelations
            .FirstOrDefaultAsync(pcr => pcr.ProductId == productId && pcr.CategoryId == categoryId, cancellationToken);

        if (relation != null)
        {
            _context.ProductCategoryRelations.Remove(relation);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    #endregion

    #region Search & Filter

    public async Task<IEnumerable<ProductEntity>> SearchAsync(string query, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        // Search by SKU first
        var productIds = await _context.Products
            .Where(p => p.Sku.Contains(query))
            .Select(p => p.EntityId)
            .ToListAsync(cancellationToken);

        // Search by name attribute
        var nameAttr = await GetAttributeByCodeAsync("name", cancellationToken);
        if (nameAttr != null)
        {
            var nameProductIds = await _context.ProductVarcharValues
                .Where(v => v.AttributeId == nameAttr.AttributeId && 
                           v.StoreId == storeId && 
                           v.Value != null && v.Value.Contains(query))
                .Select(v => v.EntityId)
                .ToListAsync(cancellationToken);
            
            productIds = productIds.Union(nameProductIds).Distinct().ToList();
        }

        return await _context.Products
            .Where(p => productIds.Contains(p.EntityId))
            .Include(p => p.AttributeSet)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductEntity>> FilterByAttributeAsync(string attributeCode, object value, int storeId = 0, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var attribute = await GetAttributeByCodeAsync(attributeCode, cancellationToken);
        if (attribute == null) return Enumerable.Empty<ProductEntity>();

        List<Guid> productIds = attribute.BackendType switch
        {
            "varchar" => await _context.ProductVarcharValues
                .Where(v => v.AttributeId == attribute.AttributeId && v.StoreId == storeId && v.Value == value.ToString())
                .Select(v => v.EntityId)
                .ToListAsync(cancellationToken),
            "int" => await _context.ProductIntValues
                .Where(v => v.AttributeId == attribute.AttributeId && v.StoreId == storeId && v.Value == Convert.ToInt32(value))
                .Select(v => v.EntityId)
                .ToListAsync(cancellationToken),
            "decimal" => await _context.ProductDecimalValues
                .Where(v => v.AttributeId == attribute.AttributeId && v.StoreId == storeId && v.Value == Convert.ToDecimal(value))
                .Select(v => v.EntityId)
                .ToListAsync(cancellationToken),
            _ => new List<Guid>()
        };

        return await _context.Products
            .Where(p => productIds.Contains(p.EntityId))
            .Include(p => p.AttributeSet)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    #endregion

    #region Private Helpers

    private async Task<EavAttribute?> GetAttributeByCodeAsync(string attributeCode, CancellationToken cancellationToken)
    {
        var cacheKey = $"{AttributeCachePrefix}{attributeCode}";
        
        if (!_cache.TryGetValue(cacheKey, out EavAttribute? attribute))
        {
            attribute = await _context.Attributes
                .FirstOrDefaultAsync(a => a.AttributeCode == attributeCode, cancellationToken);
            
            if (attribute != null)
            {
                _cache.Set(cacheKey, attribute, CacheDuration);
            }
        }

        return attribute;
    }

    // Varchar value helpers
    private async Task<string?> GetVarcharValueAsync(Guid productId, Guid attributeId, int storeId, CancellationToken cancellationToken)
    {
        var value = await _context.ProductVarcharValues
            .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId)
            .FirstOrDefaultAsync(cancellationToken);

        // Fallback to global (storeId = 0) if store-specific not found
        if (value == null && storeId != 0)
        {
            value = await _context.ProductVarcharValues
                .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == 0)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return value?.Value;
    }

    private async Task SetVarcharValueAsync(Guid productId, Guid attributeId, string? value, int storeId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductVarcharValues
            .FirstOrDefaultAsync(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId, cancellationToken);

        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            _context.ProductVarcharValues.Add(new ProductVarcharValue
            {
                EntityId = productId,
                AttributeId = attributeId,
                StoreId = storeId,
                Value = value
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Int value helpers
    private async Task<int?> GetIntValueAsync(Guid productId, Guid attributeId, int storeId, CancellationToken cancellationToken)
    {
        var value = await _context.ProductIntValues
            .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (value == null && storeId != 0)
        {
            value = await _context.ProductIntValues
                .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == 0)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return value?.Value;
    }

    private async Task SetIntValueAsync(Guid productId, Guid attributeId, int? value, int storeId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductIntValues
            .FirstOrDefaultAsync(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId, cancellationToken);

        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            _context.ProductIntValues.Add(new ProductIntValue
            {
                EntityId = productId,
                AttributeId = attributeId,
                StoreId = storeId,
                Value = value
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Decimal value helpers
    private async Task<decimal?> GetDecimalValueAsync(Guid productId, Guid attributeId, int storeId, CancellationToken cancellationToken)
    {
        var value = await _context.ProductDecimalValues
            .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (value == null && storeId != 0)
        {
            value = await _context.ProductDecimalValues
                .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == 0)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return value?.Value;
    }

    private async Task SetDecimalValueAsync(Guid productId, Guid attributeId, decimal? value, int storeId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductDecimalValues
            .FirstOrDefaultAsync(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId, cancellationToken);

        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            _context.ProductDecimalValues.Add(new ProductDecimalValue
            {
                EntityId = productId,
                AttributeId = attributeId,
                StoreId = storeId,
                Value = value
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Text value helpers
    private async Task<string?> GetTextValueAsync(Guid productId, Guid attributeId, int storeId, CancellationToken cancellationToken)
    {
        var value = await _context.ProductTextValues
            .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (value == null && storeId != 0)
        {
            value = await _context.ProductTextValues
                .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == 0)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return value?.Value;
    }

    private async Task SetTextValueAsync(Guid productId, Guid attributeId, string? value, int storeId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductTextValues
            .FirstOrDefaultAsync(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId, cancellationToken);

        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            _context.ProductTextValues.Add(new ProductTextValue
            {
                EntityId = productId,
                AttributeId = attributeId,
                StoreId = storeId,
                Value = value
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    // Datetime value helpers
    private async Task<DateTime?> GetDatetimeValueAsync(Guid productId, Guid attributeId, int storeId, CancellationToken cancellationToken)
    {
        var value = await _context.ProductDatetimeValues
            .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId)
            .FirstOrDefaultAsync(cancellationToken);

        if (value == null && storeId != 0)
        {
            value = await _context.ProductDatetimeValues
                .Where(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == 0)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return value?.Value;
    }

    private async Task SetDatetimeValueAsync(Guid productId, Guid attributeId, DateTime? value, int storeId, CancellationToken cancellationToken)
    {
        var existing = await _context.ProductDatetimeValues
            .FirstOrDefaultAsync(v => v.EntityId == productId && v.AttributeId == attributeId && v.StoreId == storeId, cancellationToken);

        if (existing != null)
        {
            existing.Value = value;
        }
        else
        {
            _context.ProductDatetimeValues.Add(new ProductDatetimeValue
            {
                EntityId = productId,
                AttributeId = attributeId,
                StoreId = storeId,
                Value = value
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
