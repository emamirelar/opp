using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDataAccess.Seed.Models;
using UNOPS.PAO.UNOPSDomain.Entities;

namespace UNOPS.PAO.UNOPSDataAccess.Seed
{
    /// <summary>
    /// Generic seed runner that can execute SQL scripts and C# seeders in configured order.
    /// 
    /// By default, seed files are read from the bin directory (copied during build).
    /// To read directly from source files instead, set environment variable:
    /// SEED_USE_SOURCE_FILES=true
    /// 
    /// This is useful during development to avoid the need to rebuild when modifying seed files.
    /// </summary>
    public static class GenericSeedRunner
    {
        /// <summary>
        /// Always use bin directory approach like MigrationSqlScriptExecutor
        /// This ensures files are read from the output directory where they're copied during build
        /// </summary>
        private static bool UseSourceFiles => false;


        /// <summary>
        /// Executes all configured seed steps (SQL scripts and C# seeders) in the specified order
        /// </summary>
        public static async Task ExecuteConfiguredSeedsAsync(UNOPSAppDbContext context, IConfiguration? appConfiguration = null)
        {
            Console.WriteLine("Starting generic seed execution...");
            Console.WriteLine($"Reading seed files from: {AppDomain.CurrentDomain.BaseDirectory}");
            
            var seedConfiguration = await LoadSeedConfigurationAsync();
            var orderedSteps = seedConfiguration.SeedSteps.OrderBy(s => s.Order).ToList();

            Console.WriteLine($"Found {orderedSteps.Count} seed steps to process");

            bool anyStepExecuted = false;

            foreach (var step in orderedSteps)
            {
                Console.WriteLine($"Processing step {step.Order}: {step.Name} ({step.Type})");

                try
                {
                    var currentHash = await CalculateStepHashAsync(step);
                    var existingScript = await context.SeedScripts
                        .FirstOrDefaultAsync(s => s.ScriptName == step.Name);

                    bool shouldExecute = ShouldExecuteStep(existingScript, currentHash, step, anyStepExecuted);

                    if (!shouldExecute)
                    {
                        Console.WriteLine($"Step {step.Name} unchanged. Skipping...");
                        continue;
                    }

                    var action = existingScript == null ? "NEW" : "CHANGED";
                    if (step.ForceExecuteIfAnyChanged && anyStepExecuted)
                    {
                        action = "FORCED (previous steps changed)";
                    }
                    Console.WriteLine($"Executing step: {step.Name} ({action})");

                    if (step.IsSqlScript)
                    {
                        await ExecuteSqlStepAsync(context, step, currentHash, existingScript, appConfiguration);
                    }
                    else if (step.IsSeeder)
                    {
                        await ExecuteSeederStepAsync(context, step, currentHash, existingScript);
                    }

                    anyStepExecuted = true;
                    Console.WriteLine($"Step {step.Name} executed successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing step {step.Name}: {ex.Message}");
                    throw;
                }
            }
            
            Console.WriteLine("All configured seed steps processed.");
        }

        /// <summary>
        /// Loads the seed configuration from JSON file
        /// </summary>
        private static async Task<SeedConfiguration> LoadSeedConfigurationAsync()
        {
            try
            {
                var configPath = GetSeedConfigurationPath();
                
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException($"Seed configuration file not found: {configPath}");
                }

                var jsonContent = await File.ReadAllTextAsync(configPath);
                var configuration = JsonSerializer.Deserialize<SeedConfiguration>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return configuration ?? throw new InvalidOperationException("Failed to deserialize seed configuration");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading seed configuration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Gets the path to the seed configuration file
        /// </summary>
        private static string GetSeedConfigurationPath()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Seed", "SeedConfiguration.json");
        }

        /// <summary>
        /// Calculates hash for a seed step (SQL file content or C# file content)
        /// </summary>
        private static async Task<string> CalculateStepHashAsync(SeedStep step)
        {
            var filePath = GetAutoDetectedFilePath(step);
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"{step.Type.ToUpper()} file not found: {filePath}");
            }

            var contentToHash = await File.ReadAllTextAsync(filePath);
            return CalculateStringHash(contentToHash);
        }

        /// <summary>
        /// Auto-detects the file path based on step type and configuration
        /// </summary>
        private static string GetAutoDetectedFilePath(SeedStep step)
        {
            if (step.IsSqlScript)
            {
                var scriptsDirectory = GetScriptsDirectory();
                
                // If path is provided, use it; otherwise auto-detect based on order and name
                if (!string.IsNullOrEmpty(step.Path))
                {
                    return Path.Combine(scriptsDirectory, step.Path);
                }
                else
                {
                    // Auto-detect: {order:D2}_{name}.sql
                    var autoFileName = $"{step.Order:D2}_{step.Name}.sql";
                    return Path.Combine(scriptsDirectory, autoFileName);
                }
            }
            else if (step.IsSeeder)
            {
                var seedersDirectory = GetSeedersDirectory();
                
                // If filePath is provided, use it; otherwise auto-detect based on className
                if (!string.IsNullOrEmpty(step.FilePath))
                {
                    var seedDirectory = GetSeedDirectory();
                    return Path.Combine(seedDirectory, step.FilePath);
                }
                else
                {
                    // Auto-detect: {className}.cs in Seeders directory
                    var autoFileName = $"{step.ClassName}.cs";
                    return Path.Combine(seedersDirectory, autoFileName);
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown step type: {step.Type}");
            }
        }

        /// <summary>
        /// Determines if a step should be executed based on hash comparison
        /// </summary>
        private static bool ShouldExecuteStep(SeedScript? existingScript, string currentHash, SeedStep step, bool anyPreviousStepExecuted)
        {
            if (existingScript == null)
            {
                return true; // New step
            }

            if (existingScript.FileHash != currentHash)
            {
                return true; // Changed step
            }

            if (step.ForceExecuteIfAnyChanged && anyPreviousStepExecuted)
            {
                return true; // Force execution if any previous step was executed
            }

            return false; // Unchanged step
        }

        /// <summary>
        /// Executes a SQL script step
        /// </summary>
        private static async Task ExecuteSqlStepAsync(UNOPSAppDbContext context, SeedStep step, string currentHash, SeedScript? existingScript, IConfiguration? appConfiguration = null)
        {
            var scriptPath = GetAutoDetectedFilePath(step);
            var scriptsDirectory = GetScriptsDirectory();
            
            var content = await File.ReadAllTextAsync(scriptPath);
            var processedContent = await ProcessScriptContentAsync(content, scriptsDirectory, appConfiguration);

            await ExecutePostgreSqlScript(context, processedContent);
            await UpdateScriptTrackingAsync(context, step.Name, step.Type, currentHash, step.Description, step.Order, existingScript);
        }

        /// <summary>
        /// Executes a C# seeder step using reflection
        /// </summary>
        private static async Task ExecuteSeederStepAsync(UNOPSAppDbContext context, SeedStep step, string currentHash, SeedScript? existingScript)
        {
            try
            {
                // Find the seeder class by name (look in both Seed and Seed.Seeders namespaces)
                var assembly = Assembly.GetExecutingAssembly();
                var seederType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == step.ClassName && 
                        (t.Namespace?.Contains("Seed.Seeders") == true || t.Namespace?.Contains("Seed") == true));

                if (seederType == null)
                {
                    throw new InvalidOperationException($"Seeder class not found: {step.ClassName}");
                }

                // Find the seeder method
                var method = seederType.GetMethod(step.MethodName!);
                if (method == null)
                {
                    throw new InvalidOperationException($"Method not found: {step.MethodName} in {step.ClassName}");
                }

                object? result;
                if (method.IsStatic)
                {
                    // Invoke static method with context parameter
                    result = method.Invoke(null, new object[] { context });
                }
                else
                {
                    // Create instance of the seeder for non-static methods
                    var seederInstance = Activator.CreateInstance(seederType, context);
                    if (seederInstance == null)
                    {
                        throw new InvalidOperationException($"Failed to create instance of: {step.ClassName}");
                    }
                    
                    // Invoke instance method
                    result = method.Invoke(seederInstance, null);
                }
                if (result is Task task)
                {
                    await task;
                }

                await UpdateScriptTrackingAsync(context, step.Name, step.Type, currentHash, step.Description, step.Order, existingScript);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing seeder {step.ClassName}.{step.MethodName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Processes SQL script content with parameter substitution
        /// </summary>
        private static async Task<string> ProcessScriptContentAsync(string content, string scriptsDirectory, IConfiguration? appConfiguration = null)
        {
            // Get PROJECT_ID from configuration, then environment variable, then default
            string projectId = "unops-pao"; // fallback default
            
            if (appConfiguration != null)
            {
                // Try multiple configuration paths
                projectId = appConfiguration["AppConfig:ProjectId"] 
                    ?? appConfiguration["AISettings:ProjectId"]
                    ?? appConfiguration["GoogleDriveSettings:ProjectId"]
                    ?? appConfiguration["PubSub:ProjectId"]
                    ?? appConfiguration["IAP:ProjectId"]
                    ?? projectId;
            }
            
            // Environment variable override
            projectId = Environment.GetEnvironmentVariable("PROJECT_ID") ?? projectId;
            
            Console.WriteLine($"Using PROJECT_ID: {projectId}");
            
            // Replace parameters like {{PROJECT_ID}}, {{SCRIPT_PATH}}
            content = content.Replace("{{PROJECT_ID}}", projectId);
            content = content.Replace("{{SCRIPT_PATH}}", scriptsDirectory);

            return await Task.FromResult(content);
        }

        /// <summary>
        /// Executes PostgreSQL script with proper error handling
        /// </summary>
        private static async Task ExecutePostgreSqlScript(UNOPSAppDbContext context, string content)
        {
            try
            {
                Console.WriteLine($"Executing SQL script with {content.Length} characters");
                Console.WriteLine($"First 200 chars: {content.Substring(0, Math.Min(200, content.Length))}");
                
                // Use direct connection to avoid format string issues with curly braces in JSON
                var connection = context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }

                using var command = connection.CreateCommand();
                command.CommandText = content;
                await command.ExecuteNonQueryAsync();
                
                Console.WriteLine("SQL script executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Execution Error: {ex.Message}");
                Console.WriteLine($"Script content (first 500 chars): {content.Substring(0, Math.Min(500, content.Length))}");
                throw;
            }
        }

        /// <summary>
        /// Updates the seed script tracking record
        /// </summary>
        private static async Task UpdateScriptTrackingAsync(UNOPSAppDbContext context, string scriptName, string scriptType, string fileHash, string description, int order, SeedScript? existingScript)
        {
            if (existingScript != null)
            {
                existingScript.FileHash = fileHash;
                existingScript.LastExecutedDate = DateTime.UtcNow;
                existingScript.Description = description;
                existingScript.ExecutionOrder = order;
                existingScript.ScriptType = scriptType;
                context.SeedScripts.Update(existingScript);
            }
            else
            {
                var newScript = new SeedScript
                {
                    ScriptName = scriptName,
                    ScriptType = scriptType,
                    FileHash = fileHash,
                    LastExecutedDate = DateTime.UtcNow,
                    Description = description,
                    ExecutionOrder = order,
                    Name = scriptName // Required by BaseBusinessEntity
                };
                await context.SeedScripts.AddAsync(newScript);
            }

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Calculates SHA256 hash of string content
        /// </summary>
        private static string CalculateStringHash(string content)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hashBytes);
        }

        /// <summary>
        /// Gets the scripts directory path
        /// </summary>
        private static string GetScriptsDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Seed", "Scripts");
        }

        /// <summary>
        /// Gets the seed directory path
        /// </summary>
        private static string GetSeedDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Seed");
        }

        /// <summary>
        /// Gets the seeders directory path
        /// </summary>
        private static string GetSeedersDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "Seed", "Seeders");
        }
    }
}
