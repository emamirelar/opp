using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Enums;
using UNOPS.PAO.UNOPSDataAccess.Context;
using Npgsql;
using NpgsqlTypes;

namespace UNOPS.PAO.UNOPSDataAccess.Seed.Seeders;

/// <summary>
/// Seeds embeddings for Output entities
/// NOTE: This seeder has TWO modes:
/// 1. Static mode (SeedOutputEmbeddingsAsync) - runs during automated seeding, just shows status
/// 2. Instance mode (GenerateOutputEmbeddingsAsync) - requires GeminiManager, generates actual embeddings
/// 
/// For actual embedding generation, use the instance method with proper dependencies.
/// </summary>
public class OutputEmbeddingSeeder
{
    private readonly UNOPSAppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly dynamic _geminiManager; // IGeminiManager - dynamic to avoid circular dependency

    // Instance constructor for actual embedding generation
    public OutputEmbeddingSeeder(
        UNOPSAppDbContext context,
        IConfiguration configuration,
        dynamic geminiManager)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _geminiManager = geminiManager ?? throw new ArgumentNullException(nameof(geminiManager));
    }

    /// <summary>
    /// Static seeder method called during automated seeding
    /// Just checks status and provides instructions
    /// </summary>
    public static async Task SeedOutputEmbeddingsAsync(UNOPSAppDbContext context)
    {
        Console.WriteLine("⚠️  OutputEmbeddingSeeder: This seeder requires AI services.");
        Console.WriteLine("   To generate embeddings, use one of these options:");
        Console.WriteLine("   1. Call API endpoint: POST /api/admin/output-embeddings/generate");
        Console.WriteLine("   2. Use OutputEmbeddingSeeder instance with proper dependencies");
        Console.WriteLine();
        
        // Check if embeddings already exist
        var existingEmbeddings = await context.Set<EntityEmbeddings>()
            .Where(e => e.EntityName == "Output")
            .CountAsync();
        
        var totalOutputs = await context.Set<Output>()
            .Where(o => o.Status == EntityStatus.Active && !o.IsDeleted)
            .CountAsync();
        
        if (existingEmbeddings > 0)
        {
            Console.WriteLine($"   ✅ Found {existingEmbeddings} existing Output embeddings");
            Console.WriteLine($"   📊 Outputs: {totalOutputs}, Embeddings: {existingEmbeddings}, Avg: {(double)existingEmbeddings / totalOutputs:F1} per output");
        }
        else
        {
            Console.WriteLine($"   ⚠️  No Output embeddings found. Generation required.");
            Console.WriteLine($"   📊 Total Outputs: {totalOutputs}");
            Console.WriteLine($"   📊 Expected embeddings: ~{totalOutputs * 3} (3-4 per Output)");
            Console.WriteLine($"   ⏱️  Estimated generation time: ~2 minutes");
        }
    }

    /// <summary>
    /// Instance method for actual embedding generation
    /// Requires GeminiManager with AI services to be injected
    /// </summary>
    public async Task GenerateOutputEmbeddingsAsync()
    {
        Console.WriteLine("🔄 Generating embeddings for Outputs...");

        try
        {
            // Get all active outputs
            var outputs = await _context.Set<Output>()
                .Where(o => o.Status == EntityStatus.Active && !o.IsDeleted)
                .AsNoTracking()
                .ToListAsync();

            Console.WriteLine($"📊 Processing {outputs.Count} active outputs");

            // Prepare texts for embedding generation
            var embeddingRequests = new List<(int outputId, string level, string text, string definition)>();

            foreach (var output in outputs)
            {
                // Level 0 (always present for active outputs)
                if (!string.IsNullOrWhiteSpace(output.Level0))
                {
                    var text = output.Level0;
                    var definition = output.DefinitionLevel1 ?? string.Empty;
                    embeddingRequests.Add((output.Id, "Level0", text, definition));
                }

                // Level 1
                if (!string.IsNullOrWhiteSpace(output.Level1))
                {
                    var text = $"{output.Level0} > {output.Level1}";
                    var definition = output.DefinitionLevel2 ?? string.Empty;
                    embeddingRequests.Add((output.Id, "Level1", text, definition));
                }

                // Level 2
                if (!string.IsNullOrWhiteSpace(output.Level2))
                {
                    var text = $"{output.Level0} > {output.Level1} > {output.Level2}";
                    var definition = output.DefinitionLevel3 ?? string.Empty;
                    embeddingRequests.Add((output.Id, "Level2", text, definition));
                }

                // Level 3
                if (!string.IsNullOrWhiteSpace(output.Level3))
                {
                    var text = $"{output.Level0} > {output.Level1} > {output.Level2} > {output.Level3}";
                    var definition = output.DefinitionLevel4 ?? string.Empty;
                    embeddingRequests.Add((output.Id, "Level3", text, definition));
                }

                // Level 4
                if (!string.IsNullOrWhiteSpace(output.Level4))
                {
                    var text = $"{output.Level0} > {output.Level1} > {output.Level2} > {output.Level3} > {output.Level4}";
                    var definition = string.Empty; // No definition for Level4
                    embeddingRequests.Add((output.Id, "Level4", text, definition));
                }
            }

            Console.WriteLine($"📝 Total embedding requests: {embeddingRequests.Count}");

            // Prepare texts for batch embedding (text + definition combined)
            var textsForEmbedding = embeddingRequests
                .Select(req => string.IsNullOrWhiteSpace(req.definition) 
                    ? req.text 
                    : $"{req.text}. {req.definition}")
                .ToList();

            // Generate embeddings in batches using GeminiManager's AiContextualService
            Console.WriteLine("🔄 Generating embeddings...");
            List<string> embeddings = await _geminiManager.CreateBatchEmbeddingsAsync(textsForEmbedding);

            if (embeddings.Count != textsForEmbedding.Count)
            {
                Console.WriteLine($"⚠️ Embedding count mismatch: expected {textsForEmbedding.Count}, got {embeddings.Count}");
            }

            // Generate keywords for all texts using GeminiManager's AiContextualService
            Console.WriteLine("🔑 Generating keywords...");
            Dictionary<string, string> keywordsMap = await _geminiManager.GenerateKeywordsAsync(textsForEmbedding);

            // Save embeddings to EntityEmbeddings table using stored procedure
            Console.WriteLine("💾 Saving embeddings to database...");
            int savedCount = 0;

            for (int i = 0; i < embeddingRequests.Count && i < embeddings.Count; i++)
            {
                var (outputId, level, text, definition) = embeddingRequests[i];
                var embeddingString = embeddings[i];
                var fullText = textsForEmbedding[i];
                string keywords = keywordsMap.TryGetValue(fullText, out var kw) ? kw : string.Empty;

                // Create metadata JSON
                var metadata = JsonConvert.SerializeObject(new
                {
                    Level = level,
                    Text = text,
                    Definition = definition ?? string.Empty,
                    Hierarchy = text
                });

                try
                {
                    // Step 1: Use the stored procedure to insert embedding (handles vector conversion)
                    var sql = "CALL public.\"InsertEntityEmbedding\"(@entityName, @entityId, @text, @embedding)";
                    
                    var parameters = new[] 
                    {
                        new NpgsqlParameter("@entityName", NpgsqlDbType.Text) { Value = "Output" },
                        new NpgsqlParameter("@entityId", NpgsqlDbType.Integer) { Value = outputId },
                        new NpgsqlParameter("@text", NpgsqlDbType.Text) { Value = fullText },
                        new NpgsqlParameter("@embedding", NpgsqlDbType.Text) { Value = embeddingString }
                    };
                    
                    await _context.Database.ExecuteSqlRawAsync(sql, parameters);

                    // Step 2: Update Metadata and Keywords fields (not handled by stored procedure)
                    var updateSql = @"
                        UPDATE public.""EntityEmbeddings"" 
                        SET ""Metadata"" = @metadata, ""Keywords"" = @keywords 
                        WHERE ""EntityName"" = @entityName AND ""EntityId"" = @entityId AND ""EntityData"" = @text";
                    
                    var updateParams = new[] 
                    {
                        new NpgsqlParameter("@metadata", NpgsqlDbType.Text) { Value = metadata },
                        new NpgsqlParameter("@keywords", NpgsqlDbType.Text) { Value = keywords ?? string.Empty },
                        new NpgsqlParameter("@entityName", NpgsqlDbType.Text) { Value = "Output" },
                        new NpgsqlParameter("@entityId", NpgsqlDbType.Integer) { Value = outputId },
                        new NpgsqlParameter("@text", NpgsqlDbType.Text) { Value = fullText }
                    };
                    
                    await _context.Database.ExecuteSqlRawAsync(updateSql, updateParams);

                    savedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Failed to save embedding for Output ID {outputId}, Level {level}: {ex.Message}");
                }
            }

            // No need to call SaveChangesAsync since we're using stored procedures
            Console.WriteLine($"✅ Generated and saved {savedCount} embeddings with keywords");
            Console.WriteLine($"📊 Average embeddings per output: {(double)savedCount / outputs.Count:F1}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error generating output embeddings: {ex.Message}");
            throw;
        }
    }
}
