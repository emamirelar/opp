namespace UNOPS.PAO.Domain.Specifications.ContactSpecifications;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;

/// <summary>
/// A specification for advanced search on contacts using search criteria
/// </summary>
public class ContactCompositeSpecification : GenericCompositeSpecification<Contact, IContactSearchFilter>
{
    /// <summary>
    /// Creates a specification for advanced search on contacts
    /// </summary>
    /// <param name="filter">The filter containing advanced search criteria</param>
    public ContactCompositeSpecification(IContactSearchFilter filter)
        : base(filter)
    {
        // Include the related partner
        AddInclude(c => c.Partner);
        
        // Default ordering is by last name
        ApplyOrderBy(c => c.LastName);
    }
} 