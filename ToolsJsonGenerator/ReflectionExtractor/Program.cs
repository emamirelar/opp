using System.CommandLine;
using System.Reflection;
using ReflectionExtractor;

namespace ReflectionExtractor;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var dllOption = new Option<string>(
            name: "--dll",
            description: "Path to the compiled assembly DLL")
        {
            IsRequired = true
        };

        var xmlOption = new Option<string>(
            name: "--xml",
            description: "Path to the XML documentation file")
        {
            IsRequired = true
        };

        var outputOption = new Option<string>(
            name: "--output",
            description: "Output path for the extracted endpoints JSON file",
            getDefaultValue: () => "api-metadata.json");

        var rootCommand = new RootCommand("Extract endpoint metadata from .NET assembly and XML documentation")
        {
            dllOption,
            xmlOption,
            outputOption
        };

        rootCommand.SetHandler(async (dllPath, xmlPath, outputPath) =>
        {
            try
            {
                Console.WriteLine("[INFO] Starting endpoint extraction...");
                Console.WriteLine($"   Assembly: {dllPath}");
                Console.WriteLine($"   XML Docs: {xmlPath}");
                Console.WriteLine($"   Output File: {outputPath}");
                Console.WriteLine();

                // Validate input files exist
                if (!File.Exists(dllPath))
                {
                    Console.WriteLine($"[ERROR] Assembly file not found: {dllPath}");
                    Environment.Exit(1);
                }

                if (!File.Exists(xmlPath))
                {
                    Console.WriteLine($"[ERROR] XML documentation file not found: {xmlPath}");
                    Environment.Exit(1);
                }

                // Load and analyze the assembly
                var analyzer = new EndpointAnalyzer();
                var endpointData = await analyzer.ExtractEndpointsAsync(dllPath, xmlPath);

                // Save to output file
                await File.WriteAllTextAsync(outputPath, endpointData);

                Console.WriteLine($"[SUCCESS] Successfully extracted endpoints to: {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }, dllOption, xmlOption, outputOption);

        return await rootCommand.InvokeAsync(args);
    }


} 