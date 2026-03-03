namespace UNOPS.PAO.UNOPSBusiness.Specifications;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.UNOPSDomain.Entities;

/// <summary>
/// Adapter that allows using a UNOPSPartner specification as a Partner specification
/// </summary>
public class PartnerSpecificationAdapter : ISpecification<Partner>
{
    private readonly ISpecification<UNOPSPartner> _unosPartnerSpecification;

    public PartnerSpecificationAdapter(ISpecification<UNOPSPartner> unosPartnerSpecification)
    {
        _unosPartnerSpecification = unosPartnerSpecification ?? throw new ArgumentNullException(nameof(unosPartnerSpecification));
    }

    public Expression<Func<Partner, bool>> Criteria 
    {
        get
        {
            // Convert the UNOPSPartner criteria to Partner criteria
            var originalCriteria = _unosPartnerSpecification.Criteria;
            if (originalCriteria == null)
                return _ => true;

            // Create a parameter for Partner
            var parameter = Expression.Parameter(typeof(Partner), "p");
            
            // Convert the parameter to UNOPSPartner
            var convertedParam = Expression.Convert(parameter, typeof(UNOPSPartner));
            
            // Replace the parameter in the original expression
            var visitor = new ParameterReplacer(originalCriteria.Parameters[0], convertedParam);
            var body = visitor.Visit(originalCriteria.Body);
            
            // Create the new lambda expression
            return Expression.Lambda<Func<Partner, bool>>(body, parameter);
        }
    }

    public List<Expression<Func<Partner, object>>> Includes => 
        _unosPartnerSpecification.Includes
            .Select(ConvertInclude)
            .ToList();

    public List<string> IncludeStrings => _unosPartnerSpecification.IncludeStrings;

    public List<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>> IncludeExpressions =>
        new List<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>();

    public Expression<Func<Partner, object>>? OrderBy => 
        _unosPartnerSpecification.OrderBy != null ? ConvertOrderBy(_unosPartnerSpecification.OrderBy) : null;

    public Expression<Func<Partner, object>>? OrderByDescending => 
        _unosPartnerSpecification.OrderByDescending != null ? ConvertOrderBy(_unosPartnerSpecification.OrderByDescending) : null;

    public List<(Expression<Func<Partner, object>> KeySelector, bool Ascending)> OrderByExpressions =>
        _unosPartnerSpecification.OrderByExpressions
            .Select(expr => (ConvertOrderBy(expr.KeySelector)!, expr.Ascending))
            .ToList();

    public int Skip => _unosPartnerSpecification.Skip;
    public int Take => _unosPartnerSpecification.Take;
    public bool IsPagingEnabled => _unosPartnerSpecification.IsPagingEnabled;

    /// <summary>
    /// Gets the original UNOPS specification that was wrapped by this adapter
    /// </summary>
    public ISpecification<UNOPSPartner> GetOriginalSpecification()
    {
        return _unosPartnerSpecification;
    }

    private Expression<Func<Partner, object>> ConvertInclude(Expression<Func<UNOPSPartner, object>> include)
    {
        var parameter = Expression.Parameter(typeof(Partner), "p");
        var convertedParam = Expression.Convert(parameter, typeof(UNOPSPartner));
        var visitor = new ParameterReplacer(include.Parameters[0], convertedParam);
        var body = visitor.Visit(include.Body);
        return Expression.Lambda<Func<Partner, object>>(body, parameter);
    }

    private Expression<Func<Partner, object>> ConvertOrderBy(Expression<Func<UNOPSPartner, object>> orderBy)
    {
        var parameter = Expression.Parameter(typeof(Partner), "p");
        var convertedParam = Expression.Convert(parameter, typeof(UNOPSPartner));
        var visitor = new ParameterReplacer(orderBy.Parameters[0], convertedParam);
        var body = visitor.Visit(orderBy.Body);
        return Expression.Lambda<Func<Partner, object>>(body, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly Expression _oldParameter;
        private readonly Expression _newParameter;

        public ParameterReplacer(Expression oldParameter, Expression newParameter)
        {
            _oldParameter = oldParameter;
            _newParameter = newParameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter ? _newParameter : base.VisitParameter(node);
        }
    }
}