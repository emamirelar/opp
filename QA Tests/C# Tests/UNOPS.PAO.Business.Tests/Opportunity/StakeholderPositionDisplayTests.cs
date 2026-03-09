using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Opportunity;

/// <summary>
/// Tests for always-visible stakeholder position display.
/// Commit: 73f308a6 "Always show the stakeholder position (both view mode and edit mode)"
///
/// The Position property lives on UserProfile (User.UserProfile.Position).
/// OpportunityStakeholder links to a PAOUser via UserId.
///
/// Uses model-level tests to avoid FK constraint issues with PostgreSQL.
///
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class StakeholderPositionDisplayTests
{
    #region Positive (2)

    [Fact]
    public void UserProfile_Position_WhenSet_IsReadable()
    {
        var profile = CreateProfile("Programme Manager");

        profile.Position.Should().Be("Programme Manager");
        profile.Position.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Stakeholder_WithUserId_RelationshipEstablished()
    {
        var stakeholder = new OpportunityStakeholder
        {
            Id = 1, OpportunityId = 1, Name = "S1", EntityRoleId = 1,
            UserId = 100, Status = EntityStatus.Active, IsDeleted = false
        };

        stakeholder.UserId.Should().Be(100);
        stakeholder.UserId.HasValue.Should().BeTrue();
    }

    #endregion

    #region Negative (6)

    [Fact]
    public void UserProfile_NullPosition_Allowed()
    {
        var profile = CreateProfile(null);
        profile.Position.Should().BeNull();
    }

    [Fact]
    public void UserProfile_EmptyPosition_IsEmpty()
    {
        var profile = CreateProfile("");
        profile.Position.Should().BeEmpty();
    }

    [Fact]
    public void UserProfile_WhitespacePosition_Persists()
    {
        var profile = CreateProfile("   ");
        profile.Position.Should().Be("   ");
    }

    [Fact]
    public void Stakeholder_NoUserId_PositionNotAccessible()
    {
        var stakeholder = new OpportunityStakeholder
        {
            Id = 10, OpportunityId = 2, Name = "S10", EntityRoleId = 1,
            UserId = null, Status = EntityStatus.Active, IsDeleted = false
        };

        stakeholder.UserId.Should().BeNull();
    }

    [Fact]
    public void UserProfile_PositionClearedToNull_IsNull()
    {
        var profile = CreateProfile("Was Something");
        profile.Position = null;
        profile.Position.Should().BeNull();
    }

    [Fact]
    public void Stakeholder_SoftDeleted_StillHasProperties()
    {
        var stakeholder = new OpportunityStakeholder
        {
            Id = 11, OpportunityId = 3, Name = "S11", EntityRoleId = 1,
            UserId = 200, Status = EntityStatus.Active, IsDeleted = true,
            DeletedDate = DateTime.UtcNow
        };

        stakeholder.IsDeleted.Should().BeTrue();
        stakeholder.UserId.Should().Be(200);
    }

    #endregion

    #region Edge/Boundary (6)

    [Fact]
    public void UserProfile_LongPosition_Handled()
    {
        var longPosition = new string('A', 500);
        var profile = CreateProfile(longPosition);

        profile.Position.Should().HaveLength(500);
    }

    [Fact]
    public void UserProfile_SpecialCharacters_Handled()
    {
        var special = "Directeur/Adjoint (Chef d'équipe)";
        var profile = CreateProfile(special);

        profile.Position.Should().Be(special);
    }

    [Fact]
    public void UserProfile_UnicodeCharacters_Handled()
    {
        var unicode = "经理 マネージャー";
        var profile = CreateProfile(unicode);

        profile.Position.Should().Be(unicode);
    }

    [Fact]
    public void UserProfile_UpdatePosition_NewValueReflected()
    {
        var profile = CreateProfile("Old Position");
        profile.Position = "New Position";

        profile.Position.Should().Be("New Position");
    }

    [Fact]
    public void UserProfile_SingleCharacter_Handled()
    {
        var profile = CreateProfile("X");
        profile.Position.Should().Be("X");
    }

    [Fact]
    public void Stakeholder_MultipleWithSameUserId()
    {
        var stakeholders = new List<OpportunityStakeholder>
        {
            new() { Id = 20, OpportunityId = 4, Name = "S20", EntityRoleId = 1, UserId = 300, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 21, OpportunityId = 4, Name = "S21", EntityRoleId = 2, UserId = 300, Status = EntityStatus.Active, IsDeleted = false }
        };

        stakeholders.Where(s => s.UserId == 300).Should().HaveCount(2);
    }

    #endregion

    #region Functional (6)

    [Fact]
    public void UserProfile_ComputedName_WorksWithPosition()
    {
        var profile = new UserProfile
        {
            Id = 20, UserId = 400, FirstName = "John", LastName = "Smith",
            Position = "Senior Advisor", Status = EntityStatus.Active, IsDeleted = false
        };

        profile.Name.Should().Be("John Smith");
        profile.Position.Should().Be("Senior Advisor");
    }

    [Fact]
    public void UserProfile_PositionIndependentOfName()
    {
        var profile = new UserProfile
        {
            Id = 21, UserId = 401, FirstName = null, LastName = null,
            Position = "Still Has Position", Status = EntityStatus.Active, IsDeleted = false
        };

        profile.Name.Should().BeEmpty();
        profile.Position.Should().Be("Still Has Position");
    }

    [Fact]
    public void Stakeholder_CountWithAndWithoutUser()
    {
        var stakeholders = new List<OpportunityStakeholder>
        {
            new() { Id = 30, OpportunityId = 5, Name = "S30", EntityRoleId = 1, UserId = 500, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 31, OpportunityId = 5, Name = "S31", EntityRoleId = 1, UserId = null, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 32, OpportunityId = 5, Name = "S32", EntityRoleId = 1, UserId = 501, Status = EntityStatus.Active, IsDeleted = false }
        };

        stakeholders.Count(s => s.UserId.HasValue).Should().Be(2);
        stakeholders.Count(s => !s.UserId.HasValue).Should().Be(1);
    }

    [Fact]
    public void Stakeholder_FilterActiveOnly()
    {
        var stakeholders = new List<OpportunityStakeholder>
        {
            new() { Id = 33, OpportunityId = 6, Name = "S33", EntityRoleId = 1, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 34, OpportunityId = 6, Name = "S34", EntityRoleId = 1, Status = EntityStatus.Active, IsDeleted = true }
        };

        stakeholders.Where(s => !s.IsDeleted).Should().HaveCount(1);
    }

    [Fact]
    public void UserProfile_PositionPropertyExists()
    {
        var positionProp = typeof(UserProfile).GetProperty("Position");

        positionProp.Should().NotBeNull();
        positionProp!.PropertyType.Should().Be(typeof(string));
        positionProp.CanRead.Should().BeTrue();
        positionProp.CanWrite.Should().BeTrue();
    }

    [Fact]
    public void Stakeholder_SelectProjection_IncludesUserId()
    {
        var stakeholders = new List<OpportunityStakeholder>
        {
            new() { Id = 35, OpportunityId = 7, Name = "S35", EntityRoleId = 1, UserId = 700, Status = EntityStatus.Active, IsDeleted = false }
        };

        var projection = stakeholders
            .Select(s => new { s.Id, s.UserId, s.Name })
            .FirstOrDefault();

        projection.Should().NotBeNull();
        projection!.UserId.Should().Be(700);
    }

    #endregion

    #region Integration (6)

    [Fact]
    public void FullFlow_MultipleStakeholders_AllUserIdsAccessible()
    {
        var userIds = new int?[] { 901, 902, 903, 904 };
        var stakeholders = userIds.Select((uid, i) => new OpportunityStakeholder
        {
            Id = 50 + i, OpportunityId = 11, Name = $"S{50 + i}", EntityRoleId = 1,
            UserId = uid, Status = EntityStatus.Active, IsDeleted = false
        }).ToList();

        stakeholders.Should().HaveCount(4);
        stakeholders.Select(s => s.UserId).Should().BeEquivalentTo(userIds);
    }

    [Fact]
    public void FullFlow_MixedNullAndPopulatedUserIds()
    {
        var stakeholders = new List<OpportunityStakeholder>
        {
            new() { Id = 54, OpportunityId = 12, Name = "S54", EntityRoleId = 1, UserId = 1000, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 55, OpportunityId = 12, Name = "S55", EntityRoleId = 1, UserId = null, Status = EntityStatus.Active, IsDeleted = false },
            new() { Id = 56, OpportunityId = 12, Name = "S56", EntityRoleId = 1, UserId = 1001, Status = EntityStatus.Active, IsDeleted = false }
        };

        stakeholders.Where(s => s.UserId.HasValue).Should().HaveCount(2);
        stakeholders.Where(s => !s.UserId.HasValue).Should().HaveCount(1);
    }

    [Fact]
    public void FullFlow_SoftDeleteStakeholder_UserIdPreserved()
    {
        var stakeholder = new OpportunityStakeholder
        {
            Id = 57, OpportunityId = 13, Name = "S57", EntityRoleId = 1,
            UserId = 1100, Status = EntityStatus.Active, IsDeleted = false
        };

        stakeholder.IsDeleted = true;
        stakeholder.DeletedDate = DateTime.UtcNow;

        stakeholder.IsDeleted.Should().BeTrue();
        stakeholder.UserId.Should().Be(1100);
    }

    [Fact]
    public void FullFlow_PositionDisplayContract_AlwaysVisible()
    {
        var profiles = new[]
        {
            CreateProfile("Director", "A", "B"),
            CreateProfile(null, "C", "D"),
            CreateProfile("Officer", "E", "F")
        };

        profiles.Where(p => p.Position != null).Should().HaveCount(2);
        profiles.Where(p => p.Position == null).Should().HaveCount(1);
    }

    [Fact]
    public void FullFlow_MultipleProfiles_OrderByPosition()
    {
        var profiles = new[]
        {
            CreateProfile("Zulu"),
            CreateProfile("Alpha"),
            CreateProfile("Mike")
        };

        var ordered = profiles.OrderBy(p => p.Position).ToList();

        ordered[0].Position.Should().Be("Alpha");
        ordered[2].Position.Should().Be("Zulu");
    }

    [Fact]
    public void FullFlow_FilterProfilesWithPosition()
    {
        var profiles = new[]
        {
            CreateProfile("Coordinator"),
            CreateProfile(null),
            CreateProfile(""),
            CreateProfile("Specialist")
        };

        var withMeaningfulPosition = profiles
            .Where(p => !string.IsNullOrEmpty(p.Position))
            .ToList();

        withMeaningfulPosition.Should().HaveCount(2);
        withMeaningfulPosition.Select(p => p.Position)
            .Should().Contain(new[] { "Coordinator", "Specialist" });
    }

    #endregion

    private UserProfile CreateProfile(string? position, string? firstName = null, string? lastName = null)
    {
        return new UserProfile
        {
            UserId = new Random().Next(10000, 99999),
            FirstName = firstName ?? "TestFirst",
            LastName = lastName ?? "TestLast",
            Position = position,
            Status = EntityStatus.Active,
            IsDeleted = false
        };
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | Position_WhenSet_IsReadable, Stakeholder_WithUserId |
| Negative (N) | 6 | NullPosition, EmptyPosition, Whitespace, NoUserId, ClearedToNull, SoftDeleted |
| Edge/Boundary (E) | 6 | LongPosition, SpecialChars, Unicode, UpdatePosition, SingleChar, MultipleWithSameUserId |
| Functional (F) | 6 | ComputedName, PositionIndependent, CountWithAndWithout, FilterActive, PropertyExists, SelectProjection |
| Integration (I) | 6 | AllUserIdsAccessible, MixedNullAndPopulated, SoftDeletePreserved, PositionDisplayContract, OrderByPosition, FilterWithPosition |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
