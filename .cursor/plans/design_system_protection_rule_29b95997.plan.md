---
name: Design system protection rule
overview: "Create a new `.cursor/rules/design-system-protection.mdc` rule file that enforces four constraints: no permission workarounds, no unauthorized style changes, no component modifications outside the design system, and no hard-coded values."
todos:
  - id: create-rule
    content: Create `.cursor/rules/design-system-protection.mdc` with YAML frontmatter and all 4 sections
    status: completed
  - id: todo-1773652605614-tniinvdpu
    content: ""
    status: cancelled
isProject: false
---

# Design System and Code Integrity Protection Rule

## What

Create a single new rule file at `[.cursor/rules/design-system-protection.mdc](UNOPS.PAO.ClientApp/.cursor/rules/design-system-protection.mdc)` that enforces four mandatory constraints during development.

## File Details

- **Path:** `/Users/ema/git/Opp_plus_latest/opportunityplus/.cursor/rules/design-system-protection.mdc`
- **Format:** YAML frontmatter (`alwaysApply: true`, glob `*.ts, *.html, *.scss, *.css, *.cs`) followed by Markdown body
- **Naming convention:** matches existing kebab-case `.mdc` pattern

## Rule Content (4 Sections)

### 1. No Permission/Auth Workarounds

- Never write code that bypasses, skips, or mocks authentication or authorization in production code
- Never hardcode tokens, API keys, or credentials
- Never disable or weaken `[Authorize]`, `[PermissionAuthorize]`, or Angular route guards to make something "just work"
- If a permission blocks progress, STOP and ask the user for the correct authentication details or credentials
- Reference the existing server-side permission pattern from `component-development.mdc` and `dotnet-implementation.mdc`

### 2. No Unauthorized Styling Changes

- Never modify any existing stylesheet or style-related file without explicit user request
- Protected files include:
  - `src/styles/unops-design-tokens.scss` / `.css`
  - `src/styles/primeng-unops-theme.scss`
  - `src/styles/unops-utilities.scss`
  - `src/styles/themes/unops.preset.ts`
  - `tailwind.config.js`
  - `public/layout/**/`*
  - `src/styles.scss`
  - Any component `.scss` file not being actively created
- Before touching any style, the agent must:
  1. Tell the user exactly which file and what change
  2. Explain how it would affect the existing project visually
  3. Wait for explicit confirmation

### 3. Use Existing Design System Components Only

- All UI must be built using existing PrimeNG components (via Module imports per `component-development.mdc`) and existing shared/reusable components in `src/app/shared/`
- Never create custom UI primitives (buttons, inputs, modals, cards, etc.) when a PrimeNG or existing shared component exists
- Never modify an existing shared component to fit a new feature without explicit user approval
- If a component doesn't exist for a use case, ask the user before creating one
- Reference the PrimeNG module import table already in `component-development.mdc`

### 4. No Hard-Coded Values

- Never hard-code colors, spacing, font sizes, border radii, shadows, or breakpoints
- Always use:
  - Tailwind UNOPS utility classes (`bg-unops-primary`, `p-unops-md`, `rounded-unops-md`, etc.)
  - CSS custom properties (`var(--unops-primary)`, `var(--unops-spacing-md)`, etc.)
  - SCSS variables (`$unops-primary`, `$unops-font-size-base`, etc.)
  - PrimeNG theme tokens (from the Material-based `unops.preset.ts`)
- Specifically prohibited patterns:
  - `color: #0092d1` (use `var(--unops-primary)` or `text-unops-primary`)
  - `padding: 16px` (use `var(--unops-spacing-md)` or `p-unops-md`)
  - `font-size: 14px` (use `var(--unops-font-size-sm)` or Tailwind class)
  - `border-radius: 8px` (use `var(--unops-radius-md)` or `rounded-unops-md`)
- The only exception is `0` and `1px` which are universal constants

## What Will NOT Change

- No existing rule files are modified
- No production code is modified
- No styling files are modified
- Only one new `.mdc` file is created

