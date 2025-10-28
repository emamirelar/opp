namespace UNOPS.PAO.Business.Repositories.Generic;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Models;
using UNOPS.PAO.Models.Filters;
using UNOPS.PAO.Models.Shared;
using UNOPS.PAO.Utilities.Helpers;
using Z.EntityFramework.Plus;

public class GenericDataRepository<TEntity> : IGenericDataRepository<TEntity> where TEntity : ModifiableEntity
{
    protected readonly AppDbContext _dataDbContext;
    protected readonly IMapper _mapper;
    protected DbSet<TEntity> _dbSet;

    public GenericDataRepository(IMapper mapper, AppDbContext context)
    {
        _mapper = mapper;
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
    }

    public GenericDataRepository(AppDbContext context)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task<TEntity?> GetById(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null)
    {
        var queryable = _dbSet.AsQueryable();
        if (include != null)
        {
            queryable = include(queryable);
        }

        return await queryable.SingleOrDefaultAsync(a => a.Id == id);
    }

    public async Task<TResponseModel> GetById<TResponseModel>(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include)
    {
        var entrity = await GetById(id, include);
        if (entrity is null)
        {
            throw new BusinessException($"Entity {id} does not exist.");
        }

        return _mapper.Map<TResponseModel>(entrity);
    }

    public IQueryable<TEntity> GetAllWithFilter<TEntityFilter>(TEntityFilter? filter = default)
    {
        var entities = _dbSet.AsQueryable();
        if (filter != null)
        {
            entities.ApplyFilters(filter);
        }

        return entities;
    }

    public async Task<List<TypeaheadInput>> GetForFilter<TProperty>(Expression<Func<TEntity, TProperty>> labelPropertySelector, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? selector = null)
    {
        var queryable = GetAllWithIncludeAndConditions(include, selector);
        Func<TEntity, TProperty> labelSelectorFunc = labelPropertySelector.Compile();

        return await queryable.Select(a => new TypeaheadInput
        {
            Value = a.Id.ToString(),
            Label = labelSelectorFunc(a) != null ? labelSelectorFunc(a).ToString() : a.Id.ToString(),
        }).ToListAsync();
    }

    public async Task<List<TypeaheadInput>> GetForFilterWithDescription<TProperty, TDescProperty>(Expression<Func<TEntity, TProperty>> labelPropertySelector, Expression<Func<TEntity, TDescProperty>> descriptionPropertySelector = null, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? selector = null)
    {
        var queryable = GetAllWithIncludeAndConditions(include, selector);
        Func<TEntity, TProperty> labelSelectorFunc = labelPropertySelector.Compile();
        Func<TEntity, TDescProperty> descSelectorFunc = descriptionPropertySelector.Compile();

        return await queryable.Select(a => new TypeaheadInput
        {
            Value = a.Id.ToString(),
            Label = labelSelectorFunc(a) != null ? labelSelectorFunc(a).ToString() : a.Id.ToString(),
            Description = descSelectorFunc(a) != null ? descSelectorFunc(a).ToString() : a.Id.ToString()
        }).ToListAsync();
    }

    public async Task<PaginationResponse<TResponseModel>> GetAllWithPagination<TEntityFilter, TResponseModel>(TEntityFilter? filter = default,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? selector = null) where TEntityFilter : PaginationRequest
    {
        var queryable = GetAllWithIncludeAndConditions(include, selector);
        if (filter != null)
        {
            queryable = queryable.ApplyFilters(filter);
        }

        return queryable.OrderByDescending(a => a.CreatedDate)
            .Paginate(o => _mapper.Map<TResponseModel>(o),
                new GenericPaginationRequest<TEntityFilter>(filter));
    }

    private IQueryable<TEntity> GetAllWithIncludeAndConditions(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null, Expression<Func<TEntity, bool>>? selector = null)
    {
        var queryable = _dbSet.AsQueryable();
        if (include != null)
        {
            queryable = include(queryable);
        }

        if (selector != null)
        {
            queryable = queryable.Where(selector);
        }

        return queryable;
    }

    public async Task<IEnumerable<TEntity>> GetAllWithFilterAndIncludes<TEntityFilter>(TEntityFilter? filter = default, params string[] includes)
    {
        var query = ApplyIncludes(_dbSet, includes);
        if (filter != null)
        {
            query.ApplyFilters(filter);
        }

        var entities = await query.ToListAsync();
        return _mapper.Map<IEnumerable<TEntity>>(entities);
    }

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> query, params string[] includes)
    {
        return includes.Aggregate(query, (current, include) => current.Include(include));
    }

    public async Task<IEnumerable<TEntity>> GetAll()
    {
        return await _dataDbContext.Set<TEntity>().ToListAsync();
    }

    public async Task<TResponseModel> Add<TResponseModel>(TEntity entityToInsert)
    {
        await Add(entityToInsert);
        return _mapper.Map<TResponseModel>(entityToInsert);
    }

    public async Task Add(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public void Delete(TEntity entity)
    {
        _dataDbContext.Set<TEntity>().Remove(entity);
    }

    public async Task<TResponseModel> Update<TResponseModel>(TEntity entityToUpdate)
    {
        await Update(entityToUpdate);
        return _mapper.Map<TResponseModel>(entityToUpdate);
    }

    public async Task Update(TEntity entity)
    {
        await _dataDbContext.SingleUpdateAsync<TEntity>(entity.Id);
        await _dataDbContext.SaveChangesAsync();
    }

    public async Task<PaginationResponse<TResponseModel>> GetBySpecification<TResponseModel>(
        ISpecification<TEntity> specification)
    {
        var query = _dbSet.AsQueryable();
        var evaluatedQuery = query.ApplySpecification(specification);
        
        int totalCount = await evaluatedQuery.CountAsync();
        var items = await evaluatedQuery.ToListAsync();
        
        return new PaginationResponse<TResponseModel>
        {
            TotalCount = totalCount,
            Records = items.Select(item => _mapper.Map<TResponseModel>(item)).ToList()
        };
    }

    public async Task<TEntity?> GetSingleBySpecification(ISpecification<TEntity> specification)
    {
        var query = _dbSet.AsQueryable();
        var evaluatedQuery = query.ApplySpecification(specification);
        
        return await evaluatedQuery.FirstOrDefaultAsync();
    }
}