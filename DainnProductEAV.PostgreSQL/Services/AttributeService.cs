using DainnProductEAV.PostgreSQL.Entities;
using DainnProductEAV.PostgreSQL.Repositories;

namespace DainnProductEAV.PostgreSQL.Services;

/// <summary>
/// Service implementation for EAV attribute management.
/// </summary>
public class AttributeService : IAttributeService
{
    private readonly IAttributeRepository _attributeRepository;
    private static readonly HashSet<string> ValidBackendTypes = new() { "varchar", "int", "decimal", "text", "datetime" };
    private static readonly HashSet<string> ValidFrontendInputs = new() 
    { 
        "text", "textarea", "select", "multiselect", "boolean", "date", "price", "media_image", "gallery" 
    };

    public AttributeService(IAttributeRepository attributeRepository)
    {
        _attributeRepository = attributeRepository;
    }

    #region Attribute Operations

    public async Task<EavAttribute?> GetAttributeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<EavAttribute?> GetAttributeByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetByCodeAsync(code, cancellationToken);
    }

    public async Task<IEnumerable<EavAttribute>> GetAllAttributesAsync(CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetAllAsync(cancellationToken);
    }

    public async Task<EavAttribute> CreateAttributeAsync(
        string attributeCode,
        string backendType,
        string frontendInput,
        string? frontendLabel = null,
        bool isRequired = false,
        bool isSearchable = false,
        bool isFilterable = false,
        Guid? attributeGroupId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate backend type
        if (!ValidBackendTypes.Contains(backendType.ToLowerInvariant()))
        {
            throw new ArgumentException($"Invalid backend type: {backendType}. Valid types are: {string.Join(", ", ValidBackendTypes)}");
        }

        // Validate frontend input
        if (!ValidFrontendInputs.Contains(frontendInput.ToLowerInvariant()))
        {
            throw new ArgumentException($"Invalid frontend input: {frontendInput}. Valid inputs are: {string.Join(", ", ValidFrontendInputs)}");
        }

        // Check for duplicate attribute code
        var existing = await _attributeRepository.GetByCodeAsync(attributeCode, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Attribute with code '{attributeCode}' already exists.");
        }

        var attribute = new EavAttribute
        {
            AttributeCode = attributeCode,
            BackendType = backendType.ToLowerInvariant(),
            FrontendInput = frontendInput.ToLowerInvariant(),
            FrontendLabel = frontendLabel ?? attributeCode,
            IsRequired = isRequired,
            IsSearchable = isSearchable,
            IsFilterable = isFilterable,
            AttributeGroupId = attributeGroupId
        };

        return await _attributeRepository.CreateAsync(attribute, cancellationToken);
    }

    public async Task<EavAttribute> UpdateAttributeAsync(EavAttribute attribute, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.UpdateAsync(attribute, cancellationToken);
    }

    public async Task DeleteAttributeAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _attributeRepository.DeleteAsync(id, cancellationToken);
    }

    #endregion

    #region Attribute Set Operations

    public async Task<AttributeSet?> GetAttributeSetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetAttributeSetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<AttributeSet>> GetAllAttributeSetsAsync(CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetAllAttributeSetsAsync(cancellationToken);
    }

    public async Task<AttributeSet> CreateAttributeSetAsync(string name, CancellationToken cancellationToken = default)
    {
        var attributeSet = new AttributeSet
        {
            AttributeSetName = name,
            EntityTypeId = 4 // Product entity type
        };

        return await _attributeRepository.CreateAttributeSetAsync(attributeSet, cancellationToken);
    }

    #endregion

    #region Attribute Group Operations

    public async Task<AttributeGroup?> GetAttributeGroupAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetAttributeGroupByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<AttributeGroup>> GetAttributeGroupsBySetAsync(Guid attributeSetId, CancellationToken cancellationToken = default)
    {
        return await _attributeRepository.GetAttributeGroupsBySetAsync(attributeSetId, cancellationToken);
    }

    public async Task<AttributeGroup> CreateAttributeGroupAsync(string name, Guid attributeSetId, int sortOrder = 0, CancellationToken cancellationToken = default)
    {
        var attributeGroup = new AttributeGroup
        {
            AttributeGroupName = name,
            AttributeSetId = attributeSetId,
            SortOrder = sortOrder
        };

        return await _attributeRepository.CreateAttributeGroupAsync(attributeGroup, cancellationToken);
    }

    #endregion
}
