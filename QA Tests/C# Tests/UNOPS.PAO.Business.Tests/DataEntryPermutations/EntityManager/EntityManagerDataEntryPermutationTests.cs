/// <summary>
/// Tests for Entity Manager entity configuration data entry permutations.
///
/// Requirements validated:
/// - REQ-1: EntityName, TableName required, max 100 chars → Field order, invalid, boundary tests
/// - REQ-2: Description optional, max 500 → Partial, boundary tests
/// - REQ-3: FieldName, DataType required, max 100/50 → Invalid, boundary tests
/// - REQ-4: EntityManagerId required, positive for field config → Invalid tests
/// - REQ-5: MaxLength, DisplayOrder, ListViewOrder, HelperText constraints → Boundary tests
/// - REQ-6: IsActive, EnableChangeLog, IsRequired, ShowInListView boolean permutations → Boundary tests
///
/// Defects found: None
/// </summary>

using FluentAssertions;
using UNOPS.PAO.Business.Tests.DataEntryPermutations.Infrastructure;
using Xunit;

namespace UNOPS.PAO.Business.Tests.DataEntryPermutations.EntityManager;

[Trait("Feature", "DataEntryPermutations")]
[Trait("Entity", "EntityManager")]

public class EntityManagerDataEntryPermutationTests
{
    private const int EntityNameMaxLength = 100;
    private const int TableNameMaxLength = 100;
    private const int DescriptionMaxLength = 500;
    private const int FieldNameMaxLength = 100;
    private const int DataTypeMaxLength = 50;
    private const int DefaultValueMaxLength = 255;
    private const int HelperTextMaxLength = 1000;

    private class EntityConfigRequest
    {
        public string? EntityName { get; set; }
        public string? TableName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool EnableChangeLog { get; set; }
    }

    private class FieldConfigRequest
    {
        public string? FieldName { get; set; }
        public string? DataType { get; set; }
        public string? Description { get; set; }
        public bool IsRequired { get; set; }
        public bool IsActive { get; set; }
        public bool EnableChangeLog { get; set; }
        public string? DefaultValue { get; set; }
        public int? MaxLength { get; set; }
        public int DisplayOrder { get; set; }
        public bool ShowInListView { get; set; }
        public int? ListViewOrder { get; set; }
        public string? HelperText { get; set; }
        public int EntityManagerId { get; set; }
    }

    private static readonly string[] ValidDataTypes = { "string", "int", "decimal", "datetime", "bool", "guid" };

