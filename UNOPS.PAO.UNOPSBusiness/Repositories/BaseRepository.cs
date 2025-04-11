using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.UNOPSBusiness.Repositories;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Infrastructure;
using UNOPS.PAO.Models;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Linq.Expressions;
using UNOPS.PAO.UNOPSBusiness.Models;
using UNOPS.PAO.UNOPSBusiness.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Humanizer;
using Newtonsoft.Json;

public class BaseRepository<TEntity>  where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly UNOPSAppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;
    protected readonly IConfiguration _configuration;

    protected readonly AiContextualService _aiService;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
    }

    public BaseRepository(UNOPSAppDbContext context, IConfiguration configuration)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
        _configuration = configuration;
        _aiService = new AiContextualService(configuration, context, null);
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }

    public IEnumerable<TEntity> GetAll(string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);

        return set.AsEnumerable();
    }

    public IEnumerable<TEntity> GetAll() => GetAll(Array.Empty<string>());

    public async Task<TEntity?> GetByIdAsync(int id, string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        return await set.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TEntity?> GetByIdAsync(int id) => await GetByIdAsync(id, Array.Empty<string>());


    public async Task UpdateAsync(TEntity entity)
    {
        await _dataDbContext.SingleUpdateAsync<TEntity>(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }

    public async Task Delete(TEntity entity)
    {
        _dataDbContext.Remove(entity);
        await _dataDbContext.SaveChangesAsync();

        await PublishMessageToPubSub(entity);
    }
    
    public async Task<IEnumerable<TEntity>> GetAllSortedAsync(string sortBy, bool ascending = true)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), parameter);

        IQueryable<TEntity> query = _dbSet;

        if (ascending)
        {
            query = query.OrderBy(lambda);
        }
        else
        {
            query = query.OrderByDescending(lambda);
        }

        return await query.ToListAsync();
    }

    public async Task PublishMessageToPubSub(TEntity entity)
    {
        var entityName = typeof(TEntity).Name.Replace("UNOPS", "").Pluralize();
        _aiService.PublishMessageToPubSub(entity, entityName, entity.Id);
    }
}