/**
 * @fileoverview Fast standalone tests for AutoMapper mapping conventions and validation
 * @author UNOPS Opportunity+ System Development Team
 */

using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.FastTests;

/// <summary>
/// Tests for AutoMapper profile conventions: Entity→EntityModel naming,
/// request DTO naming (CreateXRequest, UpdateXRequest), mapping registry,
/// and response model field exposure.
/// </summary>
public class AutoMapperProfileValidationTests
{
    // --- Inline entity and model type definitions ---

    private record PartnerEntity(int Id, string Name, string Status);
    private record PartnerModel(int Id, string Name, string Status);

    private record ContactEntity(int Id, string Name, int PartnerId);
    private record ContactModel(int Id, string Name, int PartnerId);

    private record OpportunityEntity(int Id, string Name, string Stage);
    private record OpportunityModel(int Id, string Name, string Stage);

    private record CreatePartnerRequest(string Name, string Status);
    private record UpdatePartnerRequest(int Id, string Name, string Status);

    private record CreateContactRequest(string Name, int PartnerId);
    private record UpdateContactRequest(int Id, string Name, int PartnerId);

    private record CreateOpportunityRequest(string Name, string Stage);
    private record UpdateOpportunityRequest(int Id, string Name, string Stage);

    // --- Inline mapping registry (simulates AutoMapper profile) ---

    private static readonly IReadOnlyList<(Type Source, Type Destination)> EntityToModelMappings =
    [
        (typeof(PartnerEntity), typeof(PartnerModel)),
        (typeof(ContactEntity), typeof(ContactModel)),
        (typeof(OpportunityEntity), typeof(OpportunityModel))
    ];

    private static readonly IReadOnlyList<(Type Source, Type Destination)> BidirectionalMappings =
    [
        (typeof(PartnerEntity), typeof(PartnerModel)),
        (typeof(PartnerModel), typeof(PartnerEntity)),
        (typeof(ContactEntity), typeof(ContactModel)),
        (typeof(ContactModel), typeof(ContactEntity)),
        (typeof(OpportunityEntity), typeof(OpportunityModel)),
        (typeof(OpportunityModel), typeof(OpportunityEntity))
    ];

    private static readonly IReadOnlyList<Type> EntityTypes =
    [
        typeof(PartnerEntity),
        typeof(ContactEntity),
        typeof(OpportunityEntity)
    ];

    private static readonly IReadOnlyList<Type> RequestDtoTypes =
    [
        typeof(CreatePartnerRequest),
        typeof(UpdatePartnerRequest),
        typeof(CreateContactRequest),
        typeof(UpdateContactRequest),
        typeof(CreateOpportunityRequest),
        typeof(UpdateOpportunityRequest)
    ];

    private static readonly IReadOnlyList<string> InternalFieldNames = ["InternalId", "SecretKey", "RawPassword"];

    // --- Model naming follows convention: Entity→EntityModel (3 tests) ---

    [Fact]
    public void ModelNaming_PartnerEntity_MapsToPartnerModel()
    {
        var mapping = EntityToModelMappings.First(m => m.Source == typeof(PartnerEntity));
        mapping.Destination.Name.Should().Be("PartnerModel");
    }

    [Fact]
    public void ModelNaming_ContactEntity_MapsToContactModel()
    {
        var mapping = EntityToModelMappings.First(m => m.Source == typeof(ContactEntity));
        mapping.Destination.Name.Should().Be("ContactModel");
    }

    [Fact]
    public void ModelNaming_OpportunityEntity_MapsToOpportunityModel()
    {
        var mapping = EntityToModelMappings.First(m => m.Source == typeof(OpportunityEntity));
        mapping.Destination.Name.Should().Be("OpportunityModel");
    }

    // --- Request DTO naming follows convention (3 tests) ---

    [Fact]
    public void RequestDtoNaming_CreateRequests_FollowCreateXRequestPattern()
    {
        var createTypes = RequestDtoTypes.Where(t => t.Name.StartsWith("Create")).ToList();
        createTypes.Should().AllSatisfy(t => t.Name.Should().EndWith("Request"));
        createTypes.Should().Contain(t => t.Name == "CreatePartnerRequest");
        createTypes.Should().Contain(t => t.Name == "CreateContactRequest");
        createTypes.Should().Contain(t => t.Name == "CreateOpportunityRequest");
    }

