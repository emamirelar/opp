using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Utilities.Helpers;

namespace UNOPS.PAO.Business.Repositories.Generic;

public class DataRepository<TEntity> where TEntity : class, IBaseBusinessEntity<int>
{
    protected readonly AppDbContext _dataDbContext;
    protected DbSet<TEntity> _dbSet;

    private IQueryable<TEntity> ApplyIncludes(IQueryable<TEntity> set, string[] includes)
    {
        return includes.Aggregate(set, (current, include) => current.Include(include));
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
        await _dataDbContext.SingleUpdateAsync(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public async Task Delete(TEntity entity)
    {
        _dataDbContext.Remove(entity);
        await _dataDbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Document>> GetDocumentsForEntityAsync(int entityId, DocumentParentEntityType entityType)
    {
        return await _dataDbContext.DocumentRelationships
            .Where(dr => dr.EntityId == entityId && 
                         dr.EntityType == entityType.GetEntityTypeName())
            .Select(dr => dr.Document)
            .ToListAsync();
    }
}