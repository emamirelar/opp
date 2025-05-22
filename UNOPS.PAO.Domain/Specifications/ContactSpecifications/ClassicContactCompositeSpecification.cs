namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Linq;
using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;

/// <summary>
/// A composite specification that allows filtering contacts by multiple criteria
/// </summary>
public class ClassicContactCompositeSpecification : BaseSpecification<Contact>
{
    /// <summary>
    /// Creates a composite specification with multiple filter criteria for contacts
    /// </summary>
    /// <param name="id">Optional contact ID to filter by</param>
    /// <param name="partnerId">Optional partner ID to filter by</param>
    /// <param name="status">Optional status to filter by</param>
    /// <param name="salutation">Optional salutation to filter by</param>
    /// <param name="title">Optional title to filter by</param>
    /// <param name="department">Optional department to filter by</param>
    /// <param name="phone">Optional phone number to filter by</param>
    /// <param name="mobile">Optional mobile number to filter by</param>
    /// <param name="assistant">Optional assistant name to filter by</param>
    /// <param name="assistantEmail">Optional assistant email to filter by</param>
    /// <param name="assistantPhone">Optional assistant phone to filter by</param>
    /// <param name="mailingCity">Optional mailing city to filter by</param>
    /// <param name="mailingStateProvince">Optional mailing state/province to filter by</param>
    /// <param name="mailingPostalCode">Optional mailing postal code to filter by</param>
    /// <param name="mailingCountry">Optional mailing country to filter by</param>
    /// <param name="searchText">Optional text to search for in contact name, email, or description</param>
    public ClassicContactCompositeSpecification(
        int? id = null,
        int? partnerId = null,
        string? status = null,
        string? salutation = null,
        string? title = null,
        string? department = null,
        string? phone = null,
        string? mobile = null,
        string? assistant = null,
        string? assistantEmail = null,
        string? assistantPhone = null,
        string? mailingCity = null,
        string? mailingStateProvince = null,
        string? mailingPostalCode = null,
        string? mailingCountry = null,
        string? searchText = null)
        : base(BuildExpression(id, partnerId, status, salutation, title, department, phone, mobile, 
                              assistant, assistantEmail, assistantPhone, mailingCity, mailingStateProvince, 
                              mailingPostalCode, mailingCountry, searchText))
    {
        // Include the related partner
        AddInclude(c => c.Partner);
        
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
    }
    
