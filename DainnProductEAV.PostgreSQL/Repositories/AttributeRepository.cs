using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DainnProductEAV.PostgreSQL.Repositories;

/// <summary>
/// Repository implementation for EAV attribute operations.
/// </summary>
public class AttributeRepository : IAttributeRepository
{
    private readonly ProductCatalogDbContext _context;
    private readonly IMemoryCache _cache;
    private const string AttributeListCacheKey = "all_attributes";
    private const string AttributeSetListCacheKey = "all_attribute_sets";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public AttributeRepository(ProductCatalogDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    #region Attribute Operations

    public async Task<EavAttribute?> GetByIdAsync(Guid attributeId, CancellationToken cancellationToken = default)
    {
        return await _context.Attributes
            .Include(a => a.Group)
            .FirstOrDefaultAsync(a => a.AttributeId == attributeId, cancellationToken);
    }

    public async Task<EavAttribute?> GetByCodeAsync(string attributeCode, CancellationToken cancellationToken = default)
    {
        return await _context.Attributes
            .Include(a => a.Group)
            .FirstOrDefaultAsync(a => a.AttributeCode == attributeCode, cancellationToken);
    }

    public async Task<IEnumerable<EavAttribute>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(AttributeListCacheKey, out List<EavAttribute>? attributes))
        {
            attributes = await _context.Attributes
                .Include(a => a.Group)
                .OrderBy(a => a.Position)
                .ToListAsync(cancellationToken);
            
            _cache.Set(AttributeListCacheKey, attributes, CacheDuration);
        }

        return attributes ?? new List<EavAttribute>();
    }

    public async Task<IEnumerable<EavAttribute>> GetByGroupAsync(Guid attributeGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.Attributes
            .Where(a => a.AttributeGroupId == attributeGroupId)
            .OrderBy(a => a.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<EavAttribute> CreateAsync(EavAttribute attribute, CancellationToken cancellationToken = default)
    {
        _context.Attributes.Add(attribute);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        _cache.Remove(AttributeListCacheKey);
        
        return attribute;
    }

    public async Task<EavAttribute> UpdateAsync(EavAttribute attribute, CancellationToken cancellationToken = default)
    {
        _context.Attributes.Update(attribute);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        _cache.Remove(AttributeListCacheKey);
        _cache.Remove($"attr_{attribute.AttributeCode}");
        
        return attribute;
    }

    public async Task DeleteAsync(Guid attributeId, CancellationToken cancellationToken = default)
    {
        var attribute = await _context.Attributes.FindAsync(new object[] { attributeId }, cancellationToken);
        if (attribute != null)
        {
            _context.Attributes.Remove(attribute);
            await _context.SaveChangesAsync(cancellationToken);
            
            // Invalidate cache
            _cache.Remove(AttributeListCacheKey);
            _cache.Remove($"attr_{attribute.AttributeCode}");
        }
    }

    #endregion

    #region Attribute Set Operations

    public async Task<AttributeSet?> GetAttributeSetByIdAsync(Guid attributeSetId, CancellationToken cancellationToken = default)
    {
        return await _context.AttributeSets
            .Include(s => s.Groups)
                .ThenInclude(g => g.Attributes)
            .FirstOrDefaultAsync(s => s.AttributeSetId == attributeSetId, cancellationToken);
    }

    public async Task<IEnumerable<AttributeSet>> GetAllAttributeSetsAsync(CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(AttributeSetListCacheKey, out List<AttributeSet>? attributeSets))
        {
            attributeSets = await _context.AttributeSets
                .Include(s => s.Groups)
                .OrderBy(s => s.SortOrder)
                .ToListAsync(cancellationToken);
            
            _cache.Set(AttributeSetListCacheKey, attributeSets, CacheDuration);
        }

        return attributeSets ?? new List<AttributeSet>();
    }

    public async Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet, CancellationToken cancellationToken = default)
    {
        _context.AttributeSets.Add(attributeSet);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        _cache.Remove(AttributeSetListCacheKey);
        
        return attributeSet;
    }

    #endregion

    #region Attribute Group Operations

    public async Task<AttributeGroup?> GetAttributeGroupByIdAsync(Guid attributeGroupId, CancellationToken cancellationToken = default)
    {
        return await _context.AttributeGroups
            .Include(g => g.Attributes)
            .FirstOrDefaultAsync(g => g.AttributeGroupId == attributeGroupId, cancellationToken);
    }

    public async Task<IEnumerable<AttributeGroup>> GetAttributeGroupsBySetAsync(Guid attributeSetId, CancellationToken cancellationToken = default)
    {
        return await _context.AttributeGroups
            .Where(g => g.AttributeSetId == attributeSetId)
            .Include(g => g.Attributes)
            .OrderBy(g => g.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<AttributeGroup> CreateAttributeGroupAsync(AttributeGroup attributeGroup, CancellationToken cancellationToken = default)
    {
        _context.AttributeGroups.Add(attributeGroup);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Invalidate cache
        _cache.Remove(AttributeSetListCacheKey);
        
        return attributeGroup;
    }

    #endregion
}
