namespace UNOPS.PAO.Business.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Models;
using UNOPS.PAO.Utilities.Helpers;

public class ContactManager : IContactManager
{
    private IMapper mapper;

    private DataRepository<Contact> ContactRepository;

    public ContactManager(IMapper mapper, AppDbContext context)
    {
        this.mapper = mapper;
        this.ContactRepository = new DataRepository<Contact>(context);
    }

    public async Task<ContactModel> CreateContactAsync(ContactRequest model)
    {
        var entity = mapper.Map<Contact>(model);

        await ContactRepository.AddAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    public PaginationResponse<ContactModel> GetContacts(int userId, PaginationRequest request)
    {
        var query = ContactRepository
            .GetAll()
            .AsQueryable();

        return query.Paginate(
            x => mapper.Map<ContactModel>(x),
            request
        );
    }

    public async Task<ContactModel?> GetContact(int userId, int id)
    {
        var item = await ContactRepository.GetByIdAsync(id);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ContactModel>(item);
    }

    public IEnumerable<ExternalContactModel> GetPostedContacts()
    {
        return ContactRepository
            .GetAll()
            .Select(mapper.Map<ExternalContactModel>);
    }

    public async Task<ExternalContactModel?> GetPostedContact(int id)
    {
        var item = await ContactRepository.GetByIdAsync(id, ["EligibleEntities"]);

        if (item == null)
        {
            return default;
        }

        return mapper.Map<ExternalContactModel>(item);
    }

    public async Task<ContactModel?> UpdateContactAsync(int userId, UpdateContactRequest model)
    {
        var entity = await ContactRepository.GetByIdAsync(model.Id);

        if (entity == null)
        {
            return default;
        }

        mapper.Map<UpdateContactRequest, Contact>(model, entity);

        await ContactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }

    /*public async Task<ContactModel?> UpdateStage(int userId, int id, string newStage)
    {
        var entity = await ContactRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return default;
        }

        entity.Stage = newStage;

        if (newStage == "Open")
        {
            entity.PostingDate = DateTime.Now.ToUniversalTime();
        }

        await ContactRepository.UpdateAsync(entity);

        return mapper.Map<ContactModel>(entity);
    }*/

    public async Task DeleteContactAsync(int userId, int id)
    {
        var entity = await ContactRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await ContactRepository.Delete(entity);
        }
    }

    public IEnumerable<ContactModel> GetPartnerContacts(int partnerId)
    {
        // TODO: get stage from workflow?
        return ContactRepository
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

        var item = await ContactRepository.GetByIdAsync(id, includes);

        if (item == null)
        {
            return default;
        }

        var result = mapper.Map<ContactModel>(item);

        //result.ApplicationType = applicationTypeManager.GetApplicationTypeByCode(item.ApplicationTypeCode);

        return result;
    }
}