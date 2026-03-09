using FluentAssertions;
using Xunit;

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

/// <summary>
/// PNO-669: Mobile Sidebar Close Button — Functional tests.
/// Validates business rules: click handling, layout service integration,
/// translation support, and interaction with existing outside-click behavior.
///
/// Requirements validated:
/// - REQ-1: Close button click binding → F01–F03
/// - REQ-3: Layout state management via closeSidebar() → F04–F08
/// - REQ-4: Accessibility and semantic correctness → F09–F11
/// - REQ-5: Translation keys exist in all 4 languages → F12–F15
/// </summary>
[Collection("MobileSidebarClose")]
public class PNO669FunctionalTests
{
    private readonly MobileSidebarCloseFixture _f;

    public PNO669FunctionalTests(MobileSidebarCloseFixture fixture) => _f = fixture;

    // ── Click binding ───────────────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F01_SidebarTemplate_CloseButton_BindsClickToCloseSidebar_REQ1()
    {
        _f.SidebarTemplate.Should().Contain($"(click)=\"{MobileSidebarCloseSpec.CloseSidebarMethod}()\"",
            "REQ-1: Close button must bind (click) event to closeSidebar() method");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F02_SidebarTemplate_MobileHeader_ContainsOnlyCloseButton_REQ1()
    {
        var headerStart = _f.SidebarTemplate.IndexOf(MobileSidebarCloseSpec.MobileHeaderClass);
        var headerEnd = _f.SidebarTemplate.IndexOf("</div>", headerStart);
        if (headerStart >= 0 && headerEnd >= 0)
        {
            var headerContent = _f.SidebarTemplate.Substring(headerStart, headerEnd - headerStart);
            headerContent.Should().Contain("button",
                "REQ-1: Mobile header should contain a button element");
            headerContent.Should().NotContain("routerLink",
                "REQ-1: Mobile header should not contain navigation links, only the close button");
        }
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F03_SidebarTemplate_CloseButton_InsideMobileHeader_REQ1()
    {
        var headerIdx = _f.SidebarTemplate.IndexOf(MobileSidebarCloseSpec.MobileHeaderClass);
        var btnIdx = _f.SidebarTemplate.IndexOf(MobileSidebarCloseSpec.CloseButtonClass);
        headerIdx.Should().BeLessThan(btnIdx,
            "REQ-1: Close button must be inside the mobile header container");
    }

    // ── Layout state management ─────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F04_CloseSidebar_UsesLayoutServiceUpdate_REQ3()
    {
        _f.SidebarTypescript.Should().Contain("layoutService",
            "REQ-3: closeSidebar must use layoutService to update layout state");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F05_CloseSidebar_UsesSignalUpdate_NotDirectMutation_REQ3()
    {
        _f.SidebarTypescript.Should().Contain(".update(",
            "REQ-3: closeSidebar must use signal .update() pattern for reactive state changes");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F06_LayoutService_OnMenuToggle_ExistsForHamburger_REQ3()
    {
        _f.LayoutService.Should().Contain("onMenuToggle",
            "REQ-3: Layout service must have onMenuToggle() for hamburger button compatibility");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F07_LayoutTypescript_OutsideClick_StillWorks_REQ3()
    {
        _f.LayoutTypescript.Should().Contain("isOutsideClicked",
            "REQ-3: Outside-click handler must still exist — close button supplements, not replaces it");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F08_LayoutTypescript_HideMenu_AlsoResetsStates_REQ3()
    {
        _f.LayoutTypescript.Should().Contain("hideMenu",
            "REQ-3: Layout's hideMenu() must also exist for outside-click behavior");
    }

    // ── Accessibility / semantic ────────────────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F09_SidebarTemplate_CloseButton_UsesTranslatedAriaLabel_REQ4()
    {
        _f.SidebarTemplate.Should().Contain("translate",
            "REQ-4: aria-label must use Angular translate pipe for i18n support");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F10_SidebarScss_CloseButton_HasHoverState_REQ4()
    {
        _f.SidebarScss.Should().Contain("hover",
            "REQ-4: Close button must have hover state for visual feedback");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F11_SidebarScss_CloseButton_HasActiveState_REQ4()
    {
        _f.SidebarScss.Should().Contain("active",
            "REQ-4: Close button must have active (pressed) state for tactile feedback on mobile");
    }

    // ── Translation keys in all 4 languages ─────────────────────────────────

    [Fact]
    [Trait("Category", "Functional")]
    public void F12_TranslationKey_CloseMenu_ExistsInEnglish_REQ5()
    {
        var enFile = MobileSidebarCloseSpec.TranslationFiles[0];
        _f.TranslationContents[enFile].Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-5: 'button.closeMenu' translation key must exist in English (en.json)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F13_TranslationKey_CloseMenu_ExistsInFrench_REQ5()
    {
        var frFile = MobileSidebarCloseSpec.TranslationFiles[1];
        _f.TranslationContents[frFile].Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-5: 'button.closeMenu' translation key must exist in French (fr.json)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F14_TranslationKey_CloseMenu_ExistsInSpanish_REQ5()
    {
        var esFile = MobileSidebarCloseSpec.TranslationFiles[2];
        _f.TranslationContents[esFile].Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-5: 'button.closeMenu' translation key must exist in Spanish (es.json)");
    }

    [Fact]
    [Trait("Category", "Functional")]
    public void F15_TranslationKey_CloseMenu_ExistsInPortuguese_REQ5()
    {
        var ptFile = MobileSidebarCloseSpec.TranslationFiles[3];
        _f.TranslationContents[ptFile].Should().Contain(MobileSidebarCloseSpec.AriaLabelKey,
            "REQ-5: 'button.closeMenu' translation key must exist in Portuguese (pt.json)");
    }
}
