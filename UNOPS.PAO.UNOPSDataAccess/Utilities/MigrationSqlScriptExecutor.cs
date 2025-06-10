using System;
using System.IO;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UNOPS.PAO.UNOPSDataAccess.Utilities
{
    /// <summary>
    /// Utility class for executing SQL scripts from migration files
    /// </summary>
    public static class MigrationSqlScriptExecutor
    {
        /// <summary>
        /// Executes a SQL script from the UNOPS.PAO.Scripts directory
        /// </summary>
        /// <param name="migrationBuilder">The migration builder instance</param>
        /// <param name="scriptFileName">The name of the SQL script file (e.g., "seed-entities.sql")</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within UNOPS.PAO.Scripts (default is root)</param>
        /// <exception cref="FileNotFoundException">Thrown when the SQL script file cannot be found</exception>
        /// <exception cref="InvalidOperationException">Thrown when script execution fails</exception>
        public static void ExecuteSqlScript(MigrationBuilder migrationBuilder, string scriptFileName, string scriptsSubdirectory = null)
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
        /// Executes multiple SQL scripts from the UNOPS.PAO.Scripts directory
        /// </summary>
        /// <param name="migrationBuilder">The migration builder instance</param>
        /// <param name="scriptFileNames">Array of SQL script file names to execute in order</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory within UNOPS.PAO.Scripts (default is root)</param>
        public static void ExecuteSqlScripts(MigrationBuilder migrationBuilder, string[] scriptFileNames, string scriptsSubdirectory = null)
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
        /// <param name="scriptsSubdirectory">Optional subdirectory within UNOPS.PAO.Scripts</param>
        /// <returns>The content of the SQL script</returns>
        /// <exception cref="FileNotFoundException">Thrown when the SQL script file cannot be found</exception>
        public static string ReadSqlScript(string scriptFileName, string scriptsSubdirectory = null)
        {
            if (string.IsNullOrWhiteSpace(scriptFileName))
                throw new ArgumentException("Script file name cannot be null or empty", nameof(scriptFileName));

            try
            {
                // Get the current assembly location
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                var assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
                
                // Navigate to the solution root and find the Scripts directory
                var solutionRoot = FindSolutionRoot(assemblyDirectory);
                var scriptsDirectory = string.IsNullOrWhiteSpace(scriptsSubdirectory) 
                    ? Path.Combine(solutionRoot, "UNOPS.PAO.Scripts")
                    : Path.Combine(solutionRoot, "UNOPS.PAO.Scripts", scriptsSubdirectory);
                
                var scriptsPath = Path.Combine(scriptsDirectory, scriptFileName);
                
                if (File.Exists(scriptsPath))
                {
                    return File.ReadAllText(scriptsPath);
                }
                
                // Fallback: try relative paths
                var fallbackPaths = GenerateFallbackPaths(assemblyDirectory, scriptFileName, scriptsSubdirectory);
                
                foreach (var fallbackPath in fallbackPaths)
                {
                    if (File.Exists(fallbackPath))
                    {
                        return File.ReadAllText(fallbackPath);
                    }
                }
                
                throw new FileNotFoundException($"SQL script not found: {scriptFileName}. Searched in {scriptsPath} and fallback locations.");
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
        public static bool ScriptExists(string scriptFileName, string scriptsSubdirectory = null)
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
        /// Finds the solution root directory by looking for the .sln file
        /// </summary>
        /// <param name="startDirectory">The directory to start searching from</param>
        /// <returns>The solution root directory path</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown when solution root cannot be found</exception>
        private static string FindSolutionRoot(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory ?? Directory.GetCurrentDirectory());
            
            while (directory != null)
            {
                // Look for .sln files
                if (directory.GetFiles("*.sln").Length > 0)
                {
                    return directory.FullName;
                }
                
                // Also look for common solution indicators
                if (directory.GetDirectories("UNOPS.PAO.Scripts").Length > 0)
                {
                    return directory.FullName;
                }
                
                directory = directory.Parent;
            }
            
            throw new DirectoryNotFoundException("Could not find solution root directory. Looked for *.sln files and UNOPS.PAO.Scripts directory.");
        }

        /// <summary>
        /// Generates fallback paths to search for SQL scripts
        /// </summary>
        /// <param name="assemblyDirectory">The assembly directory</param>
        /// <param name="scriptFileName">The script file name</param>
        /// <param name="scriptsSubdirectory">Optional subdirectory</param>
        /// <returns>Array of fallback paths to try</returns>
        private static string[] GenerateFallbackPaths(string assemblyDirectory, string scriptFileName, string scriptsSubdirectory)
        {
            var scriptPath = string.IsNullOrWhiteSpace(scriptsSubdirectory) 
                ? Path.Combine("UNOPS.PAO.Scripts", scriptFileName)
                : Path.Combine("UNOPS.PAO.Scripts", scriptsSubdirectory, scriptFileName);

            var fallbackPaths = new[]
            {
                // Try different relative paths from assembly directory
                Path.Combine("..", "..", "..", "..", scriptPath),
                Path.Combine("..", "..", "..", scriptPath),
                Path.Combine("..", "..", scriptPath),
                Path.Combine("..", scriptPath),
                scriptPath,
                
                // Try from current working directory
                Path.Combine(Directory.GetCurrentDirectory(), scriptPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", scriptPath),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", scriptPath)
            };

            // Convert to full paths
            for (int i = 0; i < fallbackPaths.Length; i++)
            {
                fallbackPaths[i] = Path.GetFullPath(Path.Combine(assemblyDirectory ?? "", fallbackPaths[i]));
            }

            return fallbackPaths;
        }
    }
} 