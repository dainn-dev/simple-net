using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnProductEAVManagement.Services;
using DainnProductEAVManagement.Entities;
using DainnUserManagement.API.Dtos.Catalog;
using DainnUserManagement.API.Controllers.Admin;

namespace DainnUserManagement.API.Controllers.Catalog;

/// <summary>
/// Controller for category management.
/// Provides endpoints for CRUD operations on product categories.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/catalog/categories")]
[ApiVersion("1.0")]
[Tags("Catalog - Categories")]
[Produces("application/json")]
[Consumes("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;

    public CategoriesController(ICategoryService categoryService, IProductService productService)
    {
        _categoryService = categoryService;
        _productService = productService;
    }

    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <returns>List of all categories.</returns>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(List<CategoryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryListDto>>> GetAll()
    {
        var categories = await _categoryService.GetAllCategoriesAsync();

        var categoryList = categories.Select(c => new CategoryListDto
        {
            EntityId = c.EntityId,
            ParentId = c.ParentId,
            Name = c.Name,
            UrlKey = c.UrlKey,
            Level = c.Level,
            Position = c.Position,
            IsActive = c.IsActive,
            ChildCount = c.Children?.Count ?? 0
        }).ToList();

        return Ok(categoryList);
    }

    /// <summary>
    /// Gets root categories (top-level categories without a parent).
    /// </summary>
    /// <returns>List of root categories.</returns>
    [HttpGet("roots")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(List<CategoryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryListDto>>> GetRoots()
    {
        var categories = await _categoryService.GetRootCategoriesAsync();

        var categoryList = categories.Select(c => new CategoryListDto
        {
            EntityId = c.EntityId,
            ParentId = c.ParentId,
            Name = c.Name,
            UrlKey = c.UrlKey,
            Level = c.Level,
            Position = c.Position,
            IsActive = c.IsActive,
            ChildCount = c.Children?.Count ?? 0
        }).ToList();

        return Ok(categoryList);
    }

    /// <summary>
    /// Gets the category tree starting from an optional root.
    /// </summary>
    /// <param name="rootId">Optional root category ID. If not provided, returns the full tree.</param>
    /// <returns>Category tree structure.</returns>
    [HttpGet("tree")]
    [ResponseCache(Duration = 120)]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetTree(Guid? rootId = null)
    {
        var categories = await _categoryService.GetCategoryTreeAsync(rootId);

        // Build tree structure
        var categoryDict = categories.ToDictionary(c => c.EntityId);
        var rootCategories = new List<CategoryDto>();

        foreach (var category in categories)
        {
            var dto = MapToCategoryDto(category);

            if (category.ParentId == null || (rootId.HasValue && category.EntityId == rootId))
            {
                rootCategories.Add(dto);
            }
        }

        // Build children recursively
        foreach (var root in rootCategories)
        {
            BuildChildren(root, categories);
        }

        return Ok(rootCategories);
    }

    /// <summary>
    /// Gets a category by its ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The category details.</returns>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        var dto = MapToCategoryDto(category);

        // Add children
        if (category.Children != null)
        {
            dto.Children = category.Children.Select(MapToCategoryDto).ToList();
        }

        return Ok(dto);
    }

    /// <summary>
    /// Gets products in a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="storeId">Store ID.</param>
    /// <returns>Products in the category.</returns>
    [HttpGet("{id:guid}/products")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> GetProducts(
        Guid id, 
        int page = 1, 
        int pageSize = 20,
        int storeId = 0)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        var products = await _productService.GetProductsByCategoryAsync(id, storeId, page, pageSize);
        var productList = new List<ProductListDto>();

        foreach (var product in products)
        {
            var name = await _productService.GetAttributeAsync<string>(product.EntityId, "name", storeId);
            var price = await _productService.GetAttributeAsync<decimal?>(product.EntityId, "price", storeId);
            var status = await _productService.GetAttributeAsync<int?>(product.EntityId, "status", storeId);

            productList.Add(new ProductListDto
            {
                EntityId = product.EntityId,
                Sku = product.Sku,
                TypeId = product.TypeId,
                Name = name,
                Price = price,
                Status = status,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            });
        }

        return Ok(new PaginatedResponse<ProductListDto>
        {
            Items = productList,
            Page = page,
            PageSize = pageSize,
            TotalCount = productList.Count,
            TotalPages = 1
        });
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <returns>The created category.</returns>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateCategoryAsync(dto.Name, dto.ParentId);

        // Update additional properties
        if (!string.IsNullOrEmpty(dto.UrlKey))
        {
            category.UrlKey = dto.UrlKey;
        }
        if (!string.IsNullOrEmpty(dto.Description))
        {
            category.Description = dto.Description;
        }
        category.IsActive = dto.IsActive;
        category.Position = dto.Position;
        category.MetaTitle = dto.MetaTitle;
        category.MetaDescription = dto.MetaDescription;

        await _categoryService.UpdateCategoryAsync(category);

        return CreatedAtAction(nameof(GetById), new { id = category.EntityId }, MapToCategoryDto(category));
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>The updated category.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        if (!string.IsNullOrEmpty(dto.Name))
        {
            category.Name = dto.Name;
        }
        if (dto.UrlKey != null)
        {
            category.UrlKey = dto.UrlKey;
        }
        if (dto.Description != null)
        {
            category.Description = dto.Description;
        }
        if (dto.IsActive.HasValue)
        {
            category.IsActive = dto.IsActive.Value;
        }
        if (dto.Position.HasValue)
        {
            category.Position = dto.Position.Value;
        }
        if (dto.MetaTitle != null)
        {
            category.MetaTitle = dto.MetaTitle;
        }
        if (dto.MetaDescription != null)
        {
            category.MetaDescription = dto.MetaDescription;
        }

        await _categoryService.UpdateCategoryAsync(category);

        return Ok(MapToCategoryDto(category));
    }

    /// <summary>
    /// Moves a category to a new parent.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="dto">The move data.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id:guid}/move")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Move(Guid id, [FromBody] MoveCategoryDto dto)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        await _categoryService.MoveCategoryAsync(id, dto.NewParentId, dto.Position);
        return Ok(new { message = "Category moved successfully" });
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var category = await _categoryService.GetCategoryAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"Category with ID {id} not found" });
        }

        await _categoryService.DeleteCategoryAsync(id);
        return Ok(new { message = "Category deleted successfully" });
    }

    private static CategoryDto MapToCategoryDto(CategoryEntity category)
    {
        return new CategoryDto
        {
            EntityId = category.EntityId,
            ParentId = category.ParentId,
            Name = category.Name,
            UrlKey = category.UrlKey,
            Description = category.Description,
            Path = category.Path,
            Level = category.Level,
            Position = category.Position,
            IsActive = category.IsActive,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
            ProductCount = category.ProductRelations?.Count ?? 0
        };
    }

    private static void BuildChildren(CategoryDto parent, IEnumerable<CategoryEntity> allCategories)
    {
        var children = allCategories
            .Where(c => c.ParentId == parent.EntityId)
            .Select(MapToCategoryDto)
            .ToList();

        if (children.Any())
        {
            parent.Children = children;
            foreach (var child in children)
            {
                BuildChildren(child, allCategories);
            }
        }
    }
}
