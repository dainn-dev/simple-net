using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnProductEAV.PostgreSQL.Services;
using DainnProductEAV.PostgreSQL.Entities;
using DainnUserManagement.API.Dtos.Catalog;
using DainnUserManagement.API.Controllers.Admin;

namespace DainnUserManagement.API.Controllers.Catalog;

/// <summary>
/// Controller for product catalog management.
/// Provides endpoints for CRUD operations on products with EAV attribute support.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/catalog/products")]
[ApiVersion("1.0")]
[Tags("Catalog - Products")]
[Produces("application/json")]
[Consumes("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Gets a paginated list of products.
    /// </summary>
    /// <param name="page">Page number (starting from 1).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="storeId">Store ID for store-specific values. 0 = global.</param>
    /// <returns>Paginated list of products.</returns>
    [HttpGet]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> GetAll(
        int page = 1, 
        int pageSize = 20,
        int storeId = 0)
    {
        var products = await _productService.GetProductsAsync(storeId, page, pageSize);
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

        // Note: For proper pagination, you'd need to add a count method to the service
        return Ok(new PaginatedResponse<ProductListDto>
        {
            Items = productList,
            Page = page,
            PageSize = pageSize,
            TotalCount = productList.Count, // This is simplified; should be total count
            TotalPages = 1 // Should be calculated from total count
        });
    }

    /// <summary>
    /// Gets a product by its ID with all attribute values.
    /// </summary>
    /// <param name="id">The product entity ID.</param>
    /// <param name="storeId">Store ID for store-specific values.</param>
    /// <returns>The product with all attributes.</returns>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, int storeId = 0)
    {
        var product = await _productService.GetProductAsync(id, storeId);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        // Get common attributes
        var name = await _productService.GetAttributeAsync<string>(id, "name", storeId);
        var description = await _productService.GetAttributeAsync<string>(id, "description", storeId);
        var price = await _productService.GetAttributeAsync<decimal?>(id, "price", storeId);
        var status = await _productService.GetAttributeAsync<int?>(id, "status", storeId);

        var dto = new ProductDto
        {
            EntityId = product.EntityId,
            Sku = product.Sku,
            TypeId = product.TypeId,
            AttributeSetId = product.AttributeSetId,
            AttributeSetName = product.AttributeSet?.AttributeSetName,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Attributes = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["description"] = description,
                ["price"] = price,
                ["status"] = status
            }
        };

        // Add categories
        if (product.CategoryRelations != null)
        {
            dto.Categories = product.CategoryRelations.Select(cr => new CategorySummaryDto
            {
                EntityId = cr.Category.EntityId,
                Name = cr.Category.Name,
                UrlKey = cr.Category.UrlKey
            }).ToList();
        }

        // Add inventory
        if (product.Inventory != null)
        {
            dto.Inventory = new ProductInventoryDto
            {
                Qty = product.Inventory.Qty,
                IsInStock = product.Inventory.IsInStock,
                MinQty = product.Inventory.MinQty,
                MinSaleQty = product.Inventory.MinSaleQty,
                MaxSaleQty = product.Inventory.MaxSaleQty,
                ManageStock = product.Inventory.ManageStock
            };
        }

        return Ok(dto);
    }

    /// <summary>
    /// Gets a product by its SKU.
    /// </summary>
    /// <param name="sku">The product SKU.</param>
    /// <param name="storeId">Store ID for store-specific values.</param>
    /// <returns>The product with all attributes.</returns>
    [HttpGet("sku/{sku}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetBySku(string sku, int storeId = 0)
    {
        var product = await _productService.GetProductBySkuAsync(sku, storeId);
        if (product == null)
        {
            return NotFound(new { message = $"Product with SKU '{sku}' not found" });
        }

        return await GetById(product.EntityId, storeId);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var product = await _productService.CreateProductAsync(dto.Sku, dto.TypeId, dto.AttributeSetId);

            // Set initial attributes
            if (dto.Attributes != null)
            {
                await _productService.SetAttributesAsync(product.EntityId, dto.Attributes);
            }

            return CreatedAtAction(nameof(GetById), new { id = product.EntityId }, 
                await GetProductDto(product.EntityId, 0));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        if (!string.IsNullOrEmpty(dto.Sku))
        {
            product.Sku = dto.Sku;
        }

        if (!string.IsNullOrEmpty(dto.TypeId))
        {
            product.TypeId = dto.TypeId;
        }

        await _productService.UpdateProductAsync(product);

        // Update attributes
        if (dto.Attributes != null)
        {
            await _productService.SetAttributesAsync(id, dto.Attributes);
        }

        return Ok(await GetProductDto(id, 0));
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _productService.DeleteProductAsync(id);
        return Ok(new { message = "Product deleted successfully" });
    }

    /// <summary>
    /// Sets an attribute value on a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="dto">The attribute data.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id:guid}/attributes")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetAttribute(Guid id, [FromBody] SetAttributeDto dto)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        try
        {
            await _productService.SetAttributeAsync(id, dto.AttributeCode, dto.Value, dto.StoreId);
            return Ok(new { message = $"Attribute '{dto.AttributeCode}' set successfully" });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Sets multiple attribute values on a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="dto">The attributes data.</param>
    /// <returns>Success response.</returns>
    [HttpPut("{id:guid}/attributes")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetAttributes(Guid id, [FromBody] SetAttributesDto dto)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _productService.SetAttributesAsync(id, dto.Attributes, dto.StoreId);
        return Ok(new { message = "Attributes updated successfully" });
    }

    /// <summary>
    /// Gets an attribute value from a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="attributeCode">The attribute code.</param>
    /// <param name="storeId">Store ID for store-specific values.</param>
    /// <returns>The attribute value.</returns>
    [HttpGet("{id:guid}/attributes/{attributeCode}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> GetAttribute(Guid id, string attributeCode, int storeId = 0)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        var value = await _productService.GetAttributeAsync<object>(id, attributeCode, storeId);
        return Ok(new { attributeCode, value, storeId });
    }

    /// <summary>
    /// Updates product inventory.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="dto">The inventory data.</param>
    /// <returns>Success response.</returns>
    [HttpPut("{id:guid}/inventory")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateInventory(Guid id, [FromBody] UpdateInventoryDto dto)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _productService.UpdateInventoryAsync(id, dto.Qty, dto.IsInStock);
        return Ok(new { message = "Inventory updated successfully" });
    }

    /// <summary>
    /// Assigns a product to a category.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="dto">The category assignment data.</param>
    /// <returns>Success response.</returns>
    [HttpPost("{id:guid}/categories")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> AssignToCategory(Guid id, [FromBody] AssignCategoryDto dto)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _productService.AssignProductToCategoryAsync(id, dto.CategoryId, dto.Position);
        return Ok(new { message = "Product assigned to category successfully" });
    }

    /// <summary>
    /// Removes a product from a category.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="categoryId">The category ID.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{id:guid}/categories/{categoryId:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveFromCategory(Guid id, Guid categoryId)
    {
        var product = await _productService.GetProductAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found" });
        }

        await _productService.RemoveProductFromCategoryAsync(id, categoryId);
        return Ok(new { message = "Product removed from category successfully" });
    }

    /// <summary>
    /// Searches for products.
    /// </summary>
    /// <param name="query">Search query.</param>
    /// <param name="page">Page number.</param>
    /// <param name="pageSize">Page size.</param>
    /// <param name="storeId">Store ID.</param>
    /// <returns>Search results.</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PaginatedResponse<ProductListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ProductListDto>>> Search(
        string query,
        int page = 1,
        int pageSize = 20,
        int storeId = 0)
    {
        var products = await _productService.SearchProductsAsync(query, storeId, page, pageSize);
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

    private async Task<ProductDto> GetProductDto(Guid id, int storeId)
    {
        var product = await _productService.GetProductAsync(id, storeId);
        var name = await _productService.GetAttributeAsync<string>(id, "name", storeId);
        var price = await _productService.GetAttributeAsync<decimal?>(id, "price", storeId);
        var status = await _productService.GetAttributeAsync<int?>(id, "status", storeId);

        return new ProductDto
        {
            EntityId = product!.EntityId,
            Sku = product.Sku,
            TypeId = product.TypeId,
            AttributeSetId = product.AttributeSetId,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt,
            Attributes = new Dictionary<string, object?>
            {
                ["name"] = name,
                ["price"] = price,
                ["status"] = status
            }
        };
    }
}
