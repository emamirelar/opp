# UNOPS Styling System

This directory contains the comprehensive UNOPS design system implementation for Angular with PrimeNG integration.

## 🎨 Design System Overview

The UNOPS styling system provides a complete design foundation that ensures:
- **Brand Consistency**: All components follow UNOPS visual identity
- **PrimeNG Integration**: Seamless override of PrimeNG defaults
- **Accessibility**: WCAG compliant components and interactions
- **Dark Mode Support**: Automatic theme switching
- **Responsive Design**: Mobile-first approach with UNOPS breakpoints

## 📁 File Structure

```
src/styles/
├── README.md                    # This documentation
├── unops-design-tokens.scss     # SCSS variables and mixins
├── unops-design-tokens.css      # CSS custom properties
├── unops-design-tokens.json     # Design tokens for tools
├── primeng-unops-theme.scss     # PrimeNG component overrides
├── unops-utilities.scss         # Utility classes
└── /                           # Additional theme files
```

## 🔧 Key Files

### 1. `unops-design-tokens.css`
The single source of truth for all UNOPS design tokens as CSS custom properties. This file:
- Defines the complete UNOPS color palette
- Maps PrimeNG tokens to UNOPS colors
- Provides dark theme support
- Includes comprehensive token aliases

### 2. `primeng-unops-theme.scss`
Comprehensive PrimeNG component overrides that:
- **Override ALL PrimeNG component styles** with UNOPS design
- Use `!important` declarations to ensure precedence
- Maintain accessibility and interaction states
- Support dark theme automatically

### 3. `unops-utilities.scss`
Ready-to-use utility classes for common patterns:
- Button variants (`.unops-button-primary`, `.unops-icon-button`)
- Typography classes (`.unops-text-*`)
- Layout utilities (`.unops-flex`, `.unops-gap-*`)
- Spacing utilities (`.unops-p-*`, `.unops-m-*`)

## 🚀 Usage Examples

### Simple Icon Button (No Background)
```html
<button pButton 
        type="button" 
        icon="pi pi-refresh"
        class="unops-icon-button"
        (click)="refresh()">
</button>
```

### Primary Button
```html
<button pButton 
        type="button"
        class="unops-button-primary"
        label="Save Changes">
</button>
```

### Card Layout
```html
<div class="unops-card">
  <div class="unops-card-header">
    <h3 class="unops-text-headline-medium">Card Title</h3>
  </div>
  <div class="unops-card-body">
    <p class="unops-text-body-medium">Card content goes here</p>
  </div>
</div>
```

## 🎯 Design Tokens

### Colors
- **Primary**: `var(--unops-primary)` - #0092d1
- **Secondary**: `var(--unops-secondary)` - #004976
- **Success**: `var(--unops-success)` - #10b981
- **Warning**: `var(--unops-warning)` - #cc8400
- **Error**: `var(--unops-error)` - #991e66
- **Info**: `var(--unops-info)` - #4ec3e0

### Spacing
- **XS**: `var(--unops-spacing-xs)` - 4px
- **SM**: `var(--unops-spacing-sm)` - 8px
- **MD**: `var(--unops-spacing-md)` - 16px
- **LG**: `var(--unops-spacing-lg)` - 24px
- **XL**: `var(--unops-spacing-xl)` - 32px

### Typography
- **Display**: `var(--unops-font-display)` - Inter, Noto Sans, system fonts
- **Body**: `var(--unops-font-body)` - Inter, Noto Sans, system fonts
- **Sizes**: From `--unops-font-size-xs` (12px) to `--unops-font-size-display-large` (40px)

### Border Radius
- **XS**: `var(--unops-radius-xs)` - 4px
- **MD**: `var(--unops-radius-md)` - 8px
- **LG**: `var(--unops-radius-lg)` - 12px
- **Full**: `var(--unops-radius-full)` - 9999px

## 🌙 Dark Mode

Dark mode is automatically supported through CSS custom properties. The system:
- Detects the `.app-dark` class on the root element
- Automatically adjusts surface colors and text contrast
- Maintains UNOPS brand colors for interactive elements
- Preserves accessibility contrast ratios

## 📱 Responsive Design

The system includes UNOPS-specific breakpoints:
- **Mobile**: `max-width: 599px`
- **Tablet**: `600px - 839px` 
- **Desktop**: `min-width: 840px`

Responsive utilities are available:
- `.unops-mobile-hidden`
- `.unops-tablet-hidden`  
- `.unops-desktop-hidden`

## 🔧 PrimeNG Integration

The styling system completely overrides PrimeNG defaults:

1. **Button Components**: All `p-button` elements automatically use UNOPS styling
2. **Form Controls**: Input fields, dropdowns, etc. follow UNOPS design
3. **Data Components**: Tables, cards, panels use UNOPS surfaces
4. **Overlay Components**: Dialogs, menus, tooltips match UNOPS theme
5. **Feedback Components**: Toasts, messages use UNOPS colors

## ✅ Best Practices

### DO:
- Use utility classes for simple styling (`.unops-icon-button`)
- Reference design tokens in custom CSS (`var(--unops-primary)`)
- Apply semantic color classes (`.unops-text-primary`)
- Use UNOPS spacing tokens for consistency

### DON'T:
- Override PrimeNG styles manually (use the theme system)
- Hardcode colors or spacing values
- Use inline styles for common patterns
- Mix different design systems

## 🚨 Troubleshooting

### PrimeNG Styles Still Showing
1. Ensure `primeng-unops-theme.scss` is loaded after PrimeNG
2. Check that `!important` declarations are present
3. Verify CSS loading order in `styles.scss`

### Dark Mode Not Working
1. Confirm `.app-dark` class is applied to root element
2. Check that CSS custom properties are defined for both themes
3. Verify component uses tokens, not hardcoded colors

### Utility Classes Not Working
1. Ensure `unops-utilities.scss` is imported after theme
2. Check for conflicting styles with higher specificity
3. Use browser dev tools to inspect CSS cascade

## 📚 Additional Resources

- [UNOPS Brand Guidelines](https://brand.unops.org)
- [PrimeNG Documentation](https://primeng.org)
- [CSS Custom Properties MDN](https://developer.mozilla.org/en-US/docs/Web/CSS/--*)
- [WCAG Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)

## 🔄 Updates

When updating the design system:

1. **Update design tokens** in `unops-design-tokens.css`
2. **Update PrimeNG overrides** in `primeng-unops-theme.scss`
3. **Add utility classes** in `unops-utilities.scss`
4. **Test in both light and dark modes**
5. **Verify accessibility compliance**
6. **Update documentation**

---

**Version**: 1.0.0  
**Last Updated**: January 2025  
**Maintainer**: UNOPS Development Team