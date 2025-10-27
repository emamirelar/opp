using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Business.Repositories.Generic;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using UNOPS.PAO.Utilities.Helpers;
using UNOPS.PAO.Models.Links;
using UNOPS.PAO.Models.Shared;

namespace UNOPS.PAO.Business.Managers;

public class LinkManager : ILinkManager
{
    private readonly IMapper mapper;
    private readonly DataRepository<Link> linkRepository;
    private readonly AppDbContext context;

    public LinkManager(IMapper mapper, AppDbContext context
    )
    {
        this.mapper = mapper;
        this.context = context;
        this.linkRepository = new DataRepository<Link>(context);
    }

    public async Task<LinkModel> CreateLinkAsync(LinkRequest model)
    {
        await ValidateEntityExists(model.Entity, model.EntityId);
        var entity = mapper.Map<Link>(model);
        entity.Name = model.Name ?? model.Url;
        await linkRepository.AddAsync(entity);
        var linkModel = mapper.Map<LinkModel>(entity);
        return linkModel;
    }

    public async Task<LinkModel?> GetLink(int id)
    {
        var item = await linkRepository.GetByIdAsync(id);
        if (item == null || item.IsDeleted)
        {
            return null;
        }

        try
        {
            await ValidateEntityExists(item.Entity, item.EntityId);
            var linkModel = mapper.Map<LinkModel>(item);
            return linkModel;
        }
        catch (ArgumentException)
        {
            await linkRepository.Delete(item);
            return null;
        }
    }

    public async Task<LinkModel?> UpdateLinkAsync(UpdateLinkRequest model)
    {
        var entity = await linkRepository.GetByIdAsync(model.Id);
        if (entity == null)
        {
            return null;
        }

        try
        {
            await ValidateEntityExists(model.Entity, model.EntityId);
            mapper.Map<UpdateLinkRequest, Link>(model, entity);
            entity.Name = model.Name ?? model.Url;
            await linkRepository.UpdateAsync(entity);
            var linkModel = mapper.Map<LinkModel>(entity);
            return linkModel;
        }
        catch (ArgumentException)
        {
            await linkRepository.Delete(entity);
            throw new ArgumentException($"{model.Entity} with id {model.EntityId} not found");
        }
    }

    public async Task DeleteLinkAsync(int id)
    {
        var entity = await linkRepository.GetByIdAsync(id);
        if (entity != null)
        {
            await linkRepository.Delete(entity);
        }
    }

    public IEnumerable<LinkModel> GetLinks()
    {
        return linkRepository
            .GetAll()
            .Select(mapper.Map<LinkModel>)
            .ToList();
    }

    public async Task<PaginationResponse<LinkModel>> GetEntityLinks(LinkEntityType entityType, int entityId, PaginationRequest parameters)
    {
        try 
        {
            await ValidateEntityExists(entityType, entityId);
        
            var query = linkRepository
                .GetAll()
                .Where(x => x.Entity == entityType && x.EntityId == entityId && !x.IsDeleted)
                .AsQueryable();

            return query.Paginate(
                x => mapper.Map<LinkModel>(x),
                parameters
            );
        }
        catch (ArgumentException)
        {
            return new PaginationResponse<LinkModel> { Records = new List<LinkModel>(), TotalCount = 0 };
        }
        catch (Exception)
        {
            return new PaginationResponse<LinkModel> { Records = new List<LinkModel>(), TotalCount = 0 };
        }
    }

    private async Task<LinkEntityType> ValidateEntityExists(LinkEntityType entityType, int entityId)
    {
        bool entityExists = entityType switch
        {
            LinkEntityType.Contact => await context.Contacts.AnyAsync(c => c.Id == entityId),
            LinkEntityType.Partner => await context.Partners.AnyAsync(p => p.Id == entityId),
            LinkEntityType.PartnerTree => await context.PartnerTrees.AnyAsync(pt => pt.Id == entityId),
            _ => throw new ArgumentException($"Unsupported entity type: {entityType}")
        };

        if (!entityExists)
        {
            throw new ArgumentException($"{entityType} with id {entityId} not found");
        }

        return entityType;
    }
} 