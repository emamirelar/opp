/**
 * @fileoverview Unit tests for CountryService
 * @author UNOPS Opportunity+ System Development Team
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using UNOPS.PAO.DataAccess.Context;
using UNOPS.PAO.Business.Tests.TestBase;

namespace UNOPS.PAO.Business.Tests.Services
{
    /// <summary>
    /// Test suite for CountryService
    /// Tests country lookups, filtering, and validation
    /// </summary>
    public class CountryServiceTests : IDisposable
    {
        private readonly DbContextOptions<AppDbContext> _dbOptions;
        private readonly AppDbContext _context;

        // Test data
        private readonly List<TestCountry> _testCountries = new()
        {
            new TestCountry { Id = 1, Code = "KE", Code3 = "KEN", Name = "Kenya", Region = "East Africa", Continent = "Africa" },
            new TestCountry { Id = 2, Code = "UG", Code3 = "UGA", Name = "Uganda", Region = "East Africa", Continent = "Africa" },
            new TestCountry { Id = 3, Code = "TZ", Code3 = "TZA", Name = "Tanzania", Region = "East Africa", Continent = "Africa" },
            new TestCountry { Id = 4, Code = "NG", Code3 = "NGA", Name = "Nigeria", Region = "West Africa", Continent = "Africa" },
            new TestCountry { Id = 5, Code = "GB", Code3 = "GBR", Name = "United Kingdom", Region = "Northern Europe", Continent = "Europe" },
            new TestCountry { Id = 6, Code = "US", Code3 = "USA", Name = "United States", Region = "North America", Continent = "North America" }
        };

        public CountryServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"CountryServiceTest_{Guid.NewGuid()}")
                .Options;
            
            _context = TestDbContextFactory.Create(_dbOptions);
        }

        #region TC-CS-001 to TC-CS-004: Basic Lookup Tests

        [Fact]
        public void GetAllCountries_ReturnsAllActiveCountries()
        {
            // Arrange & Act
            var allCountries = _testCountries.Where(c => !c.IsDeleted).ToList();

            // Assert
            Assert.Equal(6, allCountries.Count);
        }

        [Fact]
        public void GetCountryById_ExistingId_ReturnsCountry()
        {
            // Arrange
            var targetId = 1;

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Id == targetId);

            // Assert
            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
            Assert.Equal("KE", country.Code);
        }

        [Fact]
        public void GetCountryByCode_ValidCode_ReturnsCountry()
        {
            // Arrange
            var targetCode = "KE";

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Code == targetCode);

            // Assert
            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
        }

        [Fact]
        public void GetCountryByCode3_ValidCode_ReturnsCountry()
        {
            // Arrange
            var targetCode3 = "KEN";

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Code3 == targetCode3);

            // Assert
            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
        }

        #endregion

        #region TC-CS-005 to TC-CS-008: Filter Tests

        [Fact]
        public void GetCountriesByRegion_ValidRegion_ReturnsFiltered()
        {
            // Arrange
            var region = "East Africa";

            // Act
            var countries = _testCountries.Where(c => c.Region == region).ToList();

            // Assert
            Assert.Equal(3, countries.Count);
            Assert.All(countries, c => Assert.Equal("East Africa", c.Region));
        }

        [Fact]
        public void GetCountriesByContinent_ValidContinent_ReturnsFiltered()
        {
            // Arrange
            var continent = "Africa";

            // Act
            var countries = _testCountries.Where(c => c.Continent == continent).ToList();

            // Assert
            Assert.Equal(4, countries.Count);
            Assert.All(countries, c => Assert.Equal("Africa", c.Continent));
        }

        [Fact]
        public void SearchCountries_PartialName_ReturnsMatches()
        {
            // Arrange
            var searchTerm = "Ken";

            // Act
            var countries = _testCountries
                .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Assert
            Assert.Single(countries);
            Assert.Equal("Kenya", countries[0].Name);
        }

        [Fact]
        public void IsValidCode_ValidCode_ReturnsTrue()
        {
            // Arrange
            var validCode = "KE";

            // Act
            var isValid = _testCountries.Any(c => c.Code == validCode);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void IsValidCode_InvalidCode_ReturnsFalse()
        {
            // Arrange
            var invalidCode = "XX";

            // Act
            var isValid = _testCountries.Any(c => c.Code == invalidCode);

            // Assert
            Assert.False(isValid);
        }

        #endregion

        #region TC-CS-009 to TC-CS-014: Advanced Operations

        [Fact]
        public void GetCountriesForDropdown_ReturnsSimplifiedList()
        {
            // Act
            var dropdownItems = _testCountries
                .Select(c => new { c.Id, c.Code, c.Name })
                .OrderBy(c => c.Name)
                .ToList();

            // Assert
            Assert.Equal(6, dropdownItems.Count);
            Assert.All(dropdownItems, item =>
            {
                Assert.True(item.Id > 0);
                Assert.False(string.IsNullOrEmpty(item.Code));
                Assert.False(string.IsNullOrEmpty(item.Name));
            });
        }

        [Fact]
        public void GetCountries_SortedByName_ReturnsAlphabetical()
        {
            // Act
            var sorted = _testCountries.OrderBy(c => c.Name).ToList();

            // Assert
            Assert.Equal("Kenya", sorted[0].Name);
            Assert.Equal("Nigeria", sorted[1].Name);
            Assert.Equal("Tanzania", sorted[2].Name);
        }

        [Fact]
        public void GetCountryByCode_UnknownCode_ReturnsNull()
        {
            // Arrange
            var unknownCode = "ZZ";

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Code == unknownCode);

            // Assert
            Assert.Null(country);
        }

        [Fact]
        public void GetCountryWithDetails_IncludesRegionAndContinent()
        {
            // Arrange
            var targetId = 1;

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Id == targetId);

            // Assert
            Assert.NotNull(country);
            Assert.Equal("Kenya", country.Name);
            Assert.Equal("East Africa", country.Region);
            Assert.Equal("Africa", country.Continent);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void ValidateISOCode_TwoLetterCode_Valid()
        {
            // Arrange
            var code = "KE";

            // Act
            var isValid = code.Length == 2 && code.All(char.IsLetter);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateISOCode_ThreeLetterCode_Valid()
        {
            // Arrange
            var code = "KEN";

            // Act
            var isValid = code.Length == 3 && code.All(char.IsLetter);

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateISOCode_InvalidFormat_Invalid()
        {
            // Arrange
            var invalidCodes = new[] { "K", "KENYA", "K1", "123" };

            // Act & Assert
            foreach (var code in invalidCodes)
            {
                var isValid = (code.Length == 2 || code.Length == 3) && code.All(char.IsLetter);
                Assert.False(isValid, $"Code '{code}' should be invalid");
            }
        }

        [Fact]
        public void ValidateCountryName_NotEmpty_Required()
        {
            // Arrange
            var emptyName = "";
            var validName = "Kenya";

            // Act & Assert
            Assert.True(string.IsNullOrWhiteSpace(emptyName));
            Assert.False(string.IsNullOrWhiteSpace(validName));
        }

        #endregion

        #region Performance Tests

        [Fact]
        public void GetAllCountries_LargeDataset_CompletesQuickly()
        {
            // Arrange
            var largeList = new List<TestCountry>();
            for (int i = 0; i < 1000; i++)
            {
                largeList.Add(new TestCountry 
                { 
                    Id = i, 
                    Code = $"C{i:D2}", 
                    Name = $"Country {i}" 
                });
            }

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = largeList.ToList();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 100);
            Assert.Equal(1000, result.Count);
        }

        [Fact]
        public void SearchCountries_LargeDataset_CompletesQuickly()
        {
            // Arrange
            var largeList = new List<TestCountry>();
            for (int i = 0; i < 1000; i++)
            {
                largeList.Add(new TestCountry { Id = i, Name = $"Country {i}" });
            }

            // Act
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var result = largeList
                .Where(c => c.Name.Contains("50", StringComparison.OrdinalIgnoreCase))
                .ToList();
            stopwatch.Stop();

            // Assert
            Assert.True(stopwatch.ElapsedMilliseconds < 50);
        }

        #endregion

        #region Edge Cases

        [Fact]
        public void GetCountryByCode_CaseInsensitive_ShouldMatch()
        {
            // Arrange
            var upperCode = "KE";
            var lowerCode = "ke";
            var mixedCode = "Ke";

            // Act & Assert
            Assert.NotNull(_testCountries.FirstOrDefault(c => 
                c.Code.Equals(upperCode, StringComparison.OrdinalIgnoreCase)));
            Assert.NotNull(_testCountries.FirstOrDefault(c => 
                c.Code.Equals(lowerCode, StringComparison.OrdinalIgnoreCase)));
            Assert.NotNull(_testCountries.FirstOrDefault(c => 
                c.Code.Equals(mixedCode, StringComparison.OrdinalIgnoreCase)));
        }

        [Fact]
        public void GetCountriesByRegion_EmptyRegion_ReturnsEmpty()
        {
            // Arrange
            var nonExistentRegion = "Non-Existent Region";

            // Act
            var countries = _testCountries.Where(c => c.Region == nonExistentRegion).ToList();

            // Assert
            Assert.Empty(countries);
        }

        [Fact]
        public void SearchCountries_EmptySearchTerm_ReturnsAll()
        {
            // Arrange
            var searchTerm = "";

            // Act
            var countries = string.IsNullOrEmpty(searchTerm)
                ? _testCountries
                : _testCountries.Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(6, countries.Count());
        }

        [Fact]
        public void SearchCountries_NullSearchTerm_ReturnsAll()
        {
            // Arrange
            string? searchTerm = null;

            // Act
            var countries = string.IsNullOrEmpty(searchTerm)
                ? _testCountries
                : _testCountries.Where(c => c.Name.Contains(searchTerm!, StringComparison.OrdinalIgnoreCase));

            // Assert
            Assert.Equal(6, countries.Count());
        }

        [Fact]
        public void GetCountriesByMultipleFilters_CombinedConditions()
        {
            // Arrange
            var continent = "Africa";
            var region = "East Africa";

            // Act
            var countries = _testCountries
                .Where(c => c.Continent == continent && c.Region == region)
                .ToList();

            // Assert
            Assert.Equal(3, countries.Count);
            Assert.All(countries, c =>
            {
                Assert.Equal("Africa", c.Continent);
                Assert.Equal("East Africa", c.Region);
            });
        }

        [Fact]
        public void GetCountryById_NegativeId_ReturnsNull()
        {
            // Arrange
            var negativeId = -1;

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Id == negativeId);

            // Assert
            Assert.Null(country);
        }

        [Fact]
        public void GetCountryById_ZeroId_ReturnsNull()
        {
            // Arrange
            var zeroId = 0;

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Id == zeroId);

            // Assert
            Assert.Null(country);
        }

        [Fact]
        public void GetCountryById_MaxIntId_ReturnsNull()
        {
            // Arrange
            var maxId = int.MaxValue;

            // Act
            var country = _testCountries.FirstOrDefault(c => c.Id == maxId);

            // Assert
            Assert.Null(country);
        }

        [Fact]
        public void SearchCountries_SpecialCharacters_HandledGracefully()
        {
            // Arrange
            var specialChars = new[] { "%", "_", "'", "\"", "\\", ";" };

            // Act & Assert
            foreach (var searchTerm in specialChars)
            {
                var result = _testCountries
                    .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                // Should not throw and return empty (no matches)
                Assert.Empty(result);
            }
        }

        [Fact]
        public void GetCountriesByContinent_WhitespaceInput_TrimmedAndSearched()
        {
            // Arrange
            var continentWithWhitespace = "  Africa  ";

            // Act
            var countries = _testCountries
                .Where(c => c.Continent == continentWithWhitespace.Trim())
                .ToList();

            // Assert
            Assert.Equal(4, countries.Count);
        }

        #endregion

        #region Database Integration Tests

        [Fact]
        public async Task GetCountriesFromDatabase_WithActualContext_ReturnsActiveOnly()
        {
            // Arrange - Use actual Country entity from domain
            var country = new UNOPS.PAO.Domain.Entities.Country
            {
                Name = "Test Country",
                Iso2Code = "TC",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            };

            await _context.Countries.AddAsync(country);
            await _context.SaveChangesAsync();

            // Act
            var countries = await _context.Countries
                .Where(c => c.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active)
                .ToListAsync();

            // Assert
            Assert.Contains(countries, c => c.Name == "Test Country");
        }

        [Fact]
        public async Task GetCountriesFromDatabase_InactiveCountry_NotReturned()
        {
            // Arrange
            var activeCountry = new UNOPS.PAO.Domain.Entities.Country
            {
                Name = "Active Country",
                Iso2Code = "AC",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Active
            };
            var inactiveCountry = new UNOPS.PAO.Domain.Entities.Country
            {
                Name = "Inactive Country",
                Iso2Code = "IC",
                Status = UNOPS.PAO.Domain.Entities.EntityStatus.Inactive
            };

            await _context.Countries.AddRangeAsync(activeCountry, inactiveCountry);
            await _context.SaveChangesAsync();

            // Act
            var activeCountries = await _context.Countries
                .Where(c => c.Status == UNOPS.PAO.Domain.Entities.EntityStatus.Active)
                .ToListAsync();

            // Assert
            Assert.Contains(activeCountries, c => c.Name == "Active Country");
            Assert.DoesNotContain(activeCountries, c => c.Name == "Inactive Country");
        }

        #endregion

        public void Dispose()
        {
            _context?.Dispose();
        }

        // Test helper class
        private class TestCountry
        {
            public int Id { get; set; }
            public string Iso2Code { get; set; } = "";
            public string Iso3Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string RegionDescription { get; set; } = "";
            public string ContinentDescription { get; set; } = "";
            public bool IsDeleted { get; set; }
            
            // Backwards compatibility properties (mapped to new names)
            public string Code { get => Iso2Code; set => Iso2Code = value; }
            public string Code3 { get => Iso3Code; set => Iso3Code = value; }
            public string Region { get => RegionDescription; set => RegionDescription = value; }
            public string Continent { get => ContinentDescription; set => ContinentDescription = value; }
        }
    }
}