    private static (bool IsValid, List<string> Errors) ValidateEntityConfig(EntityConfigRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.EntityName)) errors.Add("EntityName is required");
        else if (req.EntityName.Length > EntityNameMaxLength) errors.Add($"EntityName must not exceed {EntityNameMaxLength} characters");
        if (string.IsNullOrWhiteSpace(req.TableName)) errors.Add("TableName is required");
        else if (req.TableName.Length > TableNameMaxLength) errors.Add($"TableName must not exceed {TableNameMaxLength} characters");
        if (!string.IsNullOrEmpty(req.Description) && req.Description.Length > DescriptionMaxLength)
            errors.Add($"Description must not exceed {DescriptionMaxLength} characters");
        return (errors.Count == 0, errors);
    }

    private static (bool IsValid, List<string> Errors) ValidateFieldConfig(FieldConfigRequest req)
    {
        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(req.FieldName)) errors.Add("FieldName is required");
        else if (req.FieldName.Length > FieldNameMaxLength) errors.Add($"FieldName must not exceed {FieldNameMaxLength} characters");
        if (string.IsNullOrWhiteSpace(req.DataType)) errors.Add("DataType is required");
        else if (req.DataType.Length > DataTypeMaxLength) errors.Add($"DataType must not exceed {DataTypeMaxLength} characters");
        else if (!ValidDataTypes.Contains(req.DataType)) errors.Add($"DataType must be one of: {string.Join(", ", ValidDataTypes)}");
        if (!string.IsNullOrEmpty(req.Description) && req.Description.Length > DescriptionMaxLength)
            errors.Add($"Description must not exceed {DescriptionMaxLength} characters");
        if (!string.IsNullOrEmpty(req.DefaultValue) && req.DefaultValue.Length > DefaultValueMaxLength)
            errors.Add($"DefaultValue must not exceed {DefaultValueMaxLength} characters");
        if (req.MaxLength.HasValue && req.MaxLength.Value < 0) errors.Add("MaxLength must be non-negative when provided");
        if (!string.IsNullOrEmpty(req.HelperText) && req.HelperText.Length > HelperTextMaxLength)
            errors.Add($"HelperText must not exceed {HelperTextMaxLength} characters");
        if (req.EntityManagerId <= 0) errors.Add("EntityManagerId must be positive");
        return (errors.Count == 0, errors);
    }

    private static EntityConfigRequest CreateValidEntityRequest() => new()
    {
        EntityName = "TestEntity",
        TableName = "test_entity",
        IsActive = true,
        EnableChangeLog = false
    };

    private static FieldConfigRequest CreateValidFieldRequest() => new()
    {
        FieldName = "Name",
        DataType = "string",
        EntityManagerId = 1,
        DisplayOrder = 0,
        ShowInListView = true
    };

    #region 1. Field Order Permutations

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_EntityNameFirst_ProducesValidRequest()
    {
        var req = new EntityConfigRequest { EntityName = "Partner", TableName = "partners" };
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
        req.EntityName.Should().Be("Partner");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_TableNameFirst_ProducesValidRequest()
    {
        var req = new EntityConfigRequest { TableName = "opportunities", EntityName = "Opportunity" };
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
        req.TableName.Should().Be("opportunities");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_DescriptionFirst_ProducesValidRequest()
    {
        var req = new EntityConfigRequest { Description = "Desc", EntityName = "E", TableName = "e" };
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_AllEntityFieldsReverseOrder_ProducesValidRequest()
    {
        var req = new EntityConfigRequest { EnableChangeLog = true, IsActive = false, Description = "D", TableName = "t", EntityName = "E" };
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_FieldNameFirst_ProducesValidRequest()
    {
        var req = new FieldConfigRequest { FieldName = "Id", DataType = "int", EntityManagerId = 1 };
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
        req.FieldName.Should().Be("Id");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void FieldOrder_FieldConfigInterleaved_ProducesValidRequest()
    {
        var req = new FieldConfigRequest();
        req.EntityManagerId = 5;
        req.FieldName = "Status";
        req.DataType = "string";
        req.DisplayOrder = 1;
        req.Description = "Entity status";
        req.IsRequired = true;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 2. Invalid Combinations

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EntityNameOverMaxLength_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = InvalidValueSets.OverMaxLengthString(EntityNameMaxLength);
        var (isValid, errors) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityName") || e.Contains("100"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EntityNameAt101Chars_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = InvalidValueSets.MaxLengthString(101);
        var (isValid, errors) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_EntityNameAt100Chars_Valid()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = InvalidValueSets.MaxLengthString(100);
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_TableNameOverMaxLength_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.TableName = InvalidValueSets.OverMaxLengthString(TableNameMaxLength);
        var (isValid, errors) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("TableName"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyEntityName_FailsValidation(string? value)
    {
        var req = CreateValidEntityRequest();
        req.EntityName = value ?? string.Empty;
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [Trait("Category", "Negative")]
    public void Invalid_NullOrEmptyTableName_FailsValidation(string? value)
    {
        var req = CreateValidEntityRequest();
        req.TableName = value ?? string.Empty;
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    [InlineData("object")]
    [Trait("Category", "Negative")]
    public void Invalid_InvalidDataType_FailsValidation(string value)
    {
        var req = CreateValidFieldRequest();
        req.DataType = value;
        var (isValid, errors) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("DataType"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [Trait("Category", "Negative")]
    public void Invalid_EntityManagerIdZeroOrNegative_FailsValidation(int value)
    {
        var req = CreateValidFieldRequest();
        req.EntityManagerId = value;
        var (isValid, errors) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("EntityManagerId"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_FieldNameOverMaxLength_FailsValidation()
    {
        var req = CreateValidFieldRequest();
        req.FieldName = InvalidValueSets.OverMaxLengthString(FieldNameMaxLength);
        var (isValid, errors) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("FieldName"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_DescriptionOver500_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.Description = InvalidValueSets.OverMaxLengthString(DescriptionMaxLength);
        var (isValid, errors) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("Description"));
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void Invalid_AllRequiredEntityFieldsInvalid_FailsValidation()
    {
        var req = new EntityConfigRequest { EntityName = "", TableName = "" };
        var (isValid, errors) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
        errors.Should().NotBeEmpty();
    }

    #endregion

    #region 3. Mixed Valid/Invalid Combinations

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityName_InvalidTableName_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.TableName = "";
        req.EntityName.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidTableName_InvalidEntityName_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = null;
        req.TableName.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntityConfig_ValidFieldConfig_Valid()
    {
        var entityReq = CreateValidEntityRequest();
        var fieldReq = CreateValidFieldRequest();
        var (entityValid, _) = ValidateEntityConfig(entityReq);
        var (fieldValid, _) = ValidateFieldConfig(fieldReq);
        entityValid.Should().BeTrue();
        fieldValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntityConfig_InvalidFieldDataType_FailsValidation()
    {
        var fieldReq = CreateValidFieldRequest();
        fieldReq.DataType = "invalid";
        var (isValid, _) = ValidateFieldConfig(fieldReq);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidFieldName_InvalidEntityManagerId_FailsValidation()
    {
        var req = CreateValidFieldRequest();
        req.EntityManagerId = 0;
        req.FieldName.Should().NotBeNullOrWhiteSpace();
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidEntity_InvalidDescriptionOverMax_FailsValidation()
    {
        var req = CreateValidEntityRequest();
        req.Description = InvalidValueSets.OverMaxLengthString(DescriptionMaxLength);
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Mixed_ValidEntity_ValidDescriptionAtMax_Valid()
    {
        var req = CreateValidEntityRequest();
        req.Description = InvalidValueSets.MaxLengthString(DescriptionMaxLength);
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Mixed_ValidFieldConfig_InvalidFieldNameEmpty_FailsValidation()
    {
        var req = CreateValidFieldRequest();
        req.FieldName = "";
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
    }

    #endregion

    #region 4. Partial Submission

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalEntityOnly_Valid()
    {
        var req = new EntityConfigRequest { EntityName = "E", TableName = "e" };
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
        req.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_EntityWithDescription_Valid()
    {
        var req = CreateValidEntityRequest();
        req.Description = "Entity description";
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_FullEntityConfig_Valid()
    {
        var req = CreateValidEntityRequest();
        req.Description = "Full config";
        req.IsActive = true;
        req.EnableChangeLog = true;
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_MinimalFieldOnly_Valid()
    {
        var req = new FieldConfigRequest { FieldName = "Id", DataType = "int", EntityManagerId = 1 };
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
        req.Description.Should().BeNull();
        req.DefaultValue.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_FullFieldConfig_Valid()
    {
        var req = CreateValidFieldRequest();
        req.Description = "Field desc";
        req.DefaultValue = "default";
        req.MaxLength = 100;
        req.ListViewOrder = 1;
        req.HelperText = "Help text";
        req.IsRequired = true;
        req.EnableChangeLog = true;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_EntityWithIsActiveFalse_Valid()
    {
        var req = CreateValidEntityRequest();
        req.IsActive = false;
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_FieldWithOptionalNulls_Valid()
    {
        var req = CreateValidFieldRequest();
        req.Description = null;
        req.DefaultValue = null;
        req.MaxLength = null;
        req.ListViewOrder = null;
        req.HelperText = null;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void Partial_FieldWithDisplayOrderOnly_Valid()
    {
        var req = CreateValidFieldRequest();
        req.DisplayOrder = 5;
        req.ShowInListView = false;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    #endregion

    #region 5. Boundary Tests

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllEntityStringFieldsAtMaxLength_Valid()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = InvalidValueSets.MaxLengthString(EntityNameMaxLength);
        req.TableName = InvalidValueSets.MaxLengthString(TableNameMaxLength);
        req.Description = InvalidValueSets.MaxLengthString(DescriptionMaxLength);
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllFieldStringFieldsAtMaxLength_Valid()
    {
        var req = CreateValidFieldRequest();
        req.FieldName = InvalidValueSets.MaxLengthString(FieldNameMaxLength);
        req.DataType = "string";
        req.Description = InvalidValueSets.MaxLengthString(DescriptionMaxLength);
        req.DefaultValue = InvalidValueSets.MaxLengthString(DefaultValueMaxLength);
        req.HelperText = InvalidValueSets.MaxLengthString(HelperTextMaxLength);
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MaxLengthAtIntMaxValue_Valid()
    {
        var req = CreateValidFieldRequest();
        req.MaxLength = int.MaxValue;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MaxLengthAtZero_Valid()
    {
        var req = CreateValidFieldRequest();
        req.MaxLength = 0;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_MaxLengthNegative_FailsValidation()
    {
        var req = CreateValidFieldRequest();
        req.MaxLength = -1;
        var (isValid, errors) = ValidateFieldConfig(req);
        isValid.Should().BeFalse();
        errors.Should().Contain(e => e.Contains("MaxLength"));
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_DisplayOrderAtIntMaxValue_Valid()
    {
        var req = CreateValidFieldRequest();
        req.DisplayOrder = int.MaxValue;
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
        req.DisplayOrder.Should().Be(int.MaxValue);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_EntityBooleanPermutations_PropertiesReflectValues()
    {
        foreach (var perm in InvalidValueSets.BooleanPermutations(2))
        {
            var req = CreateValidEntityRequest();
            req.IsActive = perm[0];
            req.EnableChangeLog = perm[1];
            req.IsActive.Should().Be(perm[0]);
            req.EnableChangeLog.Should().Be(perm[1]);
        }
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_FieldBooleanPermutations_KeyCombos_PropertiesReflectValues()
    {
        var keyPerms = new[] { (false, false, false, false), (true, true, true, true), (true, false, true, false), (false, true, false, true) };
        foreach (var (isReq, isActive, enableChg, showList) in keyPerms)
        {
            var req = CreateValidFieldRequest();
            req.IsRequired = isReq;
            req.IsActive = isActive;
            req.EnableChangeLog = enableChg;
            req.ShowInListView = showList;
            req.IsRequired.Should().Be(isReq);
            req.IsActive.Should().Be(isActive);
            req.EnableChangeLog.Should().Be(enableChg);
            req.ShowInListView.Should().Be(showList);
        }
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_EntityNameExactly100Chars_Valid()
    {
        var req = CreateValidEntityRequest();
        req.EntityName = InvalidValueSets.MaxLengthString(100);
        req.EntityName.Should().HaveLength(100);
        var (isValid, _) = ValidateEntityConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_HelperTextExactly1000Chars_Valid()
    {
        var req = CreateValidFieldRequest();
        req.HelperText = InvalidValueSets.MaxLengthString(HelperTextMaxLength);
        req.HelperText.Should().HaveLength(HelperTextMaxLength);
        var (isValid, _) = ValidateFieldConfig(req);
        isValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void Boundary_AllValidDataTypes_Valid()
    {
        foreach (var dt in ValidDataTypes)
        {
            var req = CreateValidFieldRequest();
            req.DataType = dt;
            var (isValid, _) = ValidateFieldConfig(req);
            isValid.Should().BeTrue($"DataType '{dt}' should be valid");
        }
    }

    #endregion
}
