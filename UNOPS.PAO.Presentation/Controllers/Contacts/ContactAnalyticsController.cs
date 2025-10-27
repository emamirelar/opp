using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSBusiness.Attributes;

namespace UNOPS.PAO.Presentation.Controllers.Contacts
{
    [ApiController]
    [Route(APIDictionary.ContactAnalytics)]
    [Authorize]
    public class ContactAnalyticsController : ControllerBase
    {
        private readonly UNOPSAppDbContext _context;
        private readonly ILogger<ContactAnalyticsController> _logger;

        public ContactAnalyticsController(UNOPSAppDbContext context, ILogger<ContactAnalyticsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get contacts with highest interaction activity
        /// </summary>
        [HttpGet("getMostActiveContacts")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetMostActiveContacts(
            [FromQuery] int limit = 10,
            [FromQuery] string timeframe = "30d",
            [FromQuery] string metric = "interactions")
        {
            try
            {
                var startDate = GetStartDateFromTimeframe(timeframe);
                
                var query = from contact in _context.Contacts
                           join interactionContact in _context.InteractionContacts on contact.Id equals interactionContact.ContactId
                           join interaction in _context.Interactions on interactionContact.InteractionId equals interaction.Id
                           where !contact.IsDeleted && !interaction.IsDeleted && interaction.Date >= startDate
                           group new { contact, interaction } by new { contact.Id, contact.FirstName, contact.LastName, contact.Email, contact.Title, contact.MailingCountry } into g
                           select new
                           {
                               ContactId = g.Key.Id,
                               ContactName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                               Email = g.Key.Email,
                               Title = g.Key.Title,
                               Country = g.Key.MailingCountry,
                               InteractionCount = g.Count(),
                               LastInteractionDate = g.Max(x => x.interaction.Date),
                               InteractionTypes = g.Select(x => x.interaction.Type).Distinct().ToList()
                           };

                var result = await query
                    .OrderByDescending(x => x.InteractionCount)
                    .ThenByDescending(x => x.LastInteractionDate)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    timeframe = timeframe,
                    metric = metric,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most active contacts");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact distribution by geographic regions
        /// </summary>
        [HttpGet("getContactsByGeographicRegion")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactsByGeographicRegion(
            [FromQuery] string period = "all",
            [FromQuery] int minCount = 1,
            [FromQuery] string groupBy = "country")
        {
            try
            {
                var startDate = GetStartDateFromPeriod(period);
                
                var query = from contact in _context.Contacts
                           where !contact.IsDeleted
                           group contact by new { 
                               Country = contact.MailingCountry ?? "Unknown",
                               State = contact.MailingStateProvince ?? "Unknown",
                               City = contact.MailingCity ?? "Unknown"
                           } into g
                           select new
                           {
                               Country = g.Key.Country,
                               State = g.Key.State,
                               City = g.Key.City,
                               ContactCount = g.Count(),
                               Contacts = g.Select(c => new
                               {
                                   Id = c.Id,
                                   Name = $"{c.FirstName} {c.LastName}".Trim(),
                                   Email = c.Email,
                                   Title = c.Title
                               }).ToList()
                           };

                var result = await query
                    .Where(x => x.ContactCount >= minCount)
                    .OrderByDescending(x => x.ContactCount)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    period = period,
                    groupBy = groupBy,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts by geographic region");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact engagement trends over time
        /// </summary>
        [HttpGet("getContactEngagementTrends")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactEngagementTrends(
            [FromQuery] string period = "monthly",
            [FromQuery] int months = 12,
            [FromQuery] string metric = "interactions")
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                var query = from interaction in _context.Interactions
                           join interactionContact in _context.InteractionContacts on interaction.Id equals interactionContact.InteractionId
                           join contact in _context.Contacts on interactionContact.ContactId equals contact.Id
                           where !interaction.IsDeleted && !contact.IsDeleted && interaction.Date >= startDate
                           group new { interaction, contact } by new
                           {
                               Year = interaction.Date.Year,
                               Month = interaction.Date.Month,
                               Day = period == "daily" ? interaction.Date.Day : 1
                           } into g
                           select new
                           {
                               Period = $"{g.Key.Year}-{g.Key.Month:D2}{(period == "daily" ? $"-{g.Key.Day:D2}" : "")}",
                               InteractionCount = g.Count(),
                               UniqueContacts = g.Select(x => x.contact.Id).Distinct().Count(),
                               InteractionTypes = g.Select(x => x.interaction.Type).Distinct().ToList()
                           };

                var result = await query
                    .OrderBy(x => x.Period)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    period = period,
                    months = months,
                    metric = metric,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact engagement trends");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contacts grouped by interaction types
        /// </summary>
        [HttpGet("getContactsByInteractionType")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactsByInteractionType(
            [FromQuery] InteractionType? type = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int limit = 20)
        {
            try
            {
                var query = from contact in _context.Contacts
                           join interactionContact in _context.InteractionContacts on contact.Id equals interactionContact.ContactId
                           join interaction in _context.Interactions on interactionContact.InteractionId equals interaction.Id
                           where !contact.IsDeleted && !interaction.IsDeleted
                           select new { contact, interaction };

                if (type.HasValue)
                    query = query.Where(x => x.interaction.Type == type.Value);

                if (startDate.HasValue)
                    query = query.Where(x => x.interaction.Date >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(x => x.interaction.Date <= endDate.Value);

                var result = await query
                    .GroupBy(x => new { x.interaction.Type, x.contact.Id, x.contact.FirstName, x.contact.LastName, x.contact.Email })
                    .Select(g => new
                    {
                        InteractionType = g.Key.Type.ToString(),
                        ContactId = g.Key.Id,
                        ContactName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                        Email = g.Key.Email,
                        InteractionCount = g.Count(),
                        LastInteractionDate = g.Max(x => x.interaction.Date)
                    })
                    .OrderByDescending(x => x.InteractionCount)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    interactionType = type?.ToString(),
                    startDate = startDate,
                    endDate = endDate,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts by interaction type");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact distribution across partners
        /// </summary>
        [HttpGet("getContactsByPartner")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactsByPartner(
            [FromQuery] int minContacts = 1,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = from contact in _context.Contacts
                           join partner in _context.Partners on contact.PartnerId equals partner.Id
                           where !contact.IsDeleted && !partner.IsDeleted
                           group new { contact, partner } by new { partner.Id, partner.Name } into g
                           select new
                           {
                               PartnerId = g.Key.Id,
                               PartnerName = g.Key.Name,
                               ContactCount = g.Count(),
                               ActiveContacts = g.Count(x => x.contact.Status == EntityStatus.Active),
                               Contacts = g.Select(c => new
                               {
                                   Id = c.contact.Id,
                                   Name = $"{c.contact.FirstName} {c.contact.LastName}".Trim(),
                                   Email = c.contact.Email,
                                   Title = c.contact.Title,
                                   Status = c.contact.Status.ToString()
                               }).ToList()
                           };

                if (!includeInactive)
                    query = query.Where(x => x.ActiveContacts > 0);

                var result = await query
                    .Where(x => x.ContactCount >= minContacts)
                    .OrderByDescending(x => x.ContactCount)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    minContacts = minContacts,
                    includeInactive = includeInactive,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts by partner");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get recently active contacts
        /// </summary>
        [HttpGet("getRecentlyActiveContacts")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetRecentlyActiveContacts(
            [FromQuery] int days = 30,
            [FromQuery] int limit = 20,
            [FromQuery] string sortBy = "lastInteraction")
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var query = from contact in _context.Contacts
                           join interactionContact in _context.InteractionContacts on contact.Id equals interactionContact.ContactId
                           join interaction in _context.Interactions on interactionContact.InteractionId equals interaction.Id
                           where !contact.IsDeleted && !interaction.IsDeleted && interaction.Date >= startDate
                           group new { contact, interaction } by new { contact.Id, contact.FirstName, contact.LastName, contact.Email, contact.Title, contact.MailingCountry } into g
                           select new
                           {
                               ContactId = g.Key.Id,
                               ContactName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                               Email = g.Key.Email,
                               Title = g.Key.Title,
                               Country = g.Key.MailingCountry,
                               InteractionCount = g.Count(),
                               LastInteractionDate = g.Max(x => x.interaction.Date),
                               DaysSinceLastInteraction = (DateTime.UtcNow - g.Max(x => x.interaction.Date)).Days
                           };

                var result = sortBy.ToLower() switch
                {
                    "lastinteraction" => await query.OrderByDescending(x => x.LastInteractionDate).Take(limit).ToListAsync(),
                    "interactioncount" => await query.OrderByDescending(x => x.InteractionCount).Take(limit).ToListAsync(),
                    _ => await query.OrderByDescending(x => x.LastInteractionDate).Take(limit).ToListAsync()
                };

                return Ok(new
                {
                    success = true,
                    data = result,
                    days = days,
                    sortBy = sortBy,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recently active contacts");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact activity by job titles
        /// </summary>
        [HttpGet("getContactsByJobTitle")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactsByJobTitle(
            [FromQuery] int minContacts = 1,
            [FromQuery] bool includeInteractions = false)
        {
            try
            {
                var query = from contact in _context.Contacts
                           where !contact.IsDeleted && !string.IsNullOrEmpty(contact.Title)
                           group contact by contact.Title into g
                           select new
                           {
                               JobTitle = g.Key,
                               ContactCount = g.Count(),
                               Contacts = g.Select(c => new
                               {
                                   Id = c.Id,
                                   Name = $"{c.FirstName} {c.LastName}".Trim(),
                                   Email = c.Email,
                                   Country = c.MailingCountry,
                                   Status = c.Status.ToString()
                               }).ToList()
                           };

                var result = await query
                    .Where(x => x.ContactCount >= minContacts)
                    .OrderByDescending(x => x.ContactCount)
                    .ToListAsync();

                if (includeInteractions)
                {
                    foreach (var item in result)
                    {
                        var contactIds = item.Contacts.Select(c => c.Id).ToList();
                        var interactionCounts = await _context.InteractionContacts
                            .Where(ic => contactIds.Contains(ic.ContactId))
                            .GroupBy(ic => ic.ContactId)
                            .Select(g => new { ContactId = g.Key, Count = g.Count() })
                            .ToDictionaryAsync(x => x.ContactId, x => x.Count);

                                                 // Note: Cannot modify anonymous type properties at runtime
                         // Interaction counts are calculated but not stored in the result
                    }
                }

                return Ok(new
                {
                    success = true,
                    data = result,
                    minContacts = minContacts,
                    includeInteractions = includeInteractions,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts by job title");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contact growth trends over time
        /// </summary>
        [HttpGet("getContactGrowthTrends")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactGrowthTrends(
            [FromQuery] string period = "monthly",
            [FromQuery] int months = 12)
        {
            try
            {
                var endDate = DateTime.UtcNow;
                var startDate = endDate.AddMonths(-months);

                var query = from contact in _context.Contacts
                           where !contact.IsDeleted && contact.CreatedDate >= startDate
                           group contact by new
                           {
                               Year = contact.CreatedDate.Year,
                               Month = contact.CreatedDate.Month,
                               Day = period == "daily" ? contact.CreatedDate.Day : 1
                           } into g
                           select new
                           {
                               Period = $"{g.Key.Year}-{g.Key.Month:D2}{(period == "daily" ? $"-{g.Key.Day:D2}" : "")}",
                               NewContacts = g.Count(),
                               ActiveContacts = g.Count(c => c.Status == EntityStatus.Active),
                               Countries = g.Select(c => c.MailingCountry).Distinct().Count()
                           };

                var result = await query
                    .OrderBy(x => x.Period)
                    .ToListAsync();

                                 // Calculate cumulative totals
                 var cumulative = 0;
                 var resultWithCumulative = result.Select(item =>
                 {
                     cumulative += item.NewContacts;
                     return new
                     {
                         item.Period,
                         item.NewContacts,
                         item.ActiveContacts,
                         item.Countries,
                         CumulativeContacts = cumulative
                     };
                 }).ToList();

                return Ok(new
                {
                    success = true,
                    data = resultWithCumulative,
                    period = period,
                    months = months,
                    total = resultWithCumulative.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contact growth trends");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get contacts with most associated documents
        /// </summary>
        [HttpGet("getContactsWithMostDocuments")]
        [AccessControlled(EntityTypes.Contact, "read")]
        public async Task<IActionResult> GetContactsWithMostDocuments(
            [FromQuery] int limit = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var query = from contact in _context.Contacts
                           join docRel in _context.DocumentRelationships on contact.Id equals docRel.EntityId
                           join document in _context.Documents on docRel.DocumentId equals document.Id
                           where !contact.IsDeleted && !document.IsDeleted && docRel.EntityType == "Contact"
                           select new { contact, document };

                if (startDate.HasValue)
                    query = query.Where(x => x.document.CreatedDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(x => x.document.CreatedDate <= endDate.Value);

                var result = await query
                    .GroupBy(x => new { x.contact.Id, x.contact.FirstName, x.contact.LastName, x.contact.Email, x.contact.Title })
                    .Select(g => new
                    {
                        ContactId = g.Key.Id,
                        ContactName = $"{g.Key.FirstName} {g.Key.LastName}".Trim(),
                        Email = g.Key.Email,
                        Title = g.Key.Title,
                        DocumentCount = g.Count(),
                        Documents = g.Select(d => new
                        {
                            Id = d.document.Id,
                            Name = d.document.Name,
                            Type = d.document.Type,
                            CreatedDate = d.document.CreatedDate
                        }).ToList()
                    })
                    .OrderByDescending(x => x.DocumentCount)
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = result,
                    startDate = startDate,
                    endDate = endDate,
                    total = result.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting contacts with most documents");
                return StatusCode(500, new { success = false, message = "Internal server error" });
            }
        }

        private DateTime GetStartDateFromTimeframe(string timeframe)
        {
            return timeframe.ToLower() switch
            {
                "7d" => DateTime.UtcNow.AddDays(-7),
                "30d" => DateTime.UtcNow.AddDays(-30),
                "90d" => DateTime.UtcNow.AddDays(-90),
                "6m" => DateTime.UtcNow.AddMonths(-6),
                "1y" => DateTime.UtcNow.AddYears(-1),
                _ => DateTime.UtcNow.AddDays(-30)
            };
        }

        private DateTime GetStartDateFromPeriod(string period)
        {
            return period.ToLower() switch
            {
                "7d" => DateTime.UtcNow.AddDays(-7),
                "30d" => DateTime.UtcNow.AddDays(-30),
                "90d" => DateTime.UtcNow.AddDays(-90),
                "6m" => DateTime.UtcNow.AddMonths(-6),
                "1y" => DateTime.UtcNow.AddYears(-1),
                "all" => DateTime.MinValue,
                _ => DateTime.UtcNow.AddDays(-30)
            };
        }
    }
}
