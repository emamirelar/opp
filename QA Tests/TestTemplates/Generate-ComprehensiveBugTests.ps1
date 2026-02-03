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

# 1. UnitTests.cs
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
    /// Tests core validation logic and business rules
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

        [Fact]
        public async Task ValidScenario_ShouldSucceed()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Valid scenario");
            
            // Act & Assert
            Assert.True(true, "TODO: Implement test for $IssueKey");
        }
    }
}
"@ | Out-File -FilePath (Join-Path $testFolderPath "UnitTests.cs") -Encoding UTF8

# 2. IntegrationTests.cs
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
    /// Tests full workflow including database and API
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

# 5. FunctionalTests.cs
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

# 7. PerformanceTests.cs
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
    /// Tests scalability and response times
    /// </summary>
    public sealed class PerformanceTests : IClassFixture<FormManagerTestFixture>
    {
        private readonly FormManagerTestFixture _fixture;
        private readonly ILogger<PerformanceTests> _logger;

        public PerformanceTests(FormManagerTestFixture fixture, ITestOutputHelper output)
        {
            _fixture = fixture;
            _logger = _fixture.GetLogger<PerformanceTests>();
        }

        [Fact]
        public async Task HighLoad_ShouldMeetPerformanceThreshold()
        {
            // Arrange
            _logger.LogInformation("Testing $IssueKey - Performance under load");
            var stopwatch = Stopwatch.StartNew();
            
            // Act
            // TODO: Implement performance test
            
            stopwatch.Stop();
            
            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 1000, $"Operation took {stopwatch.ElapsedMilliseconds}ms (threshold: 1000ms)");
        }
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

# 10. README.md
@"
# ${IssueKey}: $BugDescription

## Overview
**JIRA Ticket:** [$IssueKey](https://unops.atlassian.net/browse/$IssueKey)  
**Module:** $ModuleName  
**Status:** To Do

## Test Coverage

### C# Tests (9 categories)

#### 5 Mandatory Categories (with minimums per comprehensive-test-strategy.mdc)

| File | Category | Minimum Required |
|------|----------|------------------|
| **PositiveTests.cs** | Positive (Happy Path) | 30-50 tests (baseline) |
| **NegativeTests.cs** | Negative (Error Handling) | ≥50 AND ≥2×Positive |
| **BoundaryTests.cs** | Edge Cases | ≥50 AND ≥2×Positive |
| **SecurityTests.cs** | Security/Validation | ≥50 (FIXED) |
| **ConcurrencyTests.cs** | Concurrency | ≥25 (FIXED) |

#### Additional Categories (recommended)
- **UnitTests.cs** - Core validation logic (isolated tests)
- **IntegrationTests.cs** - Full workflow with database
- **FunctionalTests.cs** - Business rules
- **PerformanceTests.cs** - Scalability

### 3:1 Ratio Requirement
``````
(Negative + Boundary) >= 3 × Positive
``````

## Validation
Run the validation script to check compliance:
``````powershell
.\UNOPS.Pdj.Tests\Scripts\Validate-TestRatios.ps1 -Path "$ModuleName\$folderName"
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
Write-Host "   - Positive: 30-50 tests" -ForegroundColor Gray
Write-Host "   - Negative: ≥50 tests" -ForegroundColor Gray
Write-Host "   - Boundary: ≥50 tests" -ForegroundColor Gray
Write-Host "   - Security: ≥50 tests" -ForegroundColor Gray
Write-Host "   - Concurrency: ≥25 tests" -ForegroundColor Gray
Write-Host ""
Write-Host "   Run validation: .\UNOPS.Pdj.Tests\Scripts\Validate-TestRatios.ps1 -Path `"$ModuleName\$folderName`"" -ForegroundColor Cyan
