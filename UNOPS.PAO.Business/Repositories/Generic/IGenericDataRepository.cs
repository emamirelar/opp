namespace UNOPS.PAO.Business.Repositories.Generic;

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using UNOPS.PAO.Models;

public interface IGenericDataRepository<TEntity>
{
    Task<TResponseModel> GetById<TResponseModel>(int id,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
    Task<List<TypeaheadInput>> GetForFilterWithDescription<TProperty, TDescProperty>(
        Expression<Func<TEntity, TProperty>> labelPropertySelector,
        Expression<Func<TEntity, TDescProperty>> descriptionPropertySelector,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Expression<Func<TEntity, bool>>? selector = null);

    Task<List<TypeaheadInput>> GetForFilter<TProperty>(Expression<Func<TEntity, TProperty>> labelPropertySelector,
        Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null,
        Expression<Func<TEntity, bool>>? selector = null);

    Task<PaginationResponse<TResponseModel>> GetAllWithPagination<TEntityFilter, TResponseModel>(
        TEntityFilter? filter, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include,
        Expression<Func<TEntity, bool>>? selector = null) where TEntityFilter : PaginationRequest;
    Task<TEntity?> GetById(int id, Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>? include = null);
    Task<TResponseModel> Add<TResponseModel>(TEntity entity);
    Task<IEnumerable<TEntity>> GetAll();
    Task Add(TEntity entity);
    void Delete(TEntity entity);
    Task<TResponseModel> Update<TResponseModel>(TEntity entity);
    Task Update(TEntity entity);
}