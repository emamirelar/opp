using System.Security.Claims;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.EntityConfiguration;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IUNOPSEntityConfigurationManager
{
    // Entity operations
    Task<IEnumerable<Entities>> GetAllEntitiesAsync();
    Task<IEnumerable<EntityManager>> GetAllEntityConfigurationsAsync(ClaimsPrincipal user);
    Task<EntityManager?> GetEntityConfigurationAsync(ClaimsPrincipal user, int id);
    Task<EntityManager?> GetEntityConfigurationByNameAsync(ClaimsPrincipal user, string entityName);
    Task<EntityManager> CreateEntityConfigurationAsync(ClaimsPrincipal user, CreateEntityConfigurationRequest request);
    Task<EntityManager> UpdateEntityConfigurationAsync(ClaimsPrincipal user, UpdateEntityConfigurationRequest request);
    Task DeleteEntityConfigurationAsync(ClaimsPrincipal user, int id);
    
    // Entity field operations
    Task<IEnumerable<EntityFieldManager>> GetEntityFieldsAsync(ClaimsPrincipal user, int entityManagerId);
    Task<EntityFieldManager?> GetEntityFieldAsync(ClaimsPrincipal user, int fieldId);
    Task<EntityFieldManager> CreateEntityFieldAsync(ClaimsPrincipal user, CreateEntityFieldRequest request);
    Task<EntityFieldManager> UpdateEntityFieldAsync(ClaimsPrincipal user, UpdateEntityFieldRequest request);
    Task DeleteEntityFieldAsync(ClaimsPrincipal user, int fieldId);
    
    // Combined operations for UI
    Task<EntityConfigurationDetailsResponse> GetEntityConfigurationDetailsAsync(ClaimsPrincipal user, string entityName);
    Task<EntityConfigurationDetailsResponse> SaveEntityConfigurationDetailsAsync(ClaimsPrincipal user, SaveEntityConfigurationRequest request);
    
    // Related entity field options
    Task<IEnumerable<RelatedFieldOptionDto>> GetRelatedEntityFieldsAsync(ClaimsPrincipal user, string entityType);
    Task<IEnumerable<RelatedFieldOptionDto>> GetFieldOptionsForDataTypeAsync(ClaimsPrincipal user, string dataType, string contextEntityName);
    Task<IEnumerable<ListViewColumnDto>> GetEntityListViewConfigurationAsync(ClaimsPrincipal user, string entityName);
    
    // SQL Export functionality
    Task<string> ExportEntityConfigurationAsSqlAsync(ClaimsPrincipal user);
} 