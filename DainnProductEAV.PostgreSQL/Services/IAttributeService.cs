using DainnProductEAV.PostgreSQL.Entities;

namespace DainnProductEAV.PostgreSQL.Services;

/// <summary>
/// Service interface for EAV attribute management.
/// </summary>
public interface IAttributeService
{
    Task<EavAttribute?> GetAttributeAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EavAttribute?> GetAttributeByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<EavAttribute>> GetAllAttributesAsync(CancellationToken cancellationToken = default);
    Task<EavAttribute> CreateAttributeAsync(
        string attributeCode, 
        string backendType, 
        string frontendInput,
        string? frontendLabel = null,
        bool isRequired = false,
        bool isSearchable = false,
        bool isFilterable = false,
        Guid? attributeGroupId = null,
        CancellationToken cancellationToken = default);
    Task<EavAttribute> UpdateAttributeAsync(EavAttribute attribute, CancellationToken cancellationToken = default);
    Task DeleteAttributeAsync(Guid id, CancellationToken cancellationToken = default);

    // Attribute Set operations
    Task<AttributeSet?> GetAttributeSetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttributeSet>> GetAllAttributeSetsAsync(CancellationToken cancellationToken = default);
    Task<AttributeSet> CreateAttributeSetAsync(string name, CancellationToken cancellationToken = default);

    // Attribute Group operations  
    Task<AttributeGroup?> GetAttributeGroupAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttributeGroup>> GetAttributeGroupsBySetAsync(Guid attributeSetId, CancellationToken cancellationToken = default);
    Task<AttributeGroup> CreateAttributeGroupAsync(string name, Guid attributeSetId, int sortOrder = 0, CancellationToken cancellationToken = default);
}
