using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UNOPS.PAO.UNOPSDataAccess.Utilities
{
    /// <summary>
    /// Utility class for executing SQL scripts from migration files.
    /// Scripts are expected to be located in the Scripts folder within the UNOPSDataAccess project.
    /// </summary>
    public static class MigrationSqlScriptExecutor
    {
        /// <summary>
        /// Executes a SQL script from the Scripts directory
        /// </summary>
        /// <param name="migrationBuilder">The migration builder instance</param>
        /// <param name="scriptFileName">The name of the SQL script file (e.g., "seed-entities.sql")</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within Scripts (default is root)</param>
        /// <exception cref="FileNotFoundException">Thrown when the SQL script file cannot be found</exception>
        /// <exception cref="InvalidOperationException">Thrown when script execution fails</exception>
        public static void ExecuteSqlScript(MigrationBuilder migrationBuilder, string scriptFileName, string? scriptsSubdirectory = null)
        {
            if (migrationBuilder == null)
                throw new ArgumentNullException(nameof(migrationBuilder));
            
            if (string.IsNullOrWhiteSpace(scriptFileName))
                throw new ArgumentException("Script file name cannot be null or empty", nameof(scriptFileName));

            try
            {
                var sqlScript = ReadSqlScript(scriptFileName, scriptsSubdirectory);
                migrationBuilder.Sql(sqlScript);
            }
            catch (Exception ex) when (!(ex is FileNotFoundException || ex is InvalidOperationException))
            {
                throw new InvalidOperationException($"Failed to execute SQL script '{scriptFileName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Executes multiple SQL scripts from the Scripts directory
        /// </summary>
        /// <param name="migrationBuilder">The migration builder instance</param>
        /// <param name="scriptFileNames">Array of SQL script file names to execute in order</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within Scripts (default is root)</param>
        public static void ExecuteSqlScripts(MigrationBuilder migrationBuilder, string[] scriptFileNames, string? scriptsSubdirectory = null)
        {
            if (migrationBuilder == null)
                throw new ArgumentNullException(nameof(migrationBuilder));
            
            if (scriptFileNames == null)
                throw new ArgumentNullException(nameof(scriptFileNames));

            foreach (var scriptFileName in scriptFileNames)
            {
                ExecuteSqlScript(migrationBuilder, scriptFileName, scriptsSubdirectory);
            }
        }

        /// <summary>
        /// Reads a SQL script file and returns its content
        /// </summary>
        /// <param name="scriptFileName">The name of the SQL script file</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within Scripts</param>
        /// <returns>The content of the SQL script</returns>
        /// <exception cref="FileNotFoundException">Thrown when the SQL script file cannot be found</exception>
        public static string ReadSqlScript(string scriptFileName, string? scriptsSubdirectory = null)
        {
            if (string.IsNullOrWhiteSpace(scriptFileName))
                throw new ArgumentException("Script file name cannot be null or empty", nameof(scriptFileName));

            try
            {
                // Get the current assembly location
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation)
                    ?? throw new InvalidOperationException("Could not determine assembly directory");
                
                // Find scripts in the local Scripts folder
                var scriptPath = FindLocalScript(assemblyDirectory, scriptFileName, scriptsSubdirectory);
                if (File.Exists(scriptPath))
                {
                    return File.ReadAllText(scriptPath);
                }
                
                throw new FileNotFoundException($"SQL script not found: {scriptFileName}. Expected location: {scriptPath}");
            }
            catch (Exception ex) when (!(ex is FileNotFoundException))
            {
                throw new InvalidOperationException($"Failed to read SQL script '{scriptFileName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Checks if a SQL script file exists
        /// </summary>
        /// <param name="scriptFileName">The name of the SQL script file</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within UNOPS.PAO.Scripts</param>
        /// <returns>True if the script file exists, false otherwise</returns>
        public static bool ScriptExists(string scriptFileName, string? scriptsSubdirectory = null)
        {
            try
            {
                ReadSqlScript(scriptFileName, scriptsSubdirectory);
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Finds a SQL script in the local Scripts folder within the UNOPSDataAccess project
        /// </summary>
        /// <param name="assemblyDirectory">The assembly directory</param>
        /// <param name="scriptFileName">The script file name</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory</param>
        /// <returns>The full path to the script file</returns>
        private static string FindLocalScript(string assemblyDirectory, string scriptFileName, string? scriptsSubdirectory)
        {
            // Look for Scripts folder relative to the assembly directory
            var scriptsPath = Path.Combine(assemblyDirectory, "Scripts");
            
            var scriptPath = string.IsNullOrWhiteSpace(scriptsSubdirectory)
                ? Path.Combine(scriptsPath, scriptFileName)
                : Path.Combine(scriptsPath, scriptsSubdirectory, scriptFileName);

            return scriptPath;
        }
    }
} 