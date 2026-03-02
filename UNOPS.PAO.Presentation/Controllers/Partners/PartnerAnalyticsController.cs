using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UNOPS.PAO.Business.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.Models;
using UNOPS.PAO.Presentation.Helpers;
using UNOPS.PAO.Presentation.Security;
using UNOPS.PAO.UNOPSBusiness.Attributes;
using UNOPS.PAO.UNOPSBusiness.Managers;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Presentation.Controllers.Shared;

namespace UNOPS.PAO.Presentation.Controllers.Partners
{
    [Route("/")]
    [Authorize(AuthenticationSchemes = "IAP")]
    public class PartnerAnalyticsController : BaseController
    {
        private readonly IPartnerManager _manager;
        private readonly UNOPSAppDbContext _context;
        private new readonly ILogger<PartnerAnalyticsController> _logger;

        public PartnerAnalyticsController(
            IManagerWrapper manager,
            UNOPSAppDbContext context,
            UserResolverService<int> userResolverService,
            IAuthorizationService authorizationService,
            ILogger<PartnerAnalyticsController> logger)
            : base(logger, authorizationService, userResolverService)
        {
            _manager = manager.PartnerManager;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves the most active partners based on engagement count, interaction count, or recent activity.
        /// </summary>
        /// <param name="limit">Maximum number of partners to return (default: 10)</param>
        /// <param name="timeframe">Time period to analyze (daily, weekly, monthly, quarterly, yearly)</param>
        /// <param name="metric">Activity metric to measure (engagements, interactions, lastActivity)</param>
        /// <example_uses>
        /// Show most active partners this month
        /// Get top 5 partners by engagement count
        /// List partners with most interactions
        /// Find most recently active partners
        /// Show partners with highest engagement in the last quarter
        /// </example_uses>
        /// <when_to_use>Use this when you need to identify the most active or engaged partners based on various metrics.</when_to_use>
        /// <returns>List of most active partners with activity metrics</returns>
        [HttpGet(APIDictionary.Partner + "/analytics/mostActive")]
        [AccessControlled(EntityTypes.Partner, "read")]
        public async Task<ActionResult<List<object>>> GetMostActivePartners(
            [FromQuery] int limit = 10,
            [FromQuery] string timeframe = "monthly",
            [FromQuery] string metric = "engagements")
        {
            try
            {
                // Validate parameters
                if (limit <= 0 || limit > 100)
                {
                    return BadRequest(new { error = "Limit must be between 1 and 100" });
                }

                // Parse timeframe
                DateTime cutoffDate = GetCutoffDateFromTimeframe(timeframe);

                // Get partners with their activity metrics based on the specified metric
                var result = await GetPartnersWithActivityMetrics(cutoffDate, metric, limit);

                return Ok(new
                {
                    metadata = new
                    {
                        timeframe,
                        metric,
                        generatedAt = DateTime.UtcNow,
                        description = $"Most active partners by {metric} ({timeframe})"
                    },
                    partners = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving most active partners");
                return StatusCode(500, new { error = "An error occurred while retrieving partner activity data" });
            }
        }

        /// <summary>
        /// Retrieves partners created or managed by a specific user with activity metrics.
        /// </summary>
        /// <param name="userId">User ID to get partners for</param>
        /// <param name="timeframe">Time period to analyze (daily, weekly, monthly, quarterly, yearly)</param>
        /// <param name="includeCreated">Include partners created by the user (default: true)</param>
        /// <param name="includeModified">Include partners modified by the user (default: true)</param>
        /// <param name="includeFocalPoint">Include partners where user is focal point (default: true)</param>
        /// <example_uses>
        /// Show all partners created by user 123
        /// Get partners managed by specific user
        /// List partners modified by user 456 this month
        /// Find partners where user 789 is the focal point
        /// Show all partner activity for a specific user
        /// </example_uses>
        /// <when_to_use>Use this when you need to see all partners associated with a specific user's activities.</when_to_use>
        /// <returns>List of partners associated with the specified user with activity metrics</returns>
        [HttpGet(APIDictionary.Partner + "/analytics/byUser/{userId}")]
        [AccessControlled(EntityTypes.Partner, "read")]
        public async Task<ActionResult<object>> GetPartnersByUser(
            int userId,
            [FromQuery] string timeframe = "monthly",
            [FromQuery] bool includeCreated = true,
            [FromQuery] bool includeModified = true,
            [FromQuery] bool includeFocalPoint = true)
        {
            try
            {
                // Parse timeframe
                DateTime cutoffDate = GetCutoffDateFromTimeframe(timeframe);

                // Build query based on parameters
                var partnersQuery = _context.Partners
                    .Where(p => !p.IsDeleted)
                    .AsQueryable();

                if (includeCreated)
                {
                    partnersQuery = partnersQuery.Where(p => p.CreatedBy == userId && p.CreatedDate >= cutoffDate);
                }

                if (includeModified)
                {
                    partnersQuery = partnersQuery.Where(p => p.LastModifiedBy == userId && p.LastModifiedDate >= cutoffDate);
                }

                if (includeFocalPoint)
                {
                    partnersQuery = partnersQuery.Where(p => p.PartnerFocalPointUserId == userId);
                }

                // Get partners with metrics
                var partners = await partnersQuery
                    .Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Status,
                        p.PartnerShortDescription,
                        p.KeyGlobalPartner,
                        p.PartnerApprovalStatus,
                        CreatedDate = p.CreatedDate,
                        LastModifiedDate = p.LastModifiedDate,
                        IsFocalPoint = p.PartnerFocalPointUserId == userId,
                        IsCreator = p.CreatedBy == userId,
                        LastModifier = p.LastModifiedBy == userId
                    })
                    .ToListAsync();

                // Get partner IDs for future analytics (engagement data no longer available)
                var partnerIds = partners.Select(p => p.Id).ToList();

                // Combine data
                var result = partners.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Status,
                    ShortName = p.PartnerShortDescription,
                    p.KeyGlobalPartner,
                    p.PartnerApprovalStatus,
                    p.CreatedDate,
                    p.LastModifiedDate,
                    p.IsFocalPoint,
                    p.IsCreator,
                    p.LastModifier,
                    EngagementCount = 0 // Engagement data no longer available
                }).ToList();

                return Ok(new
                {
                    metadata = new
                    {
                        userId,
                        timeframe,
                        includeCreated,
                        includeModified,
                        includeFocalPoint,
                        generatedAt = DateTime.UtcNow,
                        totalPartners = result.Count
                    },
                    partners = result
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partners for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while retrieving user partner data" });
            }
        }

        /// <summary>
        /// Retrieves engagement trends over time for partners.
        /// </summary>
        /// <param name="period">Time period for grouping (daily, weekly, monthly, quarterly, yearly)</param>
        /// <param name="months">Number of months to analyze (default: 12)</param>
        /// <param name="partnerId">Optional partner ID to filter by</param>
        /// <example_uses>
        /// Show partner engagement trends over time
        /// Get monthly engagement statistics
        /// Analyze quarterly partner engagement patterns
        /// Track partner activity trends over the last year
        /// View engagement history for specific partner
        /// </example_uses>
        /// <when_to_use>Use this when you need to analyze partner engagement patterns over time.</when_to_use>
        /// <returns>Time series data of partner engagements</returns>
        [HttpGet(APIDictionary.Partner + "/analytics/engagementTrends")]
        [AccessControlled(EntityTypes.Partner, "read")]
        public async Task<ActionResult<object>> GetEngagementTrends(
            [FromQuery] string period = "monthly",
            [FromQuery] int months = 12,
            [FromQuery] int? partnerId = null)
        {
            try
            {
                // Validate parameters
                if (months <= 0 || months > 60)
                {
                    return BadRequest(new { error = "Months must be between 1 and 60" });
                }

                // Calculate start date
                var startDate = DateTime.UtcNow.AddMonths(-months);

                // Build query
                var query = _context.BaseEngagements
                    .Where(e => !e.IsDeleted && e.EngagementSignedDate >= startDate);

                // Get all engagements within the time period with partner information
                var engagements = await query
                    .Join(_context.BaseEngagementPartners,
                        e => e.Id,
                        ep => ep.BaseEngagementId,
                        (e, ep) => new
                        {
                            e.Id,
                            PartnerId = ep.PartnerId,
                            CreatedDate = e.EngagementSignedDate,
                            e.Status
                        })
                    .Where(x => x.PartnerId.HasValue)
                    .Where(x => !partnerId.HasValue || x.PartnerId == partnerId.Value)
                    .ToListAsync();

                // Group by time period
                var groupedData = GroupEngagementsByTimePeriod(engagements.Cast<dynamic>().ToList(), period);

                // Get partner names if needed
                Dictionary<int, string> partnerNames = new Dictionary<int, string>();
                if (partnerId.HasValue)
                {
                    var partner = await _context.Partners.FirstOrDefaultAsync(p => p.Id == partnerId.Value);
                    if (partner != null)
                    {
                        partnerNames[partner.Id] = partner.Name;
                    }
                }
                else
                {
                    // Get names for all partners with engagements
                    var partnerIds = engagements.Select(e => e.PartnerId).Distinct().ToList();
                    var partners = await _context.Partners
                        .Where(p => partnerIds.Contains(p.Id))
                        .Select(p => new { p.Id, p.Name })
                        .ToListAsync();

                    partnerNames = partners.ToDictionary(p => p.Id, p => p.Name);
                }

                return Ok(new
                {
                    metadata = new
                    {
                        period,
                        months,
                        partnerId,
                        partnerName = partnerId.HasValue && partnerNames.ContainsKey(partnerId.Value) ? partnerNames[partnerId.Value] : null,
                        startDate,
                        endDate = DateTime.UtcNow,
                        generatedAt = DateTime.UtcNow
                    },
                    trends = groupedData,
                    summary = new
                     {
                         totalEngagements = engagements.Count,
                         activePartners = engagements.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count(),
                         averageEngagementsPerPeriod = groupedData.Count > 0 ? engagements.Count / (double)groupedData.Count : 0
                     }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving engagement trends");
                return StatusCode(500, new { error = "An error occurred while retrieving engagement trend data" });
            }
        }

        /// <summary>
        /// Retrieves partners grouped by country with counts and metrics.
        /// </summary>
        /// <param name="limit">Maximum number of countries to return (default: 20)</param>
        /// <param name="minCount">Minimum number of partners per country to include (default: 1)</param>
        /// <example_uses>
        /// Show partners by country
        /// Get geographic distribution of partners
        /// List top countries by partner count
        /// Analyze partner distribution across regions
        /// View countries with most partners
        /// </example_uses>
        /// <when_to_use>Use this when you need to analyze the geographic distribution of partners.</when_to_use>
        /// <returns>List of countries with partner counts and metrics</returns>
        [HttpGet(APIDictionary.Partner + "/analytics/byCountry")]
        [AccessControlled(EntityTypes.Partner, "read")]
        public async Task<ActionResult<object>> GetPartnersByCountry(
            [FromQuery] int limit = 20,
            [FromQuery] int minCount = 1)
        {
            try
            {
                // Validate parameters
                if (limit <= 0 || limit > 250)
                {
                    return BadRequest(new { error = "Limit must be between 1 and 250" });
                }

                if (minCount < 1)
                {
                    return BadRequest(new { error = "Minimum count must be at least 1" });
                }

                // Get partners with liaison office data
                var partners = await _context.Partners
                    .Where(p => !p.IsDeleted && p.Status == EntityStatus.Active)
                    .Include(p => p.LiaisonOffice)
                    .ToListAsync();

                // Group by country (using liaison office country as proxy)
                var countryGroups = partners
                    .Where(p => p.LiaisonOffice != null && !string.IsNullOrEmpty(p.LiaisonOffice.Country))
                    .GroupBy(p => p.LiaisonOffice!.Country!)
                    .Select(g => new
                    {
                        Country = g.Key,
                        PartnerCount = g.Count(),
                        KeyGlobalPartners = g.Count(p => p.KeyGlobalPartner),
                        UNSecretariatPartners = g.Count(p => p.UNSecretariatPartner),
                        PooledFundPartners = g.Count(p => p.PooledFund),
                        ApprovedPartners = g.Count(p => p.PartnerApprovalStatus == PartnerApprovalStatus.Approved),
                        Partners = g.Select(p => new { p.Id, p.Name, p.PartnerShortDescription }).ToList()
                    })
                    .Where(g => g.PartnerCount >= minCount)
                    .OrderByDescending(g => g.PartnerCount)
                    .Take(limit)
                    .ToList();

                return Ok(new
                {
                    metadata = new
                    {
                        totalCountries = countryGroups.Count,
                        totalPartners = countryGroups.Sum(g => g.PartnerCount),
                        generatedAt = DateTime.UtcNow
                    },
                    countries = countryGroups
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partners by country");
                return StatusCode(500, new { error = "An error occurred while retrieving country distribution data" });
            }
        }

        #region Helper Methods

        /// <summary>
        /// Gets the cutoff date based on the specified timeframe
        /// </summary>
        private DateTime GetCutoffDateFromTimeframe(string timeframe)
        {
            DateTime now = DateTime.UtcNow;
            return timeframe.ToLowerInvariant() switch
            {
                "daily" => now.AddDays(-1),
                "weekly" => now.AddDays(-7),
                "monthly" => now.AddMonths(-1),
                "quarterly" => now.AddMonths(-3),
                "yearly" => now.AddYears(-1),
                _ => throw new ArgumentException($"Invalid timeframe: {timeframe}. Valid values are: daily, weekly, monthly, quarterly, yearly.")
            };
        }

        /// <summary>
        /// Gets partners with activity metrics based on the specified metric
        /// </summary>
        private async Task<List<object>> GetPartnersWithActivityMetrics(DateTime cutoffDate, string metric, int limit)
        {
            // Base query for active partners
            var partnersQuery = _context.Partners
                .Where(p => !p.IsDeleted && p.Status == EntityStatus.Active)
                .AsQueryable();

            // Different metrics require different queries
            switch (metric.ToLowerInvariant())
            {
                case "engagements":
                    // Get partners with most engagements in the time period
                    var engagementCounts = await _context.BaseEngagements
                        .Where(e => !e.IsDeleted && e.EngagementSignedDate >= cutoffDate)
                        .Join(_context.BaseEngagementPartners,
                            e => e.Id,
                            ep => ep.BaseEngagementId,
                            (e, ep) => ep.PartnerId)
                        .Where(partnerId => partnerId.HasValue)
                        .GroupBy(partnerId => partnerId)
                        .Select(g => new { PartnerId = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .Take(limit)
                        .ToListAsync();

                    var partnerIds = engagementCounts.Select(e => e.PartnerId).ToList();
                    var partners = await _context.Partners
                        .Where(p => partnerIds.Contains(p.Id))
                        .ToListAsync();

                    return engagementCounts
                        .Where(ec => ec.PartnerId.HasValue) // Filter out null PartnerId values
                        .Select(ec => {
                            var partner = partners.FirstOrDefault(p => p.Id == ec.PartnerId);
                            return new {
                                PartnerId = ec.PartnerId,
                                PartnerName = partner?.Name,
                                ShortName = partner?.PartnerShortDescription,
                                EngagementCount = ec.Count,
                                KeyGlobalPartner = partner?.KeyGlobalPartner ?? false,
                                ApprovalStatus = partner?.PartnerApprovalStatus.ToString()
                            };
                        })
                        .Cast<object>()
                        .ToList();

                case "interactions":
                    // Get partners with most interactions in the time period
                    // This requires joining through contacts to interactions
                    var partnersWithContacts = await _context.Partners
                        .Where(p => !p.IsDeleted && p.Status == EntityStatus.Active)
                        .Include(p => p.Contacts)
                            .ThenInclude(c => c.Interactions != null ? c.Interactions.Where(i => i.Date >= cutoffDate) : new List<Interaction>())
                        .ToListAsync();

                    return partnersWithContacts
                        .Select(p => new {
                            PartnerId = p.Id,
                            PartnerName = p.Name,
                            ShortName = p.PartnerShortDescription,
                            InteractionCount = p.Contacts?.Sum(c => c.Interactions?.Count ?? 0) ?? 0,
                            KeyGlobalPartner = p.KeyGlobalPartner,
                            ApprovalStatus = p.PartnerApprovalStatus.ToString()
                        })
                        .Where(p => p.InteractionCount > 0)
                        .OrderByDescending(p => p.InteractionCount)
                        .Take(limit)
                        .Cast<object>()
                        .ToList();

                case "lastactivity":
                    // Get partners with most recent activity
                    var partnersWithActivity = await _context.Partners
                        .Where(p => !p.IsDeleted && p.Status == EntityStatus.Active)
                        .Where(p => p.LastModifiedDate >= cutoffDate)
                        .OrderByDescending(p => p.LastModifiedDate)
                        .Take(limit)
                        .Select(p => new {
                            PartnerId = p.Id,
                            PartnerName = p.Name,
                            ShortName = p.PartnerShortDescription,
                            LastActivityDate = p.LastModifiedDate,
                            KeyGlobalPartner = p.KeyGlobalPartner,
                            ApprovalStatus = p.PartnerApprovalStatus.ToString()
                        })
                        .ToListAsync();

                    return partnersWithActivity.Cast<object>().ToList();

                default:
                    throw new ArgumentException($"Invalid metric: {metric}. Valid values are: engagements, interactions, lastActivity.");
            }
        }

        /// <summary>
        /// Groups engagements by the specified time period
        /// </summary>
        private List<object> GroupEngagementsByTimePeriod(List<dynamic> engagements, string period)
        {
            // Group by time period
            var result = new List<object>();

            switch (period.ToLowerInvariant())
            {
                case "daily":
                    result = engagements
                        .GroupBy(e => e.CreatedDate.Date)
                        .Select(g => new {
                                                         Period = g.Key.ToString("yyyy-MM-dd"),
                             Count = g.Count(),
                             PartnerCount = g.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count()
                        })
                        .OrderBy(g => g.Period)
                        .Cast<object>()
                        .ToList();
                    break;

                case "weekly":
                    result = engagements
                        .GroupBy(e => {
                            var date = e.CreatedDate;
                            // Get the week start (Monday)
                            var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
                            return date.AddDays(-diff).Date;
                        })
                        .Select(g => new {
                            Period = g.Key.ToString("yyyy-MM-dd"),
                             WeekStart = g.Key.ToString("yyyy-MM-dd"),
                             WeekEnd = g.Key.AddDays(6).ToString("yyyy-MM-dd"),
                             Count = g.Count(),
                             PartnerCount = g.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count()
                        })
                        .OrderBy(g => g.Period)
                        .Cast<object>()
                        .ToList();
                    break;

                case "monthly":
                    result = engagements
                        .GroupBy(e => new DateTime(e.CreatedDate.Year, e.CreatedDate.Month, 1))
                        .Select(g => new {
                                                         Period = g.Key.ToString("yyyy-MM"),
                             MonthName = g.Key.ToString("MMMM yyyy"),
                             Count = g.Count(),
                             PartnerCount = g.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count()
                        })
                        .OrderBy(g => g.Period)
                        .Cast<object>()
                        .ToList();
                    break;

                case "quarterly":
                    result = engagements
                        .GroupBy(e => new {
                            Year = e.CreatedDate.Year,
                            Quarter = (e.CreatedDate.Month - 1) / 3 + 1
                        })
                        .Select(g => new {
                                                         Period = $"{g.Key.Year}-Q{g.Key.Quarter}",
                             Year = g.Key.Year,
                             Quarter = g.Key.Quarter,
                             Count = g.Count(),
                             PartnerCount = g.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count()
                        })
                        .OrderBy(g => g.Year)
                        .ThenBy(g => g.Quarter)
                        .Cast<object>()
                        .ToList();
                    break;

                case "yearly":
                    result = engagements
                        .GroupBy(e => e.CreatedDate.Year)
                        .Select(g => new {
                                                         Period = g.Key.ToString(),
                             Year = g.Key,
                             Count = g.Count(),
                             PartnerCount = g.Where(e => e.PartnerId.HasValue).Select(e => e.PartnerId).Distinct().Count()
                        })
                        .OrderBy(g => g.Year)
                        .Cast<object>()
                        .ToList();
                    break;

                default:
                    throw new ArgumentException($"Invalid period: {period}. Valid values are: daily, weekly, monthly, quarterly, yearly.");
            }

            return result;
        }

        #endregion
    }
}
