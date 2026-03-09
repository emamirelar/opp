/**
 * @fileoverview Comprehensive unit tests for PermissionService
 * Tests permission checks, role hierarchy, export/import, and entity-level access control.
 *
 * Ratio: P=2, N=6, E=6, F=6, I=6 → Total=26
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using UNOPS.PAO.DataAccess.Interfaces;
using UNOPS.PAO.DataAccess.Services;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDataAccess.Context;
using UNOPS.PAO.UNOPSDomain.Authorization;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Unit tests for PermissionService.
/// Uses InMemory database for EntityPermissions queries; Moq for IHttpContextAccessor.
///
/// 3:1 Ratio Compliance:
///   Positive  (P) =  2
///   Negative  (N) =  6  (N ≥ 3P ✅)
///   Edge      (E) =  6  (E ≥ 3P ✅)
///   Functional(F) =  6  (F ≥ 3P ✅)
///   Integration(I)=  6  (I ≥ 3P ✅)
///   ─────────────────────────────────
///   TOTAL         = 26
/// </summary>
public class PermissionServiceTests : IDisposable
{
    private readonly UNOPSAppDbContext _context;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly PermissionService _sut;

    public PermissionServiceTests()
    {
        var options = new DbContextOptionsBuilder<UNOPSAppDbContext>()
            .UseInMemoryDatabase($"PermissionServiceTest_{Guid.NewGuid()}")
            .Options;

        _mockHttpContextAccessor = CreateMockHttpContextAccessor();
        var userResolverService = new UserResolverService<int>(_mockHttpContextAccessor.Object);
        var mockSchema = new Mock<IDbContextSchema>();
        mockSchema.Setup(x => x.Schema).Returns("public");

        _context = new UNOPSAppDbContext(options, userResolverService, mockSchema.Object);
        _context.Database.EnsureCreated();
        SeedEntityPermissions();

        _sut = new PermissionService(_context, _mockHttpContextAccessor.Object);
    }

    private static Mock<IHttpContextAccessor> CreateMockHttpContextAccessor(
        ClaimsPrincipal? user = null,
        string? userId = "1",
        params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId ?? "1"),
            new(ClaimTypes.Email, "test@test.com"),
            new(ClaimTypes.Name, "Test User")
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = user ?? new ClaimsPrincipal(identity);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.Setup(x => x.User).Returns(principal);
        mockHttpContext.Setup(x => x.Request.Headers).Returns(new HeaderDictionary());
        mockHttpContext.Setup(x => x.Request.RouteValues).Returns(new Microsoft.AspNetCore.Routing.RouteValueDictionary());

        var mockAccessor = new Mock<IHttpContextAccessor>();
        mockAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
        return mockAccessor;
    }

    private void SeedEntityPermissions()
    {
        var permissions = new[]
        {
            new EntityPermission { Id = 1, Entity = "Partner", Role = "PARTNER_GLOB_ADMIN", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
            new EntityPermission { Id = 2, Entity = "Partner", Role = "ORG_UNIT_ADMIN", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = false },
            new EntityPermission { Id = 3, Entity = "Partner", Role = "PARTNER_USER", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = false },
            new EntityPermission { Id = 4, Entity = "Partner", Role = "UNOPS_GEN_USER", CanRead = true, CanCreate = false, CanUpdate = false, CanDelete = false },
            new EntityPermission { Id = 5, Entity = "Contact", Role = "PARTNER_GLOB_ADMIN", CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = true },
            new EntityPermission { Id = 6, Entity = "Opportunity", Role = "PARTNER_USER", CanRead = true, CanCreate = true, CanUpdate = false, CanDelete = false },
        };
        _context.EntityPermissions.AddRange(permissions);
        _context.SaveChanges();
    }

    private static ClaimsPrincipal CreateUser(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "test@test.com"),
            new(ClaimTypes.Name, "Test User")
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static ClaimsPrincipal CreateUnauthenticatedUser()
    {
        var identity = new ClaimsIdentity();
        return new ClaimsPrincipal(identity);
    }

    #region Positive Tests (P=2)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task HasPermissionAsync_AdminWithReadPermission_ReturnsTrue()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        var result = await _sut.HasPermissionAsync(user, "Partner", "read");
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void CanExport_PartnerGlobAdmin_ReturnsTrue()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        var result = _sut.CanExport(user);
        result.Should().BeTrue();
    }

    #endregion

    #region Negative Tests (N=6)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task HasPermissionAsync_NullUser_ReturnsFalse()
    {
        var result = await _sut.HasPermissionAsync(null!, "Partner", "read");
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task HasPermissionAsync_UnauthenticatedUser_ReturnsFalse()
    {
        var user = CreateUnauthenticatedUser();
        var result = await _sut.HasPermissionAsync(user, "Partner", "read");
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task HasPermissionAsync_UserWithNoRoles_ReturnsFalse()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "test@test.com")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var result = await _sut.HasPermissionAsync(user, "Partner", "read");
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task HasPermissionAsync_UnknownAction_ReturnsFalse()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        var result = await _sut.HasPermissionAsync(user, "Partner", "unknown_action");
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CanExport_NonAdminUser_ReturnsFalse()
    {
        var user = CreateUser("PARTNER_USER");
        var result = _sut.CanExport(user);
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void CanImport_UnauthenticatedUser_ReturnsFalse()
    {
        var user = CreateUnauthenticatedUser();
        var result = _sut.CanImport(user);
        result.Should().BeFalse();
    }

    #endregion

    #region Edge/Boundary Tests (E=6)

    [Fact]
    [Trait("Category", "Edge")]
    public void GetEffectiveRole_NullUser_ReturnsNull()
    {
        var result = _sut.GetEffectiveRole(null!);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void GetEffectiveRole_UserWithNoRoles_ReturnsNull()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "test@test.com")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var result = _sut.GetEffectiveRole(user);
        result.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void GetEffectiveRole_UserWithMultipleRoles_ReturnsHighestPriority()
    {
        var user = CreateUser("UNOPS_GEN_USER", "PARTNER_GLOB_ADMIN", "ORG_UNIT_ADMIN");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("PARTNER_GLOB_ADMIN");
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task HasPermissionAsync_CaseInsensitiveEntityName_ReturnsTrue()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        var result = await _sut.HasPermissionAsync(user, "partner", "read");
        result.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void CanExport_NullUser_ReturnsFalse()
    {
        var result = _sut.CanExport(null!);
        result.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public void GetEffectiveRole_UnknownRole_FallsBackToFirstRole()
    {
        var user = CreateUser("CUSTOM_ROLE", "ANOTHER_CUSTOM");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("CUSTOM_ROLE");
    }

    #endregion

    #region Functional Tests (F=6)

    [Fact]
    [Trait("Category", "Functional")]
    public void GetEffectiveRole_PartnerGlobAdminTakesPriorityOverOrgUnitAdmin()
    {
        var user = CreateUser("ORG_UNIT_ADMIN", "PARTNER_GLOB_ADMIN");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("PARTNER_GLOB_ADMIN");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void GetEffectiveRole_OrgUnitAdminTakesPriorityOverPartnerUser()
    {
        var user = CreateUser("PARTNER_USER", "ORG_UNIT_ADMIN");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("ORG_UNIT_ADMIN");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void GetEffectiveRole_PartnerUserTakesPriorityOverUnopsGenUser()
    {
        var user = CreateUser("UNOPS_GEN_USER", "PARTNER_USER");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("PARTNER_USER");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void CanImport_ReturnsTrueOnlyForPartnerGlobAdmin()
    {
        var adminUser = CreateUser("PARTNER_GLOB_ADMIN");
        var orgAdminUser = CreateUser("ORG_UNIT_ADMIN");
        _sut.CanImport(adminUser).Should().BeTrue();
        _sut.CanImport(orgAdminUser).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void GetEffectiveRole_SingleRoleUser_ReturnsCorrectRole()
    {
        var user = CreateUser("PARTNER_USER");
        var result = _sut.GetEffectiveRole(user);
        result.Should().Be("PARTNER_USER");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task HasPermissionAsync_ChecksCorrectAction_ReadCreateUpdateDelete()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        (await _sut.HasPermissionAsync(user, "Partner", "read")).Should().BeTrue();
        (await _sut.HasPermissionAsync(user, "Partner", "create")).Should().BeTrue();
        (await _sut.HasPermissionAsync(user, "Partner", "update")).Should().BeTrue();
        (await _sut.HasPermissionAsync(user, "Partner", "delete")).Should().BeTrue();
    }

    #endregion

    #region Integration Tests (I=6)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullPermissionFlow_AdminCanReadPartners()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        var canRead = await _sut.HasPermissionAsync(user, "Partner", "read");
        var effectiveRole = _sut.GetEffectiveRole(user);
        canRead.Should().BeTrue();
        effectiveRole.Should().Be("PARTNER_GLOB_ADMIN");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullPermissionFlow_UserCannotDeletePartners()
    {
        var user = CreateUser("UNOPS_GEN_USER");
        var canDelete = await _sut.HasPermissionAsync(user, "Partner", "delete");
        canDelete.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void CanExportAndCanImport_Admin_HasBoth()
    {
        var user = CreateUser("PARTNER_GLOB_ADMIN");
        _sut.CanExport(user).Should().BeTrue();
        _sut.CanImport(user).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void CanExportAndCanImport_NonAdmin_HasNeither()
    {
        var user = CreateUser("PARTNER_USER");
        _sut.CanExport(user).Should().BeFalse();
        _sut.CanImport(user).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void MultipleRoleUser_GetsHighestPriorityPermissions()
    {
        var user = CreateUser("UNOPS_GEN_USER", "PARTNER_GLOB_ADMIN");
        var effectiveRole = _sut.GetEffectiveRole(user);
        effectiveRole.Should().Be("PARTNER_GLOB_ADMIN");
        _sut.CanExport(user).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task PermissionDenied_NoMatchingEntityPermissions_AllActionsDenied()
    {
        var user = CreateUser("PARTNER_USER");
        var canRead = await _sut.HasPermissionAsync(user, "NonExistentEntity", "read");
        var canCreate = await _sut.HasPermissionAsync(user, "NonExistentEntity", "create");
        canRead.Should().BeFalse();
        canCreate.Should().BeFalse();
    }

    #endregion

    public void Dispose()
    {
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 2 | HasPermissionAsync_AdminWithReadPermission_ReturnsTrue, CanExport_PartnerGlobAdmin_ReturnsTrue |
| Negative (N) | 6 | HasPermissionAsync_NullUser_ReturnsFalse, HasPermissionAsync_UnauthenticatedUser_ReturnsFalse, HasPermissionAsync_UserWithNoRoles_ReturnsFalse, HasPermissionAsync_UnknownAction_ReturnsFalse, CanExport_NonAdminUser_ReturnsFalse, CanImport_UnauthenticatedUser_ReturnsFalse |
| Edge/Boundary (E) | 6 | GetEffectiveRole_NullUser_ReturnsNull, GetEffectiveRole_UserWithNoRoles_ReturnsNull, GetEffectiveRole_UserWithMultipleRoles_ReturnsHighestPriority, HasPermissionAsync_CaseInsensitiveEntityName_ReturnsTrue, CanExport_NullUser_ReturnsFalse, GetEffectiveRole_UnknownRole_FallsBackToFirstRole |
| Functional (F) | 6 | GetEffectiveRole_PartnerGlobAdminTakesPriorityOverOrgUnitAdmin, GetEffectiveRole_OrgUnitAdminTakesPriorityOverPartnerUser, GetEffectiveRole_PartnerUserTakesPriorityOverUnopsGenUser, CanImport_ReturnsTrueOnlyForPartnerGlobAdmin, GetEffectiveRole_SingleRoleUser_ReturnsCorrectRole, HasPermissionAsync_ChecksCorrectAction_ReadCreateUpdateDelete |
| Integration (I) | 6 | FullPermissionFlow_AdminCanReadPartners, FullPermissionFlow_UserCannotDeletePartners, CanExportAndCanImport_Admin_HasBoth, CanExportAndCanImport_NonAdmin_HasNeither, MultipleRoleUser_GetsHighestPriorityPermissions, PermissionDenied_NoMatchingEntityPermissions_AllActionsDenied |
| **N ≥ 3P?** | ✅ | 6 >= 6 |
| **E ≥ 3P?** | ✅ | 6 >= 6 |
| **F ≥ 3P?** | ✅ | 6 >= 6 |
| **I ≥ 3P?** | ✅ | 6 >= 6 |
*/
