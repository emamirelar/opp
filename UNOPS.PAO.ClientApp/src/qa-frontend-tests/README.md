# QA Frontend Tests

Standalone Angular component and service spec files maintained by the QA team.

These tests live inside the Angular project so they can use the installed
Angular libraries without any additional setup or copy steps.

## Structure

```
qa-frontend-tests/
├── components/   ← Angular component spec files (Jasmine/Karma)
└── services/     ← Angular service spec files (Jasmine/Karma)
```

## How to Run

From the `UNOPS.PAO.ClientApp/` folder:

```bash
npx ng test
```

The Angular Karma runner automatically discovers all `src/**/*.spec.ts` files,
including the ones in this folder.

To run only the QA frontend tests:

```bash
npx ng test --include="src/qa-frontend-tests/**"
```

## Test Design

All spec files in this folder use **self-contained mock components** — they do
not import or depend on the actual application source files. This means:

- No relative path issues
- Tests can be run independently of any specific component location
- Tests validate UI behaviour through mock implementations that mirror the
  real component contracts
