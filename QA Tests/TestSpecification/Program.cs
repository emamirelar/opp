// See https://aka.ms/new-console-template for more information
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Domain.Specifications.ContactSpecifications;
using System.Diagnostics;
using System.Text.Json;

// Enable debug traces
Trace.Listeners.Add(new ConsoleTraceListener());
Debug.AutoFlush = true;

Console.WriteLine("=== Test ContactCompositeSpecification with detailed debug ===");
Console.WriteLine();

try
{
    Console.WriteLine("SIMPLE TEST: FirstName property");
    Console.WriteLine("===============================");
    
    var simpleFilter = new ContactFilterRequest
    {
        FirstName = "don",
        AdvancedSearch = false
    };
    
    Console.WriteLine($"Filter.FirstName: {simpleFilter.FirstName}");
    Console.WriteLine($"Filter.AdvancedSearch: {simpleFilter.AdvancedSearch}");
    
    var simpleSpec = new ContactCompositeSpecification(simpleFilter);
    Console.WriteLine($"Expression: {simpleSpec.Criteria}");
    
    Console.WriteLine();
    
    Console.WriteLine("SIMPLE TEST: SearchText");
    Console.WriteLine("========================");
    
    var searchFilter = new ContactFilterRequest
    {
        SearchText = "don",
        AdvancedSearch = false
    };
    
    Console.WriteLine($"Filter.SearchText: {searchFilter.SearchText}");
    Console.WriteLine($"Filter.AdvancedSearch: {searchFilter.AdvancedSearch}");
    
    var searchSpec = new ContactCompositeSpecification(searchFilter);
    Console.WriteLine($"Expression: {searchSpec.Criteria}");
    
    Console.WriteLine();
    
    Console.WriteLine("1. JSON parsing test");
    Console.WriteLine("-------------------");
    
    var jsonString = @"[{
        ""field"": ""firstName"",
        ""value"": ""don"",
        ""operator"": ""like"",
        ""logicalOperator"": ""AND""
    }]";
    
    Console.WriteLine($"Original JSON: {jsonString}");
    
    try
    {
        using var doc = JsonDocument.Parse(jsonString);
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
        {
            Console.WriteLine("✓ JSON is a valid array");
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                Console.WriteLine("Element found:");
                foreach (var property in element.EnumerateObject())
                {
                    Console.WriteLine($"  {property.Name}: {property.Value.GetString()}");
                }
            }
        }
        else
        {
            Console.WriteLine("✗ JSON is not an array");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ JSON parsing error: {ex.Message}");
    }
    
    Console.WriteLine();
    
    Console.WriteLine("2. ContactFilterRequest test");
    Console.WriteLine("-----------------------------");
    
    var filter = new ContactFilterRequest
    {
        AdvancedSearch = true,
        SearchCriteria = jsonString
    };
    
    Console.WriteLine($"AdvancedSearch: {filter.AdvancedSearch}");
    Console.WriteLine($"SearchCriteria: {filter.SearchCriteria}");
    
    // Check that properties exist
    var filterType = filter.GetType();
    var advancedSearchProp = filterType.GetProperty("AdvancedSearch");
    var searchCriteriaProp = filterType.GetProperty("SearchCriteria");
    
    Console.WriteLine($"AdvancedSearch property found: {advancedSearchProp != null}");
    Console.WriteLine($"SearchCriteria property found: {searchCriteriaProp != null}");
    
    if (advancedSearchProp != null)
    {
        var advancedSearchValue = advancedSearchProp.GetValue(filter);
        Console.WriteLine($"AdvancedSearch value via reflection: {advancedSearchValue}");
    }
    
    if (searchCriteriaProp != null)
    {
        var searchCriteriaValue = searchCriteriaProp.GetValue(filter);
        Console.WriteLine($"SearchCriteria value via reflection: {searchCriteriaValue}");
    }
    
    Console.WriteLine();
    
    Console.WriteLine("3. Specification creation test");
    Console.WriteLine("------------------------------");
    
    var spec = new ContactCompositeSpecification(filter);
    Console.WriteLine($"Resulting expression: {spec.Criteria}");
    
    Console.WriteLine();
    
    Console.WriteLine("4. Simple SearchText test");
    Console.WriteLine("-------------------------------");
    
    var simpleFilter2 = new ContactFilterRequest
    {
        SearchText = "don",
        AdvancedSearch = false
    };
    
    var simpleSpec2 = new ContactCompositeSpecification(simpleFilter2);
    Console.WriteLine($"SearchText expression: {simpleSpec2.Criteria}");
    
    Console.WriteLine();
    
    Console.WriteLine("5. Direct property test");
    Console.WriteLine("-------------------------------");
    
    var directFilter = new ContactFilterRequest
    {
        FirstName = "don",
        AdvancedSearch = false
    };
    
    var directSpec = new ContactCompositeSpecification(directFilter);
    Console.WriteLine($"FirstName expression: {directSpec.Criteria}");
    
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}

Console.WriteLine();
Console.WriteLine("Test completed.");
