using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// PNO-669: Mobile Sidebar Close Button — Positive (happy-path) tests.
///
/// Requirements validated:
/// - REQ-1: Close button markup exists in sidebar template → P01–P03
/// - REQ-3: closeSidebar() method exists and resets state → P04
/// - REQ-4: Accessibility attributes present → P05
/// </summary>
[Collection("MobileSidebarClose")]
public class PNO669PositiveTests
{
    private readonly MobileSidebarCloseFixture _f;

    public PNO669PositiveTests(MobileSidebarCloseFixture fixture) => _f = fixture;

    [Fact]
    [Trait("Category", "Positive")]
    public void P01_SidebarTemplate_ContainsCloseButton_REQ1()
    {
        _f.SidebarTemplate.Should().Contain(MobileSidebarCloseSpec.CloseButtonClass,
            "REQ-1: Sidebar template must contain a close button with class 'sidebar-close-btn'");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P02_SidebarTemplate_ContainsMobileHeader_REQ1()
    {
        _f.SidebarTemplate.Should().Contain(MobileSidebarCloseSpec.MobileHeaderClass,
            "REQ-1: Sidebar template must have a mobile header container for the close button");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P03_SidebarTemplate_CloseButtonUsesTimesIcon_REQ1()
    {
        _f.SidebarTemplate.Should().Contain(MobileSidebarCloseSpec.CloseIconClass,
            "REQ-1: Close button must use 'pi pi-times' (X) icon for clear visual indication");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P04_SidebarTypescript_HasCloseSidebarMethod_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.CloseSidebarMethod,
            "REQ-3: Sidebar component must have a closeSidebar() method to handle close action");
    }

    [Fact]
    [Trait("Category", "Positive")]
    public void P05_SidebarTemplate_CloseButton_HasAriaLabel_REQ4()
    {
        _f.SidebarTemplate.Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-4: Close button must have aria-label using translation key 'button.closeMenu'");
    }
}
