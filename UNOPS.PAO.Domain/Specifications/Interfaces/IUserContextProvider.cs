namespace UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// Interface for providing user context to specifications
/// </summary>
public interface IUserContextProvider
{
    Task<string?> GetCurrentUserOrgUnitAsync();
}