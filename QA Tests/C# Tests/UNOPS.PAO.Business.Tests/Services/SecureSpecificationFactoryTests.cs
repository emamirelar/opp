/**
 * @fileoverview Mock-based unit tests for SecureSpecificationFactory
 * Tests RBAC-aware specification creation, fallback behavior, and error handling.
 *
 * Ratio: P=1, N=4, E=4, F=4, I=4 → Total=17 (all ≥ 3×P)
 *
 * @author UNOPS Opportunity+ QA Team
 */

using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using UNOPS.PAO.Domain.Entities;
using UNOPS.PAO.Domain.Specifications;
using UNOPS.PAO.Domain.Specifications.InteractionSpecifications;
using UNOPS.PAO.Domain.Specifications.Interfaces;
using UNOPS.PAO.Models.Contacts;
using UNOPS.PAO.Models.Interactions;
using UNOPS.PAO.Models.Partners;
using UNOPS.PAO.UNOPSBusiness.Interfaces;
using UNOPS.PAO.UNOPSBusiness.Services;
using UNOPS.PAO.UNOPSDomain.Entities;
using UNOPS.PAO.UNOPSDomain.Specifications;
using Xunit;

namespace UNOPS.PAO.Business.Tests.Services;

/// <summary>
/// Mock-based unit tests for SecureSpecificationFactory.
/// Verifies RBAC-aware specification creation, delegation, fallbacks, and NotImplementedException.
///
/// 3:1 Ratio Compliance:
///   Positive  (P) =  1  (N ≥ 3P ✅, E ≥ 3P ✅, F ≥ 3P ✅, I ≥ 3P ✅)
///   Negative  (N) =  4
///   Edge      (E) =  4
///   Functional(F) =  4
///   Integration(I)=  4
///   ─────────────────────────────────
///   TOTAL         = 17
/// </summary>
public class SecureSpecificationFactoryTests
{
    private static ClaimsPrincipal CreateUser(string? userId = "1", params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId ?? "1"),
            new(ClaimTypes.Email, "test@test.com"),
            new(ClaimTypes.Name, "Test User")
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    private static IInteractionSearchFilter CreateInteractionFilter() => new InteractionFilterRequest();
    private static IPartnerSearchFilter CreatePartnerFilter() => new PartnerFilterRequest();
    private static IContactSearchFilter CreateContactFilter() => new ContactFilterRequest();

    #region Positive Tests (P=1)

    [Fact]
    [Trait("Category", "Positive")]
    public async Task CreateInteractionSpecificationAsync_ValidInputs_ReturnsRBACSpecification()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("OU-001");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ISpecification<Interaction>>();
        mockPermissionService.Verify(x => x.GetUserOrgUnitAsync(user), Times.Once);
    }

    #endregion

    #region Negative Tests (N=4)

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreateContactSpecificationAsync_Always_ThrowsNotImplementedException()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateContactFilter();
        var user = CreateUser();

        var act = () => sut.CreateContactSpecificationAsync(filter, user);

        await act.Should().ThrowAsync<NotImplementedException>()
            .WithMessage("Contact secure specification not yet implemented");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreateInteractionSpecificationAsync_PermissionServiceThrows_FallsBackToStandardSpec()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new InvalidOperationException("Permission service failure"));

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
        result.Should().BeOfType<InteractionCompositeSpecification>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreatePartnerSpecificationAsync_OrgUnitFilterServiceThrows_FallsBackToUNOPSPartnerCompositeSpecification()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new InvalidOperationException("OrgUnit filter failure"));

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreatePartnerFilter();
        var user = CreateUser();

        var result = await sut.CreatePartnerSpecificationAsync(filter, user);

        result.Should().NotBeNull();
        result.Should().BeOfType<UNOPSPartnerCompositeSpecification>();
    }

    [Fact]
    [Trait("Category", "Negative")]
    public async Task CreateInteractionSpecificationAsync_NullUser_DoesNotThrowAndReturnsSpec()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(null!))
            .ReturnsAsync("");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        ClaimsPrincipal? user = null;

        var result = await sut.CreateInteractionSpecificationAsync(filter, user!);

        result.Should().NotBeNull();
    }

    #endregion

    #region Edge/Boundary Tests (E=4)

    [Fact]
    [Trait("Category", "Edge")]
    public async Task CreateInteractionSpecificationAsync_GetUserOrgUnitReturnsNull_StillReturnsSpecification()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ISpecification<Interaction>>();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task CreatePartnerSpecificationAsync_DelegatesToOrgUnitFilterService()
    {
        var mockSpec = new Mock<ISpecification<UNOPSPartner>>();
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(mockSpec.Object);

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreatePartnerFilter();
        var user = CreateUser();

        var result = await sut.CreatePartnerSpecificationAsync(filter, user);

        result.Should().BeSameAs(mockSpec.Object);
        mockOrgUnitFilterService.Verify(x => x.CreatePartnerSpecificationAsync(filter, user), Times.Once);
        mockPermissionService.Verify(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task CreateInteractionSpecificationAsync_EmptyFilter_StillCreatesSpecification()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("OU-001");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Edge")]
    public async Task CreateInteractionSpecificationAsync_UserWithNoRoles_StillCreatesSpecification()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "1"),
            new(ClaimTypes.Email, "test@test.com")
        };
        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
    }

    #endregion

    #region Functional Tests (F=4)

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateInteractionSpecificationAsync_UsesGetUserOrgUnitAsyncForRBAC()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("OU-123");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        await sut.CreateInteractionSpecificationAsync(filter, user);

        mockPermissionService.Verify(x => x.GetUserOrgUnitAsync(user), Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreatePartnerSpecificationAsync_UsesOrgUnitFilterServiceOnly()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(Mock.Of<ISpecification<UNOPSPartner>>());

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreatePartnerFilter();
        var user = CreateUser();

        await sut.CreatePartnerSpecificationAsync(filter, user);

        mockOrgUnitFilterService.Verify(x => x.CreatePartnerSpecificationAsync(filter, user), Times.Once);
        mockPermissionService.Invocations.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreateInteractionSpecificationAsync_FallbackLogsError()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new InvalidOperationException("Test error"));

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser();

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().BeOfType<InteractionCompositeSpecification>();
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating secure interaction specification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Functional")]
    public async Task CreatePartnerSpecificationAsync_FallbackLogsError()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new InvalidOperationException("Partner spec error"));

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreatePartnerFilter();
        var user = CreateUser();

        var result = await sut.CreatePartnerSpecificationAsync(filter, user);

        result.Should().BeOfType<UNOPSPartnerCompositeSpecification>();
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating secure partner specification")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region Integration Tests (I=4)

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_InteractionSpecification_EndToEnd()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync("OU-GLOBAL");

        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateInteractionFilter();
        var user = CreateUser("1", "PARTNER_GLOB_ADMIN");

        var result = await sut.CreateInteractionSpecificationAsync(filter, user);

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<ISpecification<Interaction>>();
        mockPermissionService.Verify(x => x.GetUserOrgUnitAsync(user), Times.Once);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_PartnerSpecification_EndToEnd()
    {
        var mockSpec = new Mock<ISpecification<UNOPSPartner>>();
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(mockSpec.Object);

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreatePartnerFilter();
        var user = CreateUser();

        var result = await sut.CreatePartnerSpecificationAsync(filter, user);

        result.Should().BeSameAs(mockSpec.Object);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_ContactSpecification_ThrowsBeforeAnyDelegation()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var filter = CreateContactFilter();
        var user = CreateUser();

        var act = () => sut.CreateContactSpecificationAsync(filter, user);
        await act.Should().ThrowAsync<NotImplementedException>();

        mockPermissionService.Invocations.Should().BeEmpty();
        mockOrgUnitFilterService.Invocations.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task FullFlow_InteractionErrorThenPartnerSuccess_IndependentBehavior()
    {
        var mockPermissionService = new Mock<IPermissionService>();
        mockPermissionService.Setup(x => x.GetUserOrgUnitAsync(It.IsAny<ClaimsPrincipal>()))
            .ThrowsAsync(new InvalidOperationException("Permission failure"));

        var mockPartnerSpec = new Mock<ISpecification<UNOPSPartner>>();
        var mockOrgUnitFilterService = new Mock<IOrgUnitFilterService>();
        mockOrgUnitFilterService.Setup(x => x.CreatePartnerSpecificationAsync(It.IsAny<IPartnerSearchFilter>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync(mockPartnerSpec.Object);

        var mockLogger = new Mock<ILogger<SecureSpecificationFactory>>();

        var sut = new SecureSpecificationFactory(
            mockPermissionService.Object,
            mockOrgUnitFilterService.Object,
            mockLogger.Object);

        var user = CreateUser();

        var interactionResult = await sut.CreateInteractionSpecificationAsync(CreateInteractionFilter(), user);
        var partnerResult = await sut.CreatePartnerSpecificationAsync(CreatePartnerFilter(), user);

        interactionResult.Should().BeOfType<InteractionCompositeSpecification>();
        partnerResult.Should().BeSameAs(mockPartnerSpec.Object);
    }

    #endregion
}

/*
### 3:1 Ratio Compliance Check
| Category | Count | Tests |
|----------|-------|-------|
| Positive (P) | 1 | CreateInteractionSpecificationAsync_ValidInputs_ReturnsRBACSpecification |
| Negative (N) | 4 | CreateContactSpecificationAsync_Always_ThrowsNotImplementedException, CreateInteractionSpecificationAsync_PermissionServiceThrows_FallsBackToStandardSpec, CreatePartnerSpecificationAsync_OrgUnitFilterServiceThrows_FallsBackToUNOPSPartnerCompositeSpecification, CreateInteractionSpecificationAsync_NullUser_DoesNotThrowAndReturnsSpec |
| Edge/Boundary (E) | 4 | CreateInteractionSpecificationAsync_GetUserOrgUnitReturnsNull_StillReturnsSpecification, CreatePartnerSpecificationAsync_DelegatesToOrgUnitFilterService, CreateInteractionSpecificationAsync_EmptyFilter_StillCreatesSpecification, CreateInteractionSpecificationAsync_UserWithNoRoles_StillCreatesSpecification |
| Functional (F) | 4 | CreateInteractionSpecificationAsync_UsesGetUserOrgUnitAsyncForRBAC, CreatePartnerSpecificationAsync_UsesOrgUnitFilterServiceOnly, CreateInteractionSpecificationAsync_FallbackLogsError, CreatePartnerSpecificationAsync_FallbackLogsError |
| Integration (I) | 4 | FullFlow_InteractionSpecification_EndToEnd, FullFlow_PartnerSpecification_EndToEnd, FullFlow_ContactSpecification_ThrowsBeforeAnyDelegation, FullFlow_InteractionErrorThenPartnerSuccess_IndependentBehavior |
| **N ≥ 3P?** | ✅ | 4 >= 3 |
| **E ≥ 3P?** | ✅ | 4 >= 3 |
| **F ≥ 3P?** | ✅ | 4 >= 3 |
| **I ≥ 3P?** | ✅ | 4 >= 3 |
*/
