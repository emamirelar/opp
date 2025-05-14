namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// A composite specification that allows filtering partners by multiple criteria
/// </summary>
public class PartnerCompositeClassicSearchSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for partners
    /// </summary>
    /// <param name="id">Optional partner ID to filter by</param>
    /// <param name="name">Optional partner name to filter by</param>
    /// <param name="status">Optional status to filter by</param>
    /// <param name="newEngagement">Optional new engagement status to filter by</param>
    /// <param name="phone">Optional phone number to filter by</param>
    /// <param name="website">Optional website to filter by</param>
    /// <param name="shortName">Optional short name to filter by</param>
    /// <param name="partnerOfficeId">Optional partner office ID to filter by</param>
    /// <param name="partnerCategoryId">Optional partner category ID to filter by</param>
    /// <param name="addressCity">Optional city to filter by</param>
    /// <param name="addressStateProvince">Optional state/province to filter by</param>
    /// <param name="addressPostalCode">Optional postal code to filter by</param>
    /// <param name="addressCountry">Optional country to filter by</param>
    /// <param name="searchText">Optional text to search for in partner name, short name or phone</param>
    public PartnerCompositeClassicSearchSpecification(
        int? id = null,
        string? name = null,
        string? status = null,
        string? newEngagement = null,
        string? phone = null,
        string? website = null,
        string? shortName = null,
        int? partnerOfficeId = null,
        int? partnerCategoryId = null,
        string? addressCity = null,
        string? addressStateProvince = null,
        string? addressPostalCode = null,
        string? addressCountry = null,
        string? searchText = null)
        : base(BuildExpression(id, name, status, newEngagement, phone, website, shortName, 
                              partnerOfficeId, partnerCategoryId, addressCity, addressStateProvince, 
                              addressPostalCode, addressCountry, searchText))
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
        AddInclude(p => p.PartnerCategory);
        
        // Default ordering is by name
        ApplyOrderBy(p => p.Name);
    }
    
    /// <summary>
    /// Builds the composite filter expression based on provided parameters
    /// </summary>
    private static Expression<Func<Partner, bool>> BuildExpression(
        int? id,
        string? name,
        string? status,
        string? newEngagement,
        string? phone,
        string? website,
        string? shortName,
        int? partnerOfficeId,
        int? partnerCategoryId,
        string? addressCity,
        string? addressStateProvince,
        string? addressPostalCode,
        string? addressCountry,
        string? searchText)
    {
        // Start with a predicate that matches everything
        Expression<Func<Partner, bool>> predicate = p => true;
        
        // Add ID filter if specified
        if (id.HasValue)
        {
            Expression<Func<Partner, bool>> idFilter = p => p.Id == id.Value;
            predicate = CombineExpressions(predicate, idFilter);
        }
        
        // Add name filter if specified
        if (!string.IsNullOrWhiteSpace(name))
        {
            Expression<Func<Partner, bool>> nameFilter = p => p.Name.ToLower().Contains(name.ToLower());
            predicate = CombineExpressions(predicate, nameFilter);
        }
        
        // Add status filter if specified
        if (!string.IsNullOrWhiteSpace(status))
        {
            Expression<Func<Partner, bool>> statusFilter = p => p.Status == status;
            predicate = CombineExpressions(predicate, statusFilter);
        }
        
        // Add new engagement filter if specified
        if (!string.IsNullOrWhiteSpace(newEngagement))
        {
            Expression<Func<Partner, bool>> newEngagementFilter = p => p.NewEngagement == newEngagement;
            predicate = CombineExpressions(predicate, newEngagementFilter);
        }
        
        // Add phone filter if specified
        if (!string.IsNullOrWhiteSpace(phone))
        {
            Expression<Func<Partner, bool>> phoneFilter = p => p.Phone != null && p.Phone.Contains(phone);
            predicate = CombineExpressions(predicate, phoneFilter);
        }
        
        // Add website filter if specified
        if (!string.IsNullOrWhiteSpace(website))
        {
            Expression<Func<Partner, bool>> websiteFilter = p => p.Website != null && p.Website.ToLower().Contains(website.ToLower());
            predicate = CombineExpressions(predicate, websiteFilter);
        }
        
        // Add short name filter if specified
        if (!string.IsNullOrWhiteSpace(shortName))
        {
            Expression<Func<Partner, bool>> shortNameFilter = p => p.ShortName.ToLower().Contains(shortName.ToLower());
            predicate = CombineExpressions(predicate, shortNameFilter);
        }
        
        // Add partner office filter if specified
        if (partnerOfficeId.HasValue)
        {
            Expression<Func<Partner, bool>> partnerOfficeFilter = p => p.PartnerOfficeId == partnerOfficeId.Value;
            predicate = CombineExpressions(predicate, partnerOfficeFilter);
        }
        
        // Add partner category filter if specified
        if (partnerCategoryId.HasValue)
        {
            Expression<Func<Partner, bool>> partnerCategoryFilter = p => p.PartnerCategoryId == partnerCategoryId.Value;
            predicate = CombineExpressions(predicate, partnerCategoryFilter);
        }
        
        // Add address city filter if specified
        if (!string.IsNullOrWhiteSpace(addressCity))
        {
            Expression<Func<Partner, bool>> cityFilter = p => p.Address1City != null && p.Address1City.ToLower().Contains(addressCity.ToLower());
            predicate = CombineExpressions(predicate, cityFilter);
        }
        
        // Add address state/province filter if specified
        if (!string.IsNullOrWhiteSpace(addressStateProvince))
        {
            Expression<Func<Partner, bool>> stateProvinceFilter = p => p.Address1StateProvince != null && p.Address1StateProvince.ToLower().Contains(addressStateProvince.ToLower());
            predicate = CombineExpressions(predicate, stateProvinceFilter);
        }
        
        // Add address postal code filter if specified
        if (!string.IsNullOrWhiteSpace(addressPostalCode))
        {
            Expression<Func<Partner, bool>> postalCodeFilter = p => p.Address1PostalCode != null && p.Address1PostalCode.Contains(addressPostalCode);
            predicate = CombineExpressions(predicate, postalCodeFilter);
        }
        
        // Add address country filter if specified
        if (!string.IsNullOrWhiteSpace(addressCountry))
        {
            Expression<Func<Partner, bool>> countryFilter = p => p.Address1Country != null && p.Address1Country.ToLower().Contains(addressCountry.ToLower());
            predicate = CombineExpressions(predicate, countryFilter);
        }
        
        // Add text search filter if specified
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Always perform case-insensitive search
            string lowerSearchText = searchText.ToLower();
            Expression<Func<Partner, bool>> textFilter = p => 
                (p.Name != null && p.Name.ToLower().Contains(lowerSearchText)) ||
                (p.ShortName != null && p.ShortName.ToLower().Contains(lowerSearchText)) ||
                (p.Phone != null && p.Phone.Contains(lowerSearchText));
            predicate = CombineExpressions(predicate, textFilter);
        }
        
        return predicate;
    }
    
    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // If the first expression is just "x => true", return the second expression
        if (IsMatchAllExpression(expr1))
        {
            return expr2;
        }
        
        // Create a parameter for the combined expression
        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Replace the parameter in both expressions with our new parameter
        var visitor1 = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
        var visitor2 = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        
        var body1 = visitor1.Visit(expr1.Body);
        var body2 = visitor2.Visit(expr2.Body);
        
        // Combine the two expression bodies with AND
        var combinedBody = Expression.AndAlso(body1, body2);
        
        // Create a new lambda expression with the combined body
        return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
    }
    
    private static bool IsMatchAllExpression<T>(Expression<Func<T, bool>> expr)
    {
        if (expr.Body is ConstantExpression constantExpr)
        {
            return constantExpr.Type == typeof(bool) && (bool)constantExpr.Value;
        }
        
        return false;
    }
    
    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter;
        private readonly ParameterExpression _newParameter;
        
        public ReplaceParameterVisitor(ParameterExpression oldParameter, ParameterExpression newParameter)
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