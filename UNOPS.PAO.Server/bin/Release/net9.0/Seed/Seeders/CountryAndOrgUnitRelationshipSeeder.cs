using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds OrganizationUnitRelationships between Countries and OrganizationHierarchy units
/// Generated from CountryAndOrgUnitRelationshipSeeder.csv
/// Generated on: 2025-11-17 17:53:06
/// </summary>
public static class CountryAndOrgUnitRelationshipSeeder
{
    public static async Task SeedCountryOrgUnitRelationshipsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding Country-OrganizationUnit Relationships...");

        var relationshipsToSeed = GetRelationshipsToSeed();

        // Load all countries with Iso3Code
        var countries = await context.Set<Country>()
            .Where(c => !c.IsDeleted && c.Iso3Code != null)
            .ToDictionaryAsync(c => c.Iso3Code!, c => c);

        // Load all organization hierarchy units by Code
        var orgUnits = await context.Set<OrganizationHierarchy>()
            .Where(oh => !oh.IsDeleted && oh.Type == OrganizationUnitType.OrgUnit)
            .ToDictionaryAsync(oh => oh.Code, oh => oh);

        // Load all existing relationships for Countries with EntityType "OrgUnit"
        var existingRelationships = await context.Set<OrganizationUnitRelationship>()
            .Where(r => r.EntityType == "Country" && !r.IsDeleted)
            .ToListAsync();

        int inserted = 0;
        int updated = 0;
        int skipped = 0;
        int notFoundCountry = 0;
        int notFoundOrgUnit = 0;

        foreach (var (iso3Code, orgUnitCode) in relationshipsToSeed)
        {
            // Find the country
            if (!countries.TryGetValue(iso3Code, out var country))
            {
                Console.WriteLine($"  ⚠️  Country not found for ISO3: {iso3Code}");
                notFoundCountry++;
                continue;
            }

            // Find the organization unit
            if (!orgUnits.TryGetValue(orgUnitCode, out var orgUnit))
            {
                Console.WriteLine($"  ⚠️  Organization Unit not found for Code: {orgUnitCode}");
                notFoundOrgUnit++;
                continue;
            }

            // Check if relationship already exists
            var existingRel = existingRelationships.FirstOrDefault(r =>
                r.OrganizationHierarchyId == orgUnit.Id &&
                r.EntityId == country.Id &&
                r.EntityType == "Country");

            if (existingRel == null)
            {
                // Create new relationship
                var relationshipName = $"Country-{country.Id}-{orgUnit.Code}";
                var newRelationship = new OrganizationUnitRelationship
                {
                    Name = relationshipName,
                    OrganizationHierarchyId = orgUnit.Id,
                    EntityId = country.Id,
                    EntityType = "Country",
                    Status = EntityStatus.Active,
                    CreatedBy = 1,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedBy = 0,
                    LastModifiedDate = DateTime.UtcNow,
                    DeletedBy = 0,
                    IsDeleted = false,
                };

                context.Set<OrganizationUnitRelationship>().Add(newRelationship);
                Console.WriteLine($"  ✅ Inserted: {country.Name} ({iso3Code}) → {orgUnit.Name} ({orgUnitCode})");
                inserted++;
            }
            else
            {
                // Check if update is needed
                bool hasChanges = false;
                var expectedName = $"Country-{country.Id}-{orgUnit.Code}";

                if (existingRel.Name != expectedName)
                {
                    existingRel.Name = expectedName;
                    hasChanges = true;
                }

                if (existingRel.Status != EntityStatus.Active)
                {
                    existingRel.Status = EntityStatus.Active;
                    hasChanges = true;
                }

                if (existingRel.IsDeleted)
                {
                    existingRel.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    existingRel.LastModifiedBy = 1;
                    existingRel.LastModifiedDate = DateTime.UtcNow;
                    Console.WriteLine($"  🔄 Updated: {country.Name} ({iso3Code}) → {orgUnit.Name} ({orgUnitCode})");
                    updated++;
                }
                else
                {
                    skipped++;
                }
            }
        }

        await context.SaveChangesAsync();

