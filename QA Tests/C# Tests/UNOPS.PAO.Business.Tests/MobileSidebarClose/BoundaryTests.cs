using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// PNO-669: Mobile Sidebar Close Button — Boundary/edge-case tests.
/// Validates responsive breakpoints, button dimensions, and SCSS boundaries.
///
/// Requirements validated:
/// - REQ-2: Mobile breakpoint at 991px → B01–B06
/// - REQ-1: Close button sizing and touch targets → B07–B10
/// - REQ-3: Layout state management edge cases → B11–B15
/// </summary>
[Collection("MobileSidebarClose")]
public class PNO669BoundaryTests
{
    private readonly MobileSidebarCloseFixture _f;

    public PNO669BoundaryTests(MobileSidebarCloseFixture fixture) => _f = fixture;

    // ── Responsive breakpoint boundaries ────────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B01_SidebarScss_MobileBreakpoint_Is991px_REQ2()
    {
        _f.SidebarScss.Should().Contain("max-width: 991px",
            "REQ-2: Mobile media query must use max-width: 991px (matching layout service isDesktop > 991)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B02_ResponsiveScss_MobileBreakpoint_Is991px_REQ2()
    {
        _f.ResponsiveScss.Should().Contain("991px",
            "REQ-2: Responsive SCSS must define the 991px mobile breakpoint for sidebar transform");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B03_LayoutService_DesktopBreakpoint_Is991_REQ2()
    {
        _f.LayoutService.Should().Contain("991",
            "REQ-2: Layout service isDesktop() must check window.innerWidth > 991");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B04_ResponsiveScss_SidebarTranslatesOffScreen_OnMobile_REQ2()
    {
        _f.ResponsiveScss.Should().Contain("translateX(-100%)",
            "REQ-2: Sidebar must slide off-screen (translateX(-100%)) by default on mobile");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B05_ResponsiveScss_MobileActive_TranslatesOnScreen_REQ2()
    {
        _f.ResponsiveScss.Should().Contain("translateX(0)",
            "REQ-2: Sidebar must slide on-screen (translateX(0)) when layout-mobile-active is set");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B06_ResponsiveScss_LayoutMask_ShowsOnMobileActive_REQ2()
    {
        _f.ResponsiveScss.Should().Contain("layout-mask",
            "REQ-2: Layout mask overlay must be present for dimming background on mobile");
    }

    // ── Close button size / touch target ────────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B07_SidebarScss_CloseButton_HasMinimumTouchTarget_REQ1()
    {
        _f.SidebarScss.Should().Contain("2.5rem",
            "REQ-1: Close button must have at least 2.5rem (40px) touch target for mobile usability");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B08_SidebarScss_CloseButton_IsRoundBorderRadius_REQ1()
    {
        _f.SidebarScss.Should().Contain("border-radius: 50%",
            "REQ-1: Close button should be circular (border-radius: 50%) for visual consistency");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B09_SidebarScss_CloseButton_HasTransparentBackground_REQ1()
    {
        _f.SidebarScss.Should().Contain("background: transparent",
            "REQ-1: Close button background should be transparent by default");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B10_SidebarScss_CloseButton_HasFocusVisibleStyle_REQ1()
    {
        _f.SidebarScss.Should().Contain("focus-visible",
            "REQ-1: Close button must have focus-visible styles for keyboard navigation");
    }

    // ── Layout state management boundaries ──────────────────────────────────

    [Fact]
    [Trait("Category", "Boundary")]
    public void B11_LayoutTypescript_ContainerClass_IncludesMobileActive_REQ3()
    {
        _f.LayoutTypescript.Should().Contain(MobileSidebarCloseSpec.LayoutMobileActive,
            "REQ-3: Layout containerClass must include 'layout-mobile-active' for mobile sidebar state");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B12_LayoutTypescript_HideMenu_ResetsCorrectStates_REQ3()
    {
        _f.LayoutTypescript.Should().Contain(MobileSidebarCloseSpec.OverlayMenuActive,
            "REQ-3: Layout hideMenu must reset overlayMenuActive");
        _f.LayoutTypescript.Should().Contain(MobileSidebarCloseSpec.StaticMenuMobileActive,
            "REQ-3: Layout hideMenu must reset staticMenuMobileActive");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B13_SidebarScss_MobileHeader_FlexJustifyEnd_REQ2()
    {
        _f.SidebarScss.Should().Contain("justify-content: flex-end",
            "REQ-2: Mobile header must align close button to the right (flex-end)");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B14_SidebarScss_MobileHeader_FlexShrink0_REQ2()
    {
        _f.SidebarScss.Should().Contain("flex-shrink: 0",
            "REQ-2: Mobile header must not shrink when menu items overflow");
    }

    [Fact]
    [Trait("Category", "Boundary")]
    public void B15_SidebarScss_Sidebar_MaxWidth100vw_OnMobile_REQ2()
    {
        _f.SidebarScss.Should().Contain("max-width: 100vw",
            "REQ-2: Sidebar must be constrained to 100vw on mobile to handle large text sizes");
    }
}
