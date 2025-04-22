namespace UNOPS.PAO.Domain.Specifications.InteractionSpecifications;

using System.Linq.Expressions;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;

/// <summary>
/// Specification that filters interactions by date range
/// </summary>
public class InteractionByDateRangeSpecification : BaseSpecification<Interaction>
{
    /// <summary>
    /// Creates a specification that filters interactions by a date range
    /// </summary>
    /// <param name="fromDate">Starting date (inclusive)</param>
    /// <param name="toDate">Ending date (inclusive)</param>
    public InteractionByDateRangeSpecification(DateTime fromDate, DateTime toDate)
        : base(i => i.Date >= fromDate && i.Date <= toDate)
    {
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Creates a specification that filters interactions after a specific date
    /// </summary>
    /// <param name="fromDate">Starting date (inclusive)</param>
    public InteractionByDateRangeSpecification(DateTime fromDate)
        : base(i => i.Date >= fromDate)
    {
        // Default ordering is by date descending
        ApplyOrderByDescending(i => i.Date);
    }
    
    /// <summary>
    /// Creates a specification that filters interactions for recent days
    /// </summary>
    /// <param name="days">Number of days to look back</param>
    public static InteractionByDateRangeSpecification LastDays(int days)
    {
        var fromDate = DateTime.UtcNow.AddDays(-days);
        return new InteractionByDateRangeSpecification(fromDate);
    }
} 