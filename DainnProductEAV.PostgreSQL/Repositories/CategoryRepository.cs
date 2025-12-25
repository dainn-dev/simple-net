using DainnProductEAV.PostgreSQL.Contexts;
using DainnProductEAV.PostgreSQL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DainnProductEAV.PostgreSQL.Repositories;

/// <summary>
/// Repository implementation for category operations.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly ProductCatalogDbContext _context;

    public CategoryRepository(ProductCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryEntity?> GetByIdAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.EntityId == categoryId, cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetRootCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == null)
            .OrderBy(c => c.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetChildrenAsync(Guid parentId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CategoryEntity>> GetTreeAsync(Guid? rootId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<CategoryEntity> query = _context.Categories;

        if (rootId.HasValue)
        {
            var root = await _context.Categories.FindAsync(new object[] { rootId }, cancellationToken);
            if (root != null)
            {
                query = query.Where(c => c.Path.StartsWith(root.Path));
            }
        }

        return await query
            .OrderBy(c => c.Level)
            .ThenBy(c => c.Position)
            .ToListAsync(cancellationToken);
    }

    public async Task<CategoryEntity> CreateAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        // Calculate path and level
        if (category.ParentId.HasValue)
        {
            var parent = await _context.Categories.FindAsync(new object[] { category.ParentId }, cancellationToken);
            if (parent != null)
            {
                category.Level = parent.Level + 1;
            }
        }
        else
        {
            category.Level = 0;
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);

        // Update path after getting the ID
        if (category.ParentId.HasValue)
        {
            var parent = await _context.Categories.FindAsync(new object[] { category.ParentId }, cancellationToken);
            category.Path = $"{parent?.Path}/{category.EntityId:N}";
        }
        else
        {
            category.Path = category.EntityId.ToString("N");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task<CategoryEntity> UpdateAsync(CategoryEntity category, CancellationToken cancellationToken = default)
    {
        category.UpdatedAt = DateTime.UtcNow;
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task DeleteAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { categoryId }, cancellationToken);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MoveAsync(Guid categoryId, Guid newParentId, int position = 0, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories.FindAsync(new object[] { categoryId }, cancellationToken);
        var newParent = await _context.Categories.FindAsync(new object[] { newParentId }, cancellationToken);

        if (category != null && newParent != null)
        {
            category.ParentId = newParentId;
            category.Position = position;
            category.Level = newParent.Level + 1;
            category.Path = $"{newParent.Path}/{category.EntityId:N}";
            category.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
