/// <summary>
/// PNO-669: Mobile sidebar close button — Specification helpers.
///
/// Bug: In mobile interface, users are unable to easily exit the menu screen
/// if text size is set too large. The sidebar fills the entire screen with no
/// close button, forcing users to navigate to another page or restart the app.
///
/// Fix: Added a close button (X icon) in .sidebar-mobile-header that calls
/// closeSidebar() to reset overlayMenuActive and staticMenuMobileActive.
///
/// Requirements validated:
/// - REQ-1: Close button markup exists in sidebar component template
/// - REQ-2: Close button is hidden on desktop, visible only on mobile (max-width: 991px)
/// - REQ-3: closeSidebar() method resets layout state (overlayMenuActive, staticMenuMobileActive)
/// - REQ-4: Close button has aria-label for accessibility (WCAG)
/// - REQ-5: Translation key 'button.closeMenu' exists in all 4 language files
/// </summary>

namespace UNOPS.PAO.Business.Tests.MobileSidebarClose;

public static class MobileSidebarCloseSpec
{
    public static string ResolvePath(params string[] segments)
    {
        var relative = Path.Combine(segments);
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "..", "..", "..", "..", "..", "..", relative),
            Path.Combine(baseDir, "..", "..", "..", "..", "..", relative),
            Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", relative),
        };
        foreach (var p in candidates)
        {
            var full = Path.GetFullPath(p);
            if (File.Exists(full) || Directory.Exists(full))
                return full;
        }
        return Path.Combine(baseDir, Path.GetFileName(relative));
    }

    public static string ReadFileOrEmpty(string path)
        => File.Exists(path) ? File.ReadAllText(path) : string.Empty;

    // ── Component file paths ────────────────────────────────────────────────
    public const string SidebarTemplatePath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/sidebar/sidebar.component.html";
    public const string SidebarTypescriptPath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/sidebar/sidebar.component.ts";
    public const string SidebarScssPath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/sidebar/sidebar.component.scss";
    public const string LayoutTemplatePath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/layout/layout.component.html";
    public const string LayoutTypescriptPath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/layout/layout.component.ts";
    public const string LayoutServicePath =
        "UNOPS.PAO.ClientApp/src/app/layouts/services/layout.service.ts";
    public const string ResponsiveScssPath =
        "UNOPS.PAO.ClientApp/public/layout/_responsive.scss";
    public const string MenuScssPath =
        "UNOPS.PAO.ClientApp/public/layout/_menu.scss";
    public const string TopbarTemplatePath =
        "UNOPS.PAO.ClientApp/src/app/layouts/components/topbar/topbar.component.html";

    // ── Translation file paths ──────────────────────────────────────────────
    public static readonly string[] TranslationFiles = new[]
    {
        "UNOPS.PAO.ClientApp/src/assets/i18n/en.json",
        "UNOPS.PAO.ClientApp/src/assets/i18n/fr.json",
        "UNOPS.PAO.ClientApp/src/assets/i18n/span.json",
        "UNOPS.PAO.ClientApp/src/assets/i18n/pt.json",
    };

    // ── Expected markup/code patterns ───────────────────────────────────────
    public const string CloseButtonClass = "sidebar-close-btn";
    public const string MobileHeaderClass = "sidebar-mobile-header";
    public const string CloseIconClass = "pi pi-times";
    public const string AriaLabelKey = "button.closeMenu";
    public const string CloseSidebarMethod = "closeSidebar";
    public const string OverlayMenuActive = "overlayMenuActive";
    public const string StaticMenuMobileActive = "staticMenuMobileActive";
    public const string MenuHoverActive = "menuHoverActive";
    public const string MobileBreakpoint = "991px";
    public const string LayoutMobileActive = "layout-mobile-active";
    public const string LayoutMenuButton = "layout-menu-button";
    public const string HamburgerIcon = "pi pi-bars";
}
