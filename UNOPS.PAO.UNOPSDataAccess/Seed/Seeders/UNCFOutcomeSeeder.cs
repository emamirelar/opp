using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds UNCF Outcomes (UN Cooperation Framework Outcomes) with proper insert/update logic
/// Data synced from External Data Service (ERP Database)
/// </summary>
public static class UNCFOutcomeSeeder
{
    public static async Task SeedUNCFOutcomesAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("🔄 Seeding UNCF Outcomes...");

        var outcomesToSeed = GetUNCFOutcomesToSeed();

        // Get existing UNCF Outcomes from database
        var existingOutcomes = await context.Set<UNCFOutcome>().ToListAsync();

        // Track outcome identifiers to keep
        var outcomeKeysToKeep = outcomesToSeed
            .Select(o => new { o.UNCFOutcomeId, o.UNCooperationFrameworkVersionNo })
            .ToHashSet();

        // Insert or Update UNCF Outcomes
        foreach (var outcomeData in outcomesToSeed)
        {
            var existingOutcome = existingOutcomes.FirstOrDefault(o =>
                o.UNCFOutcomeId == outcomeData.UNCFOutcomeId &&
                o.UNCooperationFrameworkVersionNo == outcomeData.UNCooperationFrameworkVersionNo);

            if (existingOutcome == null)
            {
                // Insert new UNCF Outcome
                context.Set<UNCFOutcome>().Add(outcomeData);
                Console.WriteLine($"  ✅ Inserted UNCF Outcome: {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");
            }
            else
            {
                // Update if any properties changed
                bool hasChanges = false;

                if (existingOutcome.Name != outcomeData.Name)
                {
                    existingOutcome.Name = outcomeData.Name;
                    hasChanges = true;
                }

                if (existingOutcome.Country != outcomeData.Country)
                {
                    existingOutcome.Country = outcomeData.Country;
                    hasChanges = true;
                }

                if (existingOutcome.UNCFOutcomeLastUpdatedDate != outcomeData.UNCFOutcomeLastUpdatedDate)
                {
                    existingOutcome.UNCFOutcomeLastUpdatedDate = outcomeData.UNCFOutcomeLastUpdatedDate;
                    hasChanges = true;
                }

                if (existingOutcome.Status != outcomeData.Status)
                {
                    existingOutcome.Status = outcomeData.Status;
                    hasChanges = true;
                }

                if (existingOutcome.IsDeleted)
                {
                    existingOutcome.IsDeleted = false;
                    hasChanges = true;
                }

                if (hasChanges)
                {
                    Console.WriteLine($"  🔄 Updated UNCF Outcome: {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");
                }
                else
                {
                    Console.WriteLine($"  ⏭️  Skipped UNCF Outcome (unchanged): {outcomeData.Country} v{outcomeData.UNCooperationFrameworkVersionNo} - {outcomeData.UNCFOutcomeId}");
                }
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"✅ UNCF Outcomes seeding completed - Total: {outcomesToSeed.Count}\n");
    }

