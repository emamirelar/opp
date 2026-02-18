using System;
using System.Collections.Generic;

namespace UNOPS.PAO.Models.ContactAnalytics;

/// <summary>
/// Contact analytics model for interaction tracking and engagement scoring
/// </summary>
public class ContactAnalyticsModel
{
    // ========== CONTACT IDENTIFICATION ==========
    public int ContactId { get; set; }
    public string ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactOrganization { get; set; }

    // ========== INTERACTION METRICS ==========
    public int InteractionCount { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    public DateTime? FirstInteractionDate { get; set; }
    public int? DaysSinceLastInteraction { get; set; }

    // ========== ENGAGEMENT SCORING ==========
    public decimal EngagementScore { get; set; }
    public string? EngagementLevel { get; set; } // High, Medium, Low
    public int? EmailInteractionCount { get; set; }
    public int? MeetingCount { get; set; }
    public int? CallCount { get; set; }

    // ========== ACTIVITY TRENDS ==========
    public int? InteractionsLast30Days { get; set; }
    public int? InteractionsLast90Days { get; set; }
    public int? InteractionsLastYear { get; set; }
    public decimal? EngagementTrend { get; set; } // Positive/negative trend

    // ========== RELATIONSHIP METRICS ==========
    public int? PartnerCount { get; set; }
    public int? OpportunityCount { get; set; }
    public decimal? TotalOpportunityValue { get; set; }
    public string? PrimaryPartnerName { get; set; }

    // ========== TIMESTAMPS ==========
    public DateTime? AnalyticsGeneratedDate { get; set; }
    public DateTime? LastUpdated { get; set; }
}

/// <summary>
/// Contact engagement summary model
/// </summary>
public class ContactEngagementModel
{
    public int ContactId { get; set; }
    public string ContactName { get; set; }
    public decimal EngagementScore { get; set; }
    public int TotalInteractions { get; set; }
    public DateTime? LastInteractionDate { get; set; }
    public string EngagementLevel { get; set; }
}

/// <summary>
/// Contact metrics summary model
/// </summary>
public class ContactMetricsModel
{
    public int TotalContacts { get; set; }
    public int ActiveContacts { get; set; }
    public int HighEngagementContacts { get; set; }
    public int MediumEngagementContacts { get; set; }
    public int LowEngagementContacts { get; set; }
    public decimal AverageEngagementScore { get; set; }
    public int TotalInteractions { get; set; }
    public DateTime? MetricsGeneratedDate { get; set; }
}
