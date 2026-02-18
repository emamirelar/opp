param(
    [Parameter(Mandatory=$true)]
    [string]$IssueKey,
    
    [Parameter(Mandatory=$true)]
    [string]$BugDescription,
    
    [Parameter(Mandatory=$true)]
    [string]$ModuleName
)

$ErrorActionPreference = "Stop"

# Sanitize folder name
$folderName = "$IssueKey`_$($BugDescription -replace '[^\w\s-]', '' -replace '\s+', '')"
if ($folderName.Length > 100) {
    $folderName = $folderName.Substring(0, 100)
}

$testFolderPath = ".\UNOPS.Pdj.Tests\$ModuleName\$folderName"

# Create folder
if (!(Test-Path $testFolderPath)) {
    New-Item -ItemType Directory -Path $testFolderPath -Force | Out-Null
}

$namespace = "UNOPS.Pdj.Tests.$ModuleName.$($IssueKey.Replace('-', '_'))"

# 1. UnitTests.cs - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Unit Tests
    /// Tests core validation logic and business rules (fast, isolated tests)
    /// MINIMUM REQUIRED: 21 tests - See comprehensive-test-strategy.mdc
    /// Coverage: validation(5), formatting(3), calculations(5), status logic(5), collections(3)
    /// </summary>
    public sealed class UnitTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly ILogger<UnitTests> _logger;

        public UnitTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _logger = _fixture.GetLogger<UnitTests>();
        }

        #region Validation Logic (5 tests minimum)

        [Fact]
        public async Task Validation_ValidInput_ShouldPass()
        {
            _logger.LogInformation("Testing $IssueKey - Valid input validation");
            Assert.True(true, "TODO: Implement validation test");
        }

        // TODO: Add 4 more validation tests

        #endregion

        #region Formatting (3 tests minimum)

        [Fact]
        public async Task Formatting_StandardFormat_ShouldApply()
        {
            _logger.LogInformation("Testing $IssueKey - Formatting");
            Assert.True(true, "TODO: Implement formatting test");
        }

        // TODO: Add 2 more formatting tests

        #endregion

        #region Calculations (5 tests minimum)

        [Fact]
        public async Task Calculation_BasicOperation_ShouldSucceed()
        {
            _logger.LogInformation("Testing $IssueKey - Calculation");
            Assert.True(true, "TODO: Implement calculation test");
        }

        // TODO: Add 4 more calculation tests

        #endregion

        #region Status Logic (5 tests minimum)

        [Fact]
        public async Task StatusLogic_ValidTransition_ShouldSucceed()
        {
            _logger.LogInformation("Testing $IssueKey - Status logic");
            Assert.True(true, "TODO: Implement status logic test");
        }

        // TODO: Add 4 more status logic tests

        #endregion

        #region Collections (3 tests minimum)

        [Fact]
        public async Task Collections_EmptyCollection_ShouldHandle()
        {
            _logger.LogInformation("Testing $IssueKey - Collections");
            Assert.True(true, "TODO: Implement collections test");
        }

        // TODO: Add 2 more collection tests

        #endregion
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "UnitTests.cs") -Encoding UTF8

# 2. IntegrationTests.cs - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Integration Tests
    /// Tests full workflow including database and API (expensive to run)
    /// MINIMUM REQUIRED: 25 tests - See comprehensive-test-strategy.mdc
    /// Coverage: CRUD workflow(5), search/filter(5), pagination(2), relationships(3), error handling(10)
    /// </summary>
    public sealed class IntegrationTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<IntegrationTests> _logger;

        public IntegrationTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<IntegrationTests>();
        }

        #region CRUD Workflow (5 tests minimum)

        // TODO: Add 5 CRUD workflow tests

        #endregion

        #region Search/Filter (5 tests minimum)

        // TODO: Add 5 search/filter tests

        #endregion

        #region Pagination (2 tests minimum)

        // TODO: Add 2 pagination tests

        #endregion

        #region Relationships (3 tests minimum)

        // TODO: Add 3 relationship tests

        #endregion

        #region Error Handling (10 tests minimum)

        [Fact]
        public async Task EndToEndWorkflow_ShouldComplete()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - End-to-end workflow");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "IntegrationTests.cs") -Encoding UTF8