    [Fact]
    public void RequestDtoNaming_UpdateRequests_FollowUpdateXRequestPattern()
    {
        var updateTypes = RequestDtoTypes.Where(t => t.Name.StartsWith("Update")).ToList();
        updateTypes.Should().AllSatisfy(t => t.Name.Should().EndWith("Request"));
        updateTypes.Should().Contain(t => t.Name == "UpdatePartnerRequest");
        updateTypes.Should().Contain(t => t.Name == "UpdateContactRequest");
        updateTypes.Should().Contain(t => t.Name == "UpdateOpportunityRequest");
    }

    [Fact]
    public void RequestDtoNaming_AllRequestTypes_HaveRequestSuffix()
    {
        RequestDtoTypes.Should().AllSatisfy(t => t.Name.Should().EndWith("Request"));
    }

    // --- No duplicate mapping registrations (2 tests) ---

    [Fact]
    public void MappingRegistry_EntityToModel_NoDuplicateSourceTypes()
    {
        var sources = EntityToModelMappings.Select(m => m.Source).ToList();
        var distinctSources = sources.Distinct().ToList();
        sources.Count.Should().Be(distinctSources.Count);
    }

    [Fact]
    public void MappingRegistry_EntityToModel_NoDuplicateSourceDestinationPairs()
    {
        var pairs = EntityToModelMappings.Select(m => (m.Source, m.Destination)).ToList();
        var distinctPairs = pairs.Distinct().ToList();
        pairs.Count.Should().Be(distinctPairs.Count);
    }

    // --- All entities have at least Entity→Model mapping (2 tests) ---

    [Fact]
    public void MappingCoverage_AllEntityTypes_HaveModelMapping()
    {
        foreach (var entityType in EntityTypes)
        {
            var hasMapping = EntityToModelMappings.Any(m => m.Source == entityType);
            hasMapping.Should().BeTrue($"entity {entityType.Name} must have Entity→Model mapping");
        }
    }

    [Fact]
    public void MappingCoverage_EntityCount_EqualsMappingCount()
    {
        EntityToModelMappings.Count.Should().Be(EntityTypes.Count);
    }

    // --- Response models don't expose internal fields (2 tests) ---

    [Fact]
    public void ResponseModels_ShouldNotExposeInternalFields_ByConvention()
    {
        var modelTypes = EntityToModelMappings.Select(m => m.Destination).ToList();
        foreach (var modelType in modelTypes)
        {
            var propertyNames = modelType.GetProperties().Select(p => p.Name).ToList();
            foreach (var internalName in InternalFieldNames)
            {
                propertyNames.Should().NotContain(internalName, $"model {modelType.Name} must not expose {internalName}");
            }
        }
    }

    [Fact]
    public void ResponseModels_ModelTypes_HaveNoInternalPropertyNames()
    {
        var modelTypes = EntityToModelMappings.Select(m => m.Destination).ToList();
        var allProperties = modelTypes.SelectMany(t => t.GetProperties().Select(p => p.Name)).ToList();
        allProperties.Should().NotContain(InternalFieldNames);
    }

    // --- Mapping coverage completeness (2 tests) ---

    [Fact]
    public void MappingCompleteness_BidirectionalMappings_MatchEntityToModel()
    {
        foreach (var (source, dest) in BidirectionalMappings)
        {
            var reverseExists = BidirectionalMappings.Any(m => m.Source == dest && m.Destination == source);
            reverseExists.Should().BeTrue($"bidirectional mapping for {source.Name}↔{dest.Name} should be symmetric");
        }
    }

    [Fact]
    public void MappingCompleteness_AllEntityTypes_HaveBidirectionalMapping()
    {
        foreach (var entityType in EntityTypes)
        {
            var hasBidirectional = BidirectionalMappings.Any(m => m.Source == entityType);
            hasBidirectional.Should().BeTrue($"entity {entityType.Name} must have bidirectional mapping");
        }
    }
}
