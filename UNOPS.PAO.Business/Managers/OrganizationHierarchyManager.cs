using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models.OrganizationUnits;

namespace UNOPS.PAO.Business.Managers;

public class OrganizationHierarchyManager : IOrganizationHierarchyManager
{
    private readonly ValuesRepository _valuesRepository;
    private readonly IMapper _mapper;

    public OrganizationHierarchyManager(ValuesRepository valuesRepository, IMapper mapper)
    {
        _valuesRepository = valuesRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrganizationHierarchyTreeModel>> GetOrganizationHierarchy()
    {
        return await _valuesRepository.GetOrganizationHierarchy();
    }

    public async Task<IEnumerable<OrganizationHierarchyPrimeModel>> GetOrganizationHierarchyPrime()
    {
        return await _valuesRepository.GetOrganizationHierarchyPrime();
    }

    public async Task<OrganizationHierarchyModel> GetOrganizationHierarchyById(int id)
    {
        var entity = await _valuesRepository.GetOrganizationHierarchyById(id);
        return _mapper.Map<OrganizationHierarchyModel>(entity);
    }

    public IEnumerable<OrganizationHierarchyModel> GetOrganizationsByType(OrganizationUnitType type)
    {
        var entities = _valuesRepository.GetOrganizationsByType(type);
        return _mapper.Map<IEnumerable<OrganizationHierarchyModel>>(entities);
    }

    public IEnumerable<OrganizationHierarchyModel> GetAllOrganizations()
    {
        var entities = _valuesRepository.GetAllOrganizations();
        return _mapper.Map<IEnumerable<OrganizationHierarchyModel>>(entities);
    }
} 