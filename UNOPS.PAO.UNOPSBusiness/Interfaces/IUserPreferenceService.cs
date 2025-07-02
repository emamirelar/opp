namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IUserPreferenceService
{
    Task<int?> GetDefaultOrgUnitIdAsync(int userId);
    Task UpdateDefaultOrgUnitAsync(int userId, int? orgUnitId);
}