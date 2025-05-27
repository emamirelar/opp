namespace UNOPS.PAO.Domain.Specifications.PartnerSpecifications;

using UNOPS.PAO.Domain.Entities;

/// <summary>
/// Specification that filters partners by address fields
/// </summary>
public class PartnerByAddressSpecification : BaseSpecification<Partner>
{
    /// <summary>
    /// Creates a specification that filters partners by city
    /// </summary>
    /// <param name="city">The city to filter by</param>
    public PartnerByAddressSpecification(string? city = null, string? stateProvince = null, string? postalCode = null, string? country = null)
        : base(BuildPredicate(city, stateProvince, postalCode, country))
    {
        // Include related entities
        AddInclude(p => p.PartnerOffice);
    }
    
    private static System.Linq.Expressions.Expression<Func<Partner, bool>> BuildPredicate(
        string? city, string? stateProvince, string? postalCode, string? country)
    {
        return p => 
            (string.IsNullOrWhiteSpace(city) || (p.Address1City != null && p.Address1City.ToLower().Contains(city.ToLower()))) &&
            (string.IsNullOrWhiteSpace(stateProvince) || (p.Address1StateProvince != null && p.Address1StateProvince.ToLower().Contains(stateProvince.ToLower()))) &&
            (string.IsNullOrWhiteSpace(postalCode) || (p.Address1PostalCode != null && p.Address1PostalCode.Contains(postalCode))) &&
            (string.IsNullOrWhiteSpace(country) || (p.Address1Country != null && p.Address1Country.ToLower().Contains(country.ToLower())));
    }
} 