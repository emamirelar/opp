namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Repositories;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Entities;

public class UNOPSContactManager : IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private static ContactModel MapEntityToModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);

        /*result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        
        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;*/

        //result.Extensions.Add("project", project);

        return result;
    }
    private static ExternalContactModel MapEntityToExternalModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ExternalContactModel>(entity);

        /*result.EligibleEntities = entity.EligibleEntities?.Select(mapper.Map<EligibleEntityModel>).ToList();
        
        var appType = typeof(ApplicationType)
            .GetMembers()
            .Select(x => new { value = x, attr = x.GetCustomAttributes(typeof(EnumDisplayNameAttribute), true).Cast<EnumDisplayNameAttribute>().SingleOrDefault() })
            .Where(x => x.attr != null)
            .Select(x => new ApplicationTypeModel() { Id = x.value.Name, DisplayName = x.attr?.Value })
            .Where(x => x.Id == entity.ApplicationTypeCode)
            .FirstOrDefault();

        result.ApplicationType = appType;*/

        //result.Extensions.Add("project", project);

        return result;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model, UNOPSContact entity)
    {
        mapper.Map(model, entity);

        entity.Name = String.Concat(model.Salutation, ' ', model.FirstName, ' ', model.MiddleName, ' ', model.LastName);

        // Update Eligible Entities
        /*if (entity.EligibleEntities != null)
        {
            entity.EligibleEntities.Clear();
        }
        else
        {
            entity.EligibleEntities = [];
        }

        if (model.EligibleEntityIds != null)
        {
            var entities = (from x in commonRepository.GetEligibleEntities()
                            join id in model.EligibleEntityIds on x.Id equals id
                            select x
                            );

            foreach (var e in entities)
            {
                entity.EligibleEntities.Add(e);
            }
        }*/

        return entity;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model)
    {
        return MapModelToEntity(model, new UNOPSContact());
    }

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context);

        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = MapModelToEntity(model);
        
        await contactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    public IEnumerable<ContactModel> GetContacts(int userId)
    {
        return contactRepository
            .GetAll()
            .Select(x => MapEntityToModel(x, mapper));
    }

    public async Task<ContactModel?> GetContact(int userId, int id)
    {
        var item = await contactRepository.GetByIdAsync(id);
        if (item == null)
        {
            return default;
        }

        return MapEntityToModel(item, mapper);
    }

    /*public async Task<string?> GetContactStage(int id)
    {
        var item = await contactRepository.GetByIdAsync(id);

        if (item == null)
        {
            return null;
        }

        return item.Stage;
    }*/

    public IEnumerable<ExternalContactModel> GetPostedContacts()
    {
        return contactRepository
            .GetAll()
            .Select(x => MapEntityToExternalModel(x, mapper));
    }

    public async Task<ExternalContactModel?> GetPostedContact(int id)
    {
        var item = await contactRepository.GetByIdAsync(id);

        if (item == null)
        {
            throw new BusinessException($"Contact {id} does not exist.");
        }

        return MapEntityToExternalModel(item, mapper);
    }

    public async Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model)
    {
        var entity = await contactRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            throw new BusinessException($"Contact {model.Id} does not exist.");
        }

        entity = MapModelToEntity(model, entity);

        await contactRepository.UpdateAsync(entity);

        return MapEntityToModel(entity, mapper);
    }

    /*public async Task<ContactModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        entity.Stage = newStage;

        await contactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }*/

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await contactRepository.Delete(entity);
        }
    }
}