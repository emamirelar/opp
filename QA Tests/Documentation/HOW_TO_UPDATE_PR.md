# How to Update Your Pull Request

## 📋 Quick Reference

Your new work (48 commits from Jan 28) has been pushed to the `QA-Tests` branch.

---

## Option 1: Update Existing PR (If One Exists)

### **Step 1: Check if PR exists**

Go to: `https://github.com/UNOPS-ITG/opportunityplus/pulls`

Look for an **open PR** from `QA-Tests` → `dev-deploy`

### **Step 2: If PR exists, update it**

1. Open the existing PR
2. Click **"Edit"** on the PR description
3. **Replace the entire description** with the content from:
   ```
   PR_DESCRIPTION_2026-01-28.md
   ```
4. Update the title to:
   ```
   Test Marathon + Model Creation + Infrastructure Cleanup (3,820 tests)
   ```
5. Add labels: `testing`, `models`, `documentation`, `urgent`
6. Click **"Save"**

✅ **Done!** The PR now includes your new work automatically (GitHub tracks all commits on the branch)

---

## Option 2: Create New PR (If None Exists)

### **Step 1: Go to GitHub**

Open this URL:
```
https://github.com/UNOPS-ITG/opportunityplus/compare/dev-deploy...QA-Tests
```

### **Step 2: Fill in PR Details**

**Title**:
```
Test Marathon + Model Creation + Infrastructure Cleanup (3,820 tests)
```

**Description**: Copy **entire content** from `PR_DESCRIPTION_2026-01-28.md`

### **Step 3: Set Options**

- **Base branch**: `dev-deploy`
- **Compare branch**: `QA-Tests`
- **Reviewers**: Add dev team members
- **Labels**: Add `testing`, `models`, `documentation`, `urgent`
- **Projects**: Link to current sprint (if applicable)

### **Step 4: Create Pull Request**

Click **"Create pull request"** button.

---

## 🎯 What's Being Merged

When you merge this PR, it will include:

### **From Today (Jan 28, 2026) - 48 Commits**
- ✅ 3,820 integration tests (test marathon)
- ✅ 7 model namespaces (DEF-005 Phase 1)
- ✅ Test infrastructure cleanup (QA-006)
- ✅ Test execution results
- ✅ Comprehensive documentation

### **From Previous Work** (if not yet merged)
- Playwright tests (Jan 27)
- Other test improvements
- Previous documentation

**Total**: All commits from `QA-Tests` branch that aren't in `dev-deploy`

---

## 🚨 Important Notes

### **Your PR Now Contains Multiple Sessions**

If there's an existing PR that hasn't been merged yet, it now includes:
1. **Old work** (previous sessions)
2. **New work** (today's 48 commits)

This is **normal and expected** - GitHub PRs track the entire branch.

### **Updating the Description is Critical**

Make sure to update the PR description to reflect **today's major work**:
- 3,820 tests created
- 7 models created
- Infrastructure cleanup
- Defects documented

The old description (if it exists) was focused on different work.

---

## ✅ Verification Steps

After creating/updating the PR:

1. **Check commit count**: Should show 48+ commits (depending on previous work)
2. **Check files changed**: Should show test files, models, documentation
3. **Check description**: Should mention "3,820 tests" and "DEF-005 Phase 1"
4. **Check labels**: Should have `testing`, `models`, `documentation`

---

## 📞 Need Help?

**Documentation File**: `PR_DESCRIPTION_2026-01-28.md` (full description)  
**This Guide**: `HOW_TO_UPDATE_PR.md` (instructions)  
**GitHub URL**: `https://github.com/UNOPS-ITG/opportunityplus/pulls`

---

## 🎯 Quick Commands

**Check PR status from terminal**:
```powershell
# See your remote branch
git log origin/QA-Tests --oneline -5

# See what's different from dev-deploy
git log origin/dev-deploy..origin/QA-Tests --oneline
```

**View your PR**:
```
https://github.com/UNOPS-ITG/opportunityplus/pulls
```

**Create new PR**:
```
https://github.com/UNOPS-ITG/opportunityplus/compare/dev-deploy...QA-Tests
```

---

## ✅ Status

🎉 **All work committed and pushed**  
✅ **PR description ready** (`PR_DESCRIPTION_2026-01-28.md`)  
✅ **Instructions ready** (this file)  
⏳ **Next step**: Create or update PR on GitHub
