using DainnProductEAVManagement.Entities;

namespace DainnProductEAVManagement.Repositories;

/// <summary>
/// Repository interface for EAV attribute operations.
/// </summary>
public interface IAttributeRepository
{
    Task<EavAttribute?> GetByIdAsync(Guid attributeId, CancellationToken cancellationToken = default);
    Task<EavAttribute?> GetByCodeAsync(string attributeCode, CancellationToken cancellationToken = default);
    Task<IEnumerable<EavAttribute>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EavAttribute>> GetByGroupAsync(Guid attributeGroupId, CancellationToken cancellationToken = default);
    Task<EavAttribute> CreateAsync(EavAttribute attribute, CancellationToken cancellationToken = default);
    Task<EavAttribute> UpdateAsync(EavAttribute attribute, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid attributeId, CancellationToken cancellationToken = default);
    
    // Attribute Set operations
    Task<AttributeSet?> GetAttributeSetByIdAsync(Guid attributeSetId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttributeSet>> GetAllAttributeSetsAsync(CancellationToken cancellationToken = default);
    Task<AttributeSet> CreateAttributeSetAsync(AttributeSet attributeSet, CancellationToken cancellationToken = default);
    
    // Attribute Group operations
    Task<AttributeGroup?> GetAttributeGroupByIdAsync(Guid attributeGroupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<AttributeGroup>> GetAttributeGroupsBySetAsync(Guid attributeSetId, CancellationToken cancellationToken = default);
    Task<AttributeGroup> CreateAttributeGroupAsync(AttributeGroup attributeGroup, CancellationToken cancellationToken = default);
}
