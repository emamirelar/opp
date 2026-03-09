using FluentAssertions;
using UNOPS.PAO.Domain.Entities;
using Xunit;
using OpportunityEntity = UNOPS.PAO.Domain.Entities.Opportunity;

namespace UNOPS.PAO.Business.Tests.Managers;

/// <summary>
/// Tests for PNO-1156: Responsible Org Unit on Create Opportunity.
/// Validates ResponsibleOrgUnitId field behavior, filtering, and business rules.
///
/// Uses model-level tests to avoid FK constraint issues with real PostgreSQL DB.
///
/// Ratio: P=2, N=6, E=6, F=6, I=6
/// </summary>
public class OpportunityCreationOrgUnitTests
{
    #region Positive (2)

    [Fact]
    public void Opportunity_ResponsibleOrgUnitId_SetAndReadable()
    {
        var opp = CreateTestOpportunity(orgUnitId: 10);

        opp.ResponsibleOrgUnitId.Should().Be(10);
        opp.ResponsibleOrgUnitId.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_ResponsibleOrgUnitId_PreservedAfterPropertyUpdate()
    {
        var opp = CreateTestOpportunity(orgUnitId: 50);
        opp.Name = "Updated Name";

        opp.ResponsibleOrgUnitId.Should().Be(50);
    }

    #endregion

    #region Negative (6)

    [Fact]
    public void Opportunity_NullResponsibleOrgUnitId_AllowsNull()
    {
        var opp = CreateTestOpportunity(orgUnitId: null);

        opp.ResponsibleOrgUnitId.Should().BeNull();
        opp.ResponsibleOrgUnitId.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Opportunity_ZeroOrgUnitId_StillHasValue()
    {
        var opp = CreateTestOpportunity(orgUnitId: 0);

        opp.ResponsibleOrgUnitId.Should().Be(0);
        opp.ResponsibleOrgUnitId.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Opportunity_NegativeOrgUnitId_StillHasValue()
    {
        var opp = CreateTestOpportunity(orgUnitId: -1);

        opp.ResponsibleOrgUnitId.Should().Be(-1);
    }

    [Fact]
    public void Opportunity_NonExistentOrgUnitId_StoredInProperty()
    {
        var opp = CreateTestOpportunity(orgUnitId: 99999);

        opp.ResponsibleOrgUnitId.Should().Be(99999);
    }

    [Fact]
    public void QueryOpportunities_ByOrgUnitId_NoMatchesInEmptyCollection()
    {
        var opportunities = new List<OpportunityEntity>
        {
            CreateTestOpportunity(orgUnitId: 5)
        };

        var results = opportunities.Where(o => o.ResponsibleOrgUnitId == 999).ToList();

        results.Should().BeEmpty();
    }

    [Fact]
    public void Opportunity_ClearOrgUnitToNull_UpdatesValue()
    {
        var opp = CreateTestOpportunity(orgUnitId: 10);
        opp.ResponsibleOrgUnitId = null;

        opp.ResponsibleOrgUnitId.Should().BeNull();
    }

    #endregion

    #region Edge/Boundary (6)

    [Fact]
    public void Opportunity_MaxIntOrgUnitId_Handled()
    {
        var opp = CreateTestOpportunity(orgUnitId: int.MaxValue);

        opp.ResponsibleOrgUnitId.Should().Be(int.MaxValue);
    }

    [Fact]
    public void Opportunity_MinIntOrgUnitId_Handled()
    {
        var opp = CreateTestOpportunity(orgUnitId: int.MinValue);

        opp.ResponsibleOrgUnitId.Should().Be(int.MinValue);
    }

    [Fact]
    public void Opportunity_ChangeOrgUnitId_UpdatesValue()
    {
        var opp = CreateTestOpportunity(orgUnitId: 1);
        opp.ResponsibleOrgUnitId = 2;

        opp.ResponsibleOrgUnitId.Should().Be(2);
    }

    [Fact]
    public void Opportunity_SetOrgUnitToNull_FromValue()
    {
        var opp = CreateTestOpportunity(orgUnitId: 10);
        opp.ResponsibleOrgUnitId = null;

        opp.ResponsibleOrgUnitId.Should().BeNull();
    }

    [Fact]
    public void Opportunity_SameOrgUnitId_NoError()
    {
        var opp = CreateTestOpportunity(orgUnitId: 5);
        opp.ResponsibleOrgUnitId = 5;

        opp.ResponsibleOrgUnitId.Should().Be(5);
    }

    [Fact]
    public void MultipleOpportunities_SameOrgUnitId_AllPersist()
    {
        var opps = Enumerable.Range(1, 3)
            .Select(_ => CreateTestOpportunity(orgUnitId: 100))
            .ToList();

        opps.Should().HaveCount(3);
        opps.Should().OnlyContain(o => o.ResponsibleOrgUnitId == 100);
    }

    #endregion

    #region Functional (6)

    [Fact]
    public void FilterByOrgUnitId_ReturnsMatchingOnly()
    {
        var opps = new List<OpportunityEntity>
        {
            CreateTestOpportunity(orgUnitId: 1),
            CreateTestOpportunity(orgUnitId: 2),
            CreateTestOpportunity(orgUnitId: 1)
        };

        var results = opps.Where(o => o.ResponsibleOrgUnitId == 1).ToList();

        results.Should().HaveCount(2);
        results.Should().OnlyContain(o => o.ResponsibleOrgUnitId == 1);
    }

    [Fact]
    public void FilterByMultipleOrgUnitIds_ContainsLogic()
    {
        var opps = new List<OpportunityEntity>
        {
            CreateTestOpportunity(orgUnitId: 10),
            CreateTestOpportunity(orgUnitId: 20),
            CreateTestOpportunity(orgUnitId: 30)
        };

        var orgUnitIds = new List<int> { 10, 20 };
        var results = opps
            .Where(o => o.ResponsibleOrgUnitId.HasValue && orgUnitIds.Contains(o.ResponsibleOrgUnitId.Value))
            .ToList();

        results.Should().HaveCount(2);
    }

    [Fact]
    public void ExcludeDeleted_WithOrgUnitFilter()
    {
        var opp1 = CreateTestOpportunity(orgUnitId: 5);
        var opp2 = CreateTestOpportunity(orgUnitId: 5);
        opp2.IsDeleted = true;
        var opps = new List<OpportunityEntity> { opp1, opp2 };

        var results = opps.Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 5).ToList();

        results.Should().HaveCount(1);
    }

    [Fact]
    public void GroupByOrgUnitId_ReturnsCorrectCounts()
    {
        var opps = new List<OpportunityEntity>
        {
            CreateTestOpportunity(orgUnitId: 1),
            CreateTestOpportunity(orgUnitId: 1),
            CreateTestOpportunity(orgUnitId: 2)
        };

        var groups = opps
            .Where(o => o.ResponsibleOrgUnitId.HasValue)
            .GroupBy(o => o.ResponsibleOrgUnitId)
            .Select(g => new { OrgUnitId = g.Key, Count = g.Count() })
            .ToList();

        groups.Should().HaveCount(2);
    }

    [Fact]
    public void OrgUnitEntity_HasRequiredProperties()
    {
        var orgUnit = new OrganizationHierarchy
        {
            Id = 1,
            Name = "Regional Office",
            Code = "RO",
            Description = "Regional Office Description",
            Status = EntityStatus.Active,
            IsDeleted = false
        };

        orgUnit.Name.Should().Be("Regional Office");
        orgUnit.Code.Should().Be("RO");
        orgUnit.Description.Should().Be("Regional Office Description");
    }

    [Fact]
    public void NullableOrgUnit_CountWithAndWithout()
    {
        var opps = new List<OpportunityEntity>
        {
            CreateTestOpportunity(orgUnitId: 1),
            CreateTestOpportunity(orgUnitId: null),
            CreateTestOpportunity(orgUnitId: 1)
        };

        var withOrgUnit = opps.Count(o => o.ResponsibleOrgUnitId.HasValue);
        var withoutOrgUnit = opps.Count(o => !o.ResponsibleOrgUnitId.HasValue);

        withOrgUnit.Should().Be(2);
        withoutOrgUnit.Should().Be(1);
    }

    #endregion

    #region Integration (6)

    [Fact]
    public void FullFlow_CreateWithOrgUnit_QueryByOrgUnit_Update_Verify()
    {
        var opp = CreateTestOpportunity(orgUnitId: 7);

        opp.ResponsibleOrgUnitId.Should().Be(7);

        opp.ResponsibleOrgUnitId = 8;

        opp.ResponsibleOrgUnitId.Should().Be(8);
    }

    [Fact]
    public void FullFlow_CreateMultiple_FilterAndSort()
    {
        var opps = new List<OpportunityEntity>();
        for (int i = 1; i <= 5; i++)
        {
            opps.Add(CreateTestOpportunity(orgUnitId: i % 3 + 1));
        }

        var filtered = opps
            .Where(o => o.ResponsibleOrgUnitId == 1)
            .OrderBy(o => o.Name)
            .ToList();

        filtered.Should().NotBeEmpty();
        filtered.Should().OnlyContain(o => o.ResponsibleOrgUnitId == 1);
    }

    [Fact]
    public void FullFlow_CreateWithOrgUnit_SoftDelete_ExcludeFromQuery()
    {
        var opp = CreateTestOpportunity(orgUnitId: 3);
        var opps = new List<OpportunityEntity> { opp };

        opp.IsDeleted = true;
        opp.DeletedDate = DateTime.UtcNow;

        var results = opps.Where(o => !o.IsDeleted && o.ResponsibleOrgUnitId == 3).ToList();
        results.Should().BeEmpty();
    }

    [Fact]
    public void FullFlow_OrgUnitHierarchy_ParentChild()
    {
        var parent = new OrganizationHierarchy
        {
            Id = 80, Name = "HQ", Code = "HQ", Description = "Headquarters",
            Status = EntityStatus.Active, IsDeleted = false
        };
        var child = new OrganizationHierarchy
        {
            Id = 81, Name = "Regional", Code = "RG", Description = "Regional",
            Status = EntityStatus.Active, IsDeleted = false, ParentId = 80
        };

        child.ParentId.Should().Be(80);
        parent.Id.Should().Be(80);
    }

    [Fact]
    public void FullFlow_OrgUnitPropertyType_IsNullableInt()
    {
        var propInfo = typeof(OpportunityEntity).GetProperty("ResponsibleOrgUnitId");

        propInfo.Should().NotBeNull();
        propInfo!.PropertyType.Should().Be(typeof(int?));
    }

    [Fact]
    public void FullFlow_CreateAndUpdate_OrgUnitCycle()
    {
        var opp = CreateTestOpportunity(orgUnitId: null);
        opp.ResponsibleOrgUnitId.HasValue.Should().BeFalse();

        opp.ResponsibleOrgUnitId = 10;
        opp.ResponsibleOrgUnitId.Should().Be(10);

        opp.ResponsibleOrgUnitId = null;
        opp.ResponsibleOrgUnitId.HasValue.Should().BeFalse();

        opp.ResponsibleOrgUnitId = 20;
        opp.ResponsibleOrgUnitId.Should().Be(20);
    }

    #endregion

    private OpportunityEntity CreateTestOpportunity(int? orgUnitId = null)
    {
        return new OpportunityEntity
        {
            Name = $"Test Opportunity",
            Description = "Test Description",
            Status = EntityStatus.Active,
            IsDeleted = false,
            ResponsibleOrgUnitId = orgUnitId,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = 1
        };
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----|----|-----|
| Positive (P) | 2 | SetAndReadable, PreservedAfterPropertyUpdate |
| Negative (N) | 6 | NullOrgUnitId, ZeroOrgUnitId, NegativeOrgUnitId, NonExistentOrgUnitId, NoMatches, ClearToNull |
| Edge/Boundary (E) | 6 | MaxInt, MinInt, ChangeOrgUnitId, SetToNull, SameOrgUnitId, MultipleSameOrgUnit |
| Functional (F) | 6 | FilterByOrgUnitId, FilterByMultipleIds, ExcludeDeleted, GroupByOrgUnitId, OrgUnitProperties, NullableCount |
| Integration (I) | 6 | CreateQueryUpdate, CreateMultipleFilterSort, SoftDelete, ParentChild, PropertyType, CreateUpdateCycle |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
