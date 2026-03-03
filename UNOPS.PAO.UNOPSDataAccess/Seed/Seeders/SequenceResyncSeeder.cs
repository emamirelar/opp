using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Resynchronizes all PostgreSQL sequences to prevent duplicate key errors.
    /// This should be executed as the LAST step in the seeding process.
    /// </summary>
    public static class SequenceResyncSeeder
    {
        public static async Task ResyncAllSequencesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Resynchronizing all PostgreSQL sequences...");

            var sequences = new List<(string TableName, string SequenceName)>
            {
                // Artifact management (seeded by ArtifactDataTypeSeeder, ArtifactTypeSeeder)
                ("ArtifactDataTypes", "ArtifactDataTypes_Id_seq"),
                ("ArtifactTypes", "ArtifactTypes_Id_seq"),
                
                // Partnership and organization management (seeded by bulk update seeders)
                ("PartnerTrees", "PartnerTrees_Id_seq"),
                ("Partners", "Partners_Id_seq"),
                ("Contacts", "Contacts_Id_seq"),
                ("Interactions", "Interactions_Id_seq"),
                ("OrganizationHierarchies", "OrganizationHierarchies_Id_seq"),
                ("LiaisonOffices", "LiaisonOffices_Id_seq"),
                
                // Document management (seeded by DocumentTypeSeeder)
                ("DocumentTypes", "DocumentTypes_Id_seq"),
                
                // Entity configuration (seeded by EntitySeeder, EntityManager.sql, EntityPermissions.sql)
                ("Entities", "Entities_Id_seq"),
                ("EntityManagers", "EntityManagers_Id_seq"),
                ("EntityFieldManagers", "EntityFieldManagers_Id_seq"),
                ("EntityPermissions", "EntityPermissions_Id_seq"),
                
                // Role management (WorkflowStages removed - now handled by workflow submodule)
                ("EntityRoles", "EntityRoles_Id_seq"),
                ("EntityRolePersons", "EntityRolePersons_Id_seq"),
                
                // SDG management (seeded by SDGSeeder, SDGTargetSeeder, SDGIndicatorSeeder)
                ("SDGs", "SDGs_Id_seq"),
                ("SDGTargets", "SDGTargets_Id_seq"),
                ("SDGIndicators", "SDGIndicators_Id_seq"),
                
                // UNCF management (seeded by UNCFMetadataSeeder, UNCFOutcomeSeeder, UNCFIndicatorSeeder)
                ("UNCFMetadatas", "UNCFMetadatas_Id_seq"),
                ("UNCFOutcomes", "UNCFOutcomes_Id_seq"),
                ("UNCFIndicators", "UNCFIndicators_Id_seq"),
                
                // UNOPS reference data (seeded by UNOPSMissionSeeder, ProposedInitiativeTypeSeeder)
                ("UNOPSMissions", "UNOPSMissions_Id_seq"),
                ("ProposedInitiativeTypes", "ProposedInitiativeTypes_Id_seq"),
                
                // Risk management (seeded by RiskLookupSeeder, RiskCategorySeeder, PreDefinedHighRiskSeeder)
                ("RiskTypes", "RiskTypes_Id_seq"),
                ("RiskProbabilities", "RiskProbabilities_Id_seq"),
                ("RiskProximities", "RiskProximities_Id_seq"),
                ("RiskImpactLevels", "RiskImpactLevels_Id_seq"),
                ("RiskResponseTypes", "RiskResponseTypes_Id_seq"),
                ("RiskCategories", "RiskCategories_Id_seq"),
                ("PreDefinedHighRisks", "PreDefinedHighRisks_Id_seq"),
                
                // General reference data (seeded by CountryAndOrgUnitRelationshipSeeder)
                ("Countries", "Countries_Id_seq"),
                
                // AI (seeded by AiPrompts.sql)
                ("AiPrompt", "AiPrompt_Id_seq")
            };

            foreach (var (tableName, sequenceName) in sequences)
            {
                try
                {
                    // Use GREATEST to ensure the sequence is set to at least 1 (sequences can't be 0)
                    // tableName and sequenceName come from hardcoded list - not user input
#pragma warning disable EF1002 // SQL identifiers cannot be parameterized
                    await context.Database.ExecuteSqlRawAsync($@"
                        SELECT setval(
                            'public.""{sequenceName}""',
                            GREATEST((SELECT COALESCE(MAX(""Id""), 0) FROM public.""{tableName}""), 1)
                        );
                    ");
#pragma warning restore EF1002

                    Console.WriteLine($"  ✅ {tableName}: Sequence resynchronized");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"  ⚠️  {tableName}: Could not resync sequence - {ex.Message}");
                }
            }

            // Verify the resynchronization
            var verificationResults = await VerifySequencesAsync(context);

            Console.WriteLine("\n📊 Sequence Verification:");
            foreach (var result in verificationResults)
            {
                var status = result.Difference >= 0 ? "✅ OK" : "❌ PROBLEM";
                Console.WriteLine($"  {status} {result.TableName}: Seq={result.SequenceValue}, Max={result.MaxId}, Diff={result.Difference}");
            }

            Console.WriteLine("✅ Sequence resynchronization completed\n");
        }

        private static async Task<List<SequenceVerification>> VerifySequencesAsync(UNOPSAppDbContext context)
        {
            var results = new List<SequenceVerification>();

            // Verify all seeded sequences
            
            // Artifact management
            await VerifySequenceAsync(context, results, "ArtifactDataTypes", "ArtifactDataTypes_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.ArtifactDataType>().MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "ArtifactTypes", "ArtifactTypes_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.ArtifactType>().MaxAsync(x => (int?)x.Id) ?? 0);

            // Partnership data
            await VerifySequenceAsync(context, results, "PartnerTrees", "PartnerTrees_Id_seq",
                async () => await context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "Partners", "Partners_Id_seq",
                async () => await context.Partners.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "Contacts", "Contacts_Id_seq",
                async () => await context.Contacts.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "Interactions", "Interactions_Id_seq",
                async () => await context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0);

            // Organization
            await VerifySequenceAsync(context, results, "OrganizationHierarchies", "OrganizationHierarchies_Id_seq",
                async () => await context.OrganizationHierarchies.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "LiaisonOffices", "LiaisonOffices_Id_seq",
                async () => await context.LiaisonOffices.MaxAsync(x => (int?)x.Id) ?? 0);

            // Document management
            await VerifySequenceAsync(context, results, "DocumentTypes", "DocumentTypes_Id_seq",
                async () => await context.DocumentTypes.MaxAsync(x => (int?)x.Id) ?? 0);

            // Entity configuration
            await VerifySequenceAsync(context, results, "Entities", "Entities_Id_seq",
                async () => await context.Entities.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "EntityManagers", "EntityManagers_Id_seq",
                async () => await context.EntityManagers.MaxAsync(x => (int?)x.Id) ?? 0);

            // Roles (WorkflowStages removed - workflow now handled by submodule)
            await VerifySequenceAsync(context, results, "EntityRoles", "EntityRoles_Id_seq",
                async () => await context.EntityRoles.MaxAsync(x => (int?)x.Id) ?? 0);

            // SDG data
            await VerifySequenceAsync(context, results, "SDGs", "SDGs_Id_seq",
                async () => await context.SDGs.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "SDGTargets", "SDGTargets_Id_seq",
                async () => await context.SDGTargets.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "SDGIndicators", "SDGIndicators_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.SDGIndicator>().MaxAsync(x => (int?)x.Id) ?? 0);

            // UNCF data
            await VerifySequenceAsync(context, results, "UNCFOutcomes", "UNCFOutcomes_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.UNCFOutcome>().MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "UNCFIndicators", "UNCFIndicators_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.UNCFIndicator>().MaxAsync(x => (int?)x.Id) ?? 0);

            // UNOPS reference data
            await VerifySequenceAsync(context, results, "UNOPSMissions", "UNOPSMissions_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.UNOPSMission>().MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "ProposedInitiativeTypes", "ProposedInitiativeTypes_Id_seq",
                async () => await context.ProposedInitiativeTypes.MaxAsync(x => (int?)x.Id) ?? 0);

            // Risk management
            await VerifySequenceAsync(context, results, "RiskTypes", "RiskTypes_Id_seq",
                async () => await context.RiskTypes.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "RiskProbabilities", "RiskProbabilities_Id_seq",
                async () => await context.RiskProbabilities.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "RiskProximities", "RiskProximities_Id_seq",
                async () => await context.RiskProximities.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "RiskImpactLevels", "RiskImpactLevels_Id_seq",
                async () => await context.RiskImpactLevels.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "RiskResponseTypes", "RiskResponseTypes_Id_seq",
                async () => await context.RiskResponseTypes.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "RiskCategories", "RiskCategories_Id_seq",
                async () => await context.RiskCategories.MaxAsync(x => (int?)x.Id) ?? 0);
            
            await VerifySequenceAsync(context, results, "PreDefinedHighRisks", "PreDefinedHighRisks_Id_seq",
                async () => await context.PreDefinedHighRisks.MaxAsync(x => (int?)x.Id) ?? 0);

            // General reference data
            await VerifySequenceAsync(context, results, "Countries", "Countries_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.Country>().MaxAsync(x => (int?)x.Id) ?? 0);

            // AI
            await VerifySequenceAsync(context, results, "AiPrompt", "AiPrompt_Id_seq",
                async () => await context.Set<UNOPS.PAO.Domain.Entities.AiPrompt>().MaxAsync(x => (int?)x.Id) ?? 0);

            return results;
        }

        private static async Task VerifySequenceAsync(
            UNOPSAppDbContext context,
            List<SequenceVerification> results,
            string tableName,
            string sequenceName,
            Func<Task<int>> getMaxIdFunc)
        {
            try
            {
                var sequenceValue = await GetSequenceValueAsync(context, sequenceName);
                var maxId = await getMaxIdFunc();
                results.Add(new SequenceVerification
                {
                    TableName = tableName,
                    SequenceValue = sequenceValue,
                    MaxId = maxId,
                    Difference = sequenceValue - maxId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ⚠️  {tableName}: Could not verify sequence - {ex.Message}");
            }
        }

        private static async Task<long> GetSequenceValueAsync(UNOPSAppDbContext context, string sequenceName)
        {
            var sql = $"SELECT last_value as value FROM public.\"{sequenceName}\"";
            var result = await context.Database
                .SqlQueryRaw<SequenceResult>(sql)
                .FirstOrDefaultAsync();
            return result?.value ?? 0;
        }

        private class SequenceResult
        {
            public long value { get; set; }  // lowercase to match PostgreSQL
        }

        private class SequenceVerification
        {
            public string TableName { get; set; } = string.Empty;
            public long SequenceValue { get; set; }
            public int MaxId { get; set; }
            public long Difference { get; set; }
        }
    }
}
