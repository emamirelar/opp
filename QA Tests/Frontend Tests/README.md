# Frontend Component Tests (Jasmine/Karma)

This folder contains Angular component tests using Jasmine and Karma.

## Overview

These tests verify Angular components, services, and UI logic for the UNOPS Opportunity+ frontend application.

## Folder Structure

```
Frontend Tests/
├── README.md                              # This file
├── setup-frontend-tests.ps1               # PowerShell setup script (Windows)
├── setup-frontend-tests.sh                # Bash setup script (Linux/Mac)
├── components/
│   ├── base-entity-view.component.spec.ts
│   ├── related-info-panel.component.spec.ts
│   ├── enhanced-entity-layout.component.spec.ts
│   ├── partner-view-enhanced.component.spec.ts
│   └── contact-view-enhanced.component.spec.ts
└── services/
    └── panel-layout.service.spec.ts
```

## Quick Start

### Option 1: Using Setup Script (Recommended)

**Windows (PowerShell):**
```powershell
cd "QA Tests/Frontend Tests"
.\setup-frontend-tests.ps1            # Copy files
.\setup-frontend-tests.ps1 -DryRun    # Preview what would happen
.\setup-frontend-tests.ps1 -Force     # Overwrite existing files
```

**Linux/Mac (Bash):**
```bash
cd "QA Tests/Frontend Tests"
chmod +x setup-frontend-tests.sh
./setup-frontend-tests.sh              # Copy files
./setup-frontend-tests.sh --dry-run    # Preview what would happen
./setup-frontend-tests.sh --force      # Overwrite existing files
```

### Option 2: Manual Copy

Copy spec files to corresponding component folders in `UNOPS.PAO.ClientApp`:

| Source File | Destination Folder |
|------------|-------------------|
| `components/base-entity-view.component.spec.ts` | `src/app/shared/components/base-entity-view/` |
| `components/related-info-panel.component.spec.ts` | `src/app/shared/components/related-info-panel/` |
| `components/enhanced-entity-layout.component.spec.ts` | `src/app/shared/components/enhanced-entity-layout/` |
| `components/partner-view-enhanced.component.spec.ts` | `src/app/features/partnerships/partners/components/partner/view/` |
| `components/contact-view-enhanced.component.spec.ts` | `src/app/features/partnerships/contacts/components/contact/view/` |
| `services/panel-layout.service.spec.ts` | `src/app/shared/services/` |

## Running Tests

After copying spec files:

```bash
cd UNOPS.PAO.ClientApp
npm install       # Install dependencies (first time only)
ng test           # Run tests
```

Or to run with coverage:

```bash
ng test --code-coverage
```

## Test Pattern

Each test file follows Angular testing conventions:
- Uses `TestBed` for component setup
- Uses `ComponentFixture` for DOM interaction
- Uses `jasmine.createSpyObj` for mocking services

## Dependencies

These tests require:
- `@angular/core/testing`
- `jasmine-core`
- `karma`
- `karma-jasmine`
- `karma-chrome-launcher`

---

**Total Test Files**: 6  
**Last Updated**: December 18, 2025

