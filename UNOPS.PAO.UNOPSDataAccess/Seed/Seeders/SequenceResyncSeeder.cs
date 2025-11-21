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
                ("ArtifactDataTypes", "ArtifactDataTypes_Id_seq"),
                ("ArtifactTypes", "ArtifactTypes_Id_seq"),
                ("PartnerTrees", "PartnerTrees_Id_seq"),
                ("Partners", "Partners_Id_seq"),
                ("Contacts", "Contacts_Id_seq"),
                ("Interactions", "Interactions_Id_seq"),
                ("Documents", "Documents_Id_seq"),
                ("DocumentTypes", "DocumentTypes_Id_seq"),
                ("OrganizationHierarchies", "OrganizationHierarchies_Id_seq"),
                ("LiaisonOffices", "LiaisonOffices_Id_seq")
            };

            foreach (var (tableName, sequenceName) in sequences)
            {
                try
                {
                    await context.Database.ExecuteSqlRawAsync($@"
                        SELECT setval(
                            'public.""{sequenceName}""',
                            (SELECT COALESCE(MAX(""Id""), 0) FROM public.""{tableName}"")
                        );
                    ");

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

            // ArtifactTypes
            var artifactTypeSeq = await GetSequenceValueAsync(context, "ArtifactTypes_Id_seq");
            var artifactTypeMax = await context.Set<UNOPS.PAO.Domain.Entities.ArtifactType>().MaxAsync(x => (int?)x.Id) ?? 0;
            results.Add(new SequenceVerification
            {
                TableName = "ArtifactTypes",
                SequenceValue = artifactTypeSeq,
                MaxId = artifactTypeMax,
                Difference = artifactTypeSeq - artifactTypeMax
            });

            // PartnerTrees
            var partnerTreeSeq = await GetSequenceValueAsync(context, "PartnerTrees_Id_seq");
            var partnerTreeMax = await context.PartnerTrees.MaxAsync(x => (int?)x.Id) ?? 0;
            results.Add(new SequenceVerification
            {
                TableName = "PartnerTrees",
                SequenceValue = partnerTreeSeq,
                MaxId = partnerTreeMax,
                Difference = partnerTreeSeq - partnerTreeMax
            });

            // Partners
            var partnerSeq = await GetSequenceValueAsync(context, "Partners_Id_seq");
            var partnerMax = await context.Partners.MaxAsync(x => (int?)x.Id) ?? 0;
            results.Add(new SequenceVerification
            {
                TableName = "Partners",
                SequenceValue = partnerSeq,
                MaxId = partnerMax,
                Difference = partnerSeq - partnerMax
            });

            // Contacts
            var contactSeq = await GetSequenceValueAsync(context, "Contacts_Id_seq");
            var contactMax = await context.Contacts.MaxAsync(x => (int?)x.Id) ?? 0;
            results.Add(new SequenceVerification
            {
                TableName = "Contacts",
                SequenceValue = contactSeq,
                MaxId = contactMax,
                Difference = contactSeq - contactMax
            });

            return results;
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
