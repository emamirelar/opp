using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.Domain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds all risk lookup tables (RiskType, RiskProbability, RiskProximity, RiskImpactLevel, RiskResponseType)
/// Data aligned with oUP risk management system
/// </summary>
public class RiskLookupSeeder
{
    public static async Task SeedRiskLookupsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Risk Lookup Tables...");

        await SeedRiskTypesAsync(context);
        await SeedRiskProbabilitiesAsync(context);
        await SeedRiskProximitiesAsync(context);
        await SeedRiskImpactLevelsAsync(context);
        await SeedRiskResponseTypesAsync(context);

        Console.WriteLine("✅ Risk Lookup Tables seeding completed\n");
    }

    private static async Task SeedRiskTypesAsync(UNOPSAppDbContext context)
    {
        if (await context.RiskTypes.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskTypes already exist. Skipping.");
            return;
        }

        var riskTypes = new List<RiskType>
        {
            new RiskType
            {
                Name = "Threat",
                Code = "THREAT",
                Description = "Potential negative event. Valid response types: Accept, Avoid, Reduce, Share, Transfer",
                DisplayOrder = 1,
                IsResponseTypeMandatory = false,
                Status = EntityStatus.Active
            },
            new RiskType
            {
                Name = "Opportunity",
                Code = "OPPORTUNITY",
                Description = "Potential positive event. Valid response types: Accept, Enhance, Exploit, Share, Transfer. ResponseType is MANDATORY.",
                DisplayOrder = 2,
                IsResponseTypeMandatory = true,
                Status = EntityStatus.Active
            }
        };

        await context.RiskTypes.AddRangeAsync(riskTypes);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {riskTypes.Count} RiskTypes");
    }

    private static async Task SeedRiskProbabilitiesAsync(UNOPSAppDbContext context)
    {
        if (await context.RiskProbabilities.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskProbabilities already exist. Skipping.");
            return;
        }

        var probabilities = new List<RiskProbability>
        {
            new RiskProbability
            {
                Name = "Low",
                Code = "LOW",
                DisplayLabel = "1. Low",
                NumericValue = 1,
                DisplayOrder = 1,
                Status = EntityStatus.Active
            },
            new RiskProbability
            {
                Name = "Low to medium",
                Code = "LOW_TO_MEDIUM",
                DisplayLabel = "2. Low to medium",
                NumericValue = 2,
                DisplayOrder = 2,
                Status = EntityStatus.Active
            },
            new RiskProbability
            {
                Name = "Medium to high",
                Code = "MEDIUM_TO_HIGH",
                DisplayLabel = "3. Medium to high",
                NumericValue = 3,
                DisplayOrder = 3,
                Status = EntityStatus.Active
            },
            new RiskProbability
            {
                Name = "High",
                Code = "HIGH",
                DisplayLabel = "4. High",
                NumericValue = 4,
                DisplayOrder = 4,
                Status = EntityStatus.Active
            }
        };

        await context.RiskProbabilities.AddRangeAsync(probabilities);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {probabilities.Count} RiskProbabilities");
    }

    private static async Task SeedRiskProximitiesAsync(UNOPSAppDbContext context)
    {
        if (await context.RiskProximities.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskProximities already exist. Skipping.");
            return;
        }

        var proximities = new List<RiskProximity>
        {
            new RiskProximity
            {
                Name = "Within one month",
                Code = "WITHIN_ONE_MONTH",
                MonthsValue = 1,
                DisplayOrder = 1,
                Status = EntityStatus.Active
            },
            new RiskProximity
            {
                Name = "Within three months",
                Code = "WITHIN_THREE_MONTHS",
                MonthsValue = 3,
                DisplayOrder = 2,
                Status = EntityStatus.Active
            },
            new RiskProximity
            {
                Name = "Within six months",
                Code = "WITHIN_SIX_MONTHS",
                MonthsValue = 6,
                DisplayOrder = 3,
                Status = EntityStatus.Active
            },
            new RiskProximity
            {
                Name = "One year and beyond",
                Code = "ONE_YEAR_AND_BEYOND",
                MonthsValue = 12,
                DisplayOrder = 4,
                Status = EntityStatus.Active
            }
        };

        await context.RiskProximities.AddRangeAsync(proximities);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {proximities.Count} RiskProximities");
    }

    private static async Task SeedRiskImpactLevelsAsync(UNOPSAppDbContext context)
    {
        if (await context.RiskImpactLevels.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskImpactLevels already exist. Skipping.");
            return;
        }

        var impactLevels = new List<RiskImpactLevel>
        {
            new RiskImpactLevel
            {
                Name = "Low",
                Code = "LOW",
                DisplayLabel = "1. Low",
                NumericValue = 1,
                DisplayOrder = 1,
                Status = EntityStatus.Active
            },
            new RiskImpactLevel
            {
                Name = "Low to medium",
                Code = "LOW_TO_MEDIUM",
                DisplayLabel = "2. Low to medium",
                NumericValue = 2,
                DisplayOrder = 2,
                Status = EntityStatus.Active
            },
            new RiskImpactLevel
            {
                Name = "Medium to high",
                Code = "MEDIUM_TO_HIGH",
                DisplayLabel = "3. Medium to high",
                NumericValue = 3,
                DisplayOrder = 3,
                Status = EntityStatus.Active
            },
            new RiskImpactLevel
            {
                Name = "High",
                Code = "HIGH",
                DisplayLabel = "4. High",
                NumericValue = 4,
                DisplayOrder = 4,
                Status = EntityStatus.Active
            }
        };

        await context.RiskImpactLevels.AddRangeAsync(impactLevels);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {impactLevels.Count} RiskImpactLevels");
    }

    private static async Task SeedRiskResponseTypesAsync(UNOPSAppDbContext context)
    {
        if (await context.RiskResponseTypes.AnyAsync())
        {
            Console.WriteLine("  ⏭️  RiskResponseTypes already exist. Skipping.");
            return;
        }

        var responseTypes = new List<RiskResponseType>
        {
            new RiskResponseType
            {
                Name = "Accept",
                Code = "ACCEPT",
                Description = "Accept the risk without action",
                ValidForThreat = true,
                ValidForOpportunity = true,
                DisplayOrder = 1,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Avoid",
                Code = "AVOID",
                Description = "Avoid the risk entirely",
                ValidForThreat = true,
                ValidForOpportunity = false,
                DisplayOrder = 2,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Reduce",
                Code = "REDUCE",
                Description = "Take action to reduce probability/impact",
                ValidForThreat = true,
                ValidForOpportunity = false,
                DisplayOrder = 3,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Share",
                Code = "SHARE",
                Description = "Share the risk with partners",
                ValidForThreat = true,
                ValidForOpportunity = true,
                DisplayOrder = 4,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Transfer",
                Code = "TRANSFER",
                Description = "Transfer the risk to another party",
                ValidForThreat = true,
                ValidForOpportunity = true,
                DisplayOrder = 5,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Enhance",
                Code = "ENHANCE",
                Description = "Enhance the opportunity",
                ValidForThreat = false,
                ValidForOpportunity = true,
                DisplayOrder = 6,
                Status = EntityStatus.Active
            },
            new RiskResponseType
            {
                Name = "Exploit",
                Code = "EXPLOIT",
                Description = "Exploit the opportunity",
                ValidForThreat = false,
                ValidForOpportunity = true,
                DisplayOrder = 7,
                Status = EntityStatus.Active
            }
        };

        await context.RiskResponseTypes.AddRangeAsync(responseTypes);
        await context.SaveChangesAsync();
        Console.WriteLine($"  ✅ Seeded {responseTypes.Count} RiskResponseTypes");
    }
}