        Console.WriteLine($"✅ Country-OrganizationUnit Relationships seeding completed");
        Console.WriteLine($"   📊 Statistics:");
        Console.WriteLine($"      ✅ Inserted: {inserted}");
        Console.WriteLine($"      🔄 Updated: {updated}");
        Console.WriteLine($"      ⏭️  Skipped (unchanged): {skipped}");
        if (notFoundCountry > 0)
            Console.WriteLine($"      ⚠️  Countries not found: {notFoundCountry}");
        if (notFoundOrgUnit > 0)
            Console.WriteLine($"      ⚠️  Organization Units not found: {notFoundOrgUnit}");
        Console.WriteLine();
    }

    private static List<(string Iso3Code, string OrgUnitCode)> GetRelationshipsToSeed()
    {
        return new List<(string, string)>
        {
                ("ABW", "B0054"),
                ("AFG", "B5101"),
                ("AGO", "B5328"),
                ("AIA", "B0054"),
                ("ALB", "B5131"),
                ("AND", "B0050"),
                ("ARE", "B5120"),
                ("ARG", "B5401"),
                ("ARM", "B5010"),
                ("ASM", "B0051"),
                ("ATG", "B0054"),
                ("AUS", "B0051"),
                ("AUT", "B5010"),
                ("AZE", "B5010"),
                ("BDI", "B5328"),
                ("BEL", "B0050"),
                ("BEN", "B5305"),
                ("BES", "B0054"),
                ("BFA", "B5322"),
                ("BGD", "B5514"),
                ("BGR", "B5010"),
                ("BHR", "B5120"),
                ("BHS", "B0054"),
                ("BIH", "B5109"),
                ("BLM", "B0054"),
                ("BLR", "B5109"),
                ("BLZ", "B5423"),
                ("BMU", "B5009"),
                ("BOL", "B0054"),
                ("BRA", "B5414"),
                ("BRB", "B0054"),
                ("BRN", "B0051"),
                ("BTN", "B0051"),
                ("BWA", "B5323"),
                ("CAF", "B5313"),
                ("CAN", "B5009"),
                ("CHE", "B5007"),
                ("CHL", "B0054"),
                ("CHN", "B5511"),
                ("CIV", "B5305"),
                ("CMR", "B5328"),
                ("COD", "B5301"),
                ("COG", "B5301"),
                ("COK", "B0051"),
                ("COL", "B0054"),
                ("COM", "B5328"),
                ("CPV", "B5305"),
                ("CRI", "B5416"),
                ("CUB", "B0054"),
                ("CUW", "B0054"),
                ("CYM", "B0054"),
                ("CYP", "B5030"),
                ("CZE", "B0050"),
                ("DEU", "B0050"),
                ("DJI", "B5318"),
                ("DMA", "B0054"),
                ("DNK", "B0050"),
                ("DOM", "B0054"),
                ("DZA", "B5305"),
                ("ECU", "B5423"),
                ("EGY", "B5305"),
                ("ERI", "B5323"),
                ("ESH", "B0053"),
                ("ESP", "B0050"),
                ("EST", "B0050"),
                ("ETH", "B5308"),
                ("FIN", "B3620"),
                ("FJI", "B0051"),
                ("FLK", "B0054"),
                ("FRA", "B0050"),
                ("FRO", "B0050"),
                ("FSM", "B0051"),
                ("GAB", "B5328"),
                ("GBR", "B0050"),
                ("GEO", "B5130"),
                ("GGY", "B0050"),
                ("GHA", "B5314"),
                ("GIB", "B0050"),
                ("GIN", "B5320"),
                ("GLP", "B0054"),
                ("GMB", "B5315"),
                ("GNB", "B5330"),
                ("GNQ", "B5328"),
                ("GRC", "B0050"),
                ("GRD", "B5423"),
                ("GRL", "B5009"),
                ("GTM", "B5405"),
                ("GUF", "B0054"),
                ("GUM", "B0051"),
                ("GUY", "B0054"),
                ("HKG", "B0051"),
                ("HND", "B5417"),
                ("HRV", "B0050"),
                ("HTI", "B5406"),
                ("HUN", "B0050"),
                ("IDN", "B5502"),
                ("IMN", "B0050"),
                ("IND", "B5503"),
                ("IRL", "B0050"),
                ("IRN", "B5120"),
                ("IRQ", "B5121"),
                ("ISL", "B0050"),
                ("ISR", "B5106"),
                ("ITA", "B0050"),
                ("JAM", "B5423"),
                ("JEY", "B0050"),
                ("JOR", "B5104"),
                ("JPN", "B0051"),
                ("KAZ", "B5010"),
                ("KEN", "B5323"),
                ("KGZ", "B5010"),
                ("KHM", "B5509"),
                ("KIR", "B0051"),
                ("KNA", "B0054"),
                ("KOR", "B0051"),
                ("KWT", "B5120"),
                ("LAO", "B5520"),
                ("LBN", "B5122"),
                ("LBR", "B5309"),
                ("LBY", "B5325"),
                ("LCA", "B5418"),
                ("LIE", "B0050"),
                ("LKA", "B5505"),
                ("LSO", "B5323"),
                ("LTU", "B0050"),
                ("LUX", "B0050"),
                ("LVA", "B0050"),
                ("MAC", "B0051"),
                ("MAF", "B0054"),
                ("MAR", "B5324"),
                ("MCO", "B0050"),
                ("MDA", "B5116"),
                ("MDG", "B5338"),
                ("MDV", "B5519"),
                ("MEX", "B5421"),
                ("MHL", "B0051"),
                ("MKD", "B5109"),
                ("MLI", "B5310"),
                ("MLT", "B0050"),
                ("MMR", "B5506"),
                ("MNE", "B5109"),
                ("MNG", "B0051"),
                ("MNP", "B0051"),
                ("MOZ", "B5327"),
                ("MRT", "B5305"),
                ("MSR", "B0054"),
                ("MTQ", "B0054"),
                ("MUS", "B5323"),
                ("MWI", "B5336"),
                ("MYS", "B0051"),
                ("MYT", "B0050"),
                ("NAM", "B5323"),
                ("NCL", "B0051"),
                ("NER", "B5321"),
                ("NFK", "B0051"),
                ("NGA", "B5316"),
                ("NIC", "B5407"),
                ("NIU", "B0051"),
                ("NLD", "B0050"),
                ("NOR", "B0050"),
                ("NPL", "B5516"),
                ("NRU", "B0051"),
                ("NZL", "B0051"),
                ("OMN", "B5120"),
                ("PAK", "B5507"),
                ("PAN", "B5408"),
                ("PCN", "B0051"),
                ("PER", "B5410"),
                ("PHL", "B5512"),
                ("PLW", "B0051"),
                ("PNG", "B5524"),
                ("POL", "B5116"),
                ("PRI", "B0054"),
                ("PRK", "B0051"),
                ("PRT", "B0050"),
                ("PRY", "B5411"),
                ("PSE", "B5120"),
                ("PYF", "B0051"),
                ("QAT", "B5120"),
                ("REU", "B0053"),
                ("ROU", "B5010"),
                ("RUS", "B5010"),
                ("RWA", "B5328"),
                ("SAU", "B5120"),
                ("SDN", "B5312"),
                ("SEN", "B5305"),
                ("SGP", "B0051"),
                ("SHN", "B0050"),
                ("SJM", "B0050"),
                ("SLB", "B0051"),
                ("SLE", "B5317"),
                ("SLV", "B5412"),
                ("SMR", "B0050"),
                ("SOM", "B5311"),
                ("SPM", "B5009"),
                ("SRB", "B5109"),
                ("SSD", "B5304"),
                ("STP", "B5305"),
                ("SUR", "B0054"),
                ("SVK", "B0050"),
                ("SVN", "B0050"),
                ("SWE", "B0050"),
                ("SWZ", "B5323"),
                ("SXM", "B0054"),
                ("SYC", "B5323"),
                ("SYR", "B5123"),
                ("TCA", "B0054"),
                ("TCD", "B5305"),
                ("TGO", "B5305"),
                ("THA", "B5006"),
                ("TJK", "B5010"),
                ("TKL", "B0051"),
                ("TKM", "B5010"),
                ("TLS", "B0051"),
                ("TON", "B0051"),
                ("TTO", "B0054"),
                ("TUN", "B5306"),
                ("TUR", "B5010"),
                ("TUV", "B0051"),
                ("TWN", "B0051"),
                ("TZA", "B5335"),
                ("UGA", "B5331"),
                ("UKR", "B5116"),
                ("URY", "B0054"),
                ("USA", "B5009"),
                ("UZB", "B5133"),
                ("VAT", "B0050"),
                ("VCT", "B5423"),
                ("VEN", "B0054"),
                ("VGB", "B0054"),
                ("VIR", "B0054"),
                ("VNM", "B5519"),
                ("VUT", "B0051"),
                ("WLF", "B0051"),
                ("WSM", "B0051"),
                ("XKS", "B0050"),
                ("YEM", "B5124"),
                ("ZAF", "B5323"),
                ("ZMB", "B5334"),
                ("ZWE", "B5329")
        };
    }
}
