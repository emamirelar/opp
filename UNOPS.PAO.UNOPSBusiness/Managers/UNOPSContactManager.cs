namespace UNOPS.PAO.UNOPSBusiness.Managers;

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
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
using UNOPS.PAO.Utilities.Helpers;

public class UNOPSContactManager : IContactManager
{
    private IMapper mapper;
    private BaseRepository<UNOPSContact> contactRepository;
    private BaseRepository<UNOPSPartner> partnerRepository;

    private CommonEntityRepository commonRepository;

    //private string[] includes = ["Currency", "Documents"];

    private ContactModel MapEntityToModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ContactModel>(entity);
        result.Partner = mapper.Map<Partner, PartnerModel>(entity.Partner);
        return result;
    }
    private ExternalContactModel MapEntityToExternalModel(UNOPSContact entity, IMapper mapper)
    {
        var result = mapper.Map<UNOPSContact, ExternalContactModel>(entity);
        return result;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model, UNOPSContact entity)
    {
        mapper.Map(model, entity);

        entity.Name = String.Concat(model.Salutation, ' ', model.FirstName, ' ', model.MiddleName, ' ', model.LastName);
        return entity;
    }

    private UNOPSContact MapModelToEntity(ContactRequest model)
    {
        return MapModelToEntity(model, new UNOPSContact());
    }

    public UNOPSContactManager(IMapper mapper, UNOPSAppDbContext context, IConfiguration configuration)
    {
        this.mapper = mapper;
        contactRepository = new BaseRepository<UNOPSContact>(context, configuration);
        partnerRepository = new BaseRepository<UNOPSPartner>(context, configuration);
        commonRepository = new CommonEntityRepository(context);
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = MapModelToEntity(model);
        
        await contactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    public PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request)
    {
        var query = contactRepository
            .GetAll(["Partner"])
            .AsQueryable();

        return query.Paginate(
            x => MapEntityToModel(x, mapper),
            request
        );
    }

    public async Task<ContactModel?> GetContact(int userId, int id)
    {
        var item = await contactRepository.GetByIdAsync(id);
        var partner = await partnerRepository.GetByIdAsync(item.PartnerId);
        if (item == null)
        {
            return default;
        }
        item.Partner = partner;
        return MapEntityToModel(item, mapper);
    }

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

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await contactRepository.GetByIdAsync(id);

        if (entity != null)
        {
            await contactRepository.Delete(entity);
        }
    }

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        return contactRepository
            .GetAll(["Partner"])
            .Where(x => x.PartnerId == partnerId)
            .Select(x => new ContactModel()
            {
                Id = x.Id,
                PartnerId = x.Partner.Id,
                PartnerName = x.Partner.Name,
                Salutation = x.Salutation,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                Mobile = x.Mobile
            });
    }
    public async Task<ContactModel?> GetContactAsync(int id)
    {
        string[] includes = ["Documents"];

        var item = await contactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<ContactModel>(item);

        //result.ApplicationType = applicationTypeManager.GetApplicationTypeByCode(item.ApplicationTypeCode);

        return result;
    }
}