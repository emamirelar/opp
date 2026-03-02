using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Collections.Generic;
using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using Xunit;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.Validation
{
    /// <summary>
    /// Tests for Opportunity field length validation after schema changes.
    /// Migration 20260114122208 changed Name from 255 to 120 chars and Challenges from text to 1020 chars.
    /// </summary>
    public class OpportunityFieldLengthValidationTests
    {
        [Fact]
        public void CreateOpportunity_NameExactly120Characters_ShouldSucceed()
        {
            // Arrange: Name with exactly 120 characters
            var opportunity = new OpportunityEntity
            {
                Name = new string('A', 120),
                Description = "Test description",
                Status = EntityStatus.Active
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: No validation errors for Name field
            validationResults.Should().NotContain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CreateOpportunity_Name121Characters_ShouldFailValidation()
        {
            // Arrange: Name with 121 characters (exceeds limit)
            var opportunity = new OpportunityEntity
            {
                Name = new string('A', 121),
                Description = "Test description",
                Status = EntityStatus.Active
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: Should have validation error for Name field
            validationResults.Should().Contain(v => 
                v.MemberNames.Contains("Name") && 
                v.ErrorMessage.Contains("120"));
        }

        [Fact]
        public void CreateOpportunity_Name119Characters_ShouldSucceed()
        {
            // Arrange: Name with 119 characters (under limit)
            var opportunity = new OpportunityEntity
            {
                Name = new string('A', 119),
                Description = "Test description",
                Status = EntityStatus.Active
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: No validation errors for Name field
            validationResults.Should().NotContain(v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void CreateOpportunity_ChallengesExactly1020Characters_ShouldSucceed()
        {
            // Arrange: Challenges with exactly 1,020 characters
            var opportunity = new OpportunityEntity
            {
                Name = "Test Opportunity",
                Description = "Test description",
                Status = EntityStatus.Active,
                Challenges = new string('B', 1020)
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: No validation errors for Challenges field
            validationResults.Should().NotContain(v => v.MemberNames.Contains("Challenges"));
        }

        [Fact]
        public void CreateOpportunity_Challenges1021Characters_ShouldFailValidation()
        {
            // Arrange: Challenges with 1,021 characters (exceeds limit)
            var opportunity = new OpportunityEntity
            {
                Name = "Test Opportunity",
                Description = "Test description",
                Status = EntityStatus.Active,
                Challenges = new string('B', 1021)
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: Should have validation error for Challenges field
            validationResults.Should().Contain(v => 
                v.MemberNames.Contains("Challenges") && 
                v.ErrorMessage.Contains("1020"));
        }

        [Fact]
        public void CreateOpportunity_Challenges1019Characters_ShouldSucceed()
        {
            // Arrange: Challenges with 1,019 characters (under limit)
            var opportunity = new OpportunityEntity
            {
                Name = "Test Opportunity",
                Description = "Test description",
                Status = EntityStatus.Active,
                Challenges = new string('B', 1019)
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);

            // Assert: No validation errors for Challenges field
            validationResults.Should().NotContain(v => v.MemberNames.Contains("Challenges"));
        }

        [Theory]
        [InlineData(1, true)]    // Minimum length
        [InlineData(60, true)]   // Mid-range
        [InlineData(119, true)]  // Just under limit
        [InlineData(120, true)]  // At limit
        [InlineData(121, false)] // Over limit
        [InlineData(150, false)] // Well over limit
        [InlineData(255, false)] // Old limit (should now fail)
        public void OpportunityNameValidation_VariousLengths_ValidatesCorrectly(
            int nameLength, bool shouldSucceed)
        {
            // Arrange: Create opportunity with specified name length
            var opportunity = new OpportunityEntity
            {
                Name = new string('X', nameLength),
                Description = "Test description",
                Status = EntityStatus.Active
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);
            var hasNameError = validationResults.Any(v => v.MemberNames.Contains("Name"));

            // Assert: Validation result should match expectation
            if (shouldSucceed)
            {
                hasNameError.Should().BeFalse($"Name with {nameLength} characters should be valid");
            }
            else
            {
                hasNameError.Should().BeTrue($"Name with {nameLength} characters should be invalid (max 120)");
            }
        }

        [Theory]
        [InlineData(1, true)]     // Minimum length
        [InlineData(500, true)]   // Mid-range
        [InlineData(1019, true)]  // Just under limit
        [InlineData(1020, true)]  // At limit
        [InlineData(1021, false)] // Over limit
        [InlineData(2000, false)] // Well over limit
        public void OpportunityChallengesValidation_VariousLengths_ValidatesCorrectly(
            int challengesLength, bool shouldSucceed)
        {
            // Arrange: Create opportunity with specified challenges length
            var opportunity = new OpportunityEntity
            {
                Name = "Test Opportunity",
                Description = "Test description",
                Status = EntityStatus.Active,
                Challenges = new string('Y', challengesLength)
            };

            // Act: Validate using data annotations
            var validationResults = ValidateModel(opportunity);
            var hasChallengesError = validationResults.Any(v => v.MemberNames.Contains("Challenges"));

            // Assert: Validation result should match expectation
            if (shouldSucceed)
            {
                hasChallengesError.Should().BeFalse($"Challenges with {challengesLength} characters should be valid");
            }
            else
            {
                hasChallengesError.Should().BeTrue($"Challenges with {challengesLength} characters should be invalid (max 1020)");
            }
        }

        [Fact]
        public void OpportunityName_AtMaxLength_HasCorrectMaxLengthAttribute()
        {
            // Arrange: Get Name property info
            var nameProperty = typeof(OpportunityEntity).GetProperty("Name");
            
            // Act: Get MaxLength attribute
            var maxLengthAttr = nameProperty?.GetCustomAttributes(typeof(MaxLengthAttribute), true)
                .FirstOrDefault() as MaxLengthAttribute;

            // Assert: MaxLength should be 120
            maxLengthAttr.Should().NotBeNull("Name property should have MaxLength attribute");
            maxLengthAttr!.Length.Should().Be(120, "Name MaxLength should be 120 characters");
        }

        [Fact]
        public void OpportunityChallenges_AtMaxLength_HasCorrectMaxLengthAttribute()
        {
            // Arrange: Get Challenges property info
            var challengesProperty = typeof(OpportunityEntity).GetProperty("Challenges");
            
            // Act: Get MaxLength attribute
            var maxLengthAttr = challengesProperty?.GetCustomAttributes(typeof(MaxLengthAttribute), true)
                .FirstOrDefault() as MaxLengthAttribute;

            // Assert: MaxLength should be 1020
            maxLengthAttr.Should().NotBeNull("Challenges property should have MaxLength attribute");
            maxLengthAttr!.Length.Should().Be(1020, "Challenges MaxLength should be 1020 characters");
        }

        /// <summary>
        /// Helper method to validate a model using data annotations
        /// </summary>
        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }
    }
}
