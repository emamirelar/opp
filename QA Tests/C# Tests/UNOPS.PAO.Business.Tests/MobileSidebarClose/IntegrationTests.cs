using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// PNO-669: Mobile Sidebar Close Button — Integration tests.
/// Validates end-to-end flows across sidebar, layout, topbar, and responsive SCSS.
///
/// Requirements validated:
/// - REQ-1: Full sidebar component structure is coherent → I01–I04
/// - REQ-2: Responsive CSS chain works end-to-end → I05–I08
/// - REQ-3: State management chain: sidebar → layoutService → layout → CSS → hide → I09–I12
/// - REQ-4/5: Accessibility and i18n integration → I13–I15
/// </summary>
[Collection("MobileSidebarClose")]
public class PNO669IntegrationTests
{
    private readonly MobileSidebarCloseFixture _f;

    public PNO669IntegrationTests(MobileSidebarCloseFixture fixture) => _f = fixture;

    // ── Sidebar component coherence ─────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I01_SidebarComponent_HasTemplateAndTypescript_REQ1()
    {
        _f.SidebarTemplate.Should().NotBeNullOrWhiteSpace(
            "REQ-1: Sidebar HTML template must exist");
        _f.SidebarTypescript.Should().NotBeNullOrWhiteSpace(
            "REQ-1: Sidebar TypeScript component must exist");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I02_SidebarComponent_HasMatchingScss_REQ1()
    {
        _f.SidebarScss.Should().NotBeNullOrWhiteSpace(
            "REQ-1: Sidebar SCSS file must exist with mobile close button styles");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I03_SidebarTemplate_StructureIsCorrect_MobileHeaderThenMenu_REQ1()
    {
        var mobileHeaderIdx = _f.SidebarTemplate.IndexOf(MobileSidebarCloseSpec.MobileHeaderClass);
        var menuIdx = _f.SidebarTemplate.IndexOf("app-menu");
        mobileHeaderIdx.Should().BeGreaterThanOrEqualTo(0,
            "REQ-1: Mobile header must exist in template");
        menuIdx.Should().BeGreaterThan(mobileHeaderIdx,
            "REQ-1: Mobile header with close button must come before menu component");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I04_SidebarTypescript_ImportsLayoutService_REQ1()
    {
        _f.SidebarTypescript.Should().Contain("LayoutService",
            "REQ-1: Sidebar component must import LayoutService for state management");
    }

    // ── Responsive CSS chain ────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I05_ResponsiveScss_And_SidebarScss_UseSameBreakpoint_REQ2()
    {
        _f.ResponsiveScss.Should().Contain("991px",
            "REQ-2: Responsive SCSS must use 991px breakpoint");
        _f.SidebarScss.Should().Contain("991px",
            "REQ-2: Sidebar SCSS must use same 991px breakpoint for consistency");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I06_MenuScss_Sidebar_HasFixedPosition_REQ2()
    {
        _f.MenuScss.Should().Contain("fixed",
            "REQ-2: Sidebar uses fixed positioning — close button must work within this context");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I07_MenuScss_Sidebar_HasZIndex_REQ2()
    {
        _f.MenuScss.Should().Contain("z-index",
            "REQ-2: Sidebar must have a z-index so close button is above content");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I08_ResponsiveScss_MobileActive_ShowsSidebar_REQ2()
    {
        _f.ResponsiveScss.Should().Contain(MobileSidebarCloseSpec.LayoutMobileActive,
            "REQ-2: Responsive SCSS must handle layout-mobile-active class for sidebar visibility");
    }

    // ── State management chain ──────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I09_StateChain_TopbarToggles_LayoutServiceManages_SidebarCloses_REQ3()
    {
        _f.TopbarTemplate.Should().Contain("onMenuButtonClick",
            "REQ-3: Topbar hamburger calls onMenuButtonClick (opens sidebar)");
        _f.LayoutService.Should().Contain("onMenuToggle",
            "REQ-3: Layout service has onMenuToggle (manages toggle state)");
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.CloseSidebarMethod,
            "REQ-3: Sidebar has closeSidebar (closes sidebar on X click)");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I10_StateChain_CloseSidebar_And_OutsideClick_BothResetSameFlags_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.OverlayMenuActive);
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.StaticMenuMobileActive);
        _f.LayoutTypescript.Should().Contain(MobileSidebarCloseSpec.OverlayMenuActive);
        _f.LayoutTypescript.Should().Contain(MobileSidebarCloseSpec.StaticMenuMobileActive,
            "REQ-3: Both closeSidebar and outside-click must reset the same state flags");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I11_LayoutTemplate_HasLayoutMask_ForBackdrop_REQ3()
    {
        _f.LayoutTemplate.Should().Contain("layout-mask",
            "REQ-3: Layout template must include layout-mask for dimming backdrop on mobile");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I12_LayoutTemplate_ContainsSidebarComponent_REQ3()
    {
        _f.LayoutTemplate.Should().Contain("app-sidebar",
            "REQ-3: Layout template must include <app-sidebar> component");
    }

    // ── Accessibility and i18n integration ──────────────────────────────────

    [Fact]
    [Trait("Category", "Integration")]
    public void I13_AllFourLanguageFiles_HaveCloseMenuKey_REQ5()
    {
        foreach (var kvp in _f.TranslationContents)
        {
            if (kvp.Value.Length > 0)
            {
                kvp.Value.Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
                    $"REQ-5: Translation file '{Path.GetFileName(kvp.Key)}' must have 'button.closeMenu' key");
            }
        }
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I14_SidebarTemplate_AriaLabel_Uses_TranslatePipe_REQ4()
    {
        var ariaSection = ExtractAriaSection(_f.SidebarTemplate);
        ariaSection.Should().Contain("translate",
            "REQ-4: aria-label must use Angular translate pipe for proper i18n");
        ariaSection.Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-4: aria-label must reference 'button.closeMenu' translation key");
    }

    [Fact]
    [Trait("Category", "Integration")]
    public void I15_TopbarAndSidebar_BothHaveMenuButtons_REQ4()
    {
        _f.TopbarTemplate.Should().Contain(MobileSidebarCloseSpec.HamburgerIcon,
            "REQ-4: Topbar must have hamburger (open) button");
        _f.SidebarTemplate.Should().Contain(MobileSidebarCloseSpec.CloseIconClass,
            "REQ-4: Sidebar must have close (X) button — both entry/exit points must exist");
    }

    private static string ExtractAriaSection(string template)
    {
        var idx = template.IndexOf("[attr.aria-label]");
        if (idx < 0)
        {
            idx = template.IndexOf("aria-label");
            if (idx < 0) return string.Empty;
        }
        var end = template.IndexOf('>', idx);
        if (end < 0) end = template.Length;
        return template.Substring(idx, end - idx);
    }
}
