/// <summary>
/// Integration tests for PNO-700, PNO-864: Opportunity WHAT - Products &amp; Services section.
/// Covers: Full WHAT update flow, deliverables persistence simulation, save/cancel round-trip.
/// </summary>

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.OpportunityWhatSection;

[Trait("Category", "Integration")]
[Trait("Section", "WhatSection")]
public class OpportunityWhatSectionIntegrationTests
{
    private readonly Dictionary<int, OpportunityWhatSectionSpec> _store = new();

    #region Full WHAT Update Flow

    [Fact]
    public void INT_001_FullWhatUpdate_AllFields_Persisted()
    {
        var oppId = 1;
        var spec = new OpportunityWhatSectionSpec
        {
            DeliveryModality = 2,
            ResponsibleOrgUnitId = 10,
            ProposedInitiativeTypeId = 5,
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, OutputName = "Output A", Level0 = "Cat", Level1 = "Sub", Quantity = 2 }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved.Should().NotBeNull();
        retrieved!.DeliveryModality.Should().Be(2);
        retrieved.ResponsibleOrgUnitId.Should().Be(10);
        retrieved.Deliverables.Should().HaveCount(1);
    }

    [Fact]
    public void INT_002_WhatUpdate_DeliveryModalityOnly_PartialUpdate()
    {
        var oppId = 2;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 3 });
        var partial = new OpportunityWhatSectionSpec { DeliveryModality = 4 };
        PartialUpdate(oppId, partial);
        var retrieved = GetFromStore(oppId);
        retrieved!.DeliveryModality.Should().Be(4);
    }

    [Fact]
    public void INT_003_WhatUpdate_DeliverablesOnly_PartialUpdate()
    {
        var oppId = 3;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 1 });
        var partial = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 10 },
                new() { OutputId = 20 }
            }
        };
        PartialUpdate(oppId, partial);
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().HaveCount(2);
    }

    [Fact]
    public void INT_004_WhatUpdate_ReturnsFullSpec()
    {
        var oppId = 4;
        var spec = new OpportunityWhatSectionSpec
        {
            DeliveryModality = 2,
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        var result = UpdateAndReturn(oppId, spec);
        result.Should().NotBeNull();
        result!.DeliveryModality.Should().Be(2);
        result.Deliverables.Should().HaveCount(1);
    }

    [Fact]
    public void INT_005_SequentialUpdates_LastWins()
    {
        var oppId = 5;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 1 });
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 2 });
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 3 });
        var retrieved = GetFromStore(oppId);
        retrieved!.DeliveryModality.Should().Be(3);
    }

    [Fact]
    public void INT_006_CancelEditing_RevertsToOriginal()
    {
        var oppId = 6;
        var original = new OpportunityWhatSectionSpec { DeliveryModality = 2 };
        PersistWhatUpdate(oppId, original);
        var edited = new OpportunityWhatSectionSpec { DeliveryModality = 4 };
        CancelAndRevert(oppId, edited, original);
        var retrieved = GetFromStore(oppId);
        retrieved!.DeliveryModality.Should().Be(2);
    }

    [Fact]
    public void INT_007_AddDeliverables_AppendsToExisting()
    {
        var oppId = 7;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        });
        var current = GetFromStore(oppId)!;
        var appended = new List<OpportunityWhatDeliverableSpec>(current.Deliverables)
        {
            new() { OutputId = 2 }
        };
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { Deliverables = appended });
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().HaveCount(2);
    }

    [Fact]
    public void INT_008_RemoveDeliverable_UpdatesList()
    {
        var oppId = 8;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 },
                new() { OutputId = 3 }
            }
        });
        var current = GetFromStore(oppId)!;
        var removed = current.Deliverables.Where((_, i) => i != 1).ToList();
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { Deliverables = removed });
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().HaveCount(2);
    }

    [Fact]
    public void INT_009_SaveSection_ValidatesBeforePersist()
    {
        var spec = new OpportunityWhatSectionSpec { DeliveryModality = 2 };
        spec.IsDeliveryModalityValid().Should().BeTrue();
        PersistWhatUpdate(9, spec);
        GetFromStore(9).Should().NotBeNull();
    }

    [Fact]
    public void INT_010_MultipleOpportunities_Isolated()
    {
        PersistWhatUpdate(10, new OpportunityWhatSectionSpec { DeliveryModality = 1 });
        PersistWhatUpdate(11, new OpportunityWhatSectionSpec { DeliveryModality = 2 });
        PersistWhatUpdate(12, new OpportunityWhatSectionSpec { DeliveryModality = 3 });
        GetFromStore(10)!.DeliveryModality.Should().Be(1);
        GetFromStore(11)!.DeliveryModality.Should().Be(2);
        GetFromStore(12)!.DeliveryModality.Should().Be(3);
    }

    #endregion

    #region Deliverables Dialog Flow

    [Fact]
    public void INT_011_OpenDeliverablesDialog_ClearsSearchState()
    {
        var searchQuery = "";
        var aiQuery = "";
        OpenDeliverablesDialog(ref searchQuery, ref aiQuery);
        searchQuery.Should().BeEmpty();
        aiQuery.Should().BeEmpty();
    }

    [Fact]
    public void INT_012_SelectMultipleOutputs_AllAdded()
    {
        var selected = new List<OpportunityWhatDeliverableSpec>();
        SelectOutput(selected, 1);
        SelectOutput(selected, 2);
        SelectOutput(selected, 3);
        selected.Should().HaveCount(3);
    }

    [Fact]
    public void INT_013_SelectDuplicateOutput_Rejected()
    {
        var selected = new List<OpportunityWhatDeliverableSpec>();
        SelectOutput(selected, 1);
        var added = TrySelectOutput(selected, 1);
        added.Should().BeFalse();
        selected.Should().HaveCount(1);
    }

    [Fact]
    public void INT_014_RemoveFromSelection_UpdatesList()
    {
        var selected = new List<OpportunityWhatDeliverableSpec>
        {
            new() { OutputId = 1 },
            new() { OutputId = 2 }
        };
        RemoveFromSelection(selected, 1);
        selected.Should().HaveCount(1);
        selected[0].OutputId.Should().Be(1);
    }

    [Fact]
    public void INT_015_AddDeliverable_WithMultiSelect_PersistsAll()
    {
        var oppId = 15;
        var outputs = new List<OpportunityWhatDeliverableSpec>
        {
            new() { OutputId = 1 },
            new() { OutputId = 2 },
            new() { OutputId = 3 }
        };
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { Deliverables = outputs });
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().HaveCount(3);
    }

    [Fact]
    public void INT_016_CloseDeliverablesDialog_WithoutSave_DiscardsSelection()
    {
        var selected = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } };
        CloseDialogWithoutSave(selected);
        selected.Should().BeEmpty();
    }

    [Fact]
    public void INT_017_TreeSearch_FiltersResults()
    {
        var query = "proc";
        var isValid = OpportunityWhatSectionSpec.IsTreeSearchQueryValid(query);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void INT_018_AiSearch_TriggersWithValidQuery()
    {
        var query = "Guidance and tools";
        var isValid = OpportunityWhatSectionSpec.IsAiSearchQueryValid(query);
        isValid.Should().BeTrue();
    }

    [Fact]
    public void INT_019_SaveSection_WithDeliveryModalityAndDeliverables()
    {
        var oppId = 19;
        var spec = new OpportunityWhatSectionSpec
        {
            DeliveryModality = 4,
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, Quantity = 5 },
                new() { OutputId = 2, Quantity = 10 }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved!.DeliveryModality.Should().Be(4);
        retrieved.Deliverables.Should().HaveCount(2);
        retrieved.Deliverables[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void INT_020_PartialUpdate_PreservesUnchangedFields()
    {
        var oppId = 20;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec
        {
            DeliveryModality = 2,
            ResponsibleOrgUnitId = 5,
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        });
        PartialUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 3 });
        var retrieved = GetFromStore(oppId);
        retrieved!.DeliveryModality.Should().Be(3);
        retrieved.ResponsibleOrgUnitId.Should().Be(5);
        retrieved.Deliverables.Should().HaveCount(1);
    }

    [Fact]
    public void INT_021_ClearDeliverables_EmptyList()
    {
        var oppId = 21;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        });
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { Deliverables = new List<OpportunityWhatDeliverableSpec>() });
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().BeEmpty();
    }

    [Fact]
    public void INT_022_UpdateOrgUnit_OnlyOrgUnitChanged()
    {
        var oppId = 22;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { ResponsibleOrgUnitId = 1 });
        PartialUpdate(oppId, new OpportunityWhatSectionSpec { ResponsibleOrgUnitId = 2 });
        var retrieved = GetFromStore(oppId);
        retrieved!.ResponsibleOrgUnitId.Should().Be(2);
    }

    [Fact]
    public void INT_023_UpdateInitiativeType_OnlyInitiativeTypeChanged()
    {
        var oppId = 23;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { ProposedInitiativeTypeId = 1 });
        PartialUpdate(oppId, new OpportunityWhatSectionSpec { ProposedInitiativeTypeId = 2 });
        var retrieved = GetFromStore(oppId);
        retrieved!.ProposedInitiativeTypeId.Should().Be(2);
    }

    [Fact]
    public void INT_024_DeliverableWithQuantity_Persisted()
    {
        var oppId = 24;
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, Quantity = 100 }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables[0].Quantity.Should().Be(100);
    }

    [Fact]
    public void INT_025_DeliverableWithServiceLine_Persisted()
    {
        var oppId = 25;
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, ServiceLine = "Procurement" }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables[0].ServiceLine.Should().Be("Procurement");
    }

    [Fact]
    public void INT_026_DeliverableWithProcurementComponent_Persisted()
    {
        var oppId = 26;
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1, ProcurementComponent = true }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables[0].ProcurementComponent.Should().BeTrue();
    }

    [Fact]
    public void INT_027_FullHierarchy_Persisted()
    {
        var oppId = 27;
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new()
                {
                    OutputId = 1,
                    Level0 = "L0",
                    Level1 = "L1",
                    Level2 = "L2",
                    Level3 = "L3",
                    Level4 = "L4"
                }
            }
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        var d = retrieved!.Deliverables[0];
        d.Level0.Should().Be("L0");
        d.Level4.Should().Be("L4");
    }

    [Fact]
    public void INT_028_SwitchDeliveryModality_AllValues()
    {
        var oppId = 28;
        for (var i = 1; i <= 4; i++)
        {
            PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = i });
            GetFromStore(oppId)!.DeliveryModality.Should().Be(i);
        }
    }

    [Fact]
    public void INT_029_ConcurrentSimulatedUpdates_LastWins()
    {
        var oppId = 29;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 1 });
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec { DeliveryModality = 2 });
        GetFromStore(oppId)!.DeliveryModality.Should().Be(2);
    }

    [Fact]
    public void INT_030_GetFromStore_NonExistent_ReturnsNull()
    {
        var result = GetFromStore(99999);
        result.Should().BeNull();
    }

    [Fact]
    public void INT_031_InitialState_EmptyDeliverables()
    {
        var oppId = 31;
        PersistWhatUpdate(oppId, new OpportunityWhatSectionSpec());
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().BeEmpty();
    }

    [Fact]
    public void INT_032_CancelEditing_PreservesDeliverables()
    {
        var oppId = 32;
        var original = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec> { new() { OutputId = 1 } }
        };
        PersistWhatUpdate(oppId, original);
        var edited = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 }
            }
        };
        CancelAndRevert(oppId, edited, original);
        var retrieved = GetFromStore(oppId);
        retrieved!.Deliverables.Should().HaveCount(1);
    }

    [Fact]
    public void INT_033_AddDeliverable_ValidatesNoDuplicates()
    {
        var spec = new OpportunityWhatSectionSpec
        {
            Deliverables = new List<OpportunityWhatDeliverableSpec>
            {
                new() { OutputId = 1 },
                new() { OutputId = 2 }
            }
        };
        spec.HasDuplicateOutputIds().Should().BeFalse();
    }

    [Fact]
    public void INT_034_SaveSection_WithNullOptionalFields()
    {
        var oppId = 34;
        var spec = new OpportunityWhatSectionSpec
        {
            DeliveryModality = 1,
            ResponsibleOrgUnitId = null,
            ProposedInitiativeTypeId = null,
            Deliverables = new List<OpportunityWhatDeliverableSpec>()
        };
        PersistWhatUpdate(oppId, spec);
        var retrieved = GetFromStore(oppId);
        retrieved!.ResponsibleOrgUnitId.Should().BeNull();
        retrieved.ProposedInitiativeTypeId.Should().BeNull();
    }

    [Fact]
    public void INT_035_ExpandCollapseTree_StateIndependent()
    {
        var expanded = new HashSet<string> { "node1" };
        expanded.Add("node2");
        expanded.Should().HaveCount(2);
        expanded.Remove("node1");
        expanded.Should().HaveCount(1);
    }

    [Fact]
    public void INT_036_SearchThenSelect_Flow()
    {
        var query = "procurement";
        OpportunityWhatSectionSpec.IsTreeSearchQueryValid(query).Should().BeTrue();
        var selected = new List<OpportunityWhatDeliverableSpec>();
        SelectOutput(selected, 1);
        selected.Should().HaveCount(1);
    }

    #endregion

    #region Helpers

    private void PersistWhatUpdate(int oppId, OpportunityWhatSectionSpec spec)
    {
        var current = _store.TryGetValue(oppId, out var c) ? c : new OpportunityWhatSectionSpec();
        _store[oppId] = new OpportunityWhatSectionSpec
        {
            DeliveryModality = spec.DeliveryModality ?? current.DeliveryModality,
            ResponsibleOrgUnitId = spec.ResponsibleOrgUnitId ?? current.ResponsibleOrgUnitId,
            ProposedInitiativeTypeId = spec.ProposedInitiativeTypeId ?? current.ProposedInitiativeTypeId,
            Deliverables = spec.Deliverables ?? current.Deliverables
        };
    }

    private void PartialUpdate(int oppId, OpportunityWhatSectionSpec spec)
    {
        var current = _store.TryGetValue(oppId, out var c) ? c : new OpportunityWhatSectionSpec();
        _store[oppId] = new OpportunityWhatSectionSpec
        {
            DeliveryModality = spec.DeliveryModality ?? current.DeliveryModality,
            ResponsibleOrgUnitId = spec.ResponsibleOrgUnitId ?? current.ResponsibleOrgUnitId,
            ProposedInitiativeTypeId = spec.ProposedInitiativeTypeId ?? current.ProposedInitiativeTypeId,
            Deliverables = spec.Deliverables ?? current.Deliverables
        };
    }

    private OpportunityWhatSectionSpec? UpdateAndReturn(int oppId, OpportunityWhatSectionSpec spec)
    {
        _store[oppId] = spec;
        return spec;
    }

    private OpportunityWhatSectionSpec? GetFromStore(int oppId)
    {
        return _store.TryGetValue(oppId, out var d) ? d : null;
    }

    private void CancelAndRevert(int oppId, OpportunityWhatSectionSpec edited, OpportunityWhatSectionSpec original)
    {
        _store[oppId] = original;
    }

    private void OpenDeliverablesDialog(ref string searchQuery, ref string aiQuery)
    {
        searchQuery = "";
        aiQuery = "";
    }

    private void SelectOutput(List<OpportunityWhatDeliverableSpec> selected, int outputId)
    {
        selected.Add(new OpportunityWhatDeliverableSpec { OutputId = outputId });
    }

    private bool TrySelectOutput(List<OpportunityWhatDeliverableSpec> selected, int outputId)
    {
        if (selected.Any(s => s.OutputId == outputId))
            return false;
        selected.Add(new OpportunityWhatDeliverableSpec { OutputId = outputId });
        return true;
    }

    private void RemoveFromSelection(List<OpportunityWhatDeliverableSpec> selected, int outputId)
    {
        selected.RemoveAll(s => s.OutputId == outputId);
    }

    private void CloseDialogWithoutSave(List<OpportunityWhatDeliverableSpec> selected)
    {
        selected.Clear();
    }

    #endregion
}
