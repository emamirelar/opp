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

public class BaseRepository<TEntity>  where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly UNOPSAppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;
    protected readonly IConfiguration _configuration;
    protected readonly PubSubPublisher _pubSubPublisher;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
    }

    public BaseRepository(UNOPSAppDbContext context, IConfiguration configuration)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
        _configuration = configuration;
        _pubSubPublisher = new PubSubPublisher(configuration.GetSection("PubSub")["ProjectId"], configuration.GetSection("PubSub")["TopicId"]);
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

    private static string ToConcatenatedString(object model)
    {
        if (model == null) return string.Empty;

        var properties = (model).GetType().GetProperties();
        string result = "";

        foreach (var property in properties)
        {
            if (property.Name == "Id" && (int)property.GetValue(model, null) == 0)
            {
                continue;
            }
            var value = property.GetValue(model, null);
            result += $"{property.Name}: {value}, ";
        }

        // Remove trailing comma and space
        return result.TrimEnd(',', ' ');
    }

    public async Task PublishMessageToPubSub(TEntity entity)
    {
        string result = ToConcatenatedString(entity);
        var entityName = typeof(TEntity).Name.Replace("UNOPS", "").Pluralize();
        var entityId = entity.Id;
        var idProperty = (entity).GetType().GetProperties()
                        .Where(property => (property.Name == "Id" && (int)property.GetValue(entity, null) != 0)).ToList()[0];
        if (idProperty != null)
        {
            entityId = (int)idProperty.GetValue(entity, null);
        }

        // Creating the PubSub message
        var message = new MyPubSubMessage
        {
            EntityName = entityName,
            EntityId = entityId,
            Content = result // Content as concatenated string of entity properties
        };

        // Publishing the message
        await _pubSubPublisher.PublishMessageAsync(new List<MyPubSubMessage> { message });
        //await Task.Delay(1000); // Wait some time before publishing the next message

    }
}