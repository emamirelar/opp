# 🎭 How to Use Playwright AI Agents

## Quick Reference

| Agent | Tag | Purpose | Example |
|-------|-----|---------|---------|
| **Planner** | `@playwright-test-planner` | Explore app & create test plans | Create plan for Partners feature |
| **Generator** | `@playwright-test-generator` | Convert plans to test code | Generate tests from plan |
| **Healer** | `@playwright-test-healer` | Debug & fix failing tests | Fix broken partner tests |

---

## 🎭 Agent 1: Planner

### What it Does
- Navigates your application like a real user
- Discovers all features, forms, and interactions
- Creates comprehensive test plans with scenarios

### How to Use

**In Cursor Chat (`Ctrl+L` or `Cmd+L`):**

```
@playwright-test-planner 

Create a comprehensive test plan for the Partners feature.

Target URL: http://localhost:4200/#/partnerships/partners

Please explore:
- Partner list view
- Create new partner dialog
- Partner detail page
- Search and filtering
- Edit and delete operations
```

### What You'll Get

The agent will create a markdown file in `specs/` with:
- Test scenarios (happy path, edge cases, errors)
- Step-by-step test instructions
- Expected outcomes
- Success/failure criteria

**Example Output:** `specs/partners-feature-plan.md`

---

## 🎭 Agent 2: Generator

### What it Does
- Reads test plans from `specs/` directory
- Generates working Playwright test code
- Creates `.spec.ts` files with proper structure

### How to Use

**In Cursor Chat:**

```
@playwright-test-generator

Generate Playwright tests from the plan at specs/partners-feature-plan.md

Create tests for:
- Section 1: Partner List View
- Section 2: Create New Partner

Save tests to: Playwright Tests/partners-ai-generated.spec.ts
Use seed file: Playwright Tests/seed.spec.ts
```

### What You'll Get

Generated test files like:
```typescript
// Playwright Tests/partners-ai-generated.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Partner List View', () => {
  test('should display partner list table', async ({ page }) => {
    // Generated code here
  });
  
  test('should filter partners by name', async ({ page }) => {
    // Generated code here
  });
});
```

---

## 🎭 Agent 3: Healer

### What it Does
- Runs your test suite
- Identifies failing tests
- Automatically debugs and fixes issues
- Updates selectors, assertions, and timing

### How to Use

**In Cursor Chat:**

```
@playwright-test-healer

Please debug and fix the failing tests in:
Playwright Tests/partners-ai-generated.spec.ts

Focus on:
- Selector issues
- Timing problems
- Assertion failures
```

### What You'll Get

- Fixed test files with updated selectors
- Comments explaining what was broken
- Tests that pass reliably

---

## 📋 Complete Workflow Example

### Scenario: Add Comprehensive Tests for Opportunities Feature

#### Step 1: Plan
```
@playwright-test-planner

Create test plan for Opportunities feature at:
http://localhost:4200/#/opportunities

Cover: List view, Create, Edit, Delete, Search, Status changes
```

**Output:** `specs/opportunities-feature-plan.md`

#### Step 2: Generate
```
@playwright-test-generator

Generate tests from specs/opportunities-feature-plan.md
Save to: Playwright Tests/opportunities-ai.spec.ts
Focus on sections 1-3 (List View, Create, Search)
```

**Output:** `Playwright Tests/opportunities-ai.spec.ts`

#### Step 3: Run Tests
```bash
npx playwright test opportunities-ai.spec.ts --headed
```

#### Step 4: Fix Failures (if any)
```
@playwright-test-healer

Fix failing tests in Playwright Tests/opportunities-ai.spec.ts
```

**Output:** Fixed test file

---

## 🔧 Prerequisites

### 1. Make Sure Your App is Running
```bash
cd UNOPS.PAO.ClientApp
npm start
# App should be running at http://localhost:4200
```

### 2. Ensure You're Logged In
The agents need access to authenticated pages. Options:
- Use existing auth helper in tests
- Log in manually before running planner
- Configure seed file with authentication

---

## 💡 Pro Tips

### For Planner Agent
- **Be specific** about what to explore: "Focus on the CRUD operations for partners"
- **Set boundaries**: "Only explore the list view and create dialog"
- **Specify user flows**: "Test the flow from partner list → create → detail page"

### For Generator Agent
- **Reference sections**: "Generate tests for Section 2.1 only"
- **Specify file names**: "Save to Playwright Tests/partners-crud.spec.ts"
- **Include seed file**: Always reference the seed file for setup

### For Healer Agent
- **Run first**: `npx playwright test [file] --headed` to see failures
- **Be specific**: "Fix the selector issues in test 'should create partner'"
- **Iterate**: Run healer multiple times if needed

---

## 📂 File Locations

| Item | Location |
|------|----------|
| **Test Plans** | `specs/*.md` |
| **Generated Tests** | `Playwright Tests/*-ai.spec.ts` |
| **Seed File** | `Playwright Tests/seed.spec.ts` |
| **Agent Definitions** | `.github/agents/*.agent.md` |
| **Prompts** | `.github/prompts/*.prompt.md` |

---

## 🐛 Troubleshooting

### "Agent not found"
- Make sure you're using `@playwright-test-planner` (with hyphens)
- Check `.github/agents/` folder exists
- Restart Cursor IDE

### "Cannot access http://localhost:4200"
- Start your Angular app: `cd UNOPS.PAO.ClientApp && npm start`
- Wait for compilation to complete
- Try `http://127.0.0.1:4200` instead

### "Generated tests are failing"
- Run Healer agent to auto-fix
- Check authentication is working
- Verify test data exists in database

---

## 🎯 Common Use Cases

### 1. New Feature Testing
```
@playwright-test-planner explore the new Contact Tags feature
@playwright-test-generator create tests from the plan
```

### 2. Fix Broken Tests After UI Changes
```
@playwright-test-healer fix all tests in Playwright Tests/partners.spec.ts
```

### 3. Expand Test Coverage
```
@playwright-test-planner create edge case scenarios for Opportunity filtering
@playwright-test-generator generate tests for edge cases only
```

### 4. Regression Testing
```
@playwright-test-planner verify all CRUD operations for Partners
@playwright-test-generator create full regression suite
```

---

## 🚀 Next Steps

1. **Try the Planner**: Explore one feature of your app
2. **Generate Tests**: Convert the plan to working code
3. **Run Tests**: `npx playwright test --ui`
4. **Fix Issues**: Use Healer to auto-fix any failures
5. **Repeat**: Build comprehensive test coverage incrementally

---

**Need Help?** Check the agent definitions in `.github/agents/` for detailed capabilities and examples.
