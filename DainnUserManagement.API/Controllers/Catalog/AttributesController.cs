using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DainnProductEAVManagement.Services;
using DainnProductEAVManagement.Entities;
using DainnUserManagement.API.Dtos.Catalog;

namespace DainnUserManagement.API.Controllers.Catalog;

/// <summary>
/// Controller for EAV attribute management.
/// Provides endpoints for managing product attributes, attribute sets, and attribute groups.
/// </summary>
[ApiController]
[Route("api/v{version:apiVersion}/catalog/attributes")]
[ApiVersion("1.0")]
[Tags("Catalog - Attributes")]
[Produces("application/json")]
[Consumes("application/json")]
public class AttributesController : ControllerBase
{
    private readonly IAttributeService _attributeService;

    public AttributesController(IAttributeService attributeService)
    {
        _attributeService = attributeService;
    }

    #region Attributes

    /// <summary>
    /// Gets all attributes.
    /// </summary>
    /// <returns>List of all EAV attributes.</returns>
    [HttpGet]
    [ResponseCache(Duration = 120)]
    [ProducesResponseType(typeof(List<AttributeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AttributeDto>>> GetAllAttributes()
    {
        var attributes = await _attributeService.GetAllAttributesAsync();

        var attributeList = attributes.Select(a => MapToAttributeDto(a)).ToList();

        return Ok(attributeList);
    }

    /// <summary>
    /// Gets an attribute by ID.
    /// </summary>
    /// <param name="id">The attribute ID.</param>
    /// <returns>The attribute details.</returns>
    [HttpGet("{id:guid}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeDto>> GetAttributeById(Guid id)
    {
        var attribute = await _attributeService.GetAttributeAsync(id);
        if (attribute == null)
        {
            return NotFound(new { message = $"Attribute with ID {id} not found" });
        }

        return Ok(MapToAttributeDto(attribute));
    }

    /// <summary>
    /// Gets an attribute by code.
    /// </summary>
    /// <param name="code">The attribute code.</param>
    /// <returns>The attribute details.</returns>
    [HttpGet("code/{code}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeDto>> GetAttributeByCode(string code)
    {
        var attribute = await _attributeService.GetAttributeByCodeAsync(code);
        if (attribute == null)
        {
            return NotFound(new { message = $"Attribute with code '{code}' not found" });
        }

        return Ok(MapToAttributeDto(attribute));
    }

    /// <summary>
    /// Creates a new attribute.
    /// </summary>
    /// <param name="dto">The attribute creation data.</param>
    /// <returns>The created attribute.</returns>
    [HttpPost]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AttributeDto>> CreateAttribute([FromBody] CreateAttributeDto dto)
    {
        try
        {
            var attribute = await _attributeService.CreateAttributeAsync(
                dto.AttributeCode,
                dto.BackendType,
                dto.FrontendInput,
                dto.FrontendLabel,
                dto.IsRequired,
                dto.IsSearchable,
                dto.IsFilterable,
                dto.AttributeGroupId);

            return CreatedAtAction(nameof(GetAttributeById), new { id = attribute.AttributeId }, MapToAttributeDto(attribute));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing attribute.
    /// </summary>
    /// <param name="id">The attribute ID.</param>
    /// <param name="dto">The update data.</param>
    /// <returns>The updated attribute.</returns>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(AttributeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeDto>> UpdateAttribute(Guid id, [FromBody] UpdateAttributeDto dto)
    {
        var attribute = await _attributeService.GetAttributeAsync(id);
        if (attribute == null)
        {
            return NotFound(new { message = $"Attribute with ID {id} not found" });
        }

        if (dto.FrontendLabel != null)
        {
            attribute.FrontendLabel = dto.FrontendLabel;
        }
        if (dto.FrontendInput != null)
        {
            attribute.FrontendInput = dto.FrontendInput;
        }
        if (dto.IsRequired.HasValue)
        {
            attribute.IsRequired = dto.IsRequired.Value;
        }
        if (dto.IsSearchable.HasValue)
        {
            attribute.IsSearchable = dto.IsSearchable.Value;
        }
        if (dto.IsFilterable.HasValue)
        {
            attribute.IsFilterable = dto.IsFilterable.Value;
        }
        if (dto.IsComparable.HasValue)
        {
            attribute.IsComparable = dto.IsComparable.Value;
        }
        if (dto.AttributeGroupId.HasValue)
        {
            attribute.AttributeGroupId = dto.AttributeGroupId;
        }
        if (dto.Position.HasValue)
        {
            attribute.Position = dto.Position;
        }
        if (dto.DefaultValue != null)
        {
            attribute.DefaultValue = dto.DefaultValue;
        }

        await _attributeService.UpdateAttributeAsync(attribute);

        return Ok(MapToAttributeDto(attribute));
    }

    /// <summary>
    /// Deletes an attribute.
    /// </summary>
    /// <param name="id">The attribute ID.</param>
    /// <returns>Success response.</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAttribute(Guid id)
    {
        var attribute = await _attributeService.GetAttributeAsync(id);
        if (attribute == null)
        {
            return NotFound(new { message = $"Attribute with ID {id} not found" });
        }

        await _attributeService.DeleteAttributeAsync(id);
        return Ok(new { message = "Attribute deleted successfully" });
    }

    #endregion

    #region Attribute Sets

    /// <summary>
    /// Gets all attribute sets.
    /// </summary>
    /// <returns>List of all attribute sets.</returns>
    [HttpGet("sets")]
    [ResponseCache(Duration = 120)]
    [ProducesResponseType(typeof(List<AttributeSetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AttributeSetDto>>> GetAllAttributeSets()
    {
        var attributeSets = await _attributeService.GetAllAttributeSetsAsync();

        var setList = attributeSets.Select(s => MapToAttributeSetDto(s)).ToList();

        return Ok(setList);
    }

    /// <summary>
    /// Gets an attribute set by ID.
    /// </summary>
    /// <param name="id">The attribute set ID.</param>
    /// <returns>The attribute set with groups and attributes.</returns>
    [HttpGet("sets/{id:guid}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(AttributeSetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeSetDto>> GetAttributeSetById(Guid id)
    {
        var attributeSet = await _attributeService.GetAttributeSetAsync(id);
        if (attributeSet == null)
        {
            return NotFound(new { message = $"Attribute set with ID {id} not found" });
        }

        return Ok(MapToAttributeSetDto(attributeSet));
    }

    /// <summary>
    /// Creates a new attribute set.
    /// </summary>
    /// <param name="dto">The attribute set creation data.</param>
    /// <returns>The created attribute set.</returns>
    [HttpPost("sets")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(AttributeSetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AttributeSetDto>> CreateAttributeSet([FromBody] CreateAttributeSetDto dto)
    {
        var attributeSet = await _attributeService.CreateAttributeSetAsync(dto.Name);

        return CreatedAtAction(nameof(GetAttributeSetById), new { id = attributeSet.AttributeSetId }, MapToAttributeSetDto(attributeSet));
    }

    #endregion

    #region Attribute Groups

    /// <summary>
    /// Gets attribute groups for an attribute set.
    /// </summary>
    /// <param name="setId">The attribute set ID.</param>
    /// <returns>List of attribute groups.</returns>
    [HttpGet("sets/{setId:guid}/groups")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(List<AttributeGroupDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<AttributeGroupDto>>> GetAttributeGroups(Guid setId)
    {
        var groups = await _attributeService.GetAttributeGroupsBySetAsync(setId);

        var groupList = groups.Select(g => MapToAttributeGroupDto(g)).ToList();

        return Ok(groupList);
    }

    /// <summary>
    /// Gets an attribute group by ID.
    /// </summary>
    /// <param name="id">The attribute group ID.</param>
    /// <returns>The attribute group with attributes.</returns>
    [HttpGet("groups/{id:guid}")]
    [ResponseCache(Duration = 60)]
    [ProducesResponseType(typeof(AttributeGroupDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AttributeGroupDto>> GetAttributeGroupById(Guid id)
    {
        var group = await _attributeService.GetAttributeGroupAsync(id);
        if (group == null)
        {
            return NotFound(new { message = $"Attribute group with ID {id} not found" });
        }

        return Ok(MapToAttributeGroupDto(group));
    }

    /// <summary>
    /// Creates a new attribute group.
    /// </summary>
    /// <param name="dto">The attribute group creation data.</param>
    /// <returns>The created attribute group.</returns>
    [HttpPost("groups")]
    [Authorize(Policy = "Admin")]
    [ProducesResponseType(typeof(AttributeGroupDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AttributeGroupDto>> CreateAttributeGroup([FromBody] CreateAttributeGroupDto dto)
    {
        var group = await _attributeService.CreateAttributeGroupAsync(dto.Name, dto.AttributeSetId, dto.SortOrder);

        return CreatedAtAction(nameof(GetAttributeGroupById), new { id = group.AttributeGroupId }, MapToAttributeGroupDto(group));
    }

    #endregion

    #region Mappers

    private static AttributeDto MapToAttributeDto(EavAttribute attribute)
    {
        return new AttributeDto
        {
            AttributeId = attribute.AttributeId,
            AttributeCode = attribute.AttributeCode,
            BackendType = attribute.BackendType,
            FrontendInput = attribute.FrontendInput,
            FrontendLabel = attribute.FrontendLabel,
            IsRequired = attribute.IsRequired,
            IsUnique = attribute.IsUnique,
            IsSearchable = attribute.IsSearchable,
            IsFilterable = attribute.IsFilterable,
            IsComparable = attribute.IsComparable,
            IsVisibleOnFront = attribute.IsVisibleOnFront,
            Position = attribute.Position,
            DefaultValue = attribute.DefaultValue,
            AttributeGroupId = attribute.AttributeGroupId,
            AttributeGroupName = attribute.Group?.AttributeGroupName
        };
    }

    private static AttributeSetDto MapToAttributeSetDto(AttributeSet attributeSet)
    {
        return new AttributeSetDto
        {
            AttributeSetId = attributeSet.AttributeSetId,
            AttributeSetName = attributeSet.AttributeSetName,
            SortOrder = attributeSet.SortOrder,
            Groups = attributeSet.Groups?.Select(MapToAttributeGroupDto).ToList()
        };
    }

    private static AttributeGroupDto MapToAttributeGroupDto(AttributeGroup group)
    {
        return new AttributeGroupDto
        {
            AttributeGroupId = group.AttributeGroupId,
            AttributeSetId = group.AttributeSetId,
            AttributeGroupName = group.AttributeGroupName,
            SortOrder = group.SortOrder,
            Attributes = group.Attributes?.Select(MapToAttributeDto).ToList()
        };
    }

    #endregion
}