    private static List<UNCFOutcome> GetUNCFOutcomesToSeed()
    {
        return new List<UNCFOutcome>
        {
            new UNCFOutcome
            {
                Name = "Outcome 1 - Human Capital Development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "101",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 32, 39, 10, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2 - Economic Growth, Innovation and Climate Change",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "102",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 32, 39, 23, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3 - Governance, Rule of Law, and Human Rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "103",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 32, 39, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4 - Gender Responsive Governance",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "104",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 32, 39, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: People benefit from a universal, affordable, accessible, and quality health system, while adopting healthy lifestyle practices.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "105",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 503, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: People benefit from a progressively universal, inclusive, and shockresponsive social protection system across the lifecycle.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "106",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 503, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: People exercise their talents and skills, benefitting from ageappropriate, life-long learning, inclusive and quality education in an enabling and safe environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "107",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: People, communities, and regions benefit from equitable economic opportunities, decent work, and sustainable livelihoods, enabled through competitiveness and inclusive green growth.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "108",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: Ecosystems are managed sustainably, and people benefit from participatory and resilient development and climate smart solutions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "109",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: People benefit from effective and accountable governance systems and institutions that safeguard human rights and uphold the rule of law; and a public administration that ensures effective and human-centred service delivery for all.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "110",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 7: People benefit from evidencebased, humancentric, and SDGaligned policies supported by diversified sources of financing, innovation, and partnerships for sustainable development for all.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "111",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 8: All persons benefit from gender equality and equal opportunities to realize their human rights, fulfil their economic, political, and social potential and contribute to the sustainable development of the country.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "112",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 10, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: People furthest behind participate in and benefit from a diverse and innovative economy encompassing futureoriented labor market transformation and access to decent .work",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "113",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 277, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: People furthest behind benefit from enhanced national capacities and governance structures for social protection and quality public and social services, in line with Azerbaijan’s international commitments.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "114",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 277, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: Quality, disaggregated and timely data is available and used to inform decision-making and policies that leave no one behind.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "115",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 280, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: People including those left behind benefit from climate strategies and environment protection policies that ensure natural resources are sustainably managed, livelihoods are protected, and resilience strengthened.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "116",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 280, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1: Women and girls, including those furthest behind, benefit from enhanced national mechanisms that ensure they are protected from discrimination and violence and empowered to participate in all spheres of life.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "117",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 33, 35, 280, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, people  benefit from more inclusive  and higher quality educational  programmes focused on 21st  century skills for enhanced  employability, well-being and  active participation in society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "118",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, people benefit from resilient, inclusive and sustainable growth ensured by the convergence of economic development, and management of environment and cultural resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "119",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, people contribute to and benefit from more accountable and transparent governance systems that deliver quality public services and ensure the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "120",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, people have access to better quality and inclusive health and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "121",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, there is stronger mutual understanding, respect and trust among individuals and communities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "122",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 3, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026 people, more people in Bangladesh, particularly the most vulnerable and marginalized from all gender and social groups and those from lagging districts benefit from sustainable livelihood and decent work opportunities resulting from responsible, inclusive, sustainable, green and equitable economic development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "123",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, more people, in particular, the most vulnerable and marginalized, have improved access to and utilization of quality, inclusive, gender- and shock-responsive, universal, and resilient social protection, social safety-net and basic social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "124",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, ecosystems are healthier, and all people, in particular the most vulnerable and marginalized in both rural and urban settings, benefit from and contribute in a gender-responsive manner to a cleaner and more resilient environment, an enriched natural resource base, low carbon development, and are prosperous and more resilient to climate change, shocks and disasters.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "125",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, more people, especially the most vulnerable, benefit from more equitable, nondiscriminatory, gender-responsive, participatory, accountable governance and justice, in a peaceful and tolerant society governed by the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "126",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2026, more women, girls and sexual minorities benefit from an environment in which they are empowered to exercise their rights, agency and decision-making power over all aspects of their lives and towards a life free from all forms of discrimination, violence and harmful norms and practices.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "127",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 34, 41, 513, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2023, there is enhanced access to and use of reliable and timely data for inclusive and evidence-based policy and decision-making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "128",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 15, 923, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2023, vulnerable and unreached people access and receive quality health, nutrition, protection, education, water, sanitation and hygiene services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "129",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 15, 923, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2023, national stakeholders provide equal opportunities for all, particularly women and vulnerable groups",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "130",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 15, 923, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2023, Bhutan’s communities and its economy are more resilient to climate-induced and other disasters and biodiversity loss as well as economic vulnerability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "131",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 15, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, all people, particularly vulnerable and marginalized groups, have equitable access to quality services of education, health, nutrition and social protection",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "132",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, Botswana has strengthened resilience to shocks and emergencies, and is on a sustainable, equitable economic trajectory, reducing levels of inequality, poverty and unemployment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "133",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, Botswana is a just society, where leaders are accountable, transparent and responsive, corruption is reduced, and people are empowered to access information, services and opportunities and participate in decisions that affect their lives and livelihoods",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "134",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, Botswana sustainably uses and actively manages its diverse natural resources, improves food security and effectively addresses climate change vulnerability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "135",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, gender inequality is reduced, and women and girls are empowered to access their human rights and participate in and benefit from inclusive development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "136",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 35, 39, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "A significant contribution to climate action is made by 2025 through the introduction of key measures of climate change adaptation and mitigation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "137",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 13, 160, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, adolescents, youth, families with children and vulnerable groups practice safer and healthier behaviours, enjoy better access to gender-responsive, inclusive and quality healthcare services, inclusive education,  and labour-market-oriented education, improved social protection system, more restorative approaches to justice, and opportunities to strengthen their families’ resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "138",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 13, 163, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, all people, including adolescents, young men and women, men and women aged 65 and older and other vulnerable groups, benefit from green and inclusive economic development, realised through comprehensive regulatory frameworks, promotion of business activities, private sector partnerships, and increased participatory decision-making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "139",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 13, 163, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, foundations of an efficient digital ecosystem are formed, including as part of smart sustainable cities, enabling interaction of the state, society, and business, with equal participation of women and men, adolescents, youth, and representatives of vulnerable groups",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "140",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 13, 170, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, improvements in data collection, gender equality policies, and child and gender budgeting have created conditions for men and women of all ages, including those aged 65 years and older, as well as girls and boys, to better realise their rights and increase the quality of their lives, including through increased opportunities for employment and better protection from gender-based and domestic violence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "141",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 13, 170, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, la prévalence de la violence et des conflits armés est réduite et la sécurité des personnes et des biens est améliorée en particulier celle des personnes vulnérables, y compris les réfugiés/déplacés, des femmes et les jeunes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "142",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les institutions publiques, les media et la société civile, au niveau central et décentralisé exercent efficacement leurs rôles pour une gouvernance démocratique apaisée, efficace et inclusive, porteuse d’effets sur la participation citoyenne et le renforcement de l’état de droit",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "143",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les organisations humanitaires et structures gouvernementales chargées des questions humanitaires apportent une réponse humanitaire coordonnée, rapide et efficace envers les personnes affectées par les crises dans le respect des standards et principes humanitaires en vue de réduire la surmortalité et la sur-morbidité des personnes affectées",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "144",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les populations congolaises jouissent d’une croissance économique inclusive durable portée par la transformation agricole, la diversification économique ouverte aux innovations et à la promotion de l’entrepreneuriat des jeunes et des femmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "145",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les populations tirent profit d’une gestion responsable et durable des ressources naturelles (forestières, minières, et foncières), par l’État, les entités décentralisées, les communautés, et le secteur privé, dans un contexte de changement climatique et de préservation de la biodiversité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "146",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les populations vivant en RDC bénéficient d’une protection sociale inclusive et d’un dividende démographique portée par la maitrise démographique et l’autonomisation des jeunes et des femmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "147",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "D’ici 2024, les populations vivant en RDC, plus spécifiquement les plus vulnérables (femmes, enfants, réfugiées et déplacées) jouissent de leurs droits humains, en particulier l’accès équitable à la justice, (y compris la justice juvénile), à l’identité juridique et la protection , à travers le renforcement des systèmes judiciaire, sécuritaire, des capacités de veille des organisations de la société civile sur les droits humains et la redevabilité institutionnelle",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "148",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Les populations, en particulier les plus vulnérables bénéficient d’un accès équitable, de qualité et durable aux services sociaux de base, y compris de lutte contre le VIH/SIDA",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "149",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 36, 57, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1: D’ici à 2027, les populations en RCA vivent dans un environnement où les mécanismes de consolidation de la paix, de réconciliation nationale, et de gouvernance sont inclusifs, redevables et efficaces.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "150",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 253, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2: D’ici à 2027, les populations en RCA vivent dans un État de droit et accèdent à la justice, à la sécurité, et leurs droits humains sont promus et protégés.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "151",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3: D’ici à 2027, les populations en RCA, notamment les femmes, les enfants, les jeunes et les autres groupes vulnérables accèdent et utilisent de manière inclusive et durable des services de qualité en matière d’éducation, de santé, de nutrition, d’eau, assainissement et hygiène, de sécurité alimentaire et de protection sociale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "152",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4: D’ici à 2027, les populations en RCA, notamment les femmes, les enfants, les jeunes, et les autres groupes vulnérables sont protégées de toutes formes de violences, d’exploitation, et de discrimination, y compris celles basées sur le genre, dans un environnement propice à l’égalité entre les sexes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "153",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 5: D’ici à 2027, les populations en RCA, en particulier celles vivant dans les zones rurales, accèdent de manière équitable à des infrastructures de qualité, durables et résilientes qui favorisent la relance économique, le bien-être, et l’intégration régionale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "154",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 6: D’ici à 2027, les populations en RCA, notamment les femmes, les jeunes, les réfugiés, les retournées, les PDI et tous les autres groupes vulnérables, accèdent de manière équitable et durable aux opportunités économiques qui favorisent la création d’emplois décents, l’entrepreneuriat, l’innovation et la digitalisation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "155",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 260, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 7: D’ici à 2027, la résilience de l’État et des populations est renforcée à travers la gouvernance durable des ressources naturelles et de l’environnement",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "156",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 28, 260, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 : D’ici 2024, les populations bénéficient d’un système de gouvernance amélioré sur le plan institutionnel, démocratique, des droits humains, administratif et économique pour un développement inclusif et participatif, la consolidation de la paix et de l’effort humanitaire.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "157",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 58, 597, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 : D’ici 2024, les populations bénéficient d’un système de gouvernance amélioré sur le plan institutionnel, démocratique, des droits humains, administratif et économique pour un développement inclusif et participatif, la consolidation de la paix et de l’effort humanitaire.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "157",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 27, 4, 570, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2 : D’ici 2024, les enfants (filles et garçons), les jeunes et adultes (hommes et femmes), les personnes handicapées dans les zones ciblées ont un accès accru aux services éducatifs inclusifs et de qualité dans les systèmes formel et non formel et en particulier l'éducation sexuelle complète (ODD4).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "158",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 58, 597, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2 : D’ici 2024, les enfants (filles et garçons), les jeunes et adultes (hommes et femmes), les personnes handicapées dans les zones ciblées ont un accès accru aux services éducatifs inclusifs et de qualité dans les systèmes formel et non formel et en particulier l'éducation sexuelle complète (ODD4).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "158",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 27, 4, 570, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3 : D’ici 2024, les populations ont un accès équitable à des paquets de soins et services de santé essentiels de qualité (y compris la nutrition, le VIH, l’eau, l’hygiène et l’assainissement), à une sécurité sanitaire, à une protection sociale pour valoriser le capital humain et garantir le dividende démographique (ODD3).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "159",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 58, 597, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3 : D’ici 2024, les populations ont un accès équitable à des paquets de soins et services de santé essentiels de qualité (y compris la nutrition, le VIH, l’eau, l’hygiène et l’assainissement), à une sécurité sanitaire, à une protection sociale pour valoriser le capital humain et garantir le dividende démographique (ODD3).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "159",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 27, 4, 573, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4 : D’ici 2024, les populations les plus défavorisées dans les zones ciblées mettent en œuvre des activités économiques diversifiées durables, créatrices d'emplois divers et de revenus prenant en compte, l’agriculture durable, la sécurité alimentaire, l’écotourisme en respectant les normes environnementales.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "160",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 37, 58, 600, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4 : D’ici 2024, les populations les plus défavorisées dans les zones ciblées mettent en œuvre des activités économiques diversifiées durables, créatrices d'emplois divers et de revenus prenant en compte, l’agriculture durable, la sécurité alimentaire, l’écotourisme en respectant les normes environnementales.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "160",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 27, 4, 573, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 1 : D’ici à 2025, les acteurs du secteur agricole utilisent des systèmes et modes de productions modernisés durables, compétitifs, résilients en vue de garantir la sécurité alimentaire et la transformation de l’économie ivoirienne",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "161",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 1 : D’ici à 2025, les acteurs du secteur agricole utilisent des systèmes et modes de productions modernisés durables, compétitifs, résilients en vue de garantir la sécurité alimentaire et la transformation de l’économie ivoirienne",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "161",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 49, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 2 : D’ici à 2025, les petites et moyennes entreprises/industries accèdent davantage à des systèmes innovants de diversification économique durable et inclusive et à des opportunités de commerce ainsi qu›à des investissements notamment dans le secteur de la transformation manufacturière",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "162",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 2 : D’ici à 2025, les petites et moyennes entreprises/industries accèdent davantage à des systèmes innovants de diversification économique durable et inclusive et à des opportunités de commerce ainsi qu›à des investissements notamment dans le secteur de la transformation manufacturière",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "162",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 0, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 3 : D’ici à 2025, les enfants, les adolescents, les jeunes (filles et garçons) et les adultes, en particulier ceux des ménages vulnérables accèdent à de meilleures opportunités d’éducation, d’alphabétisation fonctionnelle, et de formation inclusive de qualité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "163",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 3 : D’ici à 2025, les enfants, les adolescents, les jeunes (filles et garçons) et les adultes, en particulier ceux des ménages vulnérables accèdent à de meilleures opportunités d’éducation, d’alphabétisation fonctionnelle, et de formation inclusive de qualité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "163",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 0, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 4 : D’ici à 2025, les populations en particulier les plus vulnérables accèdent de façon équitable à un socle minimum de protection sociale et utilisent des services de santé (maternelle, néo-natale et infantile, santé de la reproduction, VIH/sida, de lutte contre les maladies non transmissibles), de nutrition, de protection (travail des enfants, violences), d’eau, d’hygiène et d’assainissement, de qualité y compris dans les situations d’urgence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "164",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 4 : D’ici à 2025, les populations en particulier les plus vulnérables accèdent de façon équitable à un socle minimum de protection sociale et utilisent des services de santé (maternelle, néo-natale et infantile, santé de la reproduction, VIH/sida, de lutte contre les maladies non transmissibles), de nutrition, de protection (travail des enfants, violences), d’eau, d’hygiène et d’assainissement, de qualité y compris dans les situations d’urgence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "164",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 0, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 5 : D’ici à 2025, les jeunes, filles et garçons, particulièrement ceux en situation de vulnérabilité accèdent davantage aux opportunités socioéconomiques et développent leur plein potentiel",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "165",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 5 : D’ici à 2025, les jeunes, filles et garçons, particulièrement ceux en situation de vulnérabilité accèdent davantage aux opportunités socioéconomiques et développent leur plein potentiel",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "165",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 0, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 6 : D’ici à 2025, les femmes, les filles accèdent davantage aux opportunités socio-économiques et technologiques pour leur autonomisation, et aux services de prévention et de prise en charge de toutes les formes de violences notamment les pratiques néfastes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "166",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 6 : D’ici à 2025, les femmes, les filles accèdent davantage aux opportunités socio-économiques et technologiques pour leur autonomisation, et aux services de prévention et de prise en charge de toutes les formes de violences notamment les pratiques néfastes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "166",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 3, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 7 : D’ici à 2025 les communautés accèdent davantage aux écosystèmes terrestres, marins, ainsi qu’à un cadre de vie, gérés de façon plus durable, intégrée, inclusive et améliorent leur résilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "167",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 7 : D’ici à 2025 les communautés accèdent davantage aux écosystèmes terrestres, marins, ainsi qu’à un cadre de vie, gérés de façon plus durable, intégrée, inclusive et améliorent leur résilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "167",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 3, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 8 : D’ici 2025, les systèmes de gouvernance sont plus inclusifs, redevables, efficaces et disposent de données de qualité, et les populations vivent dans un environnement où l’Etat de droit, les droits du travail, l’égalité des sexes, la paix et la sécurité sont respectés et effectifs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "168",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 30, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTAT ESCOMPTE 8 : D’ici 2025, les systèmes de gouvernance sont plus inclusifs, redevables, efficaces et disposent de données de qualité, et les populations vivent dans un environnement où l’Etat de droit, les droits du travail, l’égalité des sexes, la paix et la sécurité sont respectés et effectifs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "168",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 39, 50, 3, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, more people, especially youth, women, and socially and economically vulnerable groups, including refugees and internally displaced persons (IDPs), benefit equitably from increased opportunities in a green, diversified, transformative, resilient, and inclusive economy that creates decent jobs in productive sectors.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "169",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 967, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2026, more people, by age group, especially the most vulnerable, including refugees and IDPs, use quality basic social services equitably and sustainably to realize their full human potential and enhance their social and economic well-being.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "170",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 967, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2026, gaps in key socio-economic indicators are reduced, reflecting greater gender equality and progress in the empowerment of youth, women and girls, and other vulnerable groups, including in humanitarian contexts.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "171",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 967, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, youth, women, the most vulnerable groups, and people living with disabilities, including refugees and IDPs actively contribute to the efficiency of policies and the performance of public institutions at national, regional and council levels, and fully enjoy their rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "172",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 967, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, populations, in different agro-ecological zones, including youth, women and socially vulnerable groups, live in a healthier environment, sustainably manage environmental resources, including biodiversity, and are more resilient to disaster and climate change shocks.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "173",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 39, 49, 967, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Relative poverty and multi-dimensional poverty are reduced, and more coordinated development leads to reduction in gaps between rural and urban areas and among regions, as more people in China, including left-behind groups, benefit from sustainable, innovation-driven and shared high-quality economic development, with enhanced access to economic opportunities arising through innovation, entrepreneurship and rural revitalization, enjoying decent work, sustainable livelihoods and the right to development, equally for both women and men.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "174",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 917, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Relative poverty and multi-dimensional poverty are reduced, and more coordinated development leads to reduction in gaps between rural and urban areas and among regions, as more people in China, including left-behind groups, benefit from sustainable, innovation-driven and shared high-quality economic development, with enhanced access to economic opportunities arising through innovation, entrepreneurship and rural revitalization, enjoying decent work, sustainable livelihoods and the right to development, equally for both women and men.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "174",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 890, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: People’s lives in China are improved further as headway is made in ensuring access to childcare, education, healthcare services, elderly care, housing and social assistance, and more people in China, including left-behind groups, benefit from equitable and high-quality public services and social protection systems as well as accelerated efforts to reduce gender inequality and other forms of social inequality throughout the life-course.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "175",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: People’s lives in China are improved further as headway is made in ensuring access to childcare, education, healthcare services, elderly care, housing and social assistance, and more people in China, including left-behind groups, benefit from equitable and high-quality public services and social protection systems as well as accelerated efforts to reduce gender inequality and other forms of social inequality throughout the life-course.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "175",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 890, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: People in China and the region benefit from a healthier and more resilient environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "176",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: People in China and the region benefit from a healthier and more resilient environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "176",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 890, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: China accelerates its transition to a people-centred, inclusive, low carbon, and circular economy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "177",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: China accelerates its transition to a people-centred, inclusive, low carbon, and circular economy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "177",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 890, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: China’s international financing, investments and business engagements, including through connectivity initiatives, programmes and projects, contribute to SDG attainment in partner countries.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "178",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: China’s international financing, investments and business engagements, including through connectivity initiatives, programmes and projects, contribute to SDG attainment in partner countries.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "178",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 893, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: Through South-South cooperation and humanitarian cooperation, China makes greater contributions to SDG attainment and the principles of the 2030 Agenda, including leaving no one behind.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "179",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 18, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: Through South-South cooperation and humanitarian cooperation, China makes greater contributions to SDG attainment and the principles of the 2030 Agenda, including leaving no one behind.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "179",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 45, 39, 893, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 1.1: PERSONAS EMPODERADAS QUE CONOCEN SUS DERECHOS - Al 2027, todas las personas, en particular las mujeres en su diversidad y las personas en condición de vulnerabilidad son agentes de cambio que se reconocen como sujetas de derechos, participan y lideran la transformación de las normas sociales y de género e inciden de manera más efectiva en la exigibilidad y el ejercicio pleno de sus derechos humanos, con un enfoque particular en las comunidades que se están quedando atrás en el desarrollo para revertir las inequidades históricas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "180",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 1.2: MUJERES, NIÑAS Y JÓVENES AL CENTRO - Al 2027, las mujeres, las niñas y jóvenes en su diversidad están al centro del desarrollo, lideran su propia participación plena y sus voces son escuchadas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "181",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 1.3: PROTECCIÓN DE LA MOVILIDAD HUMANA - Al 2027, las personas en situación de movilidad humana, y sus organizaciones, ejercen sus derechos en ambientes libres de discriminación, integrados de forma plena en las comunidades de acogida y con acceso a oportunidades de desarrollo dignas y seguras.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "182",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2.1: SERVICIOS PÚBLICOS EFICIENTES, SOSTENIBLES Y DE CALIDAD - Al 2027, se transforman y modernizan las instituciones para brindar servicios públicos centrados en las personas y sus derechos, de calidad, sin discriminación, inclusivos, innovadores, efectivos, eficientes, oportunos y flexibles, sostenibles, transformadores de género, articulados con otras instituciones y con un fuerte enfoque territorial, permitiendo que los gobiernos locales se conviertan en los principales agentes de cambio del desarrollo local, especialmente en territorios con menores índices de desarrollo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "183",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2.2: GESTIÓN POR RESULTADOS Y FINANCIAMIENTO PARA EL DESARROLLO - Al 2027, el Estado planifica y presupuesta por resultados, con enfoques de género interseccional y derechos humanos, y consolida las alianzas entre el sector público y el privado para catalizar mecanismos de financiación innovadora que aceleren la Agenda 2030.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "184",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2.3: TRANSPARENCIA, RENDICIÓN DE CUENTAS Y ANTICORRUPCIÓN - Al 2027, se afianza el Estado de Derecho y garantiza la transparencia, la rendición de cuentas y la lucha contra la corrupción, con mayor y mejor participación de la sociedad civil.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "185",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 3.1: TRABAJO DECENTE Y CRECIMIENTO ECONÓMICO - Al 2027, el Estado acelera la creación de trabajo decente y los motores de crecimiento de la economía creativa, circular, resiliente, sostenible, inclusiva y hay una inversión mayor y más equitativa en las personas, eliminando las barreras y creando capacidades y oportunidades para la producción y el financiamiento inclusivo y sostenible, principalmente en los territorios costeros, fronterizos y rurales.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "186",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 110, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 3.2: ECONOMÍA INCLUSIVA - Al 2027, las mujeres en su diversidad y las poblaciones en condición de vulnerabilidad participan y se benefician de una economía innovadora, inclusiva, que potencia sus oportunidades de trabajo decente y el emprendedurismo, con acceso a mecanismos de financiamiento con mejores condiciones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "187",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 3.3: INNOVACIÓN Y COMPETITIVIDAD - Al 2027, el país promueve un ecosistema de innovación a escala nacional y local, priorizando las pequeñas y medianas empresas, y el desarrollo de infraestructura estratégica sostenible, resiliente y orientada a la acción climática, que contribuya a un crecimiento inclusivo, la recuperación efectiva de la pandemia y una mayor competitividad con base en una estrategia de asociatividad multinivel y un abordaje de reducción de brechas tecnológicas en territorios costeros, fronterizos y zonas rurales.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "188",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 4.1: REDUCCIÓN DEL RIESGO ANTE EVENTOS - Al 2027, el país cuenta con escenarios futuros, un sistema nacional y multisectorial de gestión del riesgo y un sistema integrado de alerta temprana, con enfoques de derechos humanos, género e interseccional.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "189",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 4.2: ADAPTACIÓN Y MITIGACIÓN FRENTE AL CAMBIO CLIMÁTICO - Al 2027, las personas, las comunidades y las instituciones mitigan y se adaptan al cambio climático y usan de manera sostenible y eficiente los recursos ecosistémicos, conservan e impiden la degradación de la naturaleza y frenan la contaminación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "190",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 4.3: RESPUESTA INTEGRADA A LAS CRISIS SISTÉMICAS TRANSFRONTERIZAS - Al 2027, las instituciones y las personas, en especial las mujeres en su diversidad y poblaciones en condición de vulnerabilidad gozan de espacios seguros, previenen y responden de manera integrada a las múltiples y complejas crisis sistémicas de alcance global y local que se retroalimentan entre sí, exacerbando sus impactos negativos en las personas y el planeta.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "191",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 40, 46, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - Gobiernos e instituciones nacionales y locales implementan estrategias de desarrollo integral territorial sostenibles, equitativas e inclusivas en el marco del proceso de descentralización.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "192",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 41, 10, 100, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - Sectores estratégicos de prioridad nacional logran niveles más elevados de productividad económica y de aprovechamiento del potencial humano mediante la diversificación, la modernización tecnológica y la innovación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "193",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 41, 10, 100, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - Instituciones, sectores productivos y de servicios, gobiernos territoriales y comunidades mejoran la protección y uso racional de los recursos naturales y de los ecosistemas, la resiliencia al cambio climático y la gestión integral de reducción de riesgos de desastre.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "194",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 41, 10, 100, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1: En 2026, las personas, considerando su edad, sexo, identidad de género, autoidentificación étnica y diversidad, en particular aquellas en situación de vulnerabilidad y en contextos de emergencia, incrementan su acceso igualitario y equitativo a la protección social y a servicios sociales de calidad, incluyendo alimentación, salud, educación, agua, saneamiento e higiene, vivienda, cuidados y cultura.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "195",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 19, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2: En 2026, el Estado y la sociedad avanzan hacia la transición ecológica y hacia una economía sostenible e inclusiva, descarbonizada y resiliente ante los efectos del cambio climático, conservando la biodiversidad, evitando la degradación de tierras y la contaminación de los ecosistemas, con enfoque de género, inclusión y diversidades",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "196",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 19, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3: En 2026, el Estado y la sociedad han reducido las desigualdades socioeconómicas y han promovido la transformación productiva sostenible y con valor agregado, la generación de medios de vida y trabajo decente, garantizando la igualdad de derechos y oportunidades, y el acceso de mujeres y hombres a los recursos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "197",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 19, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 4: En 2026, el Estado mejora la gestión pública e incrementa la protección y garantía de derechos, la igualdad de género y la cohesión social, mientras reduce las amenazas a la seguridad humana y promueve la erradicación de todas las formas de violencia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "198",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 19, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, more people have benefitted from equitable access to and use of inclusive and quality essential social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "199",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ER",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 48, 337, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2026, Eritrea’s public sector institutions are more accountable and efficient, and more people enjoy the right to development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "200",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ER",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 48, 337, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3 By 2026, people in Eritrea, especially the disadvantaged population, have increased livelihoods, as economic growth becomes more inclusive and diversified",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "201",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ER",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 48, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4 By 2026, people in Eritrea have benefited from climate resilient, sustainable environment and natural resources management",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "202",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ER",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 42, 48, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 All people in Ethiopia enjoy the rights and capabilities to realize their potential in equality and with dignity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "203",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 43, 14, 60, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 All people in Ethiopia enjoy the rights and capabilities to realize their potential in equality and with dignity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "203",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 21, 18, 870, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2 All people in Ethiopia live in a cohesive, just, inclusive and democratic society.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "204",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 43, 14, 60, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2 All people in Ethiopia live in a cohesive, just, inclusive and democratic society.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "204",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 21, 18, 870, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 All people in Ethiopia benefit from an inclusive, resilient and sustainable economy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "205",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 43, 14, 60, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 All people in Ethiopia benefit from an inclusive, resilient and sustainable economy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "205",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 21, 18, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4 All people in Ethiopia live in a society resilient to environmental risks and adapted to climate change.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "206",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 43, 14, 60, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4 All people in Ethiopia live in a society resilient to environmental risks and adapted to climate change.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "206",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ET",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 21, 18, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. D’ici 2027, le cadre légal et institutionnel est renforcé et les institutions sont plus redevables, transparentes et efficaces en vue d’assurer une gouvernance orthodoxe et inclusive et un développement durable et équitable, en ligne avec les engagements nationaux et internationaux pris par le pays. (Tous les 17 ODD).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "207",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 46, 30, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. D’ici 2027, les populations, notamment les jeunes et les femmes, participent à la gestion durable des ressources naturelles et bénéficient des fruits d’une transition maîtrisée vers l’économie verte et bleue dans un environnement plus résilient aux changements climatiques. (ODD 1, 5, 6, 8, 9, 10, 11, 12, 13, 14, 15, 17).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "208",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 46, 30, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. D’ici 2027, les populations, notamment les plus vulnérables bénéficient d’accès aux services sociaux de qualité, intégrés, résilients et inclusifs et les inégalités sociales et de genre sont réduites. (1, 2, 3, 4, 5, 6, 7, 8, 10, 16,17).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "209",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 46, 30, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2025, all people in Georgia enjoy improved good governance, more open, resilient and accountable institutions, rule of law, equal access to justice, human rights , and increased representation and participation of women in decision making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "210",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2025, all people in Georgia enjoy improved good governance, more open, resilient and accountable institutions, rule of law, equal access to justice, human rights , and increased representation and participation of women in decision making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "210",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 837, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2025, all people in Georgia have equitable and inclusive access to quality, resilient and gender-sensitive services delivered in accordance with international human rights standards",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "211",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2025, all people in Georgia have equitable and inclusive access to quality, resilient and gender-sensitive services delivered in accordance with international human rights standards",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "211",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2025, all people without discrimination benefit from a sustainable, inclusive and resilient economy in Georgia",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "212",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2025, all people without discrimination benefit from a sustainable, inclusive and resilient economy in Georgia",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "212",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2025, conflict affected communities enjoy human rights, enhanced human security and resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "213",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2025, conflict affected communities enjoy human rights, enhanced human security and resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "213",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2025, all people, without discrimination, enjoy enhanced resilience through improved environmental governance, climate action and sustainable management and use of natural resources in Georgia",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "214",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 3, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2025, all people, without discrimination, enjoy enhanced resilience through improved environmental governance, climate action and sustainable management and use of natural resources in Georgia",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "214",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 28, 45, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 1.1. TRABAJO DIGNO Y DECENTE, MEDIOS PRODUCTIVOS, SERVICIOS ECONÓMICOS, Y COMPETITIVIDAD.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "215",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.1. ACCESO A LA VIVIENDA DIGNA / ADECUADA, A LOS SERVICIOS BASICOS Y ORDENAMIENTO TERRITORIAL.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "216",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.5. SEGURIDAD ALIMENTARIA Y NUTRICIÓN.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "217",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 3.1. SEGURIDAD CIUDADANA, ACCESO A LA JUSTICIA, Y TRANSFORMACIÓN DE CONFLICTOS.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "218",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 3.2. ACCESO A JUSTICIA, REPARACIÓN, PROTECCIÓN Y PREVENCIÓN DE LA VIOLENCIA.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "219",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 4.1. GOBERNANZA DEMOCRÁTICA, GESTIÓN EFICIENTE Y TRANSPARENTE DE RECURSOS, Y TOMA DE DECISIONES BASADA EN EVIDENCIA.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "220",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 4.2. ASISTENCIA Y PROTECCIÓN A PERSONAS QUE MIGRAN, RETORNAN, TRANSITAN O SE DESPLAZAN FORZOSAMENTE DENTRO DEL PAÍS O FUERA DE SUS FRONTERAS.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "221",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 4.3. MAYOR ACCESO A ESPACIOS DE PARTICIPACIÓN POLÍTICA Y CÍVICA A NIVEL NACIONAL Y LOCAL.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "222",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 5.1. CAMBIO CLIMÁTICO, GOBERNANZA, Y MANEJO SOSTENIBLE DE RECURSOS NATURALES.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "223",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 34, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, people in Guinea-Bissau enjoy improved  democratic governance, peace and rule of law and  their basic needs are met",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "224",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 59, 163, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, people in Guinea-Bissau enjoy improved  democratic governance, peace and rule of law and  their basic needs are met",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "224",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 51, 28, 457, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, Guinea-Bissau has achieved structural economic transformation driven by enhanced productive capacity, value addition, blue economy and inclusive green growth that leaves no one behind, while capitalizing on SIDS characteristics and ensuring sustainable use and protection of natural resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "225",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 59, 167, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, Guinea-Bissau has achieved structural economic transformation driven by enhanced productive capacity, value addition, blue economy and inclusive green growth that leaves no one behind, while capitalizing on SIDS characteristics and ensuring sustainable use and protection of natural resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "225",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 51, 28, 457, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, the population of Guinea-Bissau especially the most vulnerable, will have increased and equitable access and use of essential quality social services, including in emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "226",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 48, 59, 167, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, the population of Guinea-Bissau especially the most vulnerable, will have increased and equitable access and use of essential quality social services, including in emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "226",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 51, 28, 460, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: More productive and competitive business ecosystem designed to improve people’s standards of living",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "227",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 300, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: The Caribbean has fully transitioned to a more diversified and sustainable economy that supports inclusive and resilient economic growth",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "228",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 300, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: [DATA AND LAWS] National governments and regional institutions use relevant data to design and adopt laws and policies to eliminate discrimination, address structural inequalities and ensure the advancement of those left furthest behind",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "229",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: [Enhanced service delivery for inclusion & One-ness] People in the Caribbean equitably access and utilize universal, quality and shock-responsive, social protection, education, health, and care services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "230",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: Caribbean people, communities, and institutions enhance their adaptive capacity for inclusive, gender responsive DRM and climate change adaptation and mitigation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "231",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: Caribbean countries manage natural resources and ecosystems to strengthen their resilience and enhance the resilience and prosperity of the people and communities that depend on them.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "232",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 7: Regional and national laws, policies, systems, and institutions improve access to justice and promote peace, social cohesion, and security",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "233",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 8: People in the Caribbean and communities actively contribute to and benefit from building and maintaining safer, fairer, more inclusive, and equitable societies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "234",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 30, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.1 El Estado Hondureño implementa políticas, estrategias y programas que fortalecen el Estado de Derecho, la transparencia, la rendición de cuentas, la lucha contra la corrupción y la impunidad, con una mayor participación de la sociedad civil y el sector privado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "235",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 710, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.1 El Estado Hondureño implementa políticas, estrategias y programas que fortalecen el Estado de Derecho, la transparencia, la rendición de cuentas, la lucha contra la corrupción y la impunidad, con una mayor participación de la sociedad civil y el sector privado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "235",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 290, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.2: La sociedad civil, en especial los grupos en situación de exclusión, participa e incide activamente en pro del ejercicio de sus derechos, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "236",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 713, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.2: La sociedad civil, en especial los grupos en situación de exclusión, participa e incide activamente en pro del ejercicio de sus derechos, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "236",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 290, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.3:  El Estado se erige como una administración pública eficaz y eficiente, adaptada a las necesidades de la población, y que aplica un enfoque territorial, así como enfoques de género y de derechos humanos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "237",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 713, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1.3:  El Estado se erige como una administración pública eficaz y eficiente, adaptada a las necesidades de la población, y que aplica un enfoque territorial, así como enfoques de género y de derechos humanos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "237",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 293, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.1:  La población hondureña y sus instituciones emprenden una transformación digital y tecnológica a través de la innovación, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "238",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 713, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.1:  La población hondureña y sus instituciones emprenden una transformación digital y tecnológica a través de la innovación, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "238",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 293, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.2: El Estado y la sociedad hondureña implementan políticas, estrategias y programas que permiten transitar hacia una transformación económica adecuada e inclusiva y el trabajo decente para mujeres y hombres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "239",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 713, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.2: El Estado y la sociedad hondureña implementan políticas, estrategias y programas que permiten transitar hacia una transformación económica adecuada e inclusiva y el trabajo decente para mujeres y hombres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "239",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 293, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.3:  El Estado Hondureño implementa políticas, estrategias y programas que permiten fortalecer la sostenibilidad y resiliencia de su desarrollo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "240",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 717, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.3:  El Estado Hondureño implementa políticas, estrategias y programas que permiten fortalecer la sostenibilidad y resiliencia de su desarrollo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "240",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 297, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.1:  El Estado y la población hondureña reducen la violencia y la conflictividad que les afecta, con particular atención a la violencia contra las mujeres y los jóvenes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "241",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 717, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.1:  El Estado y la población hondureña reducen la violencia y la conflictividad que les afecta, con particular atención a la violencia contra las mujeres y los jóvenes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "241",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 297, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.2: El Estado Hondureño implementa políticas públicas, estrategias y programas, a nivel local y nacional, que promueven la Igualdad de Género y el empoderamiento de las Mujeres y niñas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "242",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 717, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.2: El Estado Hondureño implementa políticas públicas, estrategias y programas, a nivel local y nacional, que promueven la Igualdad de Género y el empoderamiento de las Mujeres y niñas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "242",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 297, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.3: La población hondureña, en especial aquella excluida, ejerce plenamente sus derechos económicos, sociales, culturales, ambientales, civiles y políticos, y se ve beneficiada por una mayor equidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "243",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 49, 58, 717, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.3: La población hondureña, en especial aquella excluida, ejerce plenamente sus derechos económicos, sociales, culturales, ambientales, civiles y políticos, y se ve beneficiada por una mayor equidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "243",
                UNCooperationFrameworkVersionNo = 2,
                Country = "HN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 54, 17, 297, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - People living in Indonesia, especially those at risk of being left furthest behind, are empowered to fulfil their human development potential as members of a pluralistic, tolerant, inclusive, and just society, free of gender and all other forms of discrimination",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "244",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 31, 543, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - People living in Indonesia, especially those at risk of being left furthest behind, are empowered to fulfil their human development potential as members of a pluralistic, tolerant, inclusive, and just society, free of gender and all other forms of discrimination",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "244",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 57, 14, 287, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - Institutions and people contribute more effectively to advance a higher value-added and inclusive economic transformation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "245",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 31, 543, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - Institutions and people contribute more effectively to advance a higher value-added and inclusive economic transformation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "245",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 57, 14, 290, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - Institutions, communities and people actively apply and implement low carbon development, sustainable natural resources management and disaster resilience approaches that are all gender sensitive.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "246",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 31, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - Institutions, communities and people actively apply and implement low carbon development, sustainable natural resources management and disaster resilience approaches that are all gender sensitive.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "246",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 57, 14, 290, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 - Stakeholders adopt innovative and integrated development solutions to accelerate advancement towards the SDGs.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "247",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 31, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 - Stakeholders adopt innovative and integrated development solutions to accelerate advancement towards the SDGs.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "247",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ID",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 57, 14, 293, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 : Strengthened and effective inclusive, people-centred, gender-responsive and human rights based policies and national systems contribute to gender equality, the promotion of protection, Social Protection, social cohesion and peaceful societies, with focus on the most vulnerable populations, including women, youth and minorities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "248",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 : Strengthened and effective inclusive, people-centred, gender-responsive and human rights based policies and national systems contribute to gender equality, the promotion of protection, Social Protection, social cohesion and peaceful societies, with focus on the most vulnerable populations, including women, youth and minorities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "248",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 397, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2 : People in Iraq, particularly under-served, marginalized and vulnerable populations, have equitable and sustainable access to quality gender- and age-responsive protection and social protection systems and services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "249",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2 : People in Iraq, particularly under-served, marginalized and vulnerable populations, have equitable and sustainable access to quality gender- and age-responsive protection and social protection systems and services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "249",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3 : People in Iraq participate in and benefit from effective mechanisms – at national, subnational and community levels – that prevent, mitigate and manage conflict, and contribute to social cohesion and peaceful coexistence, with particular focus on women and youth leadership in decision-making, peace-building and reconciliation processes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "250",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3 : People in Iraq participate in and benefit from effective mechanisms – at national, subnational and community levels – that prevent, mitigate and manage conflict, and contribute to social cohesion and peaceful coexistence, with particular focus on women and youth leadership in decision-making, peace-building and reconciliation processes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "250",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 : Improved people-centred economic policies and legislation contribute to inclusive, gender sensitive and diversified economic growth, with focus on increasing income security and decent work for women, youth and vulnerable populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "251",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 : Improved people-centred economic policies and legislation contribute to inclusive, gender sensitive and diversified economic growth, with focus on increasing income security and decent work for women, youth and vulnerable populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "251",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2 : People in Iraq have strengthened capacity, enabling inclusive access to and engagement in economic activities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "252",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2 : People in Iraq have strengthened capacity, enabling inclusive access to and engagement in economic activities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "252",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 : Strengthened institutions and systems deliver people-centred, evidence and needs-based equitable and inclusive gender- and age-responsive services, especially for the most vulnerable populations, with particular focus on advocating for women’s leadership in decision-making processes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "253",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 : Strengthened institutions and systems deliver people-centred, evidence and needs-based equitable and inclusive gender- and age-responsive services, especially for the most vulnerable populations, with particular focus on advocating for women’s leadership in decision-making processes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "253",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 403, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 : People in Iraq, civil society and communities, particularly women, have improved capacity to lead, participate in and contribute to the design and delivery of equitable and responsive services, especially for the most vulnerable populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "254",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 : People in Iraq, civil society and communities, particularly women, have improved capacity to lead, participate in and contribute to the design and delivery of equitable and responsive services, especially for the most vulnerable populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "254",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 403, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 : Strengthened and resourced policies and frameworks are implemented for managing natural resources (including trans-boundary issues), developing renewable resources, and increasing resilience to climate change, environmental stress and natural hazards, and man-made and natural disasters",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "255",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 : Strengthened and resourced policies and frameworks are implemented for managing natural resources (including trans-boundary issues), developing renewable resources, and increasing resilience to climate change, environmental stress and natural hazards, and man-made and natural disasters",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "255",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 403, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2 : Increased engagement of the people of Iraq, sub-national institutions, civil society, and private sector to ensure more responsible, inclusive, accountable and transparent management of natural resources and the environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "256",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 667, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2 : Increased engagement of the people of Iraq, sub-national institutions, civil society, and private sector to ensure more responsible, inclusive, accountable and transparent management of natural resources and the environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "256",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 403, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5.1 : Strengthened stabilization, development and peacebuilding initiatives support area-based interventions in locations of displacement, return or relocation to enhance the achievement of voluntary, safe and dignified durable solutions for displacement-affected populations. or displacement-affected populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "257",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 667, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5.1 : Strengthened stabilization, development and peacebuilding initiatives support area-based interventions in locations of displacement, return or relocation to enhance the achievement of voluntary, safe and dignified durable solutions for displacement-affected populations. or displacement-affected populations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "257",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 407, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5.2 : National and local authorities are supported to lead the development and implementation of effective and inclusive policies, strategies and plans to achieve durable solutions to displacement in Iraq for all displacement-affected people, including through effective coordination mechanisms and data collection to support evidence-based outcomes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "258",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 51, 55, 667, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5.2 : National and local authorities are supported to lead the development and implementation of effective and inclusive policies, strategies and plans to achieve durable solutions to displacement in Iraq for all displacement-affected people, including through effective coordination mechanisms and data collection to support evidence-based outcomes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "258",
                UNCooperationFrameworkVersionNo = 2,
                Country = "IQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 1, 26, 407, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, people of Iran enjoy a shock responsive socio-economic development and sustainable growth integrated into development policies and programmes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "259",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 883, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, people of Iran benefit from enhanced health care and social services and enjoy healthier lifestyles.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "260",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 887, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, environmental conservation and integrated natural resource management are enhanced and the capacity to address climate change challenges is strengthened.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "261",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 887, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, the national and local resilience to disaster impacts is enhanced by improving disaster risk reduction, preparedness, response and recovery.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "262",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 887, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2027, more people of Iran are protected from drug use, and the capacity for effective border management and countering illicit trafficking are enhanced.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "263",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 20, 887, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2026, people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – inhabit an inclusive, enabling, socially cohesive, and peaceful society where human rights are upheld, and benefit from accountable institutions and participate in transformative governance systems that are gender responsive and uphold the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "264",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 547, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2026, people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – inhabit an inclusive, enabling, socially cohesive, and peaceful society where human rights are upheld, and benefit from accountable institutions and participate in transformative governance systems that are gender responsive and uphold the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "264",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2026, people in Kenya at risk of being left – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – have improved, inclusive and equitable social and protection services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "265",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 547, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2026, people in Kenya at risk of being left – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – have improved, inclusive and equitable social and protection services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "265",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2026, people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – derive benefit from inclusive, sustainable, diversified and environmentally/climate-sensitive quality livelihoods with decent work in the sector economies and realize growth that is resilient, green, and equitable",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "266",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 547, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2026, people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements – derive benefit from inclusive, sustainable, diversified and environmentally/climate-sensitive quality livelihoods with decent work in the sector economies and realize growth that is resilient, green, and equitable",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "266",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2026, people in Kenya at risk of being left behind - particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements - have access to and derive benefit from sustainably managed ecosystems for nature-based solutions in a green transition",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "267",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2026, people in Kenya at risk of being left behind - particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements - have access to and derive benefit from sustainably managed ecosystems for nature-based solutions in a green transition",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "267",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 260, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2026, Kenya’s path to achieving SDGs benefits from effective multiple stakeholder partnerships to drive a greater amount and diversity of public, private and community collaboration as well as financing and investments that accelerate sustainable development for people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "268",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 52, 49, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2026, Kenya’s path to achieving SDGs benefits from effective multiple stakeholder partnerships to drive a greater amount and diversity of public, private and community collaboration as well as financing and investments that accelerate sustainable development for people in Kenya at risk of being left behind – particularly all women and girls, all children and youth, all people in the ASAL counties and in informal urban settlements.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "268",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 34, 21, 260, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, the people of the Kyrgyz Republic, particularly vulnerable groups, have enhanced resilience, strengthened capabilities, and access to decent work, resulting in full enjoyment of their rights contributing to the socio-economic and gendertransformative development of the country.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "269",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 9, 723, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, the well-being of the population of the Kyrgyz Republic will have improved through the further rollout of a green economy based on sustainable and healthy food systems natural resource management, and effective migration processes, by accelerating the use of gender transformative social and technological innovations and entrepreneurship",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "270",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 9, 727, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, the Kyrgyz Republic has started the transition to low-carbon development and risk-informed climate resilience, contributing to people’s fair and equitable access to ecosystem benefits and to empowerment of vulnerable communities in the governance of natural resources and disaster prevention.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "271",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 9, 727, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, all people in the Kyrgyz Republic enjoy the benefits of fair and accountable democratic institutions that are free from corruption and apply innovative solutions that promote respect for human rights, and strengthen peace and cohesion.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "272",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 9, 727, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, state and non-state actors and the population of the Comoros, in particular the most vulnerable, will have strengthened their resilience to climate change, natural disasters and crises and will ensure the sustainable and integrated management of terrestrial and marine ecosystems and ecosystem goods and services, in a context in which sustainable housing with a small environmental footprint is promoted.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "273",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 34, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, the population of the Comoros, in particular the most vulnerable, will enjoy shared prosperity built upon a more competitive and inclusive economy and rejuvenated public-private partnerships, within a sustainable growth approach that focuses on sectors with the greatest future potential (green, blue and digital)",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "274",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 34, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, the population of the Comoros, in particular the most vulnerable, will be benefiting from the demographic dividend and making better use of appropriate, inclusive, equitable, gender-sensitive, permanently available, high-quality services, including for nutrition, education, social protection and assistance for the victims of violence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "275",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 34, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, public institutions will be more inclusive, efficient, accountable and resilient, strengthening the participation of citizens in public life as well as social cohesion, human rights, gender equality and democracy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "276",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 53, 34, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2025, effective, inclusive and accountable institutions ensure equal access for all people living in Kazakhstan, especially most vulnerable, to quality and gender sensitive social services according to the principle of leaving no one behind",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "277",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 227, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2025, effective, inclusive and accountable institutions ensure equal access for all people living in Kazakhstan, especially most vulnerable, to quality and gender sensitive social services according to the principle of leaving no one behind",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "277",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2. By 2025 all people in Kazakhstan especially the most vulnerable are empowered with knowledge and skills to equally contribute to sustainable development of the country.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "278",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 227, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2. By 2025 all people in Kazakhstan especially the most vulnerable are empowered with knowledge and skills to equally contribute to sustainable development of the country.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "278",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2025, all people in Kazakhstan are protected and enjoy full realization of human rights and gender equality and a life free from discrimination, violence and threats, and equally participate in decision making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "279",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 227, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2025, all people in Kazakhstan are protected and enjoy full realization of human rights and gender equality and a life free from discrimination, violence and threats, and equally participate in decision making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "279",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2025, state institutions at all levels effectively design and implement gender-sensitive, human rights and evidence-based public policies and provide quality services in an inclusive, transparent and accountable manner.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "280",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 227, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2025, state institutions at all levels effectively design and implement gender-sensitive, human rights and evidence-based public policies and provide quality services in an inclusive, transparent and accountable manner.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "280",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 993, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2025, all people in Kazakhstan, especially the most vulnerable, benefit from inclusive, resilient, and sustainable economic development with improved productive capacities, skills and equal opportunities for sustainable and decent jobs, livelihoods, and businesses.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "281",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 230, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2025, all people in Kazakhstan, especially the most vulnerable, benefit from inclusive, resilient, and sustainable economic development with improved productive capacities, skills and equal opportunities for sustainable and decent jobs, livelihoods, and businesses.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "281",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 993, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2: By 2025, all people in Kazakhstan, in particular most vulnerable, benefit from increased climate resilience, sustainable management of environment and clean energy, and sustainable rural and urban development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "282",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 12, 230, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2: By 2025, all people in Kazakhstan, in particular most vulnerable, benefit from increased climate resilience, sustainable management of environment and clean energy, and sustainable rural and urban development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "282",
                UNCooperationFrameworkVersionNo = 2,
                Country = "KZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 31, 12, 993, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1 People's Wellbeing",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "283",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 36, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2 Inclusive Prosperity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "284",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 36, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3 Governance and Rule of Law",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "285",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 36, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4 Environment, Climate Change, and Resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "286",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 36, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Enhanced inclusive, equitable, comprehensive, and sustainable social protection systems and programmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "287",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 123, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Strengthened provision of and equitable access to quality services including basic services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "288",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 123, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Enhanced protection for the most vulnerable",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "289",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 123, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Enhanced competitiveness and business environment of MSMEs and high potential productive sectors' values chains.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "290",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 127, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: Strengthened diversified income opportunities to promote social and economic inclusion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "291",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 127, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: Strengthened inclusive social contract grounded in human rights to enhance good governance, effective and accountable institutions, and women's participation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "292",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 127, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 7: Strengthened security, stability, justice, and social peace",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "293",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 130, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 8: Strengthened stabilization and green recovery to reduce vulnerabilities and environmental risks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "294",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 54, 59, 130, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, more people in Sri Lanka, particularly the most vulnerable, access and benefit from equitable, resilient and genderresponsive quality social services and with  enhanced well-being and dignity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "295",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, more people in Sri Lanka, particularly youth and the most vulnerable, have equitable, decent, just work and income opportunities, and benefit from and contribute to inclusive, gendertransformative, resilient and green-led economic recovery, growth and diversification.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "296",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, people and communities in Sri Lanka, especially the vulnerable and marginalised, are more resilient to climate change and disaster risks, have enhanced water and food security, and equitably benefit from ambitious climate action and increasingly sustainable management and protection of the environment and natural resources.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "297",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, people in Sri Lanka, particularly the most vulnerable, have increased trust and confidence to claim and benefit from enhanced, non-discriminatory, genderresponsive, participatory and efficient governance and justice systems and rights-based development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "298",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2027, people in Sri Lanka, particularly the most vulnerable, have increased trust and confidence to claim and benefit from enhanced, non-discriminatory, genderresponsive, participatory and efficient governance and justice systems and rights-based development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "299",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: By 2027, women and girls in Sri Lanka enjoy and are empowered to exercise their full rights, representation, and agency over all aspects of their lives, and live free from discrimination and violence.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "300",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 21, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2024, the most vulnerable and excluded groups have improved quality of life with rights-based, gender-sensitive, inclusive, equitable access and utilization of essential social services  in an environment free of discrimination and violence including in humanitarian situations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "301",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 46, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2024, the most vulnerable and excluded groups have improved quality of life with rights-based, gender-sensitive, inclusive, equitable access and utilization of essential social services  in an environment free of discrimination and violence including in humanitarian situations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "301",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 6, 23, 743, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2024, Liberia has sustained, diversified and inclusive economic growth driven by investments in agriculture, food security and job creation and is resilient to climate change and natural disasters.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "302",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 46, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2024, Liberia has sustained, diversified and inclusive economic growth driven by investments in agriculture, food security and job creation and is resilient to climate change and natural disasters.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "302",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 6, 23, 743, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2024, Liberia consolidates, sustains peace and enhances social cohesion, has strengthened formal and informal institutions capable of providing access to inclusive, effective, equitable justice and security services, capable of promoting and protecting the human rights of all.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "303",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 46, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2024, Liberia consolidates, sustains peace and enhances social cohesion, has strengthened formal and informal institutions capable of providing access to inclusive, effective, equitable justice and security services, capable of promoting and protecting the human rights of all.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "303",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 6, 23, 743, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2024, people in Liberia especially the vulnerable and disadvantaged, benefit from strengthened institutions that are more effective, accountable, transparent, inclusive and gender responsive in the delivery of essential services at the national and sub-national levels.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "304",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 55, 46, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2024, people in Liberia especially the vulnerable and disadvantaged, benefit from strengthened institutions that are more effective, accountable, transparent, inclusive and gender responsive in the delivery of essential services at the national and sub-national levels.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "304",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 6, 23, 747, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2023, government and non-Governmental institutions deliver their mandates and uphold good Governance, rule of law and human rights, with all people having improved access to Justice and participating in social and political decision making processes in a peaceful environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "305",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 15, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2023, government and non-Governmental institutions deliver their mandates and uphold good Governance, rule of law and human rights, with all people having improved access to Justice and participating in social and political decision making processes in a peaceful environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "305",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 4, 5, 440, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 By 2023, all people, particularly the most vulnerable benefit from gender responsive social policies and programmes for the sustainable and equitable realization of their rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "306",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 15, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 By 2023, all people, particularly the most vulnerable benefit from gender responsive social policies and programmes for the sustainable and equitable realization of their rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "306",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 4, 5, 440, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 By 2023, government and private sector increase opportunities for inclusive and sustainable economic growth, improved food security and decent work especially for women, youth and people with disabilities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "307",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 15, 113, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 By 2023, government and private sector increase opportunities for inclusive and sustainable economic growth, improved food security and decent work especially for women, youth and people with disabilities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "307",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 4, 5, 443, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 By 2023, the people of Lesotho use natural resources in a more sustainable manner and the marginalized and most vulnerable are increasingly resilient",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "308",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 15, 117, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 By 2023, the people of Lesotho use natural resources in a more sustainable manner and the marginalized and most vulnerable are increasingly resilient",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "308",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 4, 5, 443, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "COLLECTIVE OUTCOME 1: By 2025, 80 per cent of IDPs and returnees will have achieved a durable solution in harmony and with full respect of the rights of communities hosting or receiving them.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "309",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "COLLECTIVE OUTCOME 2.1: By 2025, 65 per cent migrants and persons of concern have improved protection, safety, and living conditions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "310",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1.1: By 2025, Libyan citizens, particularly youth and women, are better able to exercise their rights and obligations in an inclusive, stable, democratic, and reconciled society, underpinned by responsive, transparent, accountable, and unified public institutions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "311",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1.2: By 2025, people in Libya participate in and benefit from a more peaceful, safe, and secure society, free from armed conflict and underpinned by unified and strengthened security, justice, rule of law, and human rights institutions that promote and protect human rights based on the principles of inclusivity, non-discrimination, and equality in accordance with international norms and standards.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "312",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2.1: By 2025, people in Libya, including the most vulnerable and marginalized, benefit from inclusive, transformative, and sustainable socio-economic opportunities, contributing to reduced poverty and inequalities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "313",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3.1: By 2025, people in Libya, including the most vulnerable and marginalized, benefit from improved, equitable, inclusive, and sustainable social protection and basic social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "314",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4.1: By 2025, people in Libya, including the most vulnerable and marginalized, have increased resilience to the impacts of climate change, water scarcity, and environmental degradation.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "315",
                UNCooperationFrameworkVersionNo = 1,
                Country = "LY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 56, 41, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, institutions deliver human rightsbased, evidence-informed and gender-responsive services for all, with the focus on those who are left behind",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "316",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 9, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, more accountable and transparent human rights-based and genderresponsive governance empowers all people of Moldova to participate in and to contribute to development processes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "317",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 9, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, all people of Moldova, especially the most vulnerable, benefit from inclusive, competitive and sustainable economic development and equal access to decent work and productive employment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "318",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 9, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, institutions and all people of Moldova benefit from and contribute to green and resilient development, sustainable use of natural resources and effective genderresponsive climate change action and disaster risk management.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "319",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 9, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, all people, especially the vulnerable, benefit from improved management and state of natural resources and increasingly innovative, competitive, genderresponsive and inclusive economic development that is climate resilient and lowcarbon",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "320",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ME",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 32, 170, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, all people, especially the vulnerable, increasingly benefit from equitable, genderresponsive and universally accessible social and child protection system and quality services, including labour market activation and capabilities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "321",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ME",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 32, 173, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, all people, especially the vulnerable, benefit from strengthened human capital including early childhood development, and more resilient, genderresponsive, and quality healthcare and education",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "322",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ME",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 32, 173, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, all people, especially the vulnerable, benefit from improved social cohesion, increased realization of human rights and rule of law and accountable, genderresponsive institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "323",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ME",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 57, 32, 173, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1 - D’ici 2030, les institutions nationales sont efficaces, responsables, transparentes et agissent dans un cadre constitutionnel et légal, dans l’observation de l’état de droit et le respect des droits de l’homme, l’égalité de genre, la durabilité environnementale afin d’assurer une assise de légitimité politique",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "324",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1 - D’ici 2030, les institutions nationales sont efficaces, responsables, transparentes et agissent dans un cadre constitutionnel et légal, dans l’observation de l’état de droit et le respect des droits de l’homme, l’égalité de genre, la durabilité environnementale afin d’assurer une assise de légitimité politique",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "324",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 13, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.2 - D’ici 2030, les administrations centrales et décentralisées mettent en œuvre de manière inclusive et conformément à leurs compétences respectives, des politiques publiques efficaces et efficientes, intégrant les besoins fondamentaux de la population et les besoins prioritaires des territoires en vue de l’atteinte des ODD",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "325",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.2 - D’ici 2030, les administrations centrales et décentralisées mettent en œuvre de manière inclusive et conformément à leurs compétences respectives, des politiques publiques efficaces et efficientes, intégrant les besoins fondamentaux de la population et les besoins prioritaires des territoires en vue de l’atteinte des ODD",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "325",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 17, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.3 - D’ici 2030, la société civile, les médias, les jeunes et les femmes participent de manière quantitative, qualitative, effective et responsable à la gestion des affaires publiques et à la réalisation des ODD à tous les niveaux",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "326",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.3 - D’ici 2030, la société civile, les médias, les jeunes et les femmes participent de manière quantitative, qualitative, effective et responsable à la gestion des affaires publiques et à la réalisation des ODD à tous les niveaux",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "326",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 17, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.1 - Les institutions nationales et entités locales permettent à la population notamment aux plus vulnérables d’exercer leurs droits en matière de santé, de nutrition, d’accès à l’eau et assainissement pour la capture du dividende démographique et des ODD",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "327",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.1 - Les institutions nationales et entités locales permettent à la population notamment aux plus vulnérables d’exercer leurs droits en matière de santé, de nutrition, d’accès à l’eau et assainissement pour la capture du dividende démographique et des ODD",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "327",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 20, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.2 - Les institutions nationales et entités locales mettent en œuvre un système national de protection sociale plus intégrée et inclusive, permettent aux personnes vulnérables et marginalisées et aux victimes de catastrophes, de violence ou d’abus et exploitations de jouir pleinement de leurs droits",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "328",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.2 - Les institutions nationales et entités locales mettent en œuvre un système national de protection sociale plus intégrée et inclusive, permettent aux personnes vulnérables et marginalisées et aux victimes de catastrophes, de violence ou d’abus et exploitations de jouir pleinement de leurs droits",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "328",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 20, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.3 - Les institutions nationales et entités locales mettent en œuvre des programmes d’éducation de qualité, équitables, inclusifs accessibles à tout âge, accroissant les compétences de la population",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "329",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.3 - Les institutions nationales et entités locales mettent en œuvre des programmes d’éducation de qualité, équitables, inclusifs accessibles à tout âge, accroissant les compétences de la population",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "329",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 20, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 - L’employabilité des jeunes, des femmes et des ruraux est renforcée et mise en adéquation avec les potentialités et les besoins locaux pour permettre une productivité améliorée ainsi qu’un accès facilité et équitable au marché du travail",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "330",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 - L’employabilité des jeunes, des femmes et des ruraux est renforcée et mise en adéquation avec les potentialités et les besoins locaux pour permettre une productivité améliorée ainsi qu’un accès facilité et équitable au marché du travail",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "330",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 23, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.2 - Plus d’investissements dans les systèmes productifs et manufacturiers, incluant l’économie bleue et verte et la digitalisation, sont promus pour une croissance inclusive, durable et résiliente",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "331",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.2 - Plus d’investissements dans les systèmes productifs et manufacturiers, incluant l’économie bleue et verte et la digitalisation, sont promus pour une croissance inclusive, durable et résiliente",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "331",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 23, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.3 - L’accès des jeunes aux emplois décents, productifs, durables et résilients est favorisé à travers le développement de l’entreprenariat et la mise en œuvre des mesures d’accélération de la transition de l’économie informelle à la formelle",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "332",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 350, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.3 - L’accès des jeunes aux emplois décents, productifs, durables et résilients est favorisé à travers le développement de l’entreprenariat et la mise en œuvre des mesures d’accélération de la transition de l’économie informelle à la formelle",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "332",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.1 - D’ici 2030, toutes les institutions étatiques, le secteur privé et la société civile appliquent de manière effective et coordonnée les principes et normes de l’état de droit, des droits humains et de la bonne gouvernance dans la gestion du capital naturel, de l’environnement et du changement climatique",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "333",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 350, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.1 - D’ici 2030, toutes les institutions étatiques, le secteur privé et la société civile appliquent de manière effective et coordonnée les principes et normes de l’état de droit, des droits humains et de la bonne gouvernance dans la gestion du capital naturel, de l’environnement et du changement climatique",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "333",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.2 - D’ici 2030, les acteurs aux différents niveaux mettent effectivement en pratique les mesures de conservation, de préservation et de valorisation de la biodiversité et du capital naturel pour que l’environnement et les ressources naturelles soient piliers de la croissance économique, du développement durable et des meilleures conditions de vie des populations",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "334",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 350, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.2 - D’ici 2030, les acteurs aux différents niveaux mettent effectivement en pratique les mesures de conservation, de préservation et de valorisation de la biodiversité et du capital naturel pour que l’environnement et les ressources naturelles soient piliers de la croissance économique, du développement durable et des meilleures conditions de vie des populations",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "334",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.3 - D’ici 2030, les autorités nationales et locales, les acteurs multisectoriels renforcent la gouvernance des risques de catastrophes, mettent en œuvre des mesures de renforcement de la résilience face au changement climatique des communautés, des infrastructures et des secteurs économiques clés ; et augmentent la capacité de réponse",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "335",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 21, 350, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.3 - D’ici 2030, les autorités nationales et locales, les acteurs multisectoriels renforcent la gouvernance des risques de catastrophes, mettent en œuvre des mesures de renforcement de la résilience face au changement climatique des communautés, des infrastructures et des secteurs économiques clés ; et augmentent la capacité de réponse",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "335",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 15, 37, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Inclusive Prosperity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "336",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 41, 680, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Inclusive Prosperity",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "336",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 55, 44, 427, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Quality Services for All",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "337",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 41, 683, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Quality Services for All",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "337",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 55, 44, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Healthy Environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "338",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 41, 683, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Healthy Environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "338",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 55, 44, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Good Governance",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "339",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 58, 41, 683, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Good Governance",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "339",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 55, 44, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1 : D’ici 2024, les populations vivent dans un état de droit, un environnement de paix et de redevabilité grâce à des institutions publiques fortes, des médias et une société civile exerçant leurs rôles et responsabilités pour une gouvernance efficace et inclusive",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "340",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 1, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1 : D’ici 2024, les populations vivent dans un état de droit, un environnement de paix et de redevabilité grâce à des institutions publiques fortes, des médias et une société civile exerçant leurs rôles et responsabilités pour une gouvernance efficace et inclusive",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "340",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 137, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Households and communities' resilience strengthened and recovery supported through the provision of inclusive social protection measures and basic social services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "341",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 21, 783, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Households, communities and MSMEs are better able to withstand the adverse economic impacts of the crisis and recover when conditions allow, through the promotion of gender-responsive employment, private sector and climatic resilience mitigations.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "342",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 21, 783, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Non-state actors are empowered and able to continue operating through mechanisms, networks and programmes that promote access to justice, human rights, gender equality, democratic and civic space.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "343",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 21, 787, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, people in urban and rural areas, especially the most vulnerable and marginalised, equally realize their full human potential and benefit from inclusive, rights-based, gender- and shock-responsive health and nutrition, education, social protection, WASH and other services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "344",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 42, 527, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, the Mongolian economy is more diversified, innovative, productive, inclusive, green and geographically balanced enabling decent livelihoods, especially for women and youth, building 21st century skills, and promoting low-carbon development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "345",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 42, 527, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3 By 2027, communities and eco-systems in Mongolia are more resilient to climate change with improved capacity for evidence-informed and genderresponsive sustainable natural resource and environmental management and disaster risk reduction",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "346",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 42, 530, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4 By 2027, policy-making and implementation in Mongolia is more gender-responsive, participatory, coherent, evidence-informed and SDG-aligned; governance institutions at all levels are transparent and accountable; and people, especially the marginalised groups, have access to justice and rule of law for full realization of human rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "347",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 9, 59, 42, 530, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, youth, women and others at risk of being left behind, contribute to and benefit from inclusive, resilient, sustainable economic and human capital development, fostering innovation, entrepreneurship and decent work.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "348",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 18, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, youth, women and others at risk of being left behind, contribute to and benefit from inclusive, resilient, sustainable economic and human capital development, fostering innovation, entrepreneurship and decent work.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "348",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 25, 2, 833, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, people in the Maldives, especially the most vulnerable and marginalised benefit from increased access to and use of quality, equitable, inclusive and resilient social and protection services, and have enhanced relevant skills and live fulfilled lives with wellbeing and dignity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "349",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 18, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, people in the Maldives, especially the most vulnerable and marginalised benefit from increased access to and use of quality, equitable, inclusive and resilient social and protection services, and have enhanced relevant skills and live fulfilled lives with wellbeing and dignity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "349",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 25, 2, 837, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, national and sub-national institutions and communities in Maldives, particularly at-risk populations, are better able to manage natural resources and achieve enhanced resilience to climate change and disaster impacts, natural and human-induced hazards, and environmental degradation, inclusively and in a sustainable manner.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "350",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 18, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, national and sub-national institutions and communities in Maldives, particularly at-risk populations, are better able to manage natural resources and achieve enhanced resilience to climate change and disaster impacts, natural and human-induced hazards, and environmental degradation, inclusively and in a sustainable manner.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "350",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 25, 2, 837, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4:  By 2026, Maldives has strengthened decentralised and accountable governance under the rule of law where people are empowered, meaningfully participate in transparent and transformative processes for public policy and fully enjoy access to justice, public services, human rights, gender equality and women’s empowerment in a tolerant and peaceful society.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "351",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 18, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4:  By 2026, Maldives has strengthened decentralised and accountable governance under the rule of law where people are empowered, meaningfully participate in transparent and transformative processes for public policy and fully enjoy access to justice, public services, human rights, gender equality and women’s empowerment in a tolerant and peaceful society.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "351",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 25, 2, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Al 2025, el Edo. mexicano refuerza sus capacidades de adaptación y resiliencia frente a la variabilidad y cambio climáticos, mediante políticas, programas, herramientas y servicios multisectoriales con enfoque integrado, énfasis en poblaciones y territorios más vulnerables, considerando la gestión integral del agua y de riesgos de desastres, y basados en manejo sostenible de ecosistemas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "352",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Al 2025, el Estado mexicano cuenta con las capacidades institucionales para prevenir y sancionar todas las formas y manifestaciones de violencia, en particular contra las mujeres, niñas y las y los adolescentes, a la vez que garantiza el acceso a servicios de calidad para la debida atención y protección a las víctimas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "353",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Al 2025, el Estado mexicano fortalece la promoción, protección y respeto del ejercicio pleno de los derechos humanos y el acceso a la justicia, la verdad y reparación para toda la población, especialmente la población que se encuentra en contextos de mayor vulnerabilidad, en condiciones de igualdad y bajo el principio de no dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "354",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Al 2025, el Estado mexicano implementa políticas, estrategias y programas para transitar hacia una economía verde que promueva la mitigación al CC y el reforzamiento del marco institucional, considerando la eficiencia energética, la promoción de energías limpias y renovables, así como producción, consumo, transporte, ciudades y agricultura sostenibles; con enfoque en salud e integrado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "355",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Al 2025, instituciones del Estado mexicano y la sociedad civil se encuentran articuladas y con capacidades instaladas para prevenir, denunciar y sancionar actos de corrupción, promover mecanismos de rendición de cuentas y garantizar la participación social y política en la toma de decisiones inclusivas, de manera transparente, en condiciones de igualdad y sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "356",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED1. Al 2025, el Estado mexicano cuenta con una estrategia integral de desarrollo social, combate a la pobreza multidimensional y a la desigualdad, con enfoque integrado de derechos humanos, género, interculturalidad, ciclo de vida y territorio, que incorpora mecanismos redistributivos sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "357",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED1. Al 2025, el Estado mexicano cuenta con una estrategia integral de desarrollo social, combate a la pobreza multidimensional y a la desigualdad, con enfoque integrado de derechos humanos, género, interculturalidad, ciclo de vida y territorio, que incorpora mecanismos redistributivos sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "357",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 720, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED1. Al 2025, el Estado mexicano cuenta con una estrategia integral de desarrollo social, combate a la pobreza multidimensional y a la desigualdad, con enfoque integrado de derechos humanos, género, interculturalidad, ciclo de vida y territorio, que incorpora mecanismos redistributivos sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "357",
                UNCooperationFrameworkVersionNo = 3,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 413, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED2. Al 2025, la población, en especial la que está en condiciones de mayor vulnerabilidad, ejerce plenamente sus derechos a la salud, educación, cultura, vivienda, alimentación, protección social y de cuidados, y accede a servicios universales y a un sistema integral de cuidados de calidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "358",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED2. Al 2025, la población, en especial la que está en condiciones de mayor vulnerabilidad, ejerce plenamente sus derechos a la salud, educación, cultura, vivienda, alimentación, protección social y de cuidados, y accede a servicios universales y a un sistema integral de cuidados de calidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "358",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 720, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED2. Al 2025, la población, en especial la que está en condiciones de mayor vulnerabilidad, ejerce plenamente sus derechos a la salud, educación, cultura, vivienda, alimentación, protección social y de cuidados, y accede a servicios universales y a un sistema integral de cuidados de calidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "358",
                UNCooperationFrameworkVersionNo = 3,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 413, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED3. Al 2025, el Estado mexicano planifica e implementa con enfoque territorial, de población y con perspectivas de derechos humanos y género, estrategias inclusivas para generar prosperidad compartida que reduzcan la desigualdad y la pobreza.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "359",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED3. Al 2025, el Estado mexicano planifica e implementa con enfoque territorial, de población y con perspectivas de derechos humanos y género, estrategias inclusivas para generar prosperidad compartida que reduzcan la desigualdad y la pobreza.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "359",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 720, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED3. Al 2025, el Estado mexicano planifica e implementa con enfoque territorial, de población y con perspectivas de derechos humanos y género, estrategias inclusivas para generar prosperidad compartida que reduzcan la desigualdad y la pobreza.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "359",
                UNCooperationFrameworkVersionNo = 3,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 417, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED4. Al 2025, el Estado mexicano cuenta con una estrategia de desarrollo productivo que promueve la asociatividad, la innovación, la productividad y la competitividad, así como el incremento de contenido nacional en los encadenamientos productivos con mejor gobernanza para la igualdad, basada en el marco de los derechos humanos y con perspectiva de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "360",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED4. Al 2025, el Estado mexicano cuenta con una estrategia de desarrollo productivo que promueve la asociatividad, la innovación, la productividad y la competitividad, así como el incremento de contenido nacional en los encadenamientos productivos con mejor gobernanza para la igualdad, basada en el marco de los derechos humanos y con perspectiva de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "360",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 720, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED4. Al 2025, el Estado mexicano cuenta con una estrategia de desarrollo productivo que promueve la asociatividad, la innovación, la productividad y la competitividad, así como el incremento de contenido nacional en los encadenamientos productivos con mejor gobernanza para la igualdad, basada en el marco de los derechos humanos y con perspectiva de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "360",
                UNCooperationFrameworkVersionNo = 3,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 417, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED5. Al 2025, el Estado mexicano cuenta con programas de trabajo decente para abordar necesidades del mercado laboral, incluyendo el fortalecimiento institucional, la formación para el trabajo, la formalización, la participación económica de las mujeres, la protección de derechos, la movilidad social y la justicia laboral.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "361",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 0, 41, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED5. Al 2025, el Estado mexicano cuenta con programas de trabajo decente para abordar necesidades del mercado laboral, incluyendo el fortalecimiento institucional, la formación para el trabajo, la formalización, la participación económica de las mujeres, la protección de derechos, la movilidad social y la justicia laboral.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "361",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 38, 41, 723, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED5. Al 2025, el Estado mexicano cuenta con programas de trabajo decente para abordar necesidades del mercado laboral, incluyendo el fortalecimiento institucional, la formación para el trabajo, la formalización, la participación económica de las mujeres, la protección de derechos, la movilidad social y la justicia laboral.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "361",
                UNCooperationFrameworkVersionNo = 3,
                Country = "MX",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 12, 417, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, more people, particularly the most vulnerable and marginalized, have a more equitable access to and utilization of quality, inclusive, resilient, gender- and shock-responsive social protection and essential social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "362",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 1, 3, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, more people, particularly the most vulnerable and marginalized, have a more equitable access to and utilization of quality, inclusive, resilient, gender- and shock-responsive social protection and essential social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "362",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 40, 767, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, more people, particularly women and youths, participate in and benefit from a more diversified, inclusive, and sustainable economic growth based on increased production, productivity, and greater value-added chains.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "363",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 1, 3, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, more people, particularly women and youths, participate in and benefit from a more diversified, inclusive, and sustainable economic growth based on increased production, productivity, and greater value-added chains.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "363",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 40, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, more people, especially the most vulnerable, are resilient to climate change and disasters, and benefit from more sustainable management of environment and natural resources and resilient infrastructures and human settlements, with positive effects on national GDP.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "364",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 1, 3, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, more people, especially the most vulnerable, are resilient to climate change and disasters, and benefit from more sustainable management of environment and natural resources and resilient infrastructures and human settlements, with positive effects on national GDP.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "364",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 40, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, more people, especially the most vulnerable and marginalized, are protected, enjoy their rights, and benefit from a secure, peaceful environment, enabled by inclusive governance systems, and independent and accountable institutions abiding by the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "365",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 1, 3, 480, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, more people, especially the most vulnerable and marginalized, are protected, enjoy their rights, and benefit from a secure, peaceful environment, enabled by inclusive governance systems, and independent and accountable institutions abiding by the rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "365",
                UNCooperationFrameworkVersionNo = 2,
                Country = "MZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 51, 40, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2027, Nigeria has increased productivity and competitiveness in agriculture, manufacturing, and service sectors for inclusive and sustainable industrialization, public and private sector investment, and regional trade.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "366",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2027, all people living in Nigeria, especially women, youth, persons with disabilities, and other vulnerable groups, have improved access to decent job opportunities driven by digitalization, skills development, entrepreneurship to harness the demographic dividend.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "367",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3: By 2027 all people living in Nigeria have improved social protection coverage that are inclusive, gender-responsive, age friendly and shock-responsive, including social assistance, social insurance, and labour market interventions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "368",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.4: By 2027 Nigeria has improved data for evidence based and risk informed planning and decision making.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "369",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2027, Nigeria benefits from improved food security and nutrition, and sustainable food systems and environment and natural resource management.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "370",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2: By 2027, Nigeia is implementing improved management of climate change risk and building resilience to adapt to its long term impact through the NDCs, sustainable energy production/consumption and climate finance.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "371",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.3: By 2027, Nigeria implements inclusive policies and practices for Resilience and Disaster Risk Management for risk informed development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "372",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 367, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2027, people in Nigeria enjoy equitable access to and use of integrated, comprehensive, high-quality, people-centred health services towards attaining UHC with a particular focus on AIDS, TB, Malaria and SRHR.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "373",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 367, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2: By 2027, people in Nigeria enjoy equitable access to and use of quality education system that delivers an inclusive education for learning and transferable skills.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "374",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 367, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.3: By 2027, people in Nigeria have equitable and affordable access to safely managed water and sanitation facilities, and practise sage hygiene and behaviours.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "375",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 367, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1: By 2027, people in Nigeria of all ages, especially the most vulnerable, benefit from peace and security, and protection from conflict, violence, and crime through strengthen capacity and infrastructure.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "376",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 370, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2: By 2027, the people in Nigeria have access to a more accountable, transparent, and gender responsive, and inclusive governance and justice system for the realisation of human rights for all especially the most vulnerable population.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "377",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 370, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.3: Gender equality and human rights of women, youth, older persons and other marginalised groups including persons with disabilities in Nigeria are enhanced.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "378",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 5, 53, 370, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.1 Al 2025, Panamá propicia un desarrollo sostenible e inclusivo: asegura el acceso equitativo a los servicios esenciales y medios de vida para todas las personas; promueve la inclusión, la innovación, la competitividad, el desarrollo industrial y el emprendedurismo, con enfoque territorial y de derechos humanos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "379",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 21, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.2 Al 2025, Panamá cuenta con una gobernanza participativa e instituciones nacionales y locales inclusivas, eficaces, transparentes y justas al servicio de las personas, articuladas entre sí y en alianza con actores no gubernamentales; con enfoque de derechos humanos, de género, intercultural, curso de vida, territorial, y sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "380",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 21, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.3 Al 2025, Panamá es resiliente y cuenta con políticas públicas implementadas para la adaptación y mitigación del cambio climático, la neutralidad de la degradación de la tierra, la protección de la biodiversidad, la gestión ambiental integrada y la reducción de riesgo de desastres y crisis sanitarias, con enfoque territorial, intercultural, de derechos humanos, de género, y curso de vida.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "381",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 21, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.4 Al 2025, Panamá cuenta con un sistema de protección de derechos inclusivo e integral con especial énfasis en la prevención y atención de todas las formas de violencia y discriminación por motivos de género, curso de vida, y sensible a todas las personas en condiciones de vulnerabilidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "382",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 21, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 1. Al 2026, las personas aumentan su acceso al trabajo decente y al sistema integral de protección social, incluyendo un piso de protección social, que asegura el acceso universal a la salud incluyendo salud sexual y reproductiva, nutrición, seguridad alimentaria, seguridad básica de ingreso y sistema de cuidados, con un enfoque integrado y especial énfasis en género y derechos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "383",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 800, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 2. Al 2026, las personas mejoran su acceso equitativo a educación universal de calidad y protección especial, con un enfoque integrado y especial énfasis en género y derechos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "384",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 800, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 3. Al 2026 las personas en situación de vulnerabilidad, desprotección, pobreza e inseguridad alimentaria incrementan su resiliencia ante situaciones de crisis, fortaleciendo mecanismos de respuesta humanitaria y de recuperación post-crisis con un enfoque integrado y especial énfasis en género, derechos y territorio.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "385",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 4. Al 2026, la población y los ecosistemas, especialmente aquellos en mayor situación de vulnerabilidad, fortalecen su resiliencia como resultado de que, instituciones y comunidades, mejoran políticas e implementan mecanismos o instrumentos efectivos para la gestión ambiental, del cambio climático, del riesgo de desastres y de las crisis humanitarias, con un enfoque integrado y especial énfasis en género, derechos, interculturalidad, ciclo de vida y territorio.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "386",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 5. Al 2026, las personas, especialmente aquellas que se encuentran en mayor situación de vulnerabilidad y discriminación, mejoran su acceso a medios de vida resilientes mediante la construcción de una matriz productiva diversificada, competitiva, formalizada, innovadora, sostenible e inclusiva, con trabajo decente, alineada a las potencialidades de cada territorio, y con un enfoque integrado y especial énfasis en género, particularmente a través del empoderamiento económico de las mujeres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "387",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 6. Al 2026, las personas, especialmente aquellas que se encuentran en mayor situación de vulnerabilidad y discriminación, como niñas y niños, adolescentes, jóvenes y mujeres; ejercen sus derechos en condiciones de igualdad, como resultado del fortalecimiento de la gobernanza efectiva, la cohesión social, el acceso a la justicia y la lucha contra la desigualdad de género y toda forma de discriminación y violencia basadas en género, sobre la base de un enfoque integrado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "388",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 6, 46, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2027, the people in Pakistan, especially the most vulnerable and deprived, have increased equitable access to and utilization of quality, sustainable basic social services (BSS).",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "389",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 410, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2027, women, girls and transgender persons in Pakistan, especially those at greatest risk of being left behind, benefit from an enabling environment where they are empowered and reach their fullest potential; and their human, social, economic, cultural and political rights are fully protected and upheld.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "390",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 413, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2027, people living in the Indus River Basin, particularly the most vulnerable, including women, girls, boys, persons with disabilities and senior citizens, have their lives positively impacted by the restored and protected health of the Indus Basin, and by being better equipped to adapt to climate change and to mitigate its impact.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "391",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 413, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2027, people in Pakistan, especially those at risk of being left behind and becoming further marginalized – including youth, women, persons with disabilities and other vulnerable groups – benefit from a broadbased, job-rich and gender-responsive recovery with decent work opportunities for all.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "392",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 413, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2027, the people in Pakistan, especially women, children, the most vulnerable and marginalized, have increased access to fundamental rights, gender equality and fundamental freedoms through inclusive, accountable, effective and evidence-driven governance systems and rule of law institutions at all levels of government, that contribute to good governance and stability.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "393",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 7, 6, 420, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 Palestinians have greater access to economic opportunities that are inclusive, resilient, and sustainable, including decent employment and livelihoods opportunities in an empowered private sector.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "394",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 8, 50, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2 Palestinians, including the most vulnerable, have equal access to sustainable, inclusive, gender responsive and quality social services, social protection, and affordable utilities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "395",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 8, 50, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: Palestinian governance institutions, processes, and mechanisms at all levels are more democratic, rights-based, inclusive, and accountable.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "396",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 8, 50, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: Palestinians have better access to and management of natural and cultural resources, higher resilience and adaptation to climate change and more sustainable food systems",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "397",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 8, 50, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1: Al 2024, las personas que viven y transitan en Paraguay, especialmente aquellas en situación de vulnerabilidad, cuentan con un sistema nacional de protección social y de cuidados con acceso equitativo a servicios integrales y de calidad",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "398",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 450, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 1: Al 2024, las personas que viven y transitan en Paraguay, especialmente aquellas en situación de vulnerabilidad, cuentan con un sistema nacional de protección social y de cuidados con acceso equitativo a servicios integrales y de calidad",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "398",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 797, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2: Al 2024, la sociedad civil empoderada habrá fortalecido su capacidad de articularse, generar alianzas e incidir en espacios cívicos, para el ejercicio de los derechos humanos y la cohesión social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "399",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 450, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2: Al 2024, la sociedad civil empoderada habrá fortalecido su capacidad de articularse, generar alianzas e incidir en espacios cívicos, para el ejercicio de los derechos humanos y la cohesión social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "399",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3: Al 2024, las mujeres, adolescentes y jóvenes ejercen sus derechos civiles, políticos, económicos y sociales participando en, y beneficiándose de, las acciones del Estado, en entornos libres de violencia y en condiciones de igualdad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "400",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 450, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3: Al 2024, las mujeres, adolescentes y jóvenes ejercen sus derechos civiles, políticos, económicos y sociales participando en, y beneficiándose de, las acciones del Estado, en entornos libres de violencia y en condiciones de igualdad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "400",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 4: Al 2024, las instituciones nacionales y subnacionales del sector público, sector privado y organizaciones de la sociedad civil habrán diseñado e implementado políticas e iniciativas de gestión del capital natural para construir un desarrollo sostenible, limpio y bajo en emisiones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "401",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 450, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 4: Al 2024, las instituciones nacionales y subnacionales del sector público, sector privado y organizaciones de la sociedad civil habrán diseñado e implementado políticas e iniciativas de gestión del capital natural para construir un desarrollo sostenible, limpio y bajo en emisiones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "401",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 807, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 5: Al 2024, las instituciones nacionales y subnacionales del sector público, sector privado, organizaciones de la sociedad civil y las comunidades han fortalecido su resiliencia, capacidad de gestión de riesgo y respuesta a emergencias y efectos del cambio climático, desde un enfoque de derechos y de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "402",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 453, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 5: Al 2024, las instituciones nacionales y subnacionales del sector público, sector privado, organizaciones de la sociedad civil y las comunidades han fortalecido su resiliencia, capacidad de gestión de riesgo y respuesta a emergencias y efectos del cambio climático, desde un enfoque de derechos y de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "402",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 807, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 6: Al 2024 el Estado implementa políticas públicas que favorecen la generación de medios de vida sostenibles,  el trabajo decente y la inclusión económica de las personas en situación de vulnerabilidad,  con un enfoque de derechos y de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "403",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 453, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 6: Al 2024 el Estado implementa políticas públicas que favorecen la generación de medios de vida sostenibles,  el trabajo decente y la inclusión económica de las personas en situación de vulnerabilidad,  con un enfoque de derechos y de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "403",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 807, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 7: Al 2024, el Estado habrá fortalecido sus capacidades para la generación, adaptación y promoción de conocimientos e innovación científico-tecnológica accesible en la diversificación y mejora de la productividad y competitividad de su economía con sostenibilidad social y ambiental con carácter inclusivo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "404",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 453, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 7: Al 2024, el Estado habrá fortalecido sus capacidades para la generación, adaptación y promoción de conocimientos e innovación científico-tecnológica accesible en la diversificación y mejora de la productividad y competitividad de su economía con sostenibilidad social y ambiental con carácter inclusivo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "404",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 8: Al 2024, el Estado fortalece su capacidad de gestión con eficiencia y transparencia, mapeo de las desigualdades, implementación y monitoreo de los ODS y el PND Paraguay 2030.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "405",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 457, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 8: Al 2024, el Estado fortalece su capacidad de gestión con eficiencia y transparencia, mapeo de las desigualdades, implementación y monitoreo de los ODS y el PND Paraguay 2030.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "405",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 9: Al 2024, el Estado mejora el acceso a la justicia y la seguridad multidimensional,  el cumplimiento de marcos y acuerdos suscritos en materia de derechos humanos, igualdad y no discriminación, así como otros compromisos medioambientales y sobre cambio climático.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "406",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 14, 457, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 9: Al 2024, el Estado mejora el acceso a la justicia y la seguridad multidimensional,  el cumplimiento de marcos y acuerdos suscritos en materia de derechos humanos, igualdad y no discriminación, así como otros compromisos medioambientales y sobre cambio climático.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "406",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 19, 59, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.1  Serbia adopts and implements climate change and environmentally friendly strategies that increase community resilience, decrease carbon footprint and amplify the benefits of national investments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "407",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.2  Natural and Cultural Resources are managed in a sustainable way",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "408",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.3 Equitable economic and employment opportunities are promoted through innovation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "409",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.3 Equitable economic and employment opportunities are promoted through innovation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "409",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 767, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.1.  Universal and inclusive access to quality health, social and protection services is improved",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "410",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.2.  Skills, education and capabilities are enhanced to ensure equitable outcomes for all",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "411",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.3 Mobility and demographic transition become vectors for positive change and prosperity for all people",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "412",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.3 Mobility and demographic transition become vectors for positive change and prosperity for all people",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "412",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 773, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.1.  All people, especially the more vulnerable, benefit from the realization of human rights, gender equality and enhanced rule of law in line with international commitments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "413",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.2.  All people benefit from effective governance and meaningful civic engagement",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "414",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 9, 39, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: BY 2024, PEOPLE IN RWANDA BENEFIT FROM MORE INCLUSIVE, COMPETITIVE, AND SUSTAINABLE ECONOMIC GROWTH THAT GENERATES DECENT WORK AND PROMOTES QUALITY LIVELIHOODS FOR ALL",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "415",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 60, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: BY 2024, PEOPLE IN RWANDA BENEFIT FROM MORE INCLUSIVE, COMPETITIVE, AND SUSTAINABLE ECONOMIC GROWTH THAT GENERATES DECENT WORK AND PROMOTES QUALITY LIVELIHOODS FOR ALL",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "415",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 397, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: BY 2024, RWANDAN INSTITUTIONS AND COMMUNITIES ARE MORE EQUITABLY, PRODUCTIVELY, AND SUSTAINABLY MANAGING NATURAL RESOURCES AND ADDRESSING CLIMATE CHANGE AND NATURAL DISASTERS",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "416",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: BY 2024, RWANDAN INSTITUTIONS AND COMMUNITIES ARE MORE EQUITABLY, PRODUCTIVELY, AND SUSTAINABLY MANAGING NATURAL RESOURCES AND ADDRESSING CLIMATE CHANGE AND NATURAL DISASTERS",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "416",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 397, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 - BY 2024, PEOPLE IN RWANDA, PARTICULARLY THE MOST VULNERABLE, ENJOY INCREASED AND EQUITABLE ACCESS TO QUALITY EDUCATION, HEALTH, NUTRITION AND WATER, SANITATION, AND HYGIENE (WASH) SERVICES",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "417",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 - BY 2024, PEOPLE IN RWANDA, PARTICULARLY THE MOST VULNERABLE, ENJOY INCREASED AND EQUITABLE ACCESS TO QUALITY EDUCATION, HEALTH, NUTRITION AND WATER, SANITATION, AND HYGIENE (WASH) SERVICES",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "417",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: BY 2024, PEOPLE IN RWANDA, PARTICULARLY THE MOST VULNERABLE, HAVE INCREASED RESILIENCE TO BOTH NATURAL AND MAN-MADE SHOCKS AND LIVE A LIFE FREE FROM ALL FORMS OF VIOLENCE AND DISCRIMINATION",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "418",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: BY 2024, PEOPLE IN RWANDA, PARTICULARLY THE MOST VULNERABLE, HAVE INCREASED RESILIENCE TO BOTH NATURAL AND MAN-MADE SHOCKS AND LIVE A LIFE FREE FROM ALL FORMS OF VIOLENCE AND DISCRIMINATION",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "418",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 5: BY 2024, PEOPLE IN RWANDA BENEFIT FROM ENHANCED GENDER EQUALITY, JUSTICE, HUMAN RIGHTS, PEACE, AND SECURITY",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "419",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 5: BY 2024, PEOPLE IN RWANDA BENEFIT FROM ENHANCED GENDER EQUALITY, JUSTICE, HUMAN RIGHTS, PEACE, AND SECURITY",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "419",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 6: BY 2024, PEOPLE IN RWANDA PARTICIPATE MORE ACTIVELY IN DEMOCRATIC AND DEVELOPMENT PROCESSES AND BENEFIT FROM TRANSPARENT AND ACCOUNTABLE PUBLIC AND PRIVATE SECTOR INSTITUTIONS THAT DEVELOP EVIDENCE-BASED POLICIES AND DELIVER QUALITY SERVICES",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "420",
                UNCooperationFrameworkVersionNo = 1,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 18, 67, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 6: BY 2024, PEOPLE IN RWANDA PARTICIPATE MORE ACTIVELY IN DEMOCRATIC AND DEVELOPMENT PROCESSES AND BENEFIT FROM TRANSPARENT AND ACCOUNTABLE PUBLIC AND PRIVATE SECTOR INSTITUTIONS THAT DEVELOP EVIDENCE-BASED POLICIES AND DELIVER QUALITY SERVICES",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "420",
                UNCooperationFrameworkVersionNo = 2,
                Country = "RW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 27, 47, 400, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: All human beings are further enabled to fulfill their potential in dignity, equity, and equality in a healthy environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "421",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 35, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: The environment is better protected from degradation, including through sustainable consumption and production, sustainably managing its natural resources and taking urgent action on climate change.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "422",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 35, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Increased opportunities are available for all human beings to enjoy prosperous and fulfilling lives.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "423",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 35, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Peaceful, just and inclusing society is adequately fostered, and the means required for implementation of Agenda 2030 are mobilised.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "424",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 10, 35, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - By 2023, Sierra Leone benefits from a more productive, commercialized and sustainable agriculture, improved food and nutrition security, and increased resilience to climate change and other shocks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "425",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 4, 40, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - By 2023, Sierra Leone benefits from a more productive, commercialized and sustainable agriculture, improved food and nutrition security, and increased resilience to climate change and other shocks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "425",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 47, 51, 637, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 - By 2023, Sierra Leone benefits from a more productive, commercialized and sustainable agriculture, improved food and nutrition security, and increased resilience to climate change and other shocks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "425",
                UNCooperationFrameworkVersionNo = 3,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 43, 54, 203, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - By 2023, people in Sierra Leone benefit from more gender and youth responsive institutions that are innovative, accountable and transparent at all levels and can better advance respect for human rights and the rule of law, equity, peaceful coexistence, and protection of boys and girls (children, girls), women and men including those with disabilities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "426",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 4, 40, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - By 2023, people in Sierra Leone benefit from more gender and youth responsive institutions that are innovative, accountable and transparent at all levels and can better advance respect for human rights and the rule of law, equity, peaceful coexistence, and protection of boys and girls (children, girls), women and men including those with disabilities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "426",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 47, 51, 640, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 - By 2023, people in Sierra Leone benefit from more gender and youth responsive institutions that are innovative, accountable and transparent at all levels and can better advance respect for human rights and the rule of law, equity, peaceful coexistence, and protection of boys and girls (children, girls), women and men including those with disabilities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "426",
                UNCooperationFrameworkVersionNo = 3,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 43, 54, 207, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - By 2023, the population of Sierra Leone, particularly the most vulnerable, will benefit from increased and more equitable access to and utilization of quality education, healthcare, energy and water, sanitation and hygiene services, including during emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "427",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 4, 40, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - By 2023, the population of Sierra Leone, particularly the most vulnerable, will benefit from increased and more equitable access to and utilization of quality education, healthcare, energy and water, sanitation and hygiene services, including during emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "427",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 47, 51, 640, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 - By 2023, the population of Sierra Leone, particularly the most vulnerable, will benefit from increased and more equitable access to and utilization of quality education, healthcare, energy and water, sanitation and hygiene services, including during emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "427",
                UNCooperationFrameworkVersionNo = 3,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 43, 54, 210, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 - By 2023, the most vulnerable, particularly women, youth, adolescents and children (especially girls), and persons living with disabilities are empowered and benefit from increased social protection services, economic and social opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "428",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 4, 40, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 - By 2023, the most vulnerable, particularly women, youth, adolescents and children (especially girls), and persons living with disabilities are empowered and benefit from increased social protection services, economic and social opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "428",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 47, 51, 640, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 - By 2023, the most vulnerable, particularly women, youth, adolescents and children (especially girls), and persons living with disabilities are empowered and benefit from increased social protection services, economic and social opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "428",
                UNCooperationFrameworkVersionNo = 3,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 43, 54, 210, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 - D’ici 2023, les institutions nationales et locales améliorent la qualité et l’équité dans l’offre de services publics en vue de la promotion de la paix, la sécurité et l’efficacité de la gouvernance.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "429",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 11, 24, 690, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Women and men in South Sudan, particularly youth and vulnerable groups, benefit from and participate in more transparent, accountable, and inclusive governance that protects and promotes human rights, enables the consolidation of peace, establishes the rule of law and ensures access to justice for all",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "430",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 12, 5, 73, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. Women and men in South Sudan, particularly youth and vulnerable groups, benefit from and contribute to more sustainable and inclusive economic development, with reduced dependence on oil, improved climate change adaptation and greater resilience to economic shocks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "431",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 12, 5, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Children, women and men in South Sudan, particularly youth and vulnerable groups, enjoy improved coverage of inclusive, responsive, quality social services and social protection",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "432",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 12, 5, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Women, youth plus vulnerable groups are empowered to demand and exercise their political, economic, social, environmental and cultural rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "433",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 12, 5, 80, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, people in STP, in particular the people left behind and most vulnerable, benefit from quality and inclusive social systems and have access to integrated social protection",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "434",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ST",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 13, 40, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, institutions integrate climate change adaptation, low carbon and renewable energies into policies and programmatic implementation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "435",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ST",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 13, 40, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, national stakeholders generate substantially more decent jobs in an environmentally friendly blue and green economy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "436",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ST",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 13, 40, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, people benefit from transparent, responsive and gender-sensitive institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "437",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ST",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 13, 40, 550, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E1 Al 2026, las personas, especialmente aquellas en situación  de mayor vulnerabilidad y exclusión, tienen acceso equitativo  a educación, salud, servicios sociales y sistemas de  protección social integral, contribuyendo a la reducción de la  pobreza multidimensional y de las brechas de desigualdad  en todas sus manifestaciones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "438",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E1 Al 2026, las personas, especialmente aquellas en situación  de mayor vulnerabilidad y exclusión, tienen acceso equitativo  a educación, salud, servicios sociales y sistemas de  protección social integral, contribuyendo a la reducción de la  pobreza multidimensional y de las brechas de desigualdad  en todas sus manifestaciones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "438",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 603, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E2 Al 2026, todas las personas, especialmente aquellas en  situación de mayor vulnerabilidad, principalmente por las  desigualdades de género incrementan su participación e  inclusión en las esferas política, social y económica y ejercen  más plenamente su derecho a una vida libre de violencia y  discriminación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "439",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 33, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E2 Al 2026, todas las personas, especialmente aquellas en  situación de mayor vulnerabilidad, principalmente por las  desigualdades de género incrementan su participación e  inclusión en las esferas política, social y económica y ejercen  más plenamente su derecho a una vida libre de violencia y  discriminación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "439",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 607, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E3 Al 2026, las instituciones salvadoreñas aseguran la  cobertura y calidad de la atención, protección integral  especializada, (re)integración e inclusión económica de las  personas migrantes, retornadas, en tránsito o desplazadas  forzosamente, así como de sus familias.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "440",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 33, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E3 Al 2026, las instituciones salvadoreñas aseguran la  cobertura y calidad de la atención, protección integral  especializada, (re)integración e inclusión económica de las  personas migrantes, retornadas, en tránsito o desplazadas  forzosamente, así como de sus familias.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "440",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E4 Al 2026, las personas, particularmente las que están en  situación de vulnerabilidad, tienen mayores oportunidades  de acceder a un trabajo decente, productivo y medios de vida  sostenibles, en un entorno de transformación económica  inclusiva, innovadora y sostenible.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "441",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 33, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E4 Al 2026, las personas, particularmente las que están en  situación de vulnerabilidad, tienen mayores oportunidades  de acceder a un trabajo decente, productivo y medios de vida  sostenibles, en un entorno de transformación económica  inclusiva, innovadora y sostenible.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "441",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E5 Al 2026, las instituciones y la población en El Salvador están  más preparadas y tienen mayor resiliencia frente a desastres,  gestionan los riesgos de manera efectiva, se adaptan mejor  y mitigan los efectos del cambio climático.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "442",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E5 Al 2026, las instituciones y la población en El Salvador están  más preparadas y tienen mayor resiliencia frente a desastres,  gestionan los riesgos de manera efectiva, se adaptan mejor  y mitigan los efectos del cambio climático.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "442",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E6 Al 2026, las instituciones salvadoreñas fortalecen la  gobernabilidad democrática garantizando el Estado de  Derecho y la participación política y cívica inclusiva, así  como la prevención y combate de la corrupción, fomentando  la transparencia y la rendición de cuentas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "443",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E6 Al 2026, las instituciones salvadoreñas fortalecen la  gobernabilidad democrática garantizando el Estado de  Derecho y la participación política y cívica inclusiva, así  como la prevención y combate de la corrupción, fomentando  la transparencia y la rendición de cuentas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "443",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E7 Al 2026, las personas viven en un entorno más pacífico y  seguro, en el que están mejor protegidas frente al crimen  organizado y la violencia en sus distintas manifestaciones;  tienen mayor acceso a un sistema de justicia justo y efectivo,  y se garantiza la reparación de las víctimas y la reinserción  social de las personas en conflicto con la Ley.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "444",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 0, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "E7 Al 2026, las personas viven en un entorno más pacífico y  seguro, en el que están mejor protegidas frente al crimen  organizado y la violencia en sus distintas manifestaciones;  tienen mayor acceso a un sistema de justicia justo y efectivo,  y se garantiza la reparación de las víctimas y la reinserción  social de las personas en conflicto con la Ley.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "444",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 8, 31, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Accountable Governance, Justice and Human Rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "445",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 23, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Investing in Human Resources and Social Development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "446",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 23, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Promoting Sustainable and Inclusive Economic Growth",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "447",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 23, 540, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Strengthening natural resource management, climate resilience and environmental sustainability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "448",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 23, 543, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Thailand’s transformation into an inclusive economy based on green, resilient, low-carbon, sustainable development is accelerated.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "449",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 14, 46, 173, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Inclusive human development. By 2026, health, food security and nutrition, education and social protection systems and services are more effective, inclusive, gender-sensitive, and adequately financed.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "450",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 3, 520, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Sustainable, inclusive, and green economic growth. By 2026, public institutions and the private sector collaborate to implement innovative and gender-responsive policy frameworks and actions to green the economy and strengthen inclusion of vulnerable groups.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "451",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 3, 523, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Integrated management of climate and environmental risks. By 2026, natural resources management is inclusive and sustainable with integrated policy frameworks and actions to enhance climate-change adaptation and livelihoods of vulnerable groups.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "452",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 3, 523, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: People-centred governance and rule of law By 2026, governance is more inclusive, transparent and accountable, serving to protect human rights, empower women, and reduce violence and discrimination in alignment with the international commitments of Tajikistan.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "453",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 3, 523, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, all people of Timor-Leste, regardless of gender identity, abilities, geographic location and particular vulnerabilities, have increased access to quality formal and innovative learning pathways (from early childhood through lifelong learning) and acquire foundational, transferable, digital and job-specific skills",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "454",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, institutions and people throughout Timor-Leste in all their diversity, especially women and youth, benefit from sustainable economic opportunities and decent work to reduce poverty",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "455",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, national and sub-national institutions and communities (particularly at risk populations including women and children) in Timor-Leste are better able to manage natural resources and achieve enhanced resilience to climate change impacts, natural and human-induced hazards, and environmental degradation, inclusively and sustainably",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "456",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, nutrition, food security and agricultural productivity have improved for all, irrespective of the individual ability, gender, age, socio-economic status and geographical location",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "457",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 760, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, the most excluded people of Timor-Leste are empowered to claim their rights, including freedom from violence, through accessible, accountable and gender responsive governance systems, institutions and services at national and subnational levels",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "458",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 763, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2025, the people of Timor-Leste increasingly demand and have access to gender-responsive equitable, high quality, resilient and inclusive Primary Health Care and strengthened social protection, including in time of emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "459",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 24, 763, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1 By 2025, people have access to more effective, innovative, and transparent public administration based upon the rule of law, human rights, gender equality, labour rights, and quality data",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "460",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 470, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1 By 2025, people have access to more effective, innovative, and transparent public administration based upon the rule of law, human rights, gender equality, labour rights, and quality data",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "460",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2025, conditions for sustainable and inclusive economic diversification are strengthened with competitive private and financial sectors, enhanced trade and investment promotion, and the adoption of new and digital technologies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "461",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 470, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2025, conditions for sustainable and inclusive economic diversification are strengthened with competitive private and financial sectors, enhanced trade and investment promotion, and the adoption of new and digital technologies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "461",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.  By 2025, there is effective design and implementation of disaster risk reduction and climate adaptation and mitigation measures, enabling a more rational use of resources, increased resilience, and a ‘green’ economy transition",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "462",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 470, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.  By 2025, there is effective design and implementation of disaster risk reduction and climate adaptation and mitigation measures, enabling a more rational use of resources, increased resilience, and a ‘green’ economy transition",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "462",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2025, the population of Turkmenistan enjoys higher quality and inclusive health and social protection services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "463",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 473, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2025, the population of Turkmenistan enjoys higher quality and inclusive health and social protection services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "463",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2025, the education and skilling system offers all people the skills and knowledge for employment success in a diversifying economy and enhanced social integration and resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "464",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 15, 43, 473, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2025, the education and skilling system offers all people the skills and knowledge for employment success in a diversifying economy and enhanced social integration and resilience",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "464",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 29, 26, 147, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 En 2025, les institutions, menant des politiques publiques performantes et tenant compte des risques en partenariat avec les acteurs économiques et sociaux, mettent les ressources du pays au service d’un développement socioéconomique inclusif, durable, résilient et générateur d’emplois décents, particulièrement pour les plus vulnérables",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "465",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 4, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 En 2025, les institutions, menant des politiques publiques performantes et tenant compte des risques en partenariat avec les acteurs économiques et sociaux, mettent les ressources du pays au service d’un développement socioéconomique inclusif, durable, résilient et générateur d’emplois décents, particulièrement pour les plus vulnérables",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "465",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 27, 40, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2: En 2025, des institutions redevables soutenues par un cadre législatif harmonisé et des populations engagées garantissent le renforcement de l’état de droit, la protection des droits humains et la cohésion et justice sociales, particulièrement pour les plus vulnérables, conformément aux conventions et normes internationales et en complémentarité et interdépendance avec les efforts de développement inclusif et durable",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "466",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 4, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2: En 2025, des institutions redevables soutenues par un cadre législatif harmonisé et des populations engagées garantissent le renforcement de l’état de droit, la protection des droits humains et la cohésion et justice sociales, particulièrement pour les plus vulnérables, conformément aux conventions et normes internationales et en complémentarité et interdépendance avec les efforts de développement inclusif et durable",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "466",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 27, 40, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3: En 2025, les systèmes de santé, d’éducation et de protection sociale sont résilients et assurent un accès équitable et des services de qualité, particulièrement pour les plus vulnérables, et un engagement effectif de la population",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "467",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 4, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3: En 2025, les systèmes de santé, d’éducation et de protection sociale sont résilients et assurent un accès équitable et des services de qualité, particulièrement pour les plus vulnérables, et un engagement effectif de la population",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "467",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 27, 40, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4: En 2025, l’ensemble des acteurs engagés assurent une gestion équitable, transparente et durable des ressources naturelles, des écosystèmes et territoires, en améliorent la résilience/adaptation ainsi que celle des populations, notamment les plus vulnérables, face aux crises et aux risques climatiques",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "468",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 4, 707, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4: En 2025, l’ensemble des acteurs engagés assurent une gestion équitable, transparente et durable des ressources naturelles, des écosystèmes et territoires, en améliorent la résilience/adaptation ainsi que celle des populations, notamment les plus vulnérables, face aux crises et aux risques climatiques",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "468",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 27, 40, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.1 By 2025, public institutions and private sector contribute to a more inclusive, sustainable and innovative industrial and agricultural development, and equal and decent work opportunities for all, in cooperation with the social partners.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "469",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 423, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.1 By 2025, all relevant actors take measures to accelerate climate action, to promote responsible production and consumption, to improve the management of risks and threats to people, to ensure sustainable management of the environment and natural resources in urban and ecosystem hinterlands.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "470",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 427, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.1 By 2025, governance systems are more transparent, accountable, inclusive and rights-based with the participation of civil society and judiciary services' improved quality.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "471",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 427, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.2 By 2025, the effectiveness of the international protection and migration management system is improved.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "472",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 427, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2025, people, in particular disadvantaged groups, have better access to quality basic services and opportunities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "473",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2 By 2025, women and girls have improved and equal access to resources, opportunities and rights, and enjoy a life without violence and discrimination.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "474",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3 By 2025, Persons under the Law on Foreigners and International Protection are supported towards self-reliance.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "475",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 25, 430, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 : By 2025, Uganda has inclusive and accountable governance systems and people are empowered, engaged and enjoy human rights, peace, justice and security",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "476",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 317, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 : By 2025, Uganda has inclusive and accountable governance systems and people are empowered, engaged and enjoy human rights, peace, justice and security",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "476",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 780, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2.1: By 2025, people especially the marginalized and vulnerable, benefit from increased productivity, decent employment and equal rights to resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "477",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 320, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2.1: By 2025, people especially the marginalized and vulnerable, benefit from increased productivity, decent employment and equal rights to resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "477",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 780, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2.2: By 2025, Uganda’s natural resources and environment are sustainably managed and protected, and people, especially the vulnerable and marginalized, have the capacity to mitigate and adapt to climate change and disaster risks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "478",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 320, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2.2: By 2025, Uganda’s natural resources and environment are sustainably managed and protected, and people, especially the vulnerable and marginalized, have the capacity to mitigate and adapt to climate change and disaster risks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "478",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 780, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3.1: By 2025, people, especially the vulnerable and marginalized, have equitable access to and utilization of quality basic social and protection services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "479",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 320, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3.1: By 2025, people, especially the vulnerable and marginalized, have equitable access to and utilization of quality basic social and protection services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "479",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 783, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3.2: By 2025, gender equality and human rights of people in Uganda are promoted, protected and fulfilled in a culturally responsive environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "480",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 16, 52, 320, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3.2: By 2025, gender equality and human rights of people in Uganda are promoted, protected and fulfilled in a culturally responsive environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "480",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 15, 32, 5, 787, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.1. Promotes a transition towards sustainable production systems and consumption patterns, based on innovation, scientific knowledge and making use of technology, strengthening resilience and equity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "481",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.2. Consolidate economic recuperation based on the generation of quality and decent jobs, and the promotion of the entrepreneurial capacity of the private sector, in particular of small and medium businesses, increasing the participation of women in the economy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "482",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 653, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.3. Create an ecosystem for the financing of development that adopts a gender perspective and fosters public-private partnerships to achieve SDGs in Uruguay.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "483",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 653, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.1. Modernize public sector management, foster decentralization and the participation of citizens by improving accountability processes and government transparency.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "484",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 653, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.2. Strengthen the capacity of the government to prevent and address violence, protect security and promotes citizen coexistence.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "485",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.3. Strengthen institutional capacities to manage and analyze information (with an emphasis on disaggregations by sex and other key socio-demographic variables) useful for decision-making, the design and evaluation of policies and the provision of services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "486",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.1. The education system in Uruguay initiates an integral transformation of basic education (from primary to secondary school) to ensure access, avoid drop-outs, improve learning and reduce inequalities in results between socio-economic groups (including afro-descendants and persons with discapacities), and expand tertiary education, increasing the participation of women in science, technology and engineering.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "487",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.2. The Integrated National Health System improves primary health care and public-private complementarity, in the framework of the post-Covid 19 recuperation, as well as prevention programs on chronic non transmissible diseases and the quality and universality of sexual and reproductive health services, with a focus on women and youngsters from the most vulnerable groups.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "488",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.3. Create new public policy frameworks and governance models that promote social protection and cohesion as well as multidimensional well-being.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "489",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.1. National and subnational public institutions, the private sector, social actors and comunities in Uruguay make progress in eliminating the persistent gender gaps and in facilitating a sociocultural change necessary to enhance the participation of women in decision-making and to eradicate gender-based violence.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "490",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.2. Public institutions, the private sector, the civil society and families in Uruguay strengthen their capacities to protect the rights and well-being of boys, girls, teenagers and youngsters, in particular those in greatest state of vulnerability.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "491",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4.3. The Uruguayan government creates normative frameworks and strengthens institutions and policies to protect the rights of the most relegated populations groups (afrodescendents, persons with disabilities, from LGTBI groups, homeless persons, migrants and refugees) and combats the stigmatization and discrimination that they suffer.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "492",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 19, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2025 all people and groups in Uzbekistan, especially the most vulnerable, demand and benefit from enhanced accountable, transparent, inclusive and gender responsive governance systems and rule of law institutions for a life free from discrimination and violence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "493",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2025 all people and groups in Uzbekistan, especially the most vulnerable, demand and benefit from enhanced accountable, transparent, inclusive and gender responsive governance systems and rule of law institutions for a life free from discrimination and violence",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "493",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 323, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2025, the population of Uzbekistan benefits from more harmonized and integrated implementation of the reform agenda due to strengthened policy coherence, evidence-based and inclusive decisionmaking and financing for development mainstreamed in line with national SDGs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "494",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2025, the population of Uzbekistan benefits from more harmonized and integrated implementation of the reform agenda due to strengthened policy coherence, evidence-based and inclusive decisionmaking and financing for development mainstreamed in line with national SDGs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "494",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 327, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2025, youth, women and vulnerable groups benefit from improved access to livelihoods, decent work and expanded opportunities generated by inclusive and equitable economic growth.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "495",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2025, youth, women and vulnerable groups benefit from improved access to livelihoods, decent work and expanded opportunities generated by inclusive and equitable economic growth.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "495",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 327, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2025, the most vulnerable benefit from enhanced access to gender-sensitive quality health, education and social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "496",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 220, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2025, the most vulnerable benefit from enhanced access to gender-sensitive quality health, education and social services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "496",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 327, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2025, the most at risk regions and communities of Uzbekistan are more resilient to climate change and disasters, and benefit from increasingly sustainable and gender-sensitive efficient management of natural resources and infrastructure, robust climate action, inclusive environmental governance and protection.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "497",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 17, 39, 220, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2025, the most at risk regions and communities of Uzbekistan are more resilient to climate change and disasters, and benefit from increasingly sustainable and gender-sensitive efficient management of natural resources and infrastructure, robust climate action, inclusive environmental governance and protection.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "497",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 21, 4, 330, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 1.1: Al 2026, la población priorizada que habita en Venezuela se beneficiará de servicios de salud integrales y de calidad, con un enfoque de accesibilidad plena y protección de los grupos sociales más vulnerables, con énfasis en salud y nutrición materno?infantil y salud sexual y reproductiva, atendiendo a la diversidad cultural venezolana.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "498",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 1.2: Al 2026, la población priorizada, como los NNA, disfrutará del acceso pleno, permanencia y calidad de la educación inclusiva, así como otros programas de educación integral que brinden oportunidades de desarrollo con un enfoque público, universal, de accesibilidad plena y protección a mujeres y jóvenes, para potenciar el bono demográfico.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "499",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 1.3: Al 2026, la población priorizada que habita en Venezuela verá fortalecidos los sistemas de protección social inclusivos, el acceso continuado a servicios esenciales y políticas sociales focalizadas en el bono demográfico y la reducción de la pobreza, que contemplen un enfoque de derechos, desagregación espacial, perspectiva de género y diversidad, con un sistema estadístico y geográfico fortalecido.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "500",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 1.4: Al 2026, la población priorizada que habita en Venezuela verá reducido el riesgo ante desastres, la vulnerabilidad ante eventos adversos y cambios globales y mejorada la protección frente a ellos, asegurando la participación con igualdad y el liderazgo del Estado y del poder popular en las comunidades afectadas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "501",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.1: Al 2026, la población objeto se habrá beneficiado de un sistema productivo inclusivo, económica y ambientalmente sostenible, con cadenas productivas priorizadas y el desarrollo de cadenas de valor, que fomente la interrelación e innovación científico? tecnológica, e impulse la creación de trabajo digno y generación de ingresos, con especial énfasis en el empoderamiento económico de las mujeres y los/las jóvenes en favor del bono demográfico del país.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "502",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.2: Al 2026, la población que habita en Venezuela disfrutará de un sistema alimentario sostenible y una nutrición saludable, con inclusión plena de las y los productores agrícolas y desarrollo de la agricultura familiar, urbana y periurbana, que se combinan con la atención alimentaria de la población más vulnerable,como son los niños, niñas, adolescentes y mujeres, con especial atención a mujeres embarazadas y en período de lactancia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "503",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.3: Al 2026 se habrá adaptado, reducido la vulnerabilidad y mitigado los efectos del cambio climático, en particular de la población en mayor vulnerabilidad, junto con la aplicación de medidas para para la conservación de la biodiversidad, así como la gestión y recuperación ambiental urbana y rural, como base de un desarrollo respetuoso con la naturaleza.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "504",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 2.4: Al 2026 la población venezolana, con énfasis en los grupos en riesgo de quedarse atrás, se verá beneficiada por la expansión del derecho a la ciudad, contemplada en la nueva agenda urbana, y el desarrollo de ciudades resilientes en el marco de una estrategia de planificación y desarrollo territorial sustentado en la regionalización sistémica y funcional, contemplando la especialización productiva y el desarrollo del sistema urbano regional, la infraestructura, servicios y movilidad ambientalmente sostenibles.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "505",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 343, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 3.1.: Al 2026, la población que habita ejerce plenamente sus derechos sociales, civiles, laborales, económicos, ambientales, culturales y políticos en un espacio de inclusión y participación plena que fomenta la igualdad de género, así como el resguardo y promoción del patrimonio histórico y cultural nacional.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "506",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 3.2.: Al 2026, la población que habita en Venezuela se beneficia de los procesos de modernización del Estado a efectos de garantizar la máxima eficiencia en el desarrollo, gestión y acceso a la información de las políticas públicas, contemplando el fortalecimiento del sistema estadístico y geográfico nacional y de los sistemas de planes sectoriales, territoriales e institucionales.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "507",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "ED 3.3: Al 2026, la población que habita en Venezuela, en particular los grupos en riesgo de quedarse atrás, se beneficiará del fortalecimiento de las instituciones estatales de justicia, de un entorno de seguridad ciudadana y libre de violencia, con énfasis en la violencia basada de género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "508",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 6, 347, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "CF Outcome 1: Inclusive social development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "509",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 29, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "CF Outcome 2: Climate change response, disaster resilience and environmental sustainability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "510",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 29, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "CF Outcome 3. Shared prosperity through economic transformation",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "511",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 29, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "CF Outcome 4. Governance and access to justice",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "512",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 29, 480, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. By 2025, all women and men in Kosovo enjoy more accountable, effective, transparent, and gender-responsive institutions at all levels ensuring access to justice, equality and participation for all",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "513",
                UNCooperationFrameworkVersionNo = 1,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 983, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. By 2025, all women and men in Kosovo enjoy more accountable, effective, transparent, and gender-responsive institutions at all levels ensuring access to justice, equality and participation for all",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "513",
                UNCooperationFrameworkVersionNo = 2,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. By 2025, all girls and boys, women and men, particularly the most marginalised have improved access to and utilize equitable, qualitative, integrated social protection, universal health services and quality education",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "514",
                UNCooperationFrameworkVersionNo = 1,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 983, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. By 2025, all girls and boys, women and men, particularly the most marginalised have improved access to and utilize equitable, qualitative, integrated social protection, universal health services and quality education",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "514",
                UNCooperationFrameworkVersionNo = 2,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. By 2025, women and men in Kosovo, particularly youth and vulnerable groups, have increased access to decent work and benefit from sustainable and inclusive economic development that is more resilient to impacts of climate change, disasters and emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "515",
                UNCooperationFrameworkVersionNo = 1,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. By 2025, women and men in Kosovo, particularly youth and vulnerable groups, have increased access to decent work and benefit from sustainable and inclusive economic development that is more resilient to impacts of climate change, disasters and emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "515",
                UNCooperationFrameworkVersionNo = 2,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. By 2025, all communities in Kosovo, benefit equitably from inclusive engagement and greater social cohesion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "516",
                UNCooperationFrameworkVersionNo = 1,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. By 2025, all communities in Kosovo, benefit equitably from inclusive engagement and greater social cohesion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "516",
                UNCooperationFrameworkVersionNo = 2,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "5. By 2025, all women and men in Kosovo, particularly young people, vulnerable groups and displaced persons, increasingly achieve gender equality, claim their rights and fulfill civic responsibilities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "517",
                UNCooperationFrameworkVersionNo = 1,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 18, 58, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "5. By 2025, all women and men in Kosovo, particularly young people, vulnerable groups and displaced persons, increasingly achieve gender equality, claim their rights and fulfill civic responsibilities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "517",
                UNCooperationFrameworkVersionNo = 2,
                Country = "XK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 14, 37, 4, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2025, all people in South Africa, particularly women, youth and other marginalized groups, benefit justly from decent work and other social and economic opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "518",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 207, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2025, all people in South Africa, particularly women, youth and other marginalized groups, benefit justly from decent work and other social and economic opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "518",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 253, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2025, South Africa´s primary, secondary and tertiary sectors are more productive, diversified, sustainable and employment-intensive",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "519",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 210, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2025, South Africa´s primary, secondary and tertiary sectors are more productive, diversified, sustainable and employment-intensive",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "519",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1:  By 2025, all people in South Africa, especially women and girls, vulnerable and marginalized populations, have protection from violence and discrimination and enjoy improved human rights and social cohesion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "520",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 210, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1:  By 2025, all people in South Africa, especially women and girls, vulnerable and marginalized populations, have protection from violence and discrimination and enjoy improved human rights and social cohesion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "520",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2:  By 2025, all people in South Africa, particularly vulnerable and marginalized populations, enjoy improved health, nutrition and well-being",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "521",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 210, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2:  By 2025, all people in South Africa, particularly vulnerable and marginalized populations, enjoy improved health, nutrition and well-being",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "521",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.3:  By 2025, all children and young people in South Africa have equitable access to quality education relevant to a changing society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "522",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.3:  By 2025, all children and young people in South Africa have equitable access to quality education relevant to a changing society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "522",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2025, women and marginalized groups participate meaningfully in decision making processes and access justice",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "523",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2025, women and marginalized groups participate meaningfully in decision making processes and access justice",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "523",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2: By 2025, state institutions deliver effective public services to all and oversight bodies are strengthened",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "524",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2: By 2025, state institutions deliver effective public services to all and oversight bodies are strengthened",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "524",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1: By 2025, South Africa is on a just transition to a low-carbon society and vulnerable and marginalized communities adapt and are more resilient to adverse effects of climate change",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "525",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 220, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1: By 2025, South Africa is on a just transition to a low-carbon society and vulnerable and marginalized communities adapt and are more resilient to adverse effects of climate change",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "525",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 267, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2: By 2025, natural resources are managed and utilized sustainably for improved livelihoods, health and well-being of vulnerable communities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "526",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 223, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2: By 2025, natural resources are managed and utilized sustainably for improved livelihoods, health and well-being of vulnerable communities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "526",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 270, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.3: By 2025, South Africa is on course to a pollution/waste free environment, where the communities and the economy are optimally benefiting from waste",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "527",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 18, 223, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.3: By 2025, South Africa is on course to a pollution/waste free environment, where the communities and the economy are optimally benefiting from waste",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "527",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 4, 40, 270, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, all people in Zimbabwe, especially the most vulnerable and marginalised, benefit from more inclusive and sustainable economic growth with decent employment opportunities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "528",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 37, 300, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, all people in Zimbabwe, especially the most vulnerable and marginalized, benefit from greater environmental stability and robust food systems in support of healthy lives and equitable, sustainable and resilient livelihoods.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "529",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 37, 307, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, all people in Zimbabwe, especially women and girls and those in the most vulnerable and marginalised communities, benefit from equitable and quality social services and protection.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "530",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 37, 307, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "By 2026, all people in Zimbabwe, especially the most vulnerable and marginalized, benefit from more accountable institutions and systems of rule of law, human rights and access to justice.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "531",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 19, 37, 307, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 Al-Shabaab is reduced and degraded, and respect, protection, and promotion of human rights, gender equality, tolerance, climate security, and environmental governance is sustained through strengthened security and rule of law institutions and improved accountability mechanisms and legal frameworks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "532",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 Al-Shabaab is reduced and degraded, and respect, protection, and promotion of human rights, gender equality, tolerance, climate security, and environmental governance is sustained through strengthened security and rule of law institutions and improved accountability mechanisms and legal frameworks",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "532",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 230, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 Formal federal system strengthened, and state powers and service delivery effectively decentralized",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "533",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 613, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 Formal federal system strengthened, and state powers and service delivery effectively decentralized",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "533",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 233, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2 Somalis, particularly women and youth, benefit from and participate in functional, inclusive, accountable, and transparent democratic systems across all levels of government and governmental institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "534",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 613, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2 Somalis, particularly women and youth, benefit from and participate in functional, inclusive, accountable, and transparent democratic systems across all levels of government and governmental institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "534",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 233, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3 All Somalis live in a peaceful, inclusive, and cohesive society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "535",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 613, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3 All Somalis live in a peaceful, inclusive, and cohesive society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "535",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 237, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2 Accessibility and responsiveness of institutions in empowering communities to address underlying causes of insecurity and conflict as well as endemic violations of human rights and marginalization will be ensured by efficient civilian oversight of security and rule of law institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "536",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 617, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2 Accessibility and responsiveness of institutions in empowering communities to address underlying causes of insecurity and conflict as well as endemic violations of human rights and marginalization will be ensured by efficient civilian oversight of security and rule of law institutions",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "536",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 237, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.3 Rights and needs of Somali communities command the strengthening of security and rule of law institutions. Anti-corruption efforts, mitigation of duplication and stakeholders’ comparative advantages maximized",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "537",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 617, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.3 Rights and needs of Somali communities command the strengthening of security and rule of law institutions. Anti-corruption efforts, mitigation of duplication and stakeholders’ comparative advantages maximized",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "537",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 237, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 Economic governance institutions are strengthened and an enabling environment established for inclusive, sustainable, and broad-based economic growth driven by the emerging small and medium-sized enterprise sector",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "538",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 617, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 Economic governance institutions are strengthened and an enabling environment established for inclusive, sustainable, and broad-based economic growth driven by the emerging small and medium-sized enterprise sector",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "538",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 237, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 Natural resources are sustainably managed and binding constraints addressed in key productive sector value chains, leading to enduring productivity gains, increased value addition, and enhanced opportunities for decent work",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "539",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.2 Natural resources are sustainably managed and binding constraints addressed in key productive sector value chains, leading to enduring productivity gains, increased value addition, and enhanced opportunities for decent work",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "539",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.3 An integrated national programme for human capital development is established, increasing access to market-based skills for all – including the most marginalized and vulnerable groups – and safeguarding their rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "540",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.3 An integrated national programme for human capital development is established, increasing access to market-based skills for all – including the most marginalized and vulnerable groups – and safeguarding their rights",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "540",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 By 2025, more people in Somalia, especially the most vulnerable and marginalized, benefit from equitable and affordable access to government-led and -regulated quality basic social services at different state levels",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "541",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1 By 2025, more people in Somalia, especially the most vulnerable and marginalized, benefit from equitable and affordable access to government-led and -regulated quality basic social services at different state levels",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "541",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2 By 2025, the number of people impacted by climate change, natural disasters, and environmental degradation is reduced",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "542",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2 By 2025, the number of people impacted by climate change, natural disasters, and environmental degradation is reduced",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "542",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.3  By 2025, the proportion of vulnerable Somalis with scaled-up and sustained resilience against environmental and conflict-related shocks is increased, based on better management of life cycle risk, food security, and better nutrition outcomes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "543",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.3  By 2025, the proportion of vulnerable Somalis with scaled-up and sustained resilience against environmental and conflict-related shocks is increased, based on better management of life cycle risk, food security, and better nutrition outcomes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "543",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 243, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.4 By 2025, the capacities of local, national, and customary institutions and communities are strengthened to achieve durable solutions and increase the resilience, self-reliance, and social cohesion of urban communities affected by displacement",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "544",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 3, 10, 25, 50, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.4 By 2025, the capacities of local, national, and customary institutions and communities are strengthened to achieve durable solutions and increase the resilience, self-reliance, and social cohesion of urban communities affected by displacement",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "544",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 55, 46, 243, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Institutions in Jordan at national and local levels are more responsive, inclusive, accountable, transparent and resilient.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "545",
                UNCooperationFrameworkVersionNo = 1,
                Country = "JO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 9, 13, 24, 747, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: People especially the vulnerable proactively claim their rights and fulfil their responsibilities for improved human security and resilience.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "546",
                UNCooperationFrameworkVersionNo = 1,
                Country = "JO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 9, 13, 24, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Enhanced opportunities for inclusive engagement of all people living in Jordan within the social, economic, environmental, and political spheres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "547",
                UNCooperationFrameworkVersionNo = 1,
                Country = "JO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 9, 13, 24, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1 – By 2024, people in Yemen, especially women, adolescents and girls and those in the most vulnerable and marginalized communities benefit from better, equal and inclusive access to nutritious food, sustainable and resilient livelihoods and environmental stability.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "548",
                UNCooperationFrameworkVersionNo = 1,
                Country = "YE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 10, 51, 9, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2 – By 2024, people in Yemen, especially women, adolescents and girls and those in the most vulnerable and marginalized communities, experience more rights based good governance, comprised of effective, people-centred, equitable and inclusive gender and age responsive public services and rule of law.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "549",
                UNCooperationFrameworkVersionNo = 1,
                Country = "YE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 10, 51, 9, 563, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3 – By 2024, people in Yemen, especially women, adolescents, girls and those at risk of being left behind become more resilient to economic shocks by increased income security and access to decent work",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "550",
                UNCooperationFrameworkVersionNo = 1,
                Country = "YE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 10, 51, 9, 567, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4 – By 2024, people in Yemen, especially women, adolescents, girls and those at risk of being left behind, will experience strengthened social protection and social services which are people centred, evidence and needs based, equitable inclusive and gender and age responsive.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "551",
                UNCooperationFrameworkVersionNo = 1,
                Country = "YE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 3, 13, 10, 51, 9, 567, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Improved, equitable, inclusive, and safe access to quality basic services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "552",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 13, 13, 42, 3, 760, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Better access for people, especially the most vulnerable, to social protection services, sustainable livelihoods, and inclusive and equitable socio-economic recovery.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "553",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 13, 13, 42, 3, 767, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Improved living conditions of displaced people, returning refugees and affected communities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "554",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 13, 13, 42, 3, 767, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Vulnerable groups’ resilience is enhanced through increased institutional responsiveness in planning and providing services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "555",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 13, 13, 42, 3, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: Improved, equitable, inclusive, and safe access to quality basic services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "556",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 20, 14, 17, 51, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Better access for people, especially the most vulnerable, to social protection services, sustainable livelihoods, and inclusive and equitable socio-economic recovery.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "557",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 20, 14, 17, 51, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Improved living conditions of displaced people, returning refugees and affected communities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "558",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 20, 14, 17, 51, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: Vulnerable groups’ resilience is enhanced through increased institutional responsiveness in planning and providing services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "559",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 20, 14, 17, 51, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1: D’ici 2027, les institutions nationales et locales améliorent l’application des cadres légaux et réglementaires, l’efficacité, la redevabilité, l’inclusivité et l’équité de la mise en œuvre des politiques publiques pour promouvoir l’offre de services publics de qualité en faveur des communautés et des familles dans es zones cibles, y compris en situation d’urgence humanitaire.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "560",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 25, 12, 37, 58, 370, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2: D'ici à 2027, les populations notamment les femmes, les enfants, les adolescents, les jeunes (Garçons et filles) et les groupes vulnérables ont un accès accru, plus inclusif et équitable à des services sociaux de base de qualité, à un emploi décent, à la protection sociale et à la protection contre les pratiques éfastes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "561",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 25, 12, 37, 58, 377, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3:  D’ici 2027, les populations les plus vulnérables particulièrement les femmes, les filles, les garçons et les personnes avec des besoins spécifiques, améliorent : leur sécurité alimentaire et nutritionnelle, la gestion des ressources naturelles et du cadre de vie, et renforcent leur résilience aux changements limatiques y compris dans les zones affectées par les conflits et les catastrophes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "562",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 25, 12, 37, 58, 377, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 1 : Des institutions légitimes et redevables garantissent l’État de droit, la bonne gouvernance et le respect des droits humains",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "563",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 353, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 2 : La protection et la libre circulation des personnes, libéré des menaces des gangs, est assurée pour qu’ils vivent sans crainte pour leur intégrité physique et morale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "564",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 3 : Un nouveau modèle économique inclusif, équitable, vecteur d’investissements nouveaux, de croissance et de durabilité, favorable à la création rapide d’emplois décents avec un focus sur les jeunes et les femmes, capable de réduire substantiellement la pauvreté et les inégalités, est formulé, approuvé et mis en œuvre",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "565",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 4 : La population, particulièrement les groupes vulnérables et marginalisés, a un meilleur accès à des services sociaux de base équitables, inclusifs et de qualité, avec une attention particulière portée au respect des droits humains, à l'égalité de genre, et à l'inclusion du handicap, en vue du renforcement du contrat social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "566",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 5 : Des systèmes d’information et des mécanismes financiers et cadres normatifs sensibles au genre, à la protection et à l’inclusion sociale, sont mis en place pour aider les autorités étatiques, les communautés locales et les autres acteurs nationaux concernés à piloter et coordonner des politiques publiques pour renforcer la résilience basée sur la gestion des risques, la gouvernance territoriale et la gestion environnementale.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "567",
                UNCooperationFrameworkVersionNo = 1,
                Country = "HT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 26, 14, 16, 58, 363, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 1 : Des institutions légitimes et redevables garantissent l’État de droit, la bonne gouvernance et le respect des droits humains",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "568",
                UNCooperationFrameworkVersionNo = 2,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 28, 15, 58, 43, 500, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 2 : La protection et la libre circulation des personnes, libéré des menaces des gangs, est assurée pour qu’ils vivent sans crainte pour leur intégrité physique et morale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "569",
                UNCooperationFrameworkVersionNo = 2,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 28, 15, 58, 43, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 3 : Un nouveau modèle économique inclusif, équitable, vecteur d’investissements nouveaux, de croissance et de durabilité, favorable à la création rapide d’emplois décents avec un focus sur les jeunes et les femmes, capable de réduire substantiellement la pauvreté et les inégalités, est formulé, approuvé et mis en œuvre",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "570",
                UNCooperationFrameworkVersionNo = 2,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 28, 15, 58, 43, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 4 : La population, particulièrement les groupes vulnérables et marginalisés, a un meilleur accès à des services sociaux de base équitables, inclusifs et de qualité, avec une attention particulière portée au respect des droits humains, à l'égalité de genre, et à l'inclusion du handicap, en vue du renforcement du contrat social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "571",
                UNCooperationFrameworkVersionNo = 2,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 28, 15, 58, 43, 513, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 5 : Des systèmes d’information et des mécanismes financiers et cadres normatifs sensibles au genre, à la protection et à l’inclusion sociale, sont mis en place pour aider les autorités étatiques, les communautés locales et les autres acteurs nationaux concernés à piloter et coordonner des politiques publiques pour renforcer la résilience basée sur la gestion des risques, la gouvernance territoriale et la gestion environnementale.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "572",
                UNCooperationFrameworkVersionNo = 2,
                Country = "DE",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 4, 28, 15, 58, 43, 513, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, more people, especially women, youth, the most marginalized and poor, increasingly benefit from and contribute to inclusive, resilient, and sustainable socioeconomic transformation at federal, provincial, and local levels.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "573",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NP",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 5, 3, 10, 31, 42, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, more people, especially women, youth, children, and the most marginalized and poor, increasingly participate in and benefit from equitably improved quality social services at federal, provincial, and local levels.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "574",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NP",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 5, 3, 10, 31, 42, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, more people, especially women, youth, children, and the most marginalised and poor, increasingly benefit from and contribute to building an inclusive, sustainable, climate-resilient and green society and reduced impacts of disasters at federal, provincial, and local levels.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "575",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NP",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 5, 3, 10, 31, 42, 950, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, more people, especially women, youth, and the most marginalized and poor increasingly participate in and benefit from coordinated, inclusive, accessible, participatory, transparent, and gender-responsive governance, access to justice and human rights at federal, provincial, and local levels",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "576",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NP",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 5, 3, 10, 31, 42, 953, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 : « D’ici à 2026, la population béninoise, en particulier les personnes vulnérables, est résiliente aux chocs et bénéficie des fruits d’une croissance économique verte, diversifiée, durable et créatrice d’emplois décents »",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "577",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 32, 19, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2 : « D’ici à 2026, la population béninoise, en particulier les personnes vulnérables, utilise de façon continue et équitable, les services sociaux de base et de protection sociale de qualité ».",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "578",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 32, 19, 950, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet3 : « D’ici à 2026, la population béninoise, en particulier les personnes vulnérables, bénéficie d’un État de droit, de la bonne gouvernance, d’une démocratie apaisée et d’une cohésion sociale renforcée ».",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "579",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 32, 19, 950, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1. D'ici 2027, la population en Mauritanie, en particulier les plus vulnérables et marginalisés, bénéficient et participent activement à un processus national de développement durable, plus diversifié, plus résilient aux chocs économiques et environnementaux favorisant la réduction des inégalités.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "580",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 39, 20, 153, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2. D’ici 2027, la population en Mauritanie, particulièrement les populations vulnérables, accèdent aux services de base de qualité inclusifs et durables.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "581",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 39, 20, 160, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3. D’ici 2027, les jeunes filles et les femmes sont plus autonomes, résilientes et en mesure de participer activement aux dialogues et aux prises de décisions à tous les niveaux pour le développement de la Mauritanie.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "582",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 39, 20, 160, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4. D’ici 2027 la population en Mauritanie, particulièrement les plus vulnérables jouit pleinement de ses droits, dans un cadre sûr, paisible et respectueux de l’environnement.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "583",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 2, 14, 39, 20, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.1 La población, en particular la que está en situación de mayor vulnerabilidad, mejora su acceso a servicios integrales de cuidados de la salud y saneamiento, gestionados de manera coordinada, eficiente, con calidad y calidez, que garantizan su bienestar.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "584",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 617, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.2 Niñas, niños, adolescentes, jóvenes y adultos, con énfasis en las poblaciones en situación de mayor vulnerabilidad, ejercen su derecho a la educación en un sistema educativo plurinacional orientado hacia el ser humano integral, que asegure equidad, igualdad de género, calidad, pertinencia cultural y  ecnológica, así como la participación de la comunidad educativa.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "585",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.3 Los actores institucionales y de sociedad civil contribuyen en la disminución de los niveles de violencia contra las mujeres, niñas, niños, adolescentes, jóvenes y población LGTBIQ+, asegurando servicios esenciales para víctimas de violencia en razón de género y generacional, desarrollando estrategias  e prevención y transformación de los patrones socioculturales que la sostienen.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "586",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.4 Los grupos o poblaciones en situación de extrema vulnerabilidad acceden a políticas y programas sociales desarrollados por el Estado, en sus diferentes niveles, que dan respuesta a sus necesidades más urgentes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "587",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.1 Actores de la economía plural, con énfasis en los que componen los sistemas alimentarios y otros sectores productivos, incrementan su producción y su participación en los mercados con sostenibilidad y soberanía.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "588",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.2 El Estado desarrolla una gestión pública integral, promoviendo la industrialización, la sustitución de importaciones, el uso de tecnologías de información e innovación financiera, en el continuo urbano-rural.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "589",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 623, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.3 Las entidades estatales, las organizaciones sociales, en particular las naciones y pueblos indígena originario campesinos y las comunidades afrodescendientes, gestionan el territorio, los recursos naturales, el medio ambiente, los riesgos de desastres y la transición energética, a través de mecanismos  e  obernanza inclusivos, multinivel y multisectorial.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "590",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 2.4 Las mujeres, niños, niñas y jóvenes, con especial atención a la población indígena originaria campesina, afrodescendiente y población en situación de movilidad humana, ejercen plenamente sus derechos como agentes económicos en el desarrollo inclusivo del país.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "591",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 627, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.1 El Estado fortalece sus capacidades institucionales para el diseño, implementación y evaluación de políticas públicas, desarrolla herramientas de gestión e información y consolida el Estado Plurinacional con autonomías con el despliegue competencial, la coordinación y articulación territorial.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "592",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.2 El Estado consolida la democracia, representativa, participativa, comunitaria y paritaria, y fortalece su institucionalidad, la administración de justicia, los órganos de seguridad, la transparencia y la rendición de cuentas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "593",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto 3.3 El Estado y la sociedad fortalecen la cohesión social, la interculturalidad, la despatriarcalización y la transformación constructiva y pacífica de los conflictos y promueven sociedades libres de racismo y de toda forma de discriminación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "594",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 6, 14, 16, 41, 37, 630, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "595",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 33, 43, 740, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "596",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 33, 43, 747, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "597",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 33, 43, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "598",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 33, 43, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "599",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 37, 17, 23, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "600",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 37, 17, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "601",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 37, 17, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "602",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 37, 17, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "603",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 40, 3, 83, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "604",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 40, 3, 87, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "605",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 40, 3, 87, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "606",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 40, 3, 90, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "607",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 41, 36, 797, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "608",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 41, 36, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "609",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 41, 36, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "610",
                UNCooperationFrameworkVersionNo = 1,
                Country = "FJ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 41, 36, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "611",
                UNCooperationFrameworkVersionNo = 1,
                Country = "WS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 43, 6, 250, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "612",
                UNCooperationFrameworkVersionNo = 1,
                Country = "WS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 43, 6, 253, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "613",
                UNCooperationFrameworkVersionNo = 1,
                Country = "WS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 43, 6, 253, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "614",
                UNCooperationFrameworkVersionNo = 1,
                Country = "WS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 14, 43, 6, 257, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "615",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 0, 24, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "616",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 0, 24, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "617",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 0, 24, 877, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "618",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SB",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 0, 24, 877, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "619",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 3, 13, 917, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "620",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 3, 13, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "621",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 3, 13, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "622",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 3, 13, 923, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "623",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 25, 56, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "624",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 25, 56, 240, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "625",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 25, 56, 243, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "626",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TK",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 25, 56, 243, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "627",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 29, 40, 597, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "628",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 29, 40, 597, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "629",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 29, 40, 600, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "630",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 29, 40, 600, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "631",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 30, 48, 337, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "632",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 30, 48, 337, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "633",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 30, 48, 337, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "634",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 30, 48, 340, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "635",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 33, 11, 917, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "636",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 33, 11, 917, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "637",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 33, 11, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "638",
                UNCooperationFrameworkVersionNo = 1,
                Country = "NU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 33, 11, 920, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "639",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 34, 45, 533, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "640",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 34, 45, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "641",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 34, 45, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "642",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 34, 45, 537, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "643",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 36, 13, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "644",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 36, 13, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "645",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 36, 13, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "646",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 36, 13, 267, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2027, people, communities and institutions are more empowered and resilient to face diverse shocks and stresses, especially related to climate variability impacts, and ecosystems and biodiversity are better protected, managed and restored.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "647",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 37, 18, 267, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2027, more people, particularly those at risk of being left behind, benefit from more equitable access to resilient, and gender-responsive infrastructure, quality basic services, food security/nutrition and social protection systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "648",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 37, 18, 270, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2027, more people, especially those at risk of being left behind, contribute to and benefit from sustainable, resilient, diversified, inclusive and human-centred socio-economic systems with decent work and equal livelihoods’ opportunities, reducing inequalities and ensuring shared prosperity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "649",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 37, 18, 273, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2027, people enjoy and contribute to more accountable, inclusive, resilient and responsive governance systems that promote gender equality, climate security, justice and peace, ensure participation, and protect their human rights.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "650",
                UNCooperationFrameworkVersionNo = 1,
                Country = "VU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 3, 15, 37, 18, 273, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. Para 2028, más personas, especialmente niñez, mujeres, adolescentes y jóvenes tienen acceso a servicios sociales de calidad de manera equitativa y sostenible para alcanzar todo su potencial humano",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "651",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 10, 56, 19, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. Para 2028, las y los ciudadanos, especialmente los grupos excluidos, disfrutan de sus derechos y contribuyen a la eficiencia de las políticas y al desempeño de las instituciones públicas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "652",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 10, 56, 19, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. Para 2028, más personas, especialmente jóvenes, mujeres y grupos social y económicamente vulnerables, se benefician equitativamente de mayores oportunidades en una economía azul y verde, diversificada, transformadora, resiliente e inclusiva que cree empleos decentes en los sectores productivos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "653",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 10, 56, 19, 567, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. Para 2028 el país cuenta con entornos sostenibles y saludables protectores de la biodiversidad, resilientes al cambio climático y a los desastres naturales y sin deforestación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "654",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GQ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 10, 56, 19, 567, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1: D’ici 2026, les populations vivant au Togo, particulièrement les plus vulnérables, participent à l’économie et bénéficient des fruits d’une croissance inclusive, résiliente et créatrice d’emplois décents",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "655",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2 D’ici 2026, les institutions et les communautés gèrent durablement les ressources naturelles et sont plus résilientes aux effets néfastes du changement climatique et aux risques de catastrophes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "656",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 940, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3 : D’ici 2026, les populations vivant au Togo, en particulier les plus vulnérables ont un meilleur accès aux services sociaux de bases de qualité et à une protection sociale plus inclusive.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "657",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 940, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4 :D’ici 2026, les populations vivant au Togo, surtout les plus vulnérables jouissent de leurs droits et accèdent à des services publics équitables et de qualité à tous les échelons géographiques",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "658",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 5 :D’ici 2026, les institutions nationales et locales contribuent à plus d’efficacité de la gouvernance, au développement des partenariats et la cohésion sociale en vue de renforcer la sécurité, la paix et la résilience des populations",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "659",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 7, 4, 11, 4, 41, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.1 El Estado chileno adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales y políticas públicas -incluyendo las fiscales- para garantizar el acceso y goce efectivo de los derechos económicos, sociales, culturales y ambientales en el país; reducir la pobreza; acortar las brechas y desigualdades, incluyendo las territoriales, para fortalecer la resiliencia e incrementar el bienestar y la cohesión social, con enfoque integrado",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "660",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 980, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.2 Chile avanza hacia una sociedad del cuidado, mediante un sistema de protección social fortalecido, sistemas integrales de cuidados y una mejora en el acceso y la calidad de los servicios sociales, con un enfoque integrado y sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "661",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.3 El Estado chileno adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales, desarrolla institucionalidad, políticas públicas y programas para abordar la situación de movilidad humana con enfoque integrado de las personas refugiadas y migrantes, apoyando su inclusión, otorgando la debida atención a las comunidades de acogida, en línea con estándares internacionales, y la proyección internacional de Chile en la materia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "662",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.4 El Estado de Chile adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales, políticas públicas y programas para fortalecer los sistemas públicos de educación, alimentación y salud -incluyendo la salud sexual y reproductiva- a través de un abordaje participativo y con enfoque integrado; elevando su calidad, propiciando su sostenibilidad, fortaleciendo su resiliencia y ampliando su alcance a todas las personas, independiente de su condición, origen, género, etnia, nacionalidad, edad o nivel socioeconómico-, y con especial énfasis en la recuperación educativa y sanitaria post-covid.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "663",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.1 El Estado de Chile garantiza el pleno ejercicio de los derechos civiles y políticos, fortalece los mecanismos de representación, de participación cívica informada, de consulta, de diálogo social, y de incidencia de la población en decisiones que les afectan, con particular atención a las mujeres, personas LGTBIQ+ y los pueblos indígenas, y contribuye así a un pacto social inclusivo y a la prevención de conflictos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "664",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.2 El Estado de Chile lleva a cabo reformas institucionales que fortalecen la democracia, la transparencia, aumentan la confianza y representatividad de las instituciones; aseguran la paridad de género; y avanzan hacia un desarrollo territorial inclusivo y descentralizado .",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "665",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 990, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.3 El Estado de Chile promueve una sociedad pacífica e inclusiva e implementa medidas que abordan multidimensionalmente el vínculo entre seguridad y desarrollo, con énfasis en prevenir, mitigar, atender y responder a la delincuencia organizada nacional y transnacional, prevenir el delito y fortalecer la justicia penal conforme a las obligaciones de garantías de protección de DDHH.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "666",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 993, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3.1 El Estado de Chile adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales, políticas públicas y programas que permiten transitar hacia un modelo de producción y consumo sostenible, inclusivo y resiliente, con énfasis en la economía circular, a a través de una transición justa y socio-ecológica, el trabajo decente, la diversificación económica, la sostenibilidad financiera, financiamiento verde, promoviendo ciencia, tecnología e innovación y digitalización inclusiva, con enfoque integrado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "667",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 993, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3.2 Chile avanza hacia un nuevo modelo de desarrollo basado en el uso sostenible de los recursos naturales y los servicios medioambientales, la restauración y conservación de los ecosistemas y la biodiversidad, la gestión de los recursos hídricos, océanos y la protección y garantía de los derechos ambientales establecidos en el acuerdo de Escazú, con atención a los pueblos indígenas, con enfoque integrado que promueva la equidad y la generación de trabajo para todas y todos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "668",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3.3 El Estado de Chile adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales, políticas públicas y programas que permiten mejorar la adaptación y la mitigación del cambio climático, la resiliencia y la gestión de riesgos de desastres y emergencias, incluyendo daños y pérdidas, y facilitar los procesos de recuperación con enfoque integrado.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "669",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4.1 El Estado de Chile, guiado por los principios de igualdad de género y no discriminación, transversaliza el enfoque de género en todo el ciclo de formulación, implementación, seguimiento y evaluación de marcos normativos, políticas públicas, programas, servicios y presupuestos, acorde al marco internacional de derechos humanos, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "670",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 14, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4.2 El Estado de Chile adopta marcos internacionales, diseña, fortalece e implementa marcos normativos nacionales, políticas públicas y programas, que incluyan la participación de la sociedad civil, para avanzar hacia una vida libre de violencia basada en género, en particular contra las mujeres, niñas, niños, adolescentes y personas LGTBIQ+; a través de enfoques integrales centrados en la prevención, atención, denuncia y protección de las víctimas, incluyendo el abordaje de violencias de género en situaciones de emergencias, crisis y desastres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "671",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 1, 11, 43, 15, 0, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1. By the end of 2025, more people in Afghanistan, particularly the most marginalized, can equitably access essential services that meet minimum quality standards.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "672",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 46, 34, 483, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2. By the end of 2025, more people in Afghanistan, notably women and vulnerable groups, will benefit from an increasingly inclusive economy, with greater equality of economic opportunities, jobs, more resilient livelihoods, strengthened food value chains, and improved natural resources management.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "673",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 46, 34, 490, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3. By the end of 2025, more people in Afghanistan can participate in an increasingly sociallycohesive, gender-equal, and inclusive society, where the rule of law and human rights are progressively upheld, and more people can participate in governance and decisionmaking.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "674",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 46, 34, 493, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. La population, y compris les personnes vivant en situation de vulnérabilité, bénéficie des conditions nécessaires au développement d’une économie plus résiliente et diversifiée en s’appuyant sur un environnement favorable à l’investissement et au développement des entreprises durables créatrices de valeurs ajoutées et génératrices d’emplois décents",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "675",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 53, 23, 583, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. La population bénéficie d'institutions transparentes, redevables et efficaces qui garantissent leur participation et représentation effective, l’état de droit ainsi que la promotion des valeurs d’inclusivité et de non-discrimination, y compris pour les personnes vivant en situation de vulnérabilité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "676",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 53, 23, 583, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. La population, y compris personnes vivant en situation de vulnérabilité, bénéficie de services de santé de qualité, résilients y compris la prévention, d’un système éducatif et de formation de qualité et d’un système de protection sociale performant.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "677",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 53, 23, 583, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1: D’ici 2027, les populations du Burundi bénéficient de systèmes alimentaires inclusifs et durables",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "678",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2: D’ici 2027, les populations du Burundi, notamment les plus vulnérables, bénéficient d’un système de gouvernance renforcée et d’une économie plus diversifiée et inclusive, y compris à travers une intégration régionale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "679",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3: D’ici 2027, les populations du Burundi, pour chaque tranche d’âge, notamment les plus vulnérables, utilisent un système de protection social adapté",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "680",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4: D’ici 2027, les populations du Burundi, notamment les enfants, les jeunes, les femmes et les plus vulnérables ont un accès équitable et de qualité aux services sociaux de base adaptés au cycle de vie",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "681",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 5: D’ici 2027, les populations du Burundi bénéficient de meilleures pratiques de gestion de l’environnement et des ressources naturelles, y compris des capacités d’adaptation et des systèmes de préparation et de réponse aux chocs d’origines naturelle et humaine",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "682",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BI",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 10, 59, 30, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efeito 1.1: Até 2027, mais pessoas, especialmente as que vivem em situações mais vulneráveis e marginalizadas e aquelas que vivem com deficiência, terão acesso equitativo e utilização de serviços sociais essenciais e de protecção de alta qualidade, resilientes, sensíveis ao género e modernos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "683",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 4, 42, 983, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efeito 2.1: Até 2027, uma economia mais sustentável, inclusiva, diversificada e integrada gerará empregos decentes, segurança alimentar e nutrição para os mais vulneráveis, especialmente as mulheres, os jovens e os mais pobres.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "684",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 4, 42, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efeito 2.2: Até 2027, os principais ecossistemas marinhos e terrestres e a biodiversidade estarão melhor protegidos, restaurados e geridos de forma mais sustentável, e a resiliência aos choques e impactos das alterações climáticas será melhorada e mais sensíveis às questões do género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "685",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 4, 42, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efeito 3.1: Até 2027, mais pessoas, especialmente as que vivem em situações mais vulneráveis e marginalizadas e aquelas que vivem com deficiência, terão acesso equitativo e utilização de serviços sociais essenciais e de protecção de alta qualidade, resilientes, sensíveis ao género e modernos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "686",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CV",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 4, 42, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 By 2027, strengthened human capital through equal access to quality services, social protection and social justice ensured for all people",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "687",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 700, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2 By 2027, enhanced people-centred inclusive and environmentally sustainable economic development driven by productivity growth, industrialization decent jobs, digitalization and integrating the informal economy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "688",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 700, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 By 2027, enhanced climate resilience and efficiency of natural resource management for all people in a sustainable environment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "689",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4 By 2027, people have improved, safe and equal access to information, protection, justice and a peaceful and inclusive society through transparent, accountable, participatory, effective and efficient governance based on the rule of law and international norms and standards.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "690",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 5 By 2027 women and girls realize their rights in social health and livelihood spheres as laid out in the Egyptian Constitution, and their leadership and empowerment are guaranteed in a society free of all forms of discrimination and violence against women and girls.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "691",
                UNCooperationFrameworkVersionNo = 1,
                Country = "EG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 10, 56, 707, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2027, all people in Zambia, including the marginalized and vulnerable groups, benefit from an inclusive, resilient and sustainable economy that provides equitable, diverse and sustainable opportunities for decent jobs, livelihoods and businesses",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "692",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 21, 53, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2027, all people in Zambia, including the marginalized and vulnerable groups, have equitable access to and utilization of quality, inclusive and gender- and shock-responsive universal social services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "693",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 21, 53, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2027, all people, including the marginalized and vulnerable groups, participate in and benefit from sustained peace, democracy, human rights, the rule of law, justice, non-discrimination, equality and inclusive and transformative governance",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "694",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 21, 53, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2027, ecosystems are healthier, and more people, including the marginalized and vulnerable groups, are more resilient and contribute to and benefit from the sustainable management and use of natural resources and environmental services, and more effective responses to climate change, shocks and stresses",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "695",
                UNCooperationFrameworkVersionNo = 1,
                Country = "ZM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 11, 21, 53, 660, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 1. En 2025 la población se habrá beneficiado de un modelo productivo ambiental y económicamente sostenible, y socialmente inclusivo, que promoverá las inversiones de triple impacto, las exportaciones y la diversificación, fomentará la interrelación científico-tecnológica e industrial, e  incorporará el enfoque de futuro del trabajo centrado en las personas y en el enfoque de género, de modo de favorecer el aumento de la productividad y la generación de empleo.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "696",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 640, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 10. Al 2025, el país habrá fortalecido la promoción, protección y respeto del ejercicio de los derechos humanos basados en principios de igualdad de género y de equidad social y geográfica respecto de todas las personas, y habrá fortalecido las instituciones del Estado a nivel nacional y  ubnacional de modo de lograr eficiencia y eficacia en la gestión pública y en la promoción de políticas de acceso a la justicia y la seguridad ciudadana. También habrá garantizado el acceso a la información pública, la gestión del conocimiento y la participación efectiva e inclusiva.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "697",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 657, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 11 . En 2025 las personas, grupos y organizaciones habrán fortalecido su conocimiento sobre el ejercicio pleno de los derechos humanos y habrán aumentado su participación en espacios de involucramiento público. Así, habrán incrementado sus niveles de incidencia y de representación política para el cumplimiento de los ODS, sin discriminación de ninguna índole.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "698",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 667, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 12. Al 2025, la población en la Argentina habrá visto garantizados sus derechos, a través de la ampliación y la mejora del desempeño del Estado en la elaboración e implementación de marcos normativos y políticas alineados con los compromisos y estándares internacionales de derechos  umanos. Así también habrá consolidado su rol de cooperador en el desarrollo sostenible.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "699",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 670, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 2. En 2025 la población en situación de mayor vulnerabilidad habrá mejorado sus condiciones de acceso al mercado laboral, así como se habrán implementado medidas hacia la formalización progresiva del trabajo informal y la prevención de la destrucción de empleo formal y la eliminación del trabajo forzoso y del trabajo infantil, y se habrán fortalecido las políticas de empleo, educación y formación, con un enfoque de género, de trayectoria de vida y de derechos humanos.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "700",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 680, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 3. En 2025 el país habrá fortalecido sus alianzas y sus capacidades para el direccionamiento y la gestión del financiamiento del desarrollo sostenible, habrá puesto el énfasis en inversiones en las zonas más rezagadas del país a través de mecanismos financieros que consideren el triple impacto económico, social y ambiental, y habrá orientado dicho financiamiento a las poblaciones en situaciones de mayor vulnerabilidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "701",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 680, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 4. Al 2025 el país habrá fortalecido sus sistemas de protección social basado en evidencia para reforzar el enfoque de derechos y la perspectiva de género, ajustar su diseño y planificación, reducir su fragmentación, garantizar el acceso a servicios básicos de calidad y asegurar niveles de  restación suficientes para reducir la pobreza y la pobreza extrema en todo el territorio.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "702",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 690, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 5. En 2025 el país habrá mejorado los marcos normativos, políticos y fiscales: los habrá hecho sensibles a los enfoques de derechos y género de forma de garantizar una oferta inclusiva y de calidad de servicios sociales básicos de salud, seguridad alimentaria, educación, cuidado, protección, vivienda y justicia. Estos marcos serán respetuosos con el ambiente y con los aspectos culturales, y darán especial atención a los territorios y poblaciones más rezagadas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "703",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 693, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 6. En 2025 el país habrá fortalecido la resiliencia de sus instituciones y de su población: habrá generado innovaciones para acelerar la recuperación social y económica de las poblaciones más afectadas por la pandemia de COVID19 y habrá consolidado las oportunidades y avances generados  urante la pandemia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "704",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 697, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 7. En 2025, la población de Argentina se habrá beneficiado del avance del país en la implementación de sus marcos normativos ambientales, basados en el fortalecimiento de políticas para la acción climática, la prevención, reducción y control de la contaminación, la gestión de residuos, la  gestión de riesgos de desastres, la energía y el desarrollo sostenibles, y la producción y el consumo sostenibles. Se impulsarán soluciones basadas en la naturaleza o basadas en el enfoque ecosistémico y la evidencia científica, y se incorporarán los enfoques de derechos humanos, Intersectorialidad y género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "705",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 700, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 8. Al 2025, las comunidades han mejorado su capacidad de resiliencia frente al cambio climático, especialmente las comunidades indígenas, las comunidades rurales, los barrios vulnerables, las mujeres, niños, niñas, adolescentes y jóvenes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "706",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 700, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 9. Al 2025, el país habrá adoptado mecanismos de participación dinámicos, y habrá fortalecido las capacidades de sus comunidades y municipios, de las organizaciones públicas, privadas y sindicales e instituciones científicas y de investigación, en materia de derechos ambientales y prevención  de conflictos socioambientales, mediante la creación de instancias de colaboración interinstitucional efectivas y de diálogo social, con un enfoque de género y de derechos humanos",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "707",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 8, 14, 44, 40, 703, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, communities, especially the most disadvantaged , demand for and benefit from an inclusive, universal, affordable, accessible, accountable, and quality health care services, while adopting positive health practices.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "708",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027, all people, including children, women and marginalized populations, have increased access to and consumption of adequate, affordable and diverse nutritious food and quality services year-round",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "709",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 53, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, all children and young people, especially the most vulnerable, have equitable access to quality learning and skills development within safe and inclusive education environments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "710",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 57, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, people will benefit from and contribute to sustainable and inclusive growth through higher productivity, competitive - ness and diversification in economic activities that create decent work, liveli - hoods and income, partic - ularly for youth and women",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "711",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2027, Government of India, state governments, communities, the private sector and other actors take informed actions to address climate change, pollution and biodiversity loss and restore ecological integrity through improved knowledge, capacity and mainstreaming of relevant actions across sectoral programmes, policies and plans",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "712",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 63, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 6: By 2027, a strengthened and more coordinated, inclusive and accountable governance system is in place at the national and local levels enabling all people, especially most marginalized and vulnerable, to be protected, empowered, engaged and enjoy human rights and social justice, and lead their lives with respect and dignity.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "713",
                UNCooperationFrameworkVersionNo = 1,
                Country = "IN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 8, 16, 16, 4, 43, 100, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2028, women and girls in Papua New Guinea, especially the most marginalized and vulnerable, exercise their rights and agency and live a life free from all forms of discrimination and violence.in Papua New Guinea, especially the most marginalized and vulnerable, exercise their rights and agency and live a life free from all forms of discrimination and violence.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "714",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 350, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2028, people in Papua New Guinea, especially the most marginalized and vulnerable, participate in and benefit from more accountable, gender responsive, inclusiveand transparent governance that promotes peace, security, equality and social cohesion.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "715",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 357, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2028, people in Papua New Guinea, especially the most marginalized, benefit from gender sensitive, shock responsive, rights based and quality basic and social services, and equitably realize their full potential to meaningfully contribute to PNG development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "716",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 357, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2028, people in Papua New Guinea, especially the most vulnerable and marginalized, benefit from improved and sustainable livelihoods and expanded access to diversified economic opportunities that deliver inclusive and green growth.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "717",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2028, people in Papua New Guinea, especially the most marginalized and vulnerable, benefit from equitable and participatory access to climate resilient services that improve livelihoods and protect natural resources.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "718",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PG",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 9, 14, 11, 50, 15, 360, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1 El Estado fortalece una respuesta integral a la reducción de la pobreza y las desigualdades con enfoque multidimensional, de derechos, igualdad de género y territorial, sin dejar a nadie atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "719",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2 La población, en particular la que se encuentra en situación de mayor vulnerabilidad, tiene mayor acceso a bienes y servicios sociales de calidad, universales, inclusivos y resilientes, sin discriminación en todo el territorio.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "720",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 217, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3 La población, con énfasis en las mujeres, jóvenes y otros grupos en situación de vulnerabilidad, tiene mayor acceso a oportunidades inclusivas de empleo productivo, trabajo decente y medios de vida sostenibles y resilientes, en un contexto de transformación económica y digital que impulsa mayor productividad, competitividad e innovación, sin dejar a ningún territorio atrás.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "721",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 223, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4 Las personas, comunidades, instituciones nacionales y locales, y sectores estratégicos gestionan con enfoque multidimensional el riesgo de desastres, afrontan los desafíos del cambio climático y promueven la gestión sostenible de los ecosistemas y los asentamientos humanos urbanos y rurales, en un entorno resiliente, inclusivo, con igualdad de género, responsable con el planeta y las generaciones futuras.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "722",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 230, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 5 Las personas que se encuentran en el territorio, especialmente las mujeres, niñas y adolescentes y otros grupos en situación de vulnerabilidad, tienen mayor acceso a una protección integral de sus derechos humanos, y a entornos libres de violencia y discriminación en todas sus manifestaciones.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "723",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 233, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 6 El Estado fortalece la gobernabilidad democrática y la participación ciudadana, promueve la gestión pública eficiente y transparente, mejora el acceso a la justicia con igualdad, y combate la corrupción y el crimen organizado, en todo el territorio.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "724",
                UNCooperationFrameworkVersionNo = 1,
                Country = "DO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 50, 37, 260, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome A1. By 2026 there is increased and more equitable investment in people, removing barriers and creating opportunities for those at risk of exclusion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "725",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 57, 14, 170, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome B1. By 2026 innovative and integrated policy solutions accelerate sustainable, productive and inclusive economic development, enhancing climate change adaptation and mitigation and transition to a green and blue economy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "726",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 57, 14, 177, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome C1. By 2026, governance is more transparent and accountable, enabling people (women and girls, men and boys, and persons at risk of exclusion), to enjoy quality, inclusive services, enhanced rule of law and access to justice in line with Albania’s human rights commitments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "727",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 57, 14, 177, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome C2. By 2026, gender responsive governance strengthens equality and non-discrimination, promotes women’s empowerment and human rights, and reduces violence against women and children",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "728",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 9, 57, 14, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2025, people beneft from resilient, inclusive and sustainable growth ensured by the convergence of economic development, and management of environment and cultural resources",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "729",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 647, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. By 2025, people beneft from more inclusive and higher quality educational programmes focused on 21st century skills for enhanced employability, well-being and active participation in society",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "730",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 647, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2025, people have access to better quality and inclusive health and social protection systems",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "731",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2025, people contribute to, and beneft from more accountable and transparent governance systems that deliver quality public services, and ensure rule of law",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "732",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2025, there is stronger mutual understanding, respect and trust among individuals and communities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "733",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 2, 49, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, more people in Bangladesh, particularly the most vulnerable and marginalized from all gender and social groups, and those from lagging districts benefit from sustainable livelihood and decent work opportunities resulting from responsible, inclusive, sustainable, green28, and equitable economic development.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "734",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026 people, in particular the most vulnerable and marginalized, have improved access to and utilization of quality, inclusive, gender- and shock-responsive, universal and resilient social protection, social safety-net and basic social services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "735",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 183, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, ecosystems are healthier, and all people, in particular the most vulnerable and marginalized in both rural and urban settings, benefit from and contribute to, in a gender-responsive manner, a cleaner environment, an enriched natural resource base, low carbon development, prosperous and are more prosperous and resilient to climate change, shocks and disasters",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "736",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 187, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2026, more people, especially the most vulnerable, benefit from more equitable, non-discriminatory, gender-responsive, participatory, accountable and governance and justice, in a peaceful and tolerant society governed by the rule of law",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "737",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 187, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5: By 2026, more women, girls and sexual minorities benefit from an environment in which they are empowered to exercise their rights, agency and decision-making power over all aspects of their lives towards a life are free from all forms of discrimination, violence and harmful norms and practices.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "738",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 7, 5, 187, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4. By 2023, Bhutan’s communities and its economy are more resilient to climate-induced and other disasters and biodiversity loss as well as economic vulnerability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "739",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 11, 30, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome One By 2023, there is enhanced access to and use of reliable and timely data for inclusive and evidence-based policy and decision-making",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "740",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 11, 30, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME Three. By 2023, national stakeholders strengthened to provide equal opportunities for all, particularly women and vulnerable groups",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "741",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 11, 30, 940, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome Two. By 2023, vulnerable and unreached people access and receive quality health, nutrition, protection, education, water sanitation and hygiene services",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "742",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 11, 30, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. By 2026, gender inequality is reduced, and women and girls are empowered to access their human rights and participate in and benefit from inclusive development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "743",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. by 2026, all people, particularly vulnerable and marginalized groups, have equitable access to quality services of education, health, nutrition and social protection",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "744",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 873, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. By 2026, Botswana sustainably uses and actively manages its diverse natural resources, improves food security and effectively addresses climate change vulnerability",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "745",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 877, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2026, Botswana has strengthened resilience to shocks and emergencies, and is on a sustainable, equitable economic trajectory, reducing levels of inequality, poverty and unemployment",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "746",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 877, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 5. By 2026, Botswana is a just society, where leaders are accountable, transparent and responsive, corruption is reduced, and people are empowered to access information, services and opportunities and participate in decisions that affect their lives and livelihoods",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "747",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 22, 15, 880, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1 : d’ici 2024, la prévalence de la violence et des conflits armés est réduite et la sécurité des personnes et des biens est améliorée en particulier celle des personnes vulnérables, y compris les réfugiés/déplacés, des femmes et les jeunes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "748",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 507, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.2 : D’ici 2024, les populations vivant en RDC, plus spécifiquement les plus vulnérables (femmes, enfants, réfugiées et déplacées) jouissent de leurs droits humains, en particulier l’accès équitable à la justice, (y compris la justice juvénile), à l’identité juridique et la protection , à travers le renforcement des  ystèmes judiciaire, sécuritaire, des capacités de veille des organisations de la société civile sur les droits humains et la redevabilité institutionnelle",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "749",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.3 : D’ici 2024, les institutions publiques, les media et la société civile, au niveau central et décentralisé exercent efficacement leurs rôles pour une gouvernance démocratique apaisée, efficace et inclusive, porteuse d’effets sur la participation citoyenne et le renforcement de l’état de droit",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "750",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 510, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.1 : D’ici 2024, les populations congolaises jouissent d’une croissance économique inclusive durable portée par la transformation agricole, la diversification économique ouverte aux innovations et à la promotion de l’entrepreneuriat des jeunes et des femmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "751",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 513, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.2. : D’ici 2024, les populations vivant en RDC bénéficient d’une protection sociale inclusive et d’un dividende démographique portée par la maitrise démographique et l’autonomisation des jeunes et des femmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "752",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 513, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.3 : D’ici 2024, les populations tirent profit d’une gestion responsable et durable des ressources naturelles (forestières, minières, et foncières), par l’État, les entités décentralisées, les communautés, et le secteur privé, dans un contexte de changement climatique et de préservation de la biodiversité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "753",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 517, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 : Les populations, en particulier les plus vulnérables bénéficient d’un accès équitable, de qualité et durable aux services sociaux de base, y compris de lutte contre le VIH/SIDA",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "754",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 517, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.2 : d’ici 2024, les organisations humanitaires et structures gouvernementales chargées des questions humanitaires apportent une réponse humanitaire coordonnée, rapide et efficace envers les personnes affectées par les crises dans le respect des standards et principes humanitaires en vue de réduire la surmortalité et la sur-morbidité des personnes affectées",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "755",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 10, 42, 5, 520, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 1: Gobiernos e instituciones nacionales y locales implementan estrategias de desarrollo integral territorial sostenibles, equitativas e inclusivas en el marco del proceso de descentralización.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "756",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 54, 47, 577, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 2 Sectores estratégicos de prioridad nacional logran niveles más elevados de productividad económica y de aprovechamiento del potencial humano mediante la diversificación, la modernización tecnológica y la innovación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "757",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 54, 47, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO DIRECTO 3 Instituciones, sectores productivos y de servicios, gobiernos territoriales y comunidades mejoran la protección y uso racional de los recursos naturales y de los ecosistemas, la resiliencia al cambio climático y la gestión integral de reducción de riesgos de desastre.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "758",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 54, 47, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 4 Mejorada la accesibilidad y calidad de los servicios públicos y los sistemas de protección social y de cuidados, considerando la dinámica demográfica, con énfasis en grupos en condiciones de vulnerabilidad, con enfoque de género y derechos humanos",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "759",
                UNCooperationFrameworkVersionNo = 2,
                Country = "CU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 12, 54, 47, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2025, women, men and youth, including marginalized persons, contribute to and benefit from economic progress, through greater access to decent employment, equitable social economic opportunities, sustainable enterprise opportunities as well as resilient, financially sustainable social protection systems",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "760",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 16, 53, 790, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2025, all children, adolescent, young people, men and women including marginalized persons, benefit from equitable, effective and efficient quality social services, lifelong learning and market-relevant skills.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "761",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 16, 53, 790, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2025, oversight bodies and government institutions at national and regional levels operate in an independent, participatory and accountable manner, ensuring equal access to justice and services, with a systematic, participatory implementation and reporting mechanism for human rights obligations and SDGs, with a focus on leaving no one behind.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "762",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 16, 53, 793, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2025, Eswatini is on an inclusive low-carbon development pathway that is resilient to climate change and in which natural resources are managed sustainably, and community adaptation to climate change is enhanced, for improved livelihoods, health and food security, especially for vulnerable and marginalized communities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "763",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 16, 53, 793, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 1.1: Para el año 2025, el Estado guatemalteco incrementa el acceso de la población priorizada a trabajo digno y decente, medios productivos, y servicios económicos a nivel nacional y local, adecuados para la competitividad y el clima de negocios, en un marco de desarrollo socioeconómico inclusivo, sostenible y sustentable.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "764",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.1: Para el año 2025 se amplía el acceso a la vivienda digna / adecuada y a los servicios básicos para la población priorizada, en un marco de ordenamiento territorial y el mejoramiento rural y urbano integral, con especial énfasis en los asentamientos informales y comunidades marginales, contribuyendo a la inclusión social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "765",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.2: Para el año 2025 las instituciones del Estado avanzan en el diseño e implementación de un sistema integral de protección social, contributivo y no contributivo, buscando mayor cobertura y calidad con equidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "766",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.3: Para el año 2025 la población priorizada tiene mayor acceso a una educación inclusiva, equitativa, pertinente, sostenible y de calidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "767",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.4: Para el año 2025 la población, especialmente la priorizada, en las diferentes etapas del curso de vida, tiene una mayor cobertura y acceso a servicios de salud esenciales (definida como el promedio de la cobertura de servicios esenciales sobre la base de intervenciones trazadoras, por ejemplo la salud reproductiva, materna, neonatal e infantil, las enfermedades infecciosas, las enfermedades no transmisibles y la capacidad de los servicios y el acceso a ellos, entre la población general y los más desfavorecidos) buscando que sean integrados, integrales, pertinentes y de calidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "768",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 2.5: Para el año 2025 la población priorizada mejora su seguridad alimentaria y nutrición.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "769",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 940, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3.1: Para el año 2025 las instituciones del Estado fortalecidas incrementan la seguridad ciudadana, el acceso a la justicia y la transformación de conflictos, buscando mayor coordinación a nivel nacional y local.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "770",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 940, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 3.2: Para el año 2025 las instituciones del Estado mejoran el acceso a la justicia, la reparación digna y transformadora, la protección integral y la prevención de la violencia en contra de las mujeres, jóvenes, adolescentes y la niñez.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "771",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4.1: Para el año 2025 las instituciones del Estado mejoran la gobernanza democrática, la gestión eficiente y transparente de los recursos, y la toma de decisiones basada en evidencia, incluyendo el uso de tecnologías de la información y comunicación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "772",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 943, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4.2: Para el año 2025 las instituciones del Estado mejoran la asistencia y protección a personas que migran, retornan, transitan o se desplazan forzosamente dentro del país o fuera de sus fronteras, incluyendo a personas que requieren de protección internacional.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "773",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 4.3: Para el año 2025 la población priorizada tiene mayor acceso en condiciones de igualdad y seguridad a espacios de participación política y cívica a nivel nacional y local, y promoviendo desde los diferentes espacios el desarrollo urbano y rural integral.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "774",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFECTO 5.1: Para el año 2025 el Estado de Guatemala fortalece sus políticas, estrategias y programas que promueven la mitigación y adaptación al cambio climático, la gobernanza de los territorios, recursos naturales y ecosistemas; mejorando la gestión integral de los riesgos ambientales, climáticos, sanitarios, hidrológicos y geodinámicos, con enfoque integrado, garantizando el uso y manejo sostenible de los recursos naturales, con énfasis en los grupos de población y territorios más vulnerables.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "775",
                UNCooperationFrameworkVersionNo = 2,
                Country = "GT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 13, 49, 11, 950, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. By 2026, people, especially the most vulnerable and marginalized, will have more equitable and inclusive access to and will benefit from better quality health, nutrition, food, shelter, protection, water, sanitation, and hygiene (WASH), and education and learning, including during emergencies.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "776",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 1, 19, 387, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. By 2026, people, especially the most vulnerable and marginalized, will benefit from more inclusive, resilient, transformative, and sustainable socio-economic and demographic opportunities to reduce poverty and inequalities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "777",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 1, 19, 390, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. By 2026, people, especially the most vulnerable and marginalized, will be better served by public institutions at all levels in a transparent and inclusive manner, able to exercise their rights and obligations and the institutions shall be strengthened and more accountable while the rule of law and international human rights commitments made by Lao PDR are upheld.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "778",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 1, 19, 390, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. By 2026, people, especially the most vulnerable and marginalized, and institutions will be better able to sustainably access, manage, preserve, and benefit from natural resources and promote green growth that is risk-informed, disaster and climate-resilient.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "779",
                UNCooperationFrameworkVersionNo = 2,
                Country = "LA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 15, 1, 19, 390, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. Al 2025, Panamá propicia un desarrollo sostenible e inclusivo: asegura el acceso equitativo a los servicios esenciales y medios de vida para todas las personas; promueve la inclusión, la innovación, la competitividad, el desarrollo industrial y el emprendedurismo, con enfoque territorial y de derechos humanos",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "780",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 16, 2, 6, 810, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. Al 2025, Panamá cuenta con una gobernanza participativa e instituciones nacionales y subnacionales inclusivas, eficaces, transparentes, que cumplen la ley al servicio de las personas, articuladas entre sí y en alianza con actores no gubernamentales; con enfoque de derechos humanos, intercultural, de género, urso de vida, territorial, y sin dejar a nadie atrás",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "781",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 16, 2, 6, 813, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. Al 2025, Panamá es resiliente y cuenta con políticas públicas implementadas para la adaptación y mitigación del cambio climático, la neutralidad de la degradación de la tierra, la protección de la biodiversidad, la gestión ambiental integrada y la reducción de riesgo de desastres y crisis sanitarias, con enfoque Territorial, intercultural, de derechos humanos, de género, y curso de vida.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "782",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 16, 2, 6, 827, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. Al 2025, Panamá cuenta con un sistema de protección de derechos inclusivo e integral con especial énfasis en la prevención y atención de todas las formas de violencia y discriminación por motivos de género, curso de vida, y sensible a todas las personas en condiciones de vulnerabilidad",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "783",
                UNCooperationFrameworkVersionNo = 2,
                Country = "PA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 13, 16, 2, 6, 837, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1. D’ici 2028, les systèmes de production y compris les systèmes alimentaires sont organisés de manière à stimuler l'entreprenariat, l'innovation technologique et assurer l'emploi décent aux populations notamment les plus vulnérables y compris les jeunes, femmes, les personnes handicapées, les migrants, en milieu rural et périurbain",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "784",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 34, 46, 23, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2. D'ici à 2028, les systèmes de protection sociale et des services sociaux de base assurent un accès équitable inclusif et de qualité aux personnes les plus vulnérables notamment celles vivant dans zones enclavées, périurbaines, rurales ou frontalières",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "785",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 34, 46, 27, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3. D'ici à 2028, les politiques publiques inclusives et transparentes, permettent aux populations d'être plus résilientes et d'accéder aux ressources naturelles et aux services publics de manière équitable.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "786",
                UNCooperationFrameworkVersionNo = 2,
                Country = "SN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 11, 34, 46, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1 Thailand’s transformation into an inclusive economy based on a green, resilient, low-carbon, sustainable development is accelerated.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "789",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 31, 47, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2 Human capital needed for social and inclusive development is improved through strengthening of institutions, partnerships, and the empowerment of people",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "790",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 31, 47, 930, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3 People living in Thailand, especially those at risk of being left furthest behind, are able to participate in and benefit from development, free from all forms of discrimination.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "791",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 12, 31, 47, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2025, nutrition, food security and agricultural productivity have improved for all, irrespective of the individual ability, gender, age, socio-economic status and geographical location",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "792",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 723, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2025, institutions and people throughout Timor-Leste in all their diversity, especially women and youth, benefit from sustainable economic opportunities and decent work to reduce poverty",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "793",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 723, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2025, all people of Timor-Leste, regardless of gender identity, abilities, geographic location and particular vulnerabilities, have increased access to quality formal and innovative learning pathways (from early childhood through life-long learning) and acquire foundational, transferable, digital and job-specific skills",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "794",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 727, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2025, the people of Timor-Leste increasingly demand and have access to genderresponsive equitable, high-quality, resilient and inclusive primary health care and strengthened social protection, including in time of emergencies",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "795",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 727, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 5: By 2025, the most excluded people of Timor-Leste are empowered to claim their rights, including freedom from violence, through accessible, accountable and gender-responsive governance systems, institutions and services at national and sub-national levels",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "796",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 730, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 6: By 2025, national and sub-national institutions and communities (particularly at-risk populations including women and children) in Timor-Leste are better able to manage natural resources and achieve enhanced resilience to climate change impacts, natural and human-induced hazards, and environmental degradation, inclusively and sustainably",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "797",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 18, 24, 730, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1: By 2025, people, in particular disadvantaged groups, have better access to quality basic services and opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "798",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 957, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.2: By 2025, women and girls have improved and equal access to resources, opportunities and rights, and enjoy a life without violence and discrimination.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "799",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 963, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.3: By 2025, Persons under the Law on Foreigners and International Protection are supported towards self-reliance.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "800",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 963, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1: By 2025, public institutions and private sector contribute to a more inclusive, sustainable and innovative industrial and agricultural development, and equal and decent work opportunities for all, in cooperation with the social partners.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "801",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 970, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1: By 2025, all relevant actors take measures to accelerate climate action, to promote responsible production and consumption, to improve the management of risks and threats to people, to ensure sustainable management of the environment and natural resources in urban and ecosystem hinterlands.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "802",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 970, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.1: By 2025, governance systems are more transparent, accountable, inclusive and rights-based with the participation of civil society, and judiciary services are improved quality.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "803",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 970, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4.2: By 2025, the effectiveness of the international protection and migration management system is improved",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "804",
                UNCooperationFrameworkVersionNo = 2,
                Country = "TR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 14, 31, 52, 973, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 1.1. Al 2025 Uruguay habrá promovido una transición hacia sistemas de producción y consumo sostenibles, basados en la innovación, el conocimiento científico y la incorporación de tecnología, fortaleciendo la resiliencia y la equidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "805",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 117, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 1.2. Al 2025 Uruguay habrá consolidado la recuperación económica basada en la generación de empleo de calidad, el trabajo decente y la promoción de la capacidad emprendedora del sector privado, en particular de las pequeñas y medianas empresas, aumentando la participación de las mujeres  en la economía.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "806",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 120, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 1.3. Al 2025 Uruguay habrá constituido un ecosistema para la financiación del desarrollo con perspectiva de género que propicia el desarrollo de alianzas públicas/privadas para el logro de los ODS en Uruguay.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "807",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 127, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 2.1. Al 2025 Uruguay habrá modernizado la gestión pública, impulsado la descentralización y promovido la participación ciudadana en el territorio ampliando los mecanismos de rendición de cuentas y la transparencia del Estado",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "808",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 130, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 2.2. Al 2025 Uruguay habrá fortalecido las capacidades del Estado para prevenir y responder a la violencia, proteger la seguridad y promover la convivencia ciudadana.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "809",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 133, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 2.3. Al 2025 el Estado uruguayo habrá fortalecido sus capacidades institucionales para gestionar y analizar información (con énfasis en las desagregaciones por sexo y otras variables sociodemográficas clave) aplicable a la toma de decisiones, el diseño y evaluación de políticas y la provisión de servicios.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "810",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 3.1. Al 2025 el sistema educativo uruguayo habrá puesto en marcha una transformación integral de la educación básica (desde inicial a media) para asegurar el acceso y la permanencia, mejorar los aprendizajes y reducir la inequidad en los resultados entre estratos socioeconómicos y grupos de la población (afrodescendientes y personas con discapacidad), y expandir la educación terciaria/universitaria en los jóvenes, incrementando la participación de las mujeres en ciencia, tecnología e ingenierías.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "811",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 3.2. Al 2025 el Sistema Nacional Integrado de Salud de Uruguay habrá fortalecido la estrategia de atención primaria en salud y la complementación público-privada, en el marco de la recuperación post-COVID 19, así como los programas de prevención de las enfermedades crónicas no transmisibles y los servicios de salud sexual y reproductiva de calidad y acceso universal, con foco en mujeres y jóvenes de los grupos más vulnerables.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "812",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 150, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 3.3. Al 2025 Uruguay habrá generado nuevas arquitecturas de políticas públicas y modelos de gobernanza para promover la cohesión y la protección social y el bienestar multidimensional.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "813",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 157, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 4.1. Al 2025 las instituciones públicas nacionales y subnacionales, el sector privado, los actores sociales y las comunidades en Uruguay habrán avanzado en eliminar las persistentes brechas de género y en el cambio sociocultural necesario para ampliar la participación de las mujeres en la toma de decisiones y la erradicación de la violencia basada en género.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "814",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 160, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 4.2. Al 2025 las instituciones públicas, el sector privado, la sociedad civil y las familias en Uruguay habrán fortalecido sus capacidades para proteger los derechos y el bienestar de niños, niñas, adolescentes y jóvenes, en particular de aquellos en condición de mayor vulnerabilidad.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "815",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 167, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Efecto Directo 4.3. Al 2025 el Estado uruguayo habrá generado marcos normativos y fortalecido instituciones y políticas para proteger los derechos de los grupos de población más relegados (afrodescendientes, personas con discapacidad, LGTBI, personas en situación de calle, migrantes y refugiados) y combatir las expresiones de estigmatización y discriminación que los afectan.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "816",
                UNCooperationFrameworkVersionNo = 2,
                Country = "UY",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 16, 5, 41, 173, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1. Inclusive social development By 2026, people in Viet Nam, especially those at risk of being left behind, will benefit from inclusive, gender-responsive, disability-sensitive, equitable, affordable and quality social services and social protection systems, will have moved further out of poverty in all its dimensions and will be empowered to reach their full potential.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "817",
                UNCooperationFrameworkVersionNo = 2,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 24, 54, 640, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2. Climate change response, disaster resilience and environmental sustainability By 2026, people in Viet Nam, especially those at risk of being left behind, will benefit from and contribute to a safer and cleaner environment resulting from Viet Nam’s effective mitigation and adaptation to climate change, disaster-risk reduction and resilience building, promotion of the circular economy, the provision of clean and renewable energy and the sustainable management of natural resources.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "818",
                UNCooperationFrameworkVersionNo = 2,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 24, 54, 643, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3. Shared prosperity through economic transformation By 2026, people in Viet Nam, especially those at risk of being left behind, will contribute to and benefit equitably from more sustainable, inclusive and gender-responsive economic transformation based on innovation, entrepreneurship, enhanced productivity, competitiveness and decent work.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "819",
                UNCooperationFrameworkVersionNo = 2,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 24, 54, 647, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. Governance and access to justice By 2026, people in Viet Nam, especially those at risk of being left behind, will benefit from and contribute to a more just, safe and inclusive society based on improved governance, more responsive institutions, strengthened rule of law and the protection of and respect for human rights, gender equality and freedom from all forms of violence and discrimination, in line with Viet Nam’s international commitments.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "820",
                UNCooperationFrameworkVersionNo = 2,
                Country = "VN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 24, 54, 650, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2026, all people in Zimbabwe, especially women and girls and those in the most vulnerable and marginalised communities, benefit from equitable and quality social services and protection.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "821",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 28, 36, 40, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2026, all people in Zimbabwe, especially the most vulnerable and marginalised, benefit from greater environmental stability and robust food systems in support of healthy lives and equitable, sustainable and resilient livelihoods.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "822",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 28, 36, 43, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2026, all people in Zimbabwe, especially the most vulnerable and marginalised, benefit from more inclusive and sustainable economic growth with decent employment opportunities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "823",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 28, 36, 47, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4. By 2026, all people in Zimbabwe, especially the most vulnerable and marginalized, benefit from more accountable institutions and systems for rule of law, human rights and access to justice.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "824",
                UNCooperationFrameworkVersionNo = 2,
                Country = "ZW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 16, 17, 28, 36, 47, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2025, people in behind will enjoy an and sustainable reducing inequality and safeguarding the Ghana, particularly women, youth, persons with disabilities and those furthest economy that creates decent jobs livelihoods by environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "825",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 25, 15, 50, 2, 617, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2025, people in Ghana, particularly those furthest behind, will have access to and use of quality, resilient, inclusive, equitable, innovative and digitalized integrated social services, supported by well managed and accountable institutions and governance systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "826",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 25, 15, 50, 2, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2025, people in Ghana will benefit from transparent, accountable, inclusive institutions and systems, including quality integrated digital services delivering a peaceful, cohesive and just society supporting durable peace and security in the subregion",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "827",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 25, 15, 50, 2, 620, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.1. : l’efficacité des institutions est améliorée et les populations en particulier les plus exposées aux risques de conflits et à l’insécurité, vivent en paix et en sécurité dans un Etat de droit",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "828",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 453, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.1: Les enfants et jeunes issus des groupes vulnérables, particulièrement les filles et enfants en situation de handicap, ont accès à une éducation de base et une formation professionnelle de qualité (particulièrement Sahel et Est)",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "829",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 487, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.2 : Chaque homme, chaque femme, chaque enfant a un accès équitable aux services de santé de qualité",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "830",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 503, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 : les populations ont un accès accru à l’eau potable et à l’assainissement en milieu rural et périurbain, et vivent dans un cadre de vie décent et un environnement sans défécation à l’air libre",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "831",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 520, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.3 : les populations, en particulier les groupes vulnérables, des zones cibles sont plus résilientes aux chocs climatiques et environnementaux",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "832",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 520, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.4 : les populations, notamment les jeunes et les femmes dans les zones d’intervention (urbain/ rural) accroissent leurs revenus, adoptent des modes d’aménagement, de consommation et de production durable, et améliorent leur sécurité alimentaire et nutritionnelle",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "833",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 543, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.1 : la transformation de l’économie numérique inclusive est développée",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "834",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 547, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.2 : l’employabilité du secteur agrosylvopastoral, faunique et halieutique est renforcée à travers la promotion de l’entreprenariat et le développement des aptitudes professionnelles",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "835",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 560, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.3 : la productivité des petites exploitations s’est améliorée tout en promouvant l’utilisation et la gestion durable des ressources naturelles",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "836",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 577, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.4 : les parties prenantes disposent de référentiels stratégiques dans le secteur agrosylvopastoral et de l’énergie",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "837",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 593, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4.5 : les populations les plus fragiles économiquement et socialement, à mobilité ou non, en particulier les jeunes, les femmes, les personnes en situation de handicap et les personnes âgées sont couvertes par des mécanismes de protection sociale",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "838",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 13, 32, 610, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2028, all people, especially those at risk of being left behind, have increased resilience to economic, climatic, disaster, and public health risk through improved, equitable, and gender-responsive access to and utilization of quality social services, social protection, healthy habitat, and enhanced good governance and peace.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "839",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 17, 31, 303, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2028, all people benefit from a more integrated, innovative, inclusive, and sustainable economy that generates decent work and livelihood opportunities",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "840",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 17, 31, 307, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2028, all people benefit from just transition to lowcarbon, climateresilient development, sustainable management of environment, natural resources and biodiversity, and strengthened resilience to disasters and natural hazards",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "841",
                UNCooperationFrameworkVersionNo = 1,
                Country = "PH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 10, 26, 15, 17, 31, 310, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.1 Serbia adopts and implements climate change and environmentally friendly strategies that increase community resilience, decrease carbon footprint, and amplify equitable benefits of investments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "842",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1.2 Natural and cultural resources are managed in a sustainable way",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "843",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 763, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.1 Universal and inclusive access to quality health, social and protection services is improved",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "844",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2.2 Skills, education and capabilities are enhanced to ensure equitable outcomes for all",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "845",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 770, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.1 All people, especially the more vulnerable, benefit from the realisation of human rights, gender equality, social cohesion and enhanced rule of law in line with international commitments",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "846",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 777, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3.2 All people benefit from effective governance and meaningful civic engagement",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "847",
                UNCooperationFrameworkVersionNo = 3,
                Country = "RS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 42, 41, 777, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1.2 : D’ici 2024, les populations sont résilientes face aux conflits, vivent réconciliées et en harmonie dans un environnement de paix, sécurisé et respectueux des Droits de l’Homme et des traditions culturelles positives",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "848",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 140, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.1 : D'ici 2024, les populations maliennes, particulièrement les plus vulnérables, participent à l'économie et bénéficient des fruits d'une croissance forte, inclusive, résiliente et créatrice d'emplois décents",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "849",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2.2 : D'ici 2024, les communautés gèrent de façon durable et équitable les ressources naturelles et l'environnement et sont plus résilientes aux effets néfastes des changements climatiques",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "850",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 143, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.1 : D’ici 2024, les populations vulnérables, notamment les femmes, les enfants, les adolescents et les jeunes, ont un accès plus équitable aux services sociaux de base de qualité, en fonction de leurs besoins spécifiques par âge et par sexe, y compris en situation humanitaire",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "851",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 147, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3.2 : D’ici 2024, Les personnes vulnérables ont un accès amélioré aux services de protection sociale, y compris en contexte humanitaire.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "852",
                UNCooperationFrameworkVersionNo = 3,
                Country = "ML",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 15, 43, 0, 150, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 1 : D’ici à fin 2026, les populations, particulièrement les femmes, les jeunes, les adolescent(e)s, les enfants et les plus vulnérables ciblés ont un accès équitable et utilisent les services sociaux de base  de qualité et inclusifs",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "853",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 9, 33, 797, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 2. :  D’ici à fin 2026, les acteurs étatiques et non étatiques particulièrement les groupes cibles développent des systèmes alimentaires durables et résilients face au changement climatique et aux crises locales et régionales leur permettant d’améliorer leur sécurité alimentaire, nutritionnelle et leur cadre de vie",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "854",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 9, 33, 800, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 3.  D’ici à fin 2026, les populations, en particulier les groupes les plus vulnérables, jouissent pleinement de l’Etat de droit, de la cohésion sociale et de la paix",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "855",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 9, 33, 800, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 4 : D'ici 2026, les acteurs étatiques et le secteur privé améliorent l’efficacité des politiques et des stratégies pour une croissance soutenue, inclusive, diversifiée et créatrice d’emplois et d’opportunités économiques pour les jeunes et les femmes",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "856",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 9, 33, 803, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2028, all people in Mauritius thrive in youth and gender responsive environment providing access to equitable and inclusive services and opportunities, within sustainable and resilient social systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "857",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 14, 30, 553, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2028, Mauritius has a resilient, sustainable and inclusive economy that allows all people, especially youth, women and persons living with disabilities, to access sustainable livelihoods and food security.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "858",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 14, 30, 557, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2028, Mauritius has integrated, gender sensitive and adapted systems for disaster risk reduction and climate change adaptation that will address climate induced disasters, biodiversity loss and pollution.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "859",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MU",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 14, 30, 557, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1.1 By 2028, women, children, displaced people, youths and PWD, particularly in rural and urban disaster and conflict prone areas are resilient to climate related and other shocks and have access to sustainable food, health and WASH systems.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "860",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 26, 53, 263, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 By 2028, marginalised and vulnerable people in The Gambia participate in functional, accountable and transparent institutions implementing relevant reforms resulting in the efficient delivery of public services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "861",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 26, 53, 267, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.2 By 2028, marginalized and vulnerable people in The Gambia enjoy efficient social and economic inclusion, right-based human development for reduced poverty and inequality.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "862",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 26, 53, 267, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 B 2028, the country experience a broadened, inclusive and coordinated partnership landscape, and increased development financing.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "863",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GM",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 26, 53, 270, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2028, people in Cambodia, especially those at risk of being left behind, are healthier and benefit from improved gender-responsive education and social protection.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "864",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 32, 39, 30, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2028, people in Cambodia, especially those at risk of being left behind, benefit from and contribute to a productive, diversified, formalised and low-carbon and climate-adapted economy",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "865",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 32, 39, 33, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2028, people in Cambodia, especially those at risk of being left behind, benefit from a healthier, gender-inclusive natural environment.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "866",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 32, 39, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: By 2028, people in Cambodia, especially those at risk of being left behind, live in an increasingly gender equal and inclusive society with active civic space and enjoy more effective and accountable institutions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "867",
                UNCooperationFrameworkVersionNo = 1,
                Country = "KH",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 13, 16, 32, 39, 37, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: Economic Opportunities and Resilient Livelihoods through the creation of an enabling environment that facilitates economic growth and the provision of decent work opportunities, especially for excluded groups such as women.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "868",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 29, 24, 927, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: Social Cohesion, Inclusion, Gender Equality, Human Rights, and the Rule of Law -as prerequisites for sustainable development and peace in Afghanistan - strengthening civil society engagement and advocacy for alignment of Afghanistan’s normative and legal frameworks with international human rights instruments.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "869",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 29, 24, 933, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome1: Sustained Essential Services in key sectors such as health, nutrition, education, employment, water, sanitation, hygiene, social protection, and protection that are accessible to all, affordable, and can be delivered free from all forms of discrimination.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "870",
                UNCooperationFrameworkVersionNo = 2,
                Country = "AF",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 29, 24, 937, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2028, all people in Seychelles, especially youth (not in education, employment or training), women, people living with disabilities, and other vulnerable groups, are empowered and have access to improved and sustainable services, protection, and opportunities that meet their needs.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "871",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 35, 29, 663, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: By 2028, all people in Seychelles, especially youth not in education, employment or training, women, and other vulnerable groups, benefit from a more resilient, sustainable and inclusive economy enabled by a responsive public and private sector.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "872",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 35, 29, 667, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: By 2028, people and institutions are better prepared for disaster risks, and natural ecosystems are more resilient to climate change, biodiversity losses and pollution.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "873",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SC",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 21, 11, 35, 29, 670, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 1 - L'économie marocaine est compétitive, inclusive et créatrice d'emploi décents, en particulier pour les femmes et les jeunes, à travers une transformation structurelle fondée sur le développement durable et sur la résilience, notamment climatique.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "874",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 15, 57, 7, 947, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 2 - La population au Maroc bénéficie d'une couverture universelle pérenne par des services de qualité, intégrés et résilients d'éducation, de formation et de santé ainsi que d'un accès accru à la culture.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "875",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 15, 57, 7, 950, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 3 - La protection sociale est universelle et soutenable, et les inégalités sociales, locales, régionales et de genre sont réduites en vue de ne laisser personne pour compte",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "876",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 15, 57, 7, 953, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Effet 4: Les politiques publiques sont performantes, inclusives, territorialisées, intégrant le développement durable, basées sur des données",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "877",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 15, 57, 7, 957, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: People living in Lesotho are better served by improved governance systems and structures that are inclusive, accountable, with people empowered, engaged, and enjoying human rights, peace, justice, and security by 2028.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "878",
                UNCooperationFrameworkVersionNo = 3,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 31, 47, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: All people living in Lesotho enjoy improved food and nutrition security, with transformed national food systems, benefiting from natural resources and green growth that is risk informed, and climate resilient.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "879",
                UNCooperationFrameworkVersionNo = 3,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 31, 47, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: People living in Lesotho, especially the most vulnerable, have equitable and sustainable access to social services, increased decent employment, in an enabling business environment, and benefit from transformative economic development",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "880",
                UNCooperationFrameworkVersionNo = 3,
                Country = "LS",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 31, 47, 753, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: By 2028, BHUTAN HAS SUSTAINABLE AND DIVERSIFIED ECONOMIC GROWTH, DECENT EMPLOYMENT AND LIVELIHOODS AND SHARED PROSPERITY.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "881",
                UNCooperationFrameworkVersionNo = 3,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 51, 53, 197, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: BY 2028, PEOPLE IN BHUTAN BENEFIT FROM STRENGTHENED QUALITY, INCLUSIVE AND LIFELONG SOCIAL SERVICES, AND PRACTICES.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "882",
                UNCooperationFrameworkVersionNo = 3,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 51, 53, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: BY 2028, BHUTAN’S ENVIRONMENT REMAINS SUSTAINABLY MANAGED AND ITS PEOPLE ARE MORE RESILIENT TO DISASTER RISKS AND CLIMATE CHANGE.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "883",
                UNCooperationFrameworkVersionNo = 3,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 51, 53, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 4: BY 2028, BHUTAN HAS MORE INCLUSIVE, TRANSPARENT AND ACCOUNTABLE GOVERNANCE AND RULE OF LAW.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "884",
                UNCooperationFrameworkVersionNo = 3,
                Country = "BT",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 11, 30, 16, 51, 53, 200, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1 By 2027, the political process will have progressed towards transition to a civilian-led government and a system of inclusive and accountable governance, with responsive national, state, and local institutions gaining the confidence of the entire population.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "885",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 12, 13, 44, 32, 987, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2 By 2027, a coherent response to the root causes of conflict will be ensured through the implementation of the JPA and other future peace agreements as well as through inclusive and sustainable peacebuilding and the advancement towards durable solutions to displacement.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "886",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 12, 13, 44, 32, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3 By 2027, more people in Sudan, including women, children, youth, IDPs, refugees, stateless persons, returnees, people with disabilities and marginalized groups, will benefit from increased community security, an environment conducive to accountability and redress for human rights violations, and equitable access to justice",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "887",
                UNCooperationFrameworkVersionNo = 1,
                Country = "SD",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 12, 13, 44, 32, 997, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2.1 Community recovery interventions and nexus approaches in key sectors address displacement and strengthen individual and community resilience.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "888",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 13, 13, 45, 9, 753, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3.1 National systems are able to effectively plan for and implement inclusive recovery.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "889",
                UNCooperationFrameworkVersionNo = 1,
                Country = "UA",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 13, 13, 45, 9, 757, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2027, people in the United Republic of Tanzania, especially the most vulnerable, increasingly utilise quality gender transformative, inclusive and integrated basic education, health (with particular focus on RMNCAH, AIDS, TB, malaria, & epidemic prone diseases), nutrition, WASH and protection services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "890",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 15, 14, 24, 7, 447, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2027 People in the United Republic of Tanzania working in MSMEs and small-scale agriculture, especially the most vulnerable, achieve increased, more sustainable productivity and incomes with more equitable access to productive resources.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "891",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 15, 14, 24, 7, 477, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2027, people in the United Republic of Tanzania, especially the most vulnerable, contribute to and benefit from more inclusive and gender-responsive management of natural resources, climate change resilience, disaster risk reduction and increased use of efficient renewable energy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "892",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 15, 14, 24, 7, 480, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2027, people in the United Republic of Tanzania, especially the most vulnerable, participate in and benefit from government institutions and systems that promote peace and justice, are gender responsive, inclusive, accountable and representative, and are compliant with international human rights norms and standards.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "893",
                UNCooperationFrameworkVersionNo = 1,
                Country = "TZ",
                UNCFOutcomeLastUpdatedDate = new DateTime(2023, 12, 15, 14, 24, 7, 487, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EIXO  5 (Relação das Ações Humanitárias e de Desenvolvimento Sustentável) RESULTADO 1 Até 2027, o Brasil terá suas estratégias, políticas públicas e capacidades institucionais fortalecidas e ampliadas, em todos os níveis de governo e em articulação com o setor privado e a sociedade civil, para prevenir, mitigar e responder a crises humanitárias e desastres, com base em evidências, com especial atenção às populações afetadas, às pessoas refugiadas, migrantes e apátridas e demais grupos e pessoas em situação de vulnerabilidade, em sua diversidade, combatendo a xenofobia e a intolerância e promovendo o respeito aos direitos humanos, a igualdade de geração, gênero, raça e etnia e o desenvolvimento sustentável.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "894",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 827, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EIXO 1 (Transformação Econômica para o Desenvolvimento Sustentável) RESULTADO 1 Em 2027, o Brasil terá avançado na transformação econômica sustentável, inclusiva e responsiva às questões de geração, gênero, raça e etnia, com a inserção da dimensão socioambiental nas cadeias produtivas, com melhoria da produtividade,  inserindo-se na transformação tecnológica e digital em curso, e ampliando a competitividade nacional e o desenvolvimento das capacidades humanas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "895",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 833, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EIXO 2 (Inclusão Social para o Desenvolvimento Sustentável) RESULTADO 1 Até 2027, o Brasil terá ampliado e fortalecido a proteção social e demais políticas públicas para serem mais intersetoriais, inclusivas, participativas e efetivas, baseadas em evidências, atentas a grupos e pessoas em situação de vulnerabilidade e orientadas, em todos os níveis de governo, ao respeito aos direitos humanos e à redução da pobreza, da fome e das desigualdades de grupos e pessoas em situação de vulnerabilidade e à promoção da igualdade de geração, gênero, raça e etnia e do desenvolvimento sustentável ambiental e socialmente.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "896",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 837, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EIXO 3 (Meio Ambiente e Mudança do Clima para o Desenvolvimento Sustentável) RESULTADO 1 Até 2027, o Brasil terá avançado ainda mais na conservação e na restauração ambientais, na redução da poluição e na produção e consumo e descarte sustentáveis, com base em conhecimento, tecnologia, capacitação, investimento e financiamento, valorizando as especificidades e os saberes regional e local, e promovendo a soberania e segurança alimentar e nutricional, sanitária, hídrica, de saneamento básico e energética, no contexto do desenvolvimento sustentável a partir da perspectiva de geração, gênero, raça e etnia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "897",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EIXO 4 (Governança e Capacidades Institucionais) RESULTADO 1 Em 2027, o Brasil terá fortalecido, no marco do estado democrático de direito, sua governança, legislação, capacidades e articulação institucionais com ampliação da participação popular para elaborar e executar políticas públicas baseadas em evidências, em direitos humanos e igualdade de geração, gênero, raça e etnia, com vistas à prevenção e ao enfrentamento à corrupção, ao crime e às múltiplas formas de violência, e orientadas às especificidades do território e à transparência, com inovação, cooperação nacional e internacional, e ampla participação da sociedade.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "898",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 840, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2 Até 2027, a sociedade brasileira - especialmente grupos e pessoas em situação de vulnerabilidade - terá maior acesso a bens e serviços públicos de qualidade, à inclusão digital e novas tecnologias, maior capacidade de exercer seus direitos e contribuir com o processo de tomada de decisão, livres de violência e discriminação, para redução das desigualdades sociais e promoção da igualdade de geração, gênero, raça e etnia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "899",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 847, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2 Até 2027, o Brasil terá ampliado e fortalecido os sistemas de garantias de direitos para que sejam ainda mais efetivos na integração transversal do respeito aos 103 direitos humanos e da igualdade de geração, gênero, raça e etnia, no enfrentamento às suas violações e às múltiplas formas de discriminação e violências, incluindo a violência de gênero, e para que promovam a atuação coordenada de diferentes instâncias em todos os níveis de governo, bem como a participação da sociedade civil.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "900",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2 Até 2027, o Brasil terá avançado na implementação de ações de mitigação da mudança do clima e adaptação aos seus efeitos, redução do desmatamento, de riscos de desastres, considerando os grupos e pessoas em situação de vulnerabilidade, inclusive pessoas forçadamente deslocadas, a partir da perspectiva de geração, gênero, raça e etnia, e proteção dos territórios dos povos indígenas, dos povos e comunidades tradicionais e das populações do campo, floresta e águas em geral, com vistas a promover uma economia resiliente e descarbonizada, à luz dos marcos legais e regulatórios nacionais, e com alinhamento aos compromissos internacionalmente vigentes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "901",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 850, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2 Em 2027, o Brasil terá avançado na inclusão econômica que contribui para a redução da pobreza, da fome, das vulnerabilidades, das desigualdades, e da 101 discriminação de geração, gênero, raça e etnia, e que garante o direito à educação transformadora para o pleno desenvolvimento da pessoa e o acesso ao trabalho decente, às oportunidades para geração de renda, à proteção social, econômica e políticas de cuidados e às infraestruturas resilientes, assegurando a igualdade de oportunidades e a sua sustentabilidade.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "902",
                UNCooperationFrameworkVersionNo = 1,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 33, 44, 853, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 1.1  Em 2027, o Brasil terá avançado na transformação econômica sustentável, inclusiva e responsiva às questões de geração, gênero, raça e etnia, com a inserção da dimensão socioambiental nas cadeias produtivas, com melhoria da produtividade,  inserindo-se na transformação tecnológica e digital em curso, e ampliando a competitividade nacional e o desenvolvimento das capacidades humanas.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "903",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 573, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 1.2 Em 2027, o Brasil terá avançado na inclusão econômica que contribui para a redução da pobreza, da fome, das vulnerabilidades, das desigualdades, e da 101 discriminação de geração, gênero, raça e etnia, e que garante o direito à educação transformadora para o pleno desenvolvimento da pessoa e o acesso ao trabalho decente, às oportunidades para geração de renda, à proteção social, econômica e políticas de cuidados e às infraestruturas resilientes, assegurando a igualdade de oportunidades e a sua sustentabilidade.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "904",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 573, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2.1 Até 2027, o Brasil terá ampliado e fortalecido a proteção social e demais políticas públicas para serem mais intersetoriais, inclusivas, participativas e efetivas, baseadas em evidências, atentas a grupos e pessoas em situação de vulnerabilidade e orientadas, em todos os níveis de governo, ao respeito aos direitos humanos e à redução da pobreza, da fome e das desigualdades de grupos e pessoas em situação de vulnerabilidade e à promoção da igualdade de geração, gênero, raça e etnia e do desenvolvimento sustentável ambiental e socialmente.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "905",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 577, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 2.2 Até 2027, o Brasil terá ampliado e fortalecido os sistemas de garantias de direitos para que sejam ainda mais efetivos na integração transversal do respeito aos 103 direitos humanos e da igualdade de geração, gênero, raça e etnia, no enfrentamento às suas violações e às múltiplas formas de discriminação e violências, incluindo a violência de gênero, e para que promovam a atuação coordenada de diferentes instâncias em todos os níveis de governo, bem como a participação da sociedade civil.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "906",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 577, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 3.1 Até 2027, o Brasil terá avançado ainda mais na conservação e na restauração ambientais, na redução da poluição e na produção e consumo e descarte sustentáveis, com base em conhecimento, tecnologia, capacitação, investimento e financiamento, valorizando as especificidades e os saberes regional e local, e promovendo a soberania e segurança alimentar e nutricional, sanitária, hídrica, de saneamento básico e energética, no contexto do desenvolvimento sustentável a partir da perspectiva de geração, gênero, raça e etnia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "907",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 3.2 Até 2027, o Brasil terá avançado na implementação de ações de mitigação da mudança do clima e adaptação aos seus efeitos, redução do desmatamento, de riscos de desastres, considerando os grupos e pessoas em situação de vulnerabilidade, inclusive pessoas forçadamente deslocadas, a partir da perspectiva de geração, gênero, raça e etnia, e proteção dos territórios dos povos indígenas, dos povos e comunidades tradicionais e das populações do campo, floresta e águas em geral, com vistas a promover uma economia resiliente e descarbonizada, à luz dos marcos legais e regulatórios nacionais, e com alinhamento aos compromissos internacionalmente vigentes.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "908",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 4.1 Em 2027, o Brasil terá fortalecido, no marco do estado democrático de direito, sua governança, legislação, capacidades e articulação institucionais com ampliação da participação popular para elaborar e executar políticas públicas baseadas em evidências, em direitos humanos e igualdade de geração, gênero, raça e etnia, com vistas à prevenção e ao enfrentamento à corrupção, ao crime e às múltiplas formas de violência, e orientadas às especificidades do território e à transparência, com inovação, cooperação nacional e internacional, e ampla participação da sociedade.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "909",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 4.2 Até 2027, a sociedade brasileira - especialmente grupos e pessoas em situação de vulnerabilidade - terá maior acesso a bens e serviços públicos de qualidade, à inclusão digital e novas tecnologias, maior capacidade de exercer seus direitos e contribuir com o processo de tomada de decisão, livres de violência e discriminação, para redução das desigualdades sociais e promoção da igualdade de geração, gênero, raça e etnia.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "910",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 580, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "RESULTADO 5.1 Até 2027, o Brasil terá suas estratégias, políticas públicas e capacidades institucionais fortalecidas e ampliadas, em todos os níveis de governo e em articulação com o setor privado e a sociedade civil, para prevenir, mitigar e responder a crises humanitárias e desastres, com base em evidências, com especial atenção às populações afetadas, às pessoas refugiadas, migrantes e apátridas e demais grupos e pessoas em situação de vulnerabilidade, em sua diversidade, combatendo a xenofobia e a intolerância e promovendo o respeito aos direitos humanos, a igualdade de geração, gênero, raça e etnia e o desenvolvimento sustentável.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "911",
                UNCooperationFrameworkVersionNo = 2,
                Country = "BR",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 3, 13, 49, 54, 583, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 1 : D'ici n 2028, les populations, y compris les femmes, les jeunes filles et garçons, les enfants, personnes handicapées, celles vivant dans les zones rurales, péri-urbains et difficiles d’accès, particulièrement les plus vulnérables, utilisent les services sociaux de base de qualité, équitables, durables et  inclusifs, y compris en situation d'urgence.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "912",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 4, 16, 19, 23, 780, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 2 : D’ici fin 2028, les populations guinéennes, en particulier les jeunes, les femmes, les personnes vivants avec handicap et les personnes vulnérables notamment dans les zones défavorisées, participent activement à la croissance économique soutenue par une économie diversifiée, inclusive, équitable, durable et génératrice d’emplois décents.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "913",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 4, 16, 19, 23, 787, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "EFFET 3 : D'ici fin 2028, les populations en particulier les femmes et les jeunes (filles et garçons) exercent leurs droits dans un environnement paisible et participent aux prises de décisions qui affectent leur bien-être",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "914",
                UNCooperationFrameworkVersionNo = 1,
                Country = "GN",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 6, 4, 16, 19, 23, 790, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Resultado esperado 1.1. A 2027 Colombia habrá avanzado en el cierre de brechas y en el acceso a derechos de las personas más afectadas por el conflicto armado a través de la generación de oportunidades de participación socioeconómica, sostenimiento de la paz territorial y la justicia social. e inclusión socioeconómica, para el sostenimiento de la paz territorial y la justica social.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "915",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 270, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Resultado esperado 2.1. A 2027 Colombia habrá avanzado en garantizar el derecho humano a la alimentación, en particular la reducción del hambre y la malnutrición.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "916",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 277, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Resultado esperado 3.1. A 2027 Colombia habrá avanzado en el goce efectivo de derechos a través de bienes y servicios que garanticen la igualdad y la equidad social y productiva.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "917",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 277, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Resultado esperado 4.1. A 2027 Colombia habra avanzado en la adaptación y mitigación de los efectos de la triple crisis planetaria - cambio climático, la pérdida y degradación de la biodiversidad, y la reducción de la contaminación.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "918",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 280, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Resultado esperado 5.1. A 2027 el Estado colombiano habra mejorado su capacidad de monitoreo e implementación de la Agenda 2030 a través de sistemas de información, y gestión de conocimiento más eficientes, una movilización de recursos con nuevas fuentes de financiamiento y estrategias eficaces para la  ncorporación de enfoques diferenciales, de género y de derechos en las iniciativas de desarrollo sostenible.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "919",
                UNCooperationFrameworkVersionNo = 1,
                Country = "CO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 5, 12, 24, 25, 280, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 1: People in Sierra Leone, especially the most vulnerable, are food and nutrition secure, benefit from effective natural resource management, are resilient to the effects of climate change, and equipped to prevent and respond to disasters",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "920",
                UNCooperationFrameworkVersionNo = 4,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 44, 23, 147, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 2: People in Sierra Leone, particularly most vulnerable groups in rural and hard-to-reach areas, have equitable access to quality, gender-responsive essential and social protection services and decent job opportunities.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "921",
                UNCooperationFrameworkVersionNo = 4,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 44, 23, 147, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "OUTCOME 3: People in Sierra Leone enjoy transparent and accountable governance systems and economic transformation that guarantee peace, rights, and social cohesion, particularly among youth, women, marginalised and vulnerable groups.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "922",
                UNCooperationFrameworkVersionNo = 4,
                Country = "SL",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 8, 19, 16, 44, 23, 147, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "1. By 2028, more people, especially the most vulnerable groups, including women, youths, and people with disability, partcipate in and benefit from food and nutrition security and a more diversified, inclusive, and sustainable economic growth resilient to shocks.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "923",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 9, 2, 15, 1, 24, 190, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "2. By 2028, people in Malawi, especially women, youth and those most left behind, experience more inclusive good governance, and robust political and civic participation.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "924",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 9, 2, 15, 1, 24, 193, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "3. By 2028, more people, in particular women, children and youth, especially the most vulnerable and marginalized, are resilient with access to and utilization of quality, equitable, efficient, gender and shock-responsive education, health, nutrition, WASH, social and protection services.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "925",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 9, 2, 15, 1, 24, 193, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "4. By 2028, more people, especially the most vulnerable, including women and youth, are resilient to climate change and shocks, benefit from and have access to better-managed waste, ecosystems and natural resources, including clean and affordable energy.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "926",
                UNCooperationFrameworkVersionNo = 1,
                Country = "MW",
                UNCFOutcomeLastUpdatedDate = new DateTime(2024, 9, 2, 15, 1, 24, 197, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 1: By 2028 more people, especially women, youth and the most vulnerable, use and have equitable access to quality social services delivered by effective institutions.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "927",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2025, 1, 14, 14, 43, 59, 740, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 2: By 2028 more people, especially women, youth and the most vulnerable, participate in and benefit from more effective and inclusive democratic governance anchored in human rights, strengthening prevention capacities and promoting peace and security in the region.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "928",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2025, 1, 14, 14, 43, 59, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 3: By 2028 more people, especially women, youth and the most vulnerable, participate in and benefit from more inclusive, diversified and sustainable economic growth.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "929",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2025, 1, 14, 14, 43, 59, 750, DateTimeKind.Utc)
            },
            new UNCFOutcome
            {
                Name = "Outcome 4: By 2028 more people, especially women, youth and the most vulnerable, benefit from sustainable management of the environment and natural resources and are resilient to disasters and climate change.",
                Status = EntityStatus.Active,
                IsDeleted = false,
                UNCFOutcomeId = "930",
                UNCooperationFrameworkVersionNo = 1,
                Country = "AO",
                UNCFOutcomeLastUpdatedDate = new DateTime(2025, 1, 14, 14, 43, 59, 753, DateTimeKind.Utc)
            }
        };
    }
}
