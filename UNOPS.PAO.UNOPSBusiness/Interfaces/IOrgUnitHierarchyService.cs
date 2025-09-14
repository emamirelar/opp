namespace UNOPS.PAO.UNOPSBusiness.Interfaces;

public interface IOrgUnitHierarchyService
{
    Task<List<int>> GetDescendantIdsAsync(int orgUnitId);
}