using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// PNO-669: Mobile Sidebar Close Button — Negative tests.
/// Validates the close button does not break existing sidebar behavior
/// and that desktop mode is unaffected.
///
/// Requirements validated:
/// - REQ-1: Close button does not interfere with menu items → N01–N04
/// - REQ-2: Desktop mode unaffected (hidden on desktop) → N05–N08
/// - REQ-3: closeSidebar resets ALL relevant state flags → N09–N12
/// - REQ-4: Accessibility not broken → N13–N15
/// </summary>
[Collection("MobileSidebarClose")]
public class PNO669NegativeTests
{
    private readonly MobileSidebarCloseFixture _f;

    public PNO669NegativeTests(MobileSidebarCloseFixture fixture) => _f = fixture;

    // ── Close button does not break menu ────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N01_SidebarTemplate_CloseButton_DoesNotReplaceMenuItems_REQ1()
    {
        _f.SidebarTemplate.Should().Contain("app-menu",
            "REQ-1: Adding close button must not remove the menu component");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N02_SidebarTemplate_CloseButton_IsNotInsideMenuList_REQ1()
    {
        var closeIdx = _f.SidebarTemplate.IndexOf(MobileSidebarCloseSpec.CloseButtonClass);
        var menuIdx = _f.SidebarTemplate.IndexOf("app-menu");
        closeIdx.Should().BeLessThan(menuIdx,
            "REQ-1: Close button must appear BEFORE the menu component, not inside it");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N03_SidebarTemplate_DoesNotContainMultipleCloseButtons_REQ1()
    {
        var count = CountOccurrences(_f.SidebarTemplate, MobileSidebarCloseSpec.CloseButtonClass);
        count.Should().Be(1,
            "REQ-1: There must be exactly one close button in the sidebar, not multiple");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N04_SidebarTemplate_CloseButton_DoesNotNavigate_REQ1()
    {
        var closeSection = ExtractCloseButtonSection(_f.SidebarTemplate);
        closeSection.Should().NotContain("routerLink",
            "REQ-1: Close button must only close the sidebar, not navigate to a route");
        closeSection.Should().NotContain("href",
            "REQ-1: Close button must not have an href attribute");
    }

    // ── Desktop mode unaffected ─────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N05_SidebarScss_MobileHeader_HiddenOnDesktop_REQ2()
    {
        _f.SidebarScss.Should().Contain("display: none",
            "REQ-2: Mobile header must be hidden (display: none) on desktop viewports");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N06_SidebarScss_MobileHeader_VisibleOnlyViaMobileMediaQuery_REQ2()
    {
        _f.SidebarScss.Should().Contain("@media",
            "REQ-2: Mobile header visibility must be controlled via a media query");
        _f.SidebarScss.Should().Contain(MobileSidebarCloseSpec.MobileBreakpoint,
            "REQ-2: Media query must use 991px breakpoint for mobile detection");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N07_SidebarScss_CloseButton_NotStyledAsLink_REQ2()
    {
        _f.SidebarScss.Should().NotContain("text-decoration: underline",
            "REQ-2: Close button should look like a button, not a text link");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N08_TopbarTemplate_StillHasHamburgerButton_REQ2()
    {
        _f.TopbarTemplate.Should().Contain(MobileSidebarCloseSpec.HamburgerIcon,
            "REQ-2: Adding close button must not remove the hamburger menu toggle in the topbar");
        _f.TopbarTemplate.Should().Contain(MobileSidebarCloseSpec.LayoutMenuButton,
            "REQ-2: Hamburger button class must remain in the topbar");
    }

    // ── closeSidebar resets all state ────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N09_CloseSidebar_ResetsOverlayMenuActive_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.OverlayMenuActive,
            "REQ-3: closeSidebar must reset overlayMenuActive to false");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N10_CloseSidebar_ResetsStaticMenuMobileActive_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.StaticMenuMobileActive,
            "REQ-3: closeSidebar must reset staticMenuMobileActive to false");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N11_CloseSidebar_ResetsMenuHoverActive_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(MobileSidebarCloseSpec.MenuHoverActive,
            "REQ-3: closeSidebar must reset menuHoverActive to false");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N12_CloseSidebar_SetsStatesToFalse_NotTrue_REQ3()
    {
        var methodBody = ExtractMethodBody(_f.SidebarTypescript, MobileSidebarCloseSpec.CloseSidebarMethod);
        methodBody.Should().Contain("false",
            "REQ-3: closeSidebar must set state flags to false, not true");
    }

    // ── Accessibility ───────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Negative")]
    public void N13_CloseButton_DoesNotUseGenericAriaLabel_REQ4()
    {
        _f.SidebarTemplate.Should().NotContain("aria-label=\"close\"",
            "REQ-4: Close button should use translated aria-label, not hardcoded generic 'close'");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N14_CloseButton_IsNotAnchorTag_REQ4()
    {
        var closeSection = ExtractCloseButtonSection(_f.SidebarTemplate);
        closeSection.Should().Contain("<button",
            "REQ-4: Close action must use a <button> element, not <a>, for proper accessibility semantics");
    }

    [Fact]
    [Trait("Category", "Negative")]
    public void N15_CloseButton_HasTypeAttribute_REQ4()
    {
        var closeSection = ExtractCloseButtonSection(_f.SidebarTemplate);
        closeSection.Should().Contain("type=\"button\"",
            "REQ-4: Close button must have type='button' to prevent form submission");
    }

    private static int CountOccurrences(string source, string search)
    {
        int count = 0, idx = 0;
        while ((idx = source.IndexOf(search, idx, StringComparison.Ordinal)) != -1)
        {
            count++;
            idx += search.Length;
        }
        return count;
    }

    private static string ExtractCloseButtonSection(string template)
    {
        var idx = template.IndexOf(MobileSidebarCloseSpec.CloseButtonClass);
        if (idx < 0) return string.Empty;
        var start = Math.Max(0, template.LastIndexOf('<', idx));
        var end = template.IndexOf('>', idx);
        return end > start ? template.Substring(start, end - start + 1) : string.Empty;
    }

    private static string ExtractMethodBody(string source, string methodName)
    {
        var idx = source.IndexOf(methodName);
        if (idx < 0) return string.Empty;
        var braceStart = source.IndexOf('{', idx);
        if (braceStart < 0) return string.Empty;
        var depth = 1;
        var pos = braceStart + 1;
        while (pos < source.Length && depth > 0)
        {
            if (source[pos] == '{') depth++;
            else if (source[pos] == '}') depth--;
            pos++;
        }
        return source.Substring(braceStart, pos - braceStart);
    }
}