    /// <summary>
    /// Builds the composite filter expression based on provided parameters
    /// </summary>
    private static Expression<Func<Contact, bool>> BuildExpression(
        int? id,
        int? partnerId,
        string? status,
        string? salutation,
        string? title,
        string? department,
        string? phone,
        string? mobile,
        string? assistant,
        string? assistantEmail,
        string? assistantPhone,
        string? mailingCity,
        string? mailingStateProvince,
        string? mailingPostalCode,
        string? mailingCountry,
        string? searchText)
    {
        // Start with a predicate that matches everything
        Expression<Func<Contact, bool>> predicate = c => true;
        
        // Add ID filter if specified
        if (id.HasValue)
        {
            Expression<Func<Contact, bool>> idFilter = c => c.Id == id.Value;
            predicate = CombineExpressions(predicate, idFilter);
        }
        
        // Add partner filter if specified
        if (partnerId.HasValue)
        {
            Expression<Func<Contact, bool>> partnerFilter = c => c.PartnerId == partnerId.Value;
            predicate = CombineExpressions(predicate, partnerFilter);
        }
        
        // Add status filter if specified
        if (!string.IsNullOrWhiteSpace(status))
        {
            Expression<Func<Contact, bool>> statusFilter = c => c.Status == status;
            predicate = CombineExpressions(predicate, statusFilter);
        }
        
        // Add salutation filter if specified
        if (!string.IsNullOrWhiteSpace(salutation))
        {
            Expression<Func<Contact, bool>> salutationFilter = c => c.Salutation == salutation;
            predicate = CombineExpressions(predicate, salutationFilter);
        }
        
        // Add title filter if specified
        if (!string.IsNullOrWhiteSpace(title))
        {
            Expression<Func<Contact, bool>> titleFilter = c => c.Title != null && c.Title.ToLower().Contains(title.ToLower());
            predicate = CombineExpressions(predicate, titleFilter);
        }
        
        // Add department filter if specified
        if (!string.IsNullOrWhiteSpace(department))
        {
            Expression<Func<Contact, bool>> departmentFilter = c => c.Department != null && c.Department.ToLower().Contains(department.ToLower());
            predicate = CombineExpressions(predicate, departmentFilter);
        }
        
        // Add phone filter if specified
        if (!string.IsNullOrWhiteSpace(phone))
        {
            string normalizedPhone = new string(phone.Where(char.IsDigit).ToArray());
            Expression<Func<Contact, bool>> phoneFilter = c => c.Phone != null && c.Phone.Contains(phone);
            predicate = CombineExpressions(predicate, phoneFilter);
        }
        
        // Add mobile filter if specified
        if (!string.IsNullOrWhiteSpace(mobile))
        {
            string normalizedMobile = new string(mobile.Where(char.IsDigit).ToArray());
            Expression<Func<Contact, bool>> mobileFilter = c => c.Mobile != null && c.Mobile.Contains(mobile);
            predicate = CombineExpressions(predicate, mobileFilter);
        }
        
        // Add assistant filter if specified
        if (!string.IsNullOrWhiteSpace(assistant))
        {
            Expression<Func<Contact, bool>> assistantFilter = c => c.Assistant != null && c.Assistant.ToLower().Contains(assistant.ToLower());
            predicate = CombineExpressions(predicate, assistantFilter);
        }
        
        // Add assistant email filter if specified
        if (!string.IsNullOrWhiteSpace(assistantEmail))
        {
            Expression<Func<Contact, bool>> assistantEmailFilter = c => c.AssistantEmail != null && c.AssistantEmail.ToLower().Contains(assistantEmail.ToLower());
            predicate = CombineExpressions(predicate, assistantEmailFilter);
        }
        
        // Add assistant phone filter if specified
        if (!string.IsNullOrWhiteSpace(assistantPhone))
        {
            Expression<Func<Contact, bool>> assistantPhoneFilter = c => c.AssistantPhone != null && c.AssistantPhone.Contains(assistantPhone);
            predicate = CombineExpressions(predicate, assistantPhoneFilter);
        }
        
        // Add mailing city filter if specified
        if (!string.IsNullOrWhiteSpace(mailingCity))
        {
            Expression<Func<Contact, bool>> mailingCityFilter = c => c.MailingCity != null && c.MailingCity.ToLower().Contains(mailingCity.ToLower());
            predicate = CombineExpressions(predicate, mailingCityFilter);
        }
        
        // Add mailing state/province filter if specified
        if (!string.IsNullOrWhiteSpace(mailingStateProvince))
        {
            Expression<Func<Contact, bool>> mailingStateProvinceFilter = c => c.MailingStateProvince != null && c.MailingStateProvince.ToLower().Contains(mailingStateProvince.ToLower());
            predicate = CombineExpressions(predicate, mailingStateProvinceFilter);
        }
        
        // Add mailing postal code filter if specified
        if (!string.IsNullOrWhiteSpace(mailingPostalCode))
        {
            Expression<Func<Contact, bool>> mailingPostalCodeFilter = c => c.MailingPostalCode != null && c.MailingPostalCode.Contains(mailingPostalCode);
            predicate = CombineExpressions(predicate, mailingPostalCodeFilter);
        }
        
        // Add mailing country filter if specified
        if (!string.IsNullOrWhiteSpace(mailingCountry))
        {
            Expression<Func<Contact, bool>> mailingCountryFilter = c => c.MailingCountry != null && c.MailingCountry.ToLower().Contains(mailingCountry.ToLower());
            predicate = CombineExpressions(predicate, mailingCountryFilter);
        }
        
        // Add text search filter if specified
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            // Always perform case-insensitive search
            string lowerSearchText = searchText.ToLower();
            Expression<Func<Contact, bool>> textFilter = c => 
                (c.FirstName != null && c.FirstName.ToLower().Contains(lowerSearchText)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(lowerSearchText)) ||
                (c.Email != null && c.Email.ToLower().Contains(lowerSearchText)) ||
                (c.Description != null && c.Description.ToLower().Contains(lowerSearchText));
            predicate = CombineExpressions(predicate, textFilter);
        }
        
        return predicate;
    }
    
    /// <summary>
    /// Combines two expressions with an AND operator
    /// </summary>
    private static Expression<Func<T, bool>> CombineExpressions<T>(
        Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        // If one of the expressions is a match-all expression (x => true), return the other
        if (IsMatchAllExpression(expr1))
            return expr2;
        if (IsMatchAllExpression(expr2))
            return expr1;
            
        // Create a parameter for the combined expression
        var parameter = Expression.Parameter(typeof(T), "x");
        
        // Replace the parameters in the expressions with our new parameter
        var leftVisitor = new ReplaceParameterVisitor(expr1.Parameters[0], parameter);
        var left = leftVisitor.Visit(expr1.Body);
        
        var rightVisitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        var right = rightVisitor.Visit(expr2.Body);
        
        // Combine the expressions with an AND operator
        var body = Expression.AndAlso(left, right);
        
        // Create and return the combined expression
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }
    
    /// <summary>
    /// Checks if the expression is a match-all expression (x => true)
    /// </summary>
    private static bool IsMatchAllExpression<T>(Expression<Func<T, bool>> expr)
    {
        if (expr.Body is ConstantExpression constExpr)
        {
            return constExpr.Type == typeof(bool) && (bool)constExpr.Value;
        }
        return false;
    }
    
    /// <summary>
    /// Expression visitor that replaces parameters in an expression
    /// </summary>
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