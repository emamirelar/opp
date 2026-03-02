using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace UNOPS.PAO.Business.Repositories.Generic;

public class DataRepository<TEntity> where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly AppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
    }

    /// <summary>
    /// Gets the Id property safely, handling ambiguous matches in inheritance hierarchies
    /// </summary>
    private PropertyInfo? GetIdProperty(Type entityType)
    {
        try
        {
            // First try without DeclaredOnly to include inherited properties
            var idProperty = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            if (idProperty != null)
            {
                System.Diagnostics.Debug.WriteLine($"DataRepository: Found Id property for {entityType.Name} of type {idProperty.PropertyType.Name} declared in {idProperty.DeclaringType.Name}");
            }
            return idProperty;
        }
        catch (AmbiguousMatchException)
        {
            System.Diagnostics.Debug.WriteLine($"DataRepository: Ambiguous Id property found for {entityType.Name}, resolving...");
            
            // If ambiguous, get all properties named "Id" and pick the most specific int one
            var idProperties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name == "Id" && p.PropertyType == typeof(int))
                .ToArray();
            
            if (idProperties.Length > 0)
            {
                // Prefer properties declared in the current type over inherited ones
                var declaredProperty = idProperties.FirstOrDefault(p => p.DeclaringType == entityType);
                if (declaredProperty != null)
                {
                    System.Diagnostics.Debug.WriteLine($"DataRepository: Using declared Id property from {declaredProperty.DeclaringType.Name}");
                    return declaredProperty;
                }
                
                // Otherwise, use the first one found
                System.Diagnostics.Debug.WriteLine($"DataRepository: Using first Id property from {idProperties[0].DeclaringType.Name}");
                return idProperties[0];
            }
            
            System.Diagnostics.Debug.WriteLine($"DataRepository: No int Id property found for {entityType.Name}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"DataRepository: Error getting Id property for {entityType.Name}: {ex.Message}");
            return null;
        }
    }

    public DataRepository(AppDbContext context)
    {
        _dataDbContext = context;
        _dbSet = context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity)
    {
        await _dbSet.AddAsync(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public IQueryable<TEntity> GetAll(string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        
        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var isDeletedProp = Expression.Property(parameter, "IsDeleted");
            var notDeleted = Expression.Not(isDeletedProp);
            var isDeletedLambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, parameter);
            set = set.Where(isDeletedLambda);
        }
        
        return set.AsQueryable();
    }

    public IQueryable<TEntity> GetAll() => GetAll(Array.Empty<string>());

    public async Task<TEntity?> GetByIdAsync(int id, string[] includes)
    {
        var set = ApplyIncludes(_dbSet, includes);
        
        // Apply soft delete filtering if the entity supports it
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var idProperty = GetIdProperty(typeof(TEntity));
            if (idProperty != null)
            {
                var idAccess = Expression.Property(parameter, idProperty);
                var idEquals = Expression.Equal(idAccess, Expression.Constant(id));
                var isDeletedProp = Expression.Property(parameter, "IsDeleted");
                var notDeleted = Expression.Not(isDeletedProp);
                var combined = Expression.AndAlso(idEquals, notDeleted);
                var lambda = Expression.Lambda<Func<TEntity, bool>>(combined, parameter);
                return await set.SingleOrDefaultAsync(lambda);
            }
        }
        
        return await set.SingleOrDefaultAsync(x => x.Id == id);
    }

    public async Task<TEntity?> GetByIdAsync(int id) => await GetByIdAsync(id, Array.Empty<string>());

    public async Task UpdateAsync(TEntity entity)
    {
        await _dataDbContext.SingleUpdateAsync(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public async Task Delete(TEntity entity)
    {
        _dataDbContext.Remove(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TEntity>> GetAllSortedAsync(string sortBy, bool ascending = true)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.Property(parameter, sortBy);
        var lambda = Expression.Lambda<Func<TEntity, object>>(Expression.Convert(property, typeof(object)), parameter);

        IQueryable<TEntity> query = _dbSet;

        // Add IsDeleted filtering if the property exists
        var isDeletedProperty = typeof(TEntity).GetProperty("IsDeleted");
        if (isDeletedProperty != null && isDeletedProperty.PropertyType == typeof(bool))
        {
            var isDeletedParam = Expression.Parameter(typeof(TEntity), "x");
            var isDeletedProp = Expression.Property(isDeletedParam, "IsDeleted");
            var notDeleted = Expression.Not(isDeletedProp);
            var isDeletedLambda = Expression.Lambda<Func<TEntity, bool>>(notDeleted, isDeletedParam);
            query = query.Where(isDeletedLambda);
        }

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
}