# 3. PositiveTests.cs
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Positive Tests
    /// Tests success scenarios and happy paths
    /// BASELINE: 30-50 tests - See comprehensive-test-strategy.mdc
    /// </summary>
    public sealed class PositiveTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<PositiveTests> _logger;

        public PositiveTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<PositiveTests>();
        }

        [Fact]
        public async Task HappyPath_ShouldSucceed()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Happy path");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "PositiveTests.cs") -Encoding UTF8

# 4. NegativeTests.cs
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Negative Tests
    /// Tests error scenarios and failure handling
    /// MINIMUM REQUIRED: 50 tests AND >= 2x Positive tests - See comprehensive-test-strategy.mdc
    /// </summary>
    public sealed class NegativeTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<NegativeTests> _logger;

        public NegativeTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<NegativeTests>();
        }

        [Fact]
        public async Task InvalidInput_ShouldThrowException()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Invalid input");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "NegativeTests.cs") -Encoding UTF8

# 5. FunctionalTests.cs - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Functional Tests
    /// Tests business rules and requirements
    /// MINIMUM REQUIRED: 26 tests - See comprehensive-test-strategy.mdc
    /// Coverage: workflow rules(10), validation rules(10), constraint rules(3), audit rules(3)
    /// </summary>
    public sealed class FunctionalTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<FunctionalTests> _logger;

        public FunctionalTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<FunctionalTests>();
        }

        #region Workflow Rules (10 tests minimum)

        // TODO: Add 10 workflow rules tests

        #endregion

        #region Validation Rules (10 tests minimum)

        // TODO: Add 10 validation rules tests

        #endregion

        #region Constraint Rules (3 tests minimum)

        // TODO: Add 3 constraint rules tests

        #endregion

        #region Audit Rules (3 tests minimum)

        [Fact]
        public async Task BusinessRule_ShouldBeEnforced()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Business rule");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "FunctionalTests.cs") -Encoding UTF8

# 6. SecurityTests.cs
@"
namespace $namespace
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Security Tests
    /// Tests authorization, input validation, and OWASP vulnerabilities
    /// MINIMUM REQUIRED: 50 tests (FIXED) - See comprehensive-test-strategy.mdc
    /// </summary>
    public sealed class SecurityTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<SecurityTests> _logger;

        public SecurityTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<SecurityTests>();
        }

        [Fact]
        public async Task UnauthorizedAccess_ShouldBeDenied()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Unauthorized access");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "SecurityTests.cs") -Encoding UTF8

# 7. PerformanceTests.cs - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Performance Tests
    /// Tests scalability and response times (focused on critical paths)
    /// MINIMUM REQUIRED: 16 tests - See comprehensive-test-strategy.mdc
    /// Coverage: single ops(2), bulk ops(3), search(5), concurrent access(3), memory(3)
    /// </summary>
    public sealed class PerformanceTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly ILogger<PerformanceTests> _logger;

        public PerformanceTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _logger = _fixture.GetLogger<PerformanceTests>();
        }

        #region Single Operations (2 tests minimum)

        [Fact]
        public async Task SingleOp_Create_ShouldCompleteUnder100ms()
        {
            var stopwatch = Stopwatch.StartNew();
            // TODO: Single create operation
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 100);
            _output.WriteLine($"Single create: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: Add 1 more single operation test

        #endregion

        #region Bulk Operations (3 tests minimum)

        [Fact]
        public async Task BulkOp_Create100_ShouldCompleteReasonably()
        {
            var stopwatch = Stopwatch.StartNew();
            // TODO: Bulk create 100 items
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 5000);
            _output.WriteLine($"Bulk create 100: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: Add 2 more bulk operation tests

        #endregion

        #region Search Performance (5 tests minimum)

        [Fact]
        public async Task Search_SimpleQuery_ShouldCompleteUnder200ms()
        {
            var stopwatch = Stopwatch.StartNew();
            // TODO: Simple search
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 200);
            _output.WriteLine($"Simple search: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: Add 4 more search performance tests

        #endregion

        #region Concurrent Access (3 tests minimum)

        [Fact]
        public async Task ConcurrentAccess_10Parallel_ShouldComplete()
        {
            var stopwatch = Stopwatch.StartNew();
            var tasks = Enumerable.Range(0, 10).Select(async i =>
            {
                // TODO: Concurrent operation
                await Task.Delay(1);
            });
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            _output.WriteLine($"10 parallel ops: {stopwatch.ElapsedMilliseconds}ms");
        }

        // TODO: Add 2 more concurrent access tests

        #endregion

        #region Memory Usage (3 tests minimum)

        [Fact]
        public async Task Memory_LargeDataSet_ShouldNotExceedLimit()
        {
            var beforeMem = GC.GetTotalMemory(true);
            // TODO: Process large dataset
            var afterMem = GC.GetTotalMemory(true);
            var memUsed = (afterMem - beforeMem) / 1024 / 1024;
            _output.WriteLine($"Memory used: {memUsed}MB");
            Assert.True(memUsed < 100, $"Memory usage {memUsed}MB exceeds 100MB limit");
        }

        // TODO: Add 2 more memory tests

        #endregion
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "PerformanceTests.cs") -Encoding UTF8

# 8. ConcurrencyTests.cs
@"
namespace $namespace
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Concurrency Tests
    /// Tests race conditions and concurrent access
    /// MINIMUM REQUIRED: 25 tests (FIXED) - See comprehensive-test-strategy.mdc
    /// </summary>
    public sealed class ConcurrencyTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<ConcurrencyTests> _logger;

        public ConcurrencyTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<ConcurrencyTests>();
        }

        [Fact]
        public async Task ConcurrentAccess_ShouldHandleCorrectly()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Concurrent access");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        // TODO: Add 24+ more concurrency tests to meet the minimum requirement of 25
        // See comprehensive-test-strategy.mdc for test ideas:
        // - Parallel read operations
        // - Parallel write operations  
        // - Mixed read/write operations
        // - Race condition scenarios
        // - Deadlock prevention tests
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "ConcurrencyTests.cs") -Encoding UTF8

# 9. BoundaryTests.cs (Edge Cases) - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Boundary/Edge Case Tests
    /// Tests boundary values, extreme inputs, and edge conditions
    /// MINIMUM REQUIRED: 50 tests AND >= 2x Positive tests - See comprehensive-test-strategy.mdc
    /// </summary>
    public sealed class BoundaryTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly ILogger<BoundaryTests> _logger;

        public BoundaryTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _logger = _fixture.GetLogger<BoundaryTests>();
        }

        #region String Length Boundaries

        [Fact]
        public async Task Boundary_EmptyString_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Empty string boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_SingleCharacter_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Single character boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_MaxLengthString_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Max length string boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        #endregion

        #region Numeric Boundaries

        [Fact]
        public async Task Boundary_ZeroValue_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Zero value boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_NegativeValue_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Negative value boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_MaxIntValue_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Max int value boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        #endregion

        #region Collection Boundaries

        [Fact]
        public async Task Boundary_EmptyCollection_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Empty collection boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_SingleItemCollection_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Single item collection boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_LargeCollection_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Large collection boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        #endregion

        #region Special Characters

        [Fact]
        public async Task Boundary_UnicodeCharacters_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Unicode characters boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        [Fact]
        public async Task Boundary_SpecialCharacters_ShouldBeHandled()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Special characters boundary");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }

        #endregion

        // TODO: Add 39+ more boundary tests to meet the minimum requirement of 50
        // See comprehensive-test-strategy.mdc for test ideas:
        // - Date/time boundaries (min/max dates, timezone edge cases)
        // - ID boundaries (zero, negative, max value, non-existent)
        // - Whitespace handling (leading, trailing, only whitespace)
        // - Null value handling
        // - Precision boundaries for decimals
        // - Enum boundaries (undefined values)
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "BoundaryTests.cs") -Encoding UTF8

# 10. LoadTests.cs - MANDATORY per comprehensive-test-strategy.mdc
@"
namespace $namespace
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using UNOPS.Pdj.Tests.TestBase;
    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// ${IssueKey}: $BugDescription - Load Tests
    /// Tests system behavior under sustained and spike load conditions
    /// MINIMUM REQUIRED: 10 tests (FIXED) - See comprehensive-test-strategy.mdc
    /// Coverage: sustained load(3), spike load(2), stress limits(3), recovery(2)
    /// </summary>
    public sealed class LoadTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ITestOutputHelper _output;
        private readonly ILogger<LoadTests> _logger;

        public LoadTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _output = output;
            _logger = _fixture.GetLogger<LoadTests>();
        }

        #region Sustained Load (3 tests minimum)

        [Fact]
        public async Task SustainedLoad_100RequestsPerSecond_ShouldMaintainPerformance()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Sustained load");
            var stopwatch = Stopwatch.StartNew();
            var results = new ConcurrentBag<long>();
            
            // Act - Simulate sustained load
            var tasks = Enumerable.Range(0, 100).Select(async i =>
            {
                var sw = Stopwatch.StartNew();
                // TODO: Add actual operation
                await Task.Delay(1);
                sw.Stop();
                results.Add(sw.ElapsedMilliseconds);
            });
            
            await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            // Assert
            Assert.True(results.Average() < 100, "Average response time should be under 100ms");
            _output.WriteLine($"Completed {results.Count} requests in {stopwatch.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task SustainedLoad_ContinuousOperations_ShouldNotDegrade()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Continuous operations");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement sustained load degradation test");
        }

        [Fact]
        public async Task SustainedLoad_MemoryStable_ShouldNotLeak()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Memory stability under load");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement memory stability test");
        }

        #endregion

        #region Spike Load (2 tests minimum)

        [Fact]
        public async Task SpikeLoad_SuddenTrafficIncrease_ShouldHandle()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Spike load");
            
            // Act - Simulate traffic spike
            var tasks = Enumerable.Range(0, 500).Select(async i =>
            {
                // TODO: Add actual operation
                await Task.Delay(1);
            });
            
            await Task.WhenAll(tasks);
            
            // Assert
            Assert.True(true, "Spike load handled");
        }

        [Fact]
        public async Task SpikeLoad_BurstRequests_ShouldNotTimeout()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Burst requests");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement burst request test");
        }

        #endregion

        #region Stress Limits (3 tests minimum)

        [Fact]
        public async Task StressLimit_MaxConcurrentUsers_ShouldIdentify()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Max concurrent users");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement max concurrent users test");
        }

        [Fact]
        public async Task StressLimit_MaxDataVolume_ShouldHandle()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Max data volume");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement max data volume test");
        }

        [Fact]
        public async Task StressLimit_ResourceExhaustion_ShouldGracefullyDegrade()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Resource exhaustion");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement graceful degradation test");
        }

        #endregion

        #region Recovery (2 tests minimum)

        [Fact]
        public async Task Recovery_AfterHighLoad_ShouldReturnToNormal()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Recovery after high load");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement recovery test");
        }

        [Fact]
        public async Task Recovery_AfterResourceExhaustion_ShouldRecover()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Recovery after exhaustion");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement exhaustion recovery test");
        }

        #endregion
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "LoadTests.cs") -Encoding UTF8

# 11. README.md
@"
# ${IssueKey}: $BugDescription

## Overview
**JIRA Ticket:** [$IssueKey](https://unops.atlassian.net/browse/$IssueKey)  
**Module:** $ModuleName  
**Status:** To Do

## Test Coverage

### C# Tests (10 mandatory categories)

#### Core Categories (scale with Positive)

| File | Category | Minimum Required |
|------|----------|------------------|
| **PositiveTests.cs** | Positive (Happy Path) | 30-50 tests (baseline P) |
| **NegativeTests.cs** | Negative (Error Handling) | ≥50 AND ≥2×P |
| **BoundaryTests.cs** | Edge Cases | ≥50 AND ≥2×P |
| **SecurityTests.cs** | Security/Validation | ≥50 (FIXED) |
| **ConcurrencyTests.cs** | Concurrency | ≥25 (FIXED) |

#### Additional Mandatory Categories (fixed minimums)

| File | Category | Minimum | Coverage Areas |
|------|----------|---------|----------------|
| **UnitTests.cs** | Unit Tests | ≥21 | validation(5), formatting(3), calculations(5), status logic(5), collections(3) |
| **FunctionalTests.cs** | Functional | ≥26 | workflow rules(10), validation rules(10), constraint rules(3), audit rules(3) |
| **IntegrationTests.cs** | Integration | ≥25 | CRUD(5), search/filter(5), pagination(2), relationships(3), error handling(10) |
| **PerformanceTests.cs** | Performance | ≥16 | single ops(2), bulk ops(3), search(5), concurrent access(3), memory(3) |
| **LoadTests.cs** | Load Tests | ≥10 | sustained load(3), spike load(2), stress limits(3), recovery(2) |

### Grand Total Minimum: ~293+ tests per suite

### 3:1 Ratio Requirement
``````
Negative >= 3 × Positive
Edge/Boundary >= 3 × Positive
Functional >= 3 × Positive
Integration >= 3 × Positive
``````

## Validation
Run the validation script to check compliance:
``````powershell
.\UNOPS.Pdj.Tests\Scripts\Validate-AllTestSuites.ps1 -Module "$ModuleName"
``````

## Test Execution
``````bash
# Run all tests
dotnet test --filter "FullyQualifiedName~$($IssueKey.Replace('-', '_'))"

# Run specific category
dotnet test --filter "FullyQualifiedName~$($IssueKey.Replace('-', '_'))_Positive"
``````
"@ | Out-File -FilePath (Join-Path $testFolderPath "README.md") -Encoding UTF8

# Run validation reminder
Write-Host "✓ Generated comprehensive test suite for $IssueKey ($folderName)" -ForegroundColor Green
Write-Host ""
Write-Host "⚠️  IMPORTANT: This is a skeleton with placeholder tests." -ForegroundColor Yellow
Write-Host "   You must implement tests to meet minimum requirements:" -ForegroundColor Yellow
Write-Host ""
Write-Host "   CORE (scale with Positive):" -ForegroundColor Cyan
Write-Host "   - Positive: 30-50 tests (baseline)" -ForegroundColor Gray
Write-Host "   - Negative: ≥50 AND ≥2×Positive" -ForegroundColor Gray
Write-Host "   - Boundary: ≥50 AND ≥2×Positive" -ForegroundColor Gray
Write-Host "   - Security: ≥50 tests" -ForegroundColor Gray
Write-Host "   - Concurrency: ≥25 tests" -ForegroundColor Gray
Write-Host ""
Write-Host "   ADDITIONAL (fixed minimums):" -ForegroundColor Cyan
Write-Host "   - Unit: ≥21 tests" -ForegroundColor Gray
Write-Host "   - Functional: ≥26 tests" -ForegroundColor Gray
Write-Host "   - Integration: ≥25 tests" -ForegroundColor Gray
Write-Host "   - Performance: ≥16 tests" -ForegroundColor Gray
Write-Host "   - Load: ≥10 tests" -ForegroundColor Gray
Write-Host ""
Write-Host "   TOTAL MINIMUM: ~293+ tests per suite" -ForegroundColor Yellow
Write-Host ""
Write-Host "   Run validation: .\UNOPS.Pdj.Tests\Scripts\Validate-AllTestSuites.ps1 -Module `"$ModuleName`"" -ForegroundColor Cyan
