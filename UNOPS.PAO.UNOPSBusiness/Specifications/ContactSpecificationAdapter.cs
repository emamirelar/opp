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
/// Adapter that allows using a UNOPSContact specification as a Contact specification
/// </summary>
public class ContactSpecificationAdapter : ISpecification<Contact>
{
    private readonly ISpecification<UNOPSContact> _unosContactSpecification;

    public ContactSpecificationAdapter(ISpecification<UNOPSContact> unosContactSpecification)
    {
        _unosContactSpecification = unosContactSpecification ?? throw new ArgumentNullException(nameof(unosContactSpecification));
    }

    public ISpecification<UNOPSContact> GetOriginalSpecification() => _unosContactSpecification;

    public Expression<Func<Contact, bool>> Criteria 
    {
        get
        {
            // Convert the UNOPSContact criteria to Contact criteria
            var originalCriteria = _unosContactSpecification.Criteria;
            if (originalCriteria == null)
                return _ => true;

            // Since UNOPSContact inherits from Contact, we need to ensure the cast works with EF
            // Create a parameter for Contact
            var parameter = Expression.Parameter(typeof(Contact), "c");
            
            // Instead of Expression.Convert, use Expression.TypeAs for EF compatibility
            var convertedParam = Expression.TypeAs(parameter, typeof(UNOPSContact));
            
            // Replace the parameter in the original expression
            var visitor = new ParameterReplacer(originalCriteria.Parameters[0], convertedParam);
            var body = visitor.Visit(originalCriteria.Body);
            
            // Create the new lambda expression
            return Expression.Lambda<Func<Contact, bool>>(body, parameter);
        }
    }

    public List<Expression<Func<Contact, object>>> Includes => 
        _unosContactSpecification.Includes
            .Select(ConvertInclude)
            .ToList();

    public List<string> IncludeStrings => _unosContactSpecification.IncludeStrings;

    public List<Func<IQueryable<Contact>, IIncludableQueryable<Contact, object>>> IncludeExpressions =>
        new List<Func<IQueryable<Contact>, IIncludableQueryable<Contact, object>>>();

    public Expression<Func<Contact, object>>? OrderBy => 
        _unosContactSpecification.OrderBy != null ? ConvertOrderBy(_unosContactSpecification.OrderBy) : null;

    public Expression<Func<Contact, object>>? OrderByDescending => 
        _unosContactSpecification.OrderByDescending != null ? ConvertOrderBy(_unosContactSpecification.OrderByDescending) : null;

    public List<(Expression<Func<Contact, object>> KeySelector, bool Ascending)> OrderByExpressions =>
        _unosContactSpecification.OrderByExpressions
            .Select(expr => (ConvertOrderBy(expr.KeySelector)!, expr.Ascending))
            .ToList();

    public int Skip => _unosContactSpecification.Skip;
    public int Take => _unosContactSpecification.Take;
    public bool IsPagingEnabled => _unosContactSpecification.IsPagingEnabled;

    private Expression<Func<Contact, object>> ConvertInclude(Expression<Func<UNOPSContact, object>> include)
    {
        var parameter = Expression.Parameter(typeof(Contact), "c");
        var convertedParam = Expression.Convert(parameter, typeof(UNOPSContact));
        var visitor = new ParameterReplacer(include.Parameters[0], convertedParam);
        var body = visitor.Visit(include.Body);
        return Expression.Lambda<Func<Contact, object>>(body, parameter);
    }

    private Expression<Func<Contact, object>> ConvertOrderBy(Expression<Func<UNOPSContact, object>> orderBy)
    {
        var parameter = Expression.Parameter(typeof(Contact), "c");
        var convertedParam = Expression.Convert(parameter, typeof(UNOPSContact));
        var visitor = new ParameterReplacer(orderBy.Parameters[0], convertedParam);
        var body = visitor.Visit(orderBy.Body);
        return Expression.Lambda<Func<Contact, object>>(body, parameter);
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