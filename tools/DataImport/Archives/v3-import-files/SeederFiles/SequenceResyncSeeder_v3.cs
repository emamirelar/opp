using Microsoft.EntityFrameworkCore;
using UNOPS.PAO.UNOPSDataAccess.Context;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders
{
    /// <summary>
    /// Resynchronizes all PostgreSQL sequences to prevent duplicate key errors.
    /// This should be executed as the LAST step in the seeding process.
    /// </summary>
    public static class SequenceResyncSeeder_v3
    {
        public static async Task ResyncAllSequencesAsync(UNOPSAppDbContext context)
        {
            Console.WriteLine("🔄 Resynchronizing all PostgreSQL sequences...");

            var sequences = new List<(string TableName, string SequenceName)>
            {
                ("PartnerTrees", "PartnerTrees_Id_seq"),
                ("Interactions", "Interactions_Id_seq")
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

            // Interactions
            var interactionSeq = await GetSequenceValueAsync(context, "Interactions_Id_seq");
            var interactionMax = await context.Interactions.MaxAsync(x => (int?)x.Id) ?? 0;
            results.Add(new SequenceVerification
            {
                TableName = "Interactions",
                SequenceValue = interactionSeq,
                MaxId = interactionMax,
                Difference = interactionSeq - interactionMax
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
