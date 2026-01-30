# 🎭 Playwright AI Agents - Quick Start Guide

## ✅ Installation Complete!

Your Playwright AI Agents are installed and ready to use!

---

## 📍 **Where Are the Agents?**

### In Explorer Tree (Left Sidebar):
```
📁 .github/
  └── 📁 agents/           ← 🎭 YOUR 3 AGENTS ARE HERE!
      ├── playwright-test-planner.agent.md      🎭 Planner Agent
      ├── playwright-test-generator.agent.md    🎭 Generator Agent  
      └── playwright-test-healer.agent.md       🎭 Healer Agent
```

---

## 🚀 **How to Use the Agents**

### **In Cursor Chat** (`Ctrl+L` or `Cmd+L`):

Type `@` and you'll see the agents appear:
- `@playwright-test-planner`
- `@playwright-test-generator`
- `@playwright-test-healer`

---

## 🎯 **Try It Now: 5-Minute Demo**

### **Step 1: Start Your Angular App** ⚙️

Open a **new terminal** in Cursor and run:

```powershell
cd UNOPS.PAO.ClientApp
npm start
```

Wait until you see:
```
** Angular Live Development Server is listening on localhost:4200 **
✔ Compiled successfully.
```

---

### **Step 2: Use the Planner Agent** 🎭

**Open Cursor Chat** (`Ctrl+L`):

```
@playwright-test-planner 

Create a comprehensive test plan for the Partners list page.

URL: http://localhost:4200/#/partnerships/partners

Please explore and document:
1. Page layout and components
2. Partner list table
3. Search functionality
4. Filter options
5. "New Partner" button behavior
6. Navigation to partner details

Save the plan to: specs/partners-list-plan.md
```

**What Happens:**
- Agent opens your app in a browser
- Navigates and explores the Partners page
- Takes screenshots and analyzes UI
- Creates a detailed test plan
- Saves to `specs/partners-list-plan.md`

**Time:** ~2-3 minutes

---

### **Step 3: Generate Tests from the Plan** 🎭

After the plan is created, use the Generator:

```
@playwright-test-generator

Generate Playwright tests from the plan at:
specs/partners-list-plan.md

Focus on:
- Section 1: Page layout verification
- Section 2: Partner list functionality
- Section 3: Search and filter

Save tests to: Playwright Tests/partners-list-ai.spec.ts
Use seed: Playwright Tests/seed.spec.ts
```

**What Happens:**
- Agent reads the test plan
- Converts scenarios into TypeScript code
- Creates proper test structure
- Saves to `Playwright Tests/partners-list-ai.spec.ts`

**Time:** ~3-5 minutes

---

### **Step 4: Run the Generated Tests** 🧪

```powershell
npx playwright test partners-list-ai.spec.ts --headed
```

---

### **Step 5: Fix Any Failures** 🎭

If tests fail, use the Healer:

```
@playwright-test-healer

Please fix all failing tests in:
Playwright Tests/partners-list-ai.spec.ts

Analyze and fix:
- Selector issues
- Timing problems  
- Assertion errors
```

**What Happens:**
- Agent runs the tests
- Identifies failures
- Debugs each issue
- Updates the code
- Re-runs to verify fixes

**Time:** ~2-4 minutes

---

## 🎓 **Example Prompts for Each Agent**

### 🎭 **Planner Agent Examples:**

```
@playwright-test-planner explore the Opportunities feature at http://localhost:4200/#/opportunities
```

```
@playwright-test-planner create edge case test scenarios for the Contact creation form
```

```
@playwright-test-planner map out the complete user journey for creating and editing a Partner
```

### 🎭 **Generator Agent Examples:**

```
@playwright-test-generator convert specs/opportunities-plan.md into tests
```

```
@playwright-test-generator create tests for Section 3 of specs/partners-plan.md only
```

```
@playwright-test-generator generate CRUD tests from specs/contacts-plan.md
```

### 🎭 **Healer Agent Examples:**

```
@playwright-test-healer fix failing tests in Playwright Tests/partners.spec.ts
```

```
@playwright-test-healer debug and repair all tests in the Playwright Tests directory
```

```
@playwright-test-healer update selectors in Playwright Tests/opportunities-ai.spec.ts
```

---

## 📁 **File Organization**

| What | Where |
|------|-------|
| **Agent Definitions** | `.github/agents/*.agent.md` |
| **Test Plans (MD)** | `specs/*.md` |
| **Generated Tests (TS)** | `Playwright Tests/*-ai.spec.ts` |
| **Seed Template** | `Playwright Tests/seed.spec.ts` |
| **Prompts** | `.github/prompts/*.prompt.md` |

---

## 💡 **Pro Tips**

### ✅ **Do:**
- Be specific in your prompts
- Start with small features first
- Review generated plans before generating tests
- Run tests after generation to catch issues early
- Use Healer iteratively for stubborn failures

### ❌ **Don't:**
- Ask agents to test while app is not running
- Generate tests without a plan first
- Skip the Healer if tests fail
- Forget to commit generated tests

---

## 🐛 **Troubleshooting**

### **"Agent not found"**
- Type `@playwright` in chat and wait for autocomplete
- Check `.github/agents/` folder exists
- Restart Cursor IDE

### **"Cannot connect to localhost:4200"**
```powershell
cd UNOPS.PAO.ClientApp
npm start
```

### **"Generated tests are failing"**
```
@playwright-test-healer fix the tests
```

### **"Planner is stuck"**
- Check if app is loaded in browser
- Try closing and reopening browser
- Restart the planner with simpler scope

---

## 🎯 **Recommended First Projects**

### **1. Partners Feature** (Simple)
- List view
- Search
- Create new partner

### **2. Opportunities Feature** (Medium)
- CRUD operations
- Status changes
- Filtering

### **3. Complete User Flow** (Advanced)
- Login → Create Partner → Add Contact → Create Opportunity
- Multi-page journey
- Complex interactions

---

## 📚 **Additional Resources**

- **Complete Guide**: `.github/HOW_TO_USE_AGENTS.md`
- **Agent Definitions**: `.github/agents/`
- **Playwright Docs**: https://playwright.dev
- **Existing Tests**: `Playwright Tests/` (for reference)

---

## 🎉 **You're Ready!**

1. **Start your app**: `cd UNOPS.PAO.ClientApp && npm start`
2. **Open Chat**: `Ctrl+L`
3. **Tag agent**: `@playwright-test-planner`
4. **Start testing**: "Create test plan for Partners feature"

**Happy Testing! 🎭**

---

**Questions?** Read the full guide at `.github/HOW_TO_USE_AGENTS.md`
