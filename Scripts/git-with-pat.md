# Using a GitHub Personal Access Token (PAT) with Git

This guide explains how to use a GitHub Personal Access Token for Git operations by setting the remote URL with your username and token. Use this when password authentication fails (GitHub no longer accepts account passwords for Git over HTTPS).

---

## Prerequisites

- A [GitHub Personal Access Token (PAT)](https://github.com/settings/tokens) with at least the **repo** scope.
- Your GitHub username.

**Remote URL format:**

```
https://USERNAME:TOKEN@github.com/ORGANIZATION/REPO-NAME.git
```

Replace:
- `USERNAME` — your GitHub username
- `TOKEN` — your PAT (e.g. `ghp_xxxxxxxxxxxx`)
- `ORGANIZATION` — e.g. `UNOPS-ITG`
- `REPO-NAME` — e.g. `business-gms-plus`

---

## 1. Cloning a repository

Use the URL with username and token instead of the plain HTTPS URL.

**Standard clone (without PAT):**
```powershell
git clone https://github.com/UNOPS-ITG/business-gms-plus.git
```

**Clone with PAT (use this when auth fails):**
```powershell
git clone https://USERNAME:TOKEN@github.com/UNOPS-ITG/business-gms-plus.git
```

**Example:**
```powershell
git clone https://AnushaSwami:ghp_YourTokenHere@github.com/UNOPS-ITG/business-gms-plus.git
```

Then:
```powershell
cd business-gms-plus
```

The remote `origin` is already set with your credentials, so future `git pull` and `git push` will use the same token until you change the remote URL.

---

## 2. Pulling the latest changes

If the repo is already cloned but authentication is failing, update the remote URL to include your PAT, then pull.

**Step 1 — Set the remote URL with your PAT:**
```powershell
cd path\to\your\repo
git remote set-url origin https://USERNAME:TOKEN@github.com/ORGANIZATION/REPO-NAME.git
```

**Step 2 — Pull:**
```powershell
git pull origin development
```

**Example:**
```powershell
cd C:\Users\anushas_unops\Documents\GMS\business-gms-plus
git remote set-url origin https://AnushaSwami:ghp_YourTokenHere@github.com/UNOPS-ITG/business-gms-plus.git
git pull origin development
```

Use your actual branch name instead of `development` if different (e.g. `main`, `master`).

---

## 3. Updating submodules

After the main repo’s remote is set with the PAT (as above), submodules can be updated so they also use authentication.

**Option A — Update all submodules (recommended):**
```powershell
cd path\to\your\repo
git submodule update --init --recursive
```

**Option B — If submodules still ask for credentials**, set each submodule’s remote URL to use the PAT, then update:

```powershell
cd path\to\your\repo

# List submodules
git submodule status

# For each submodule, set remote and update (replace SUBMODULE_PATH and SUBMODULE_REPO)
git submodule foreach "git remote set-url origin https://USERNAME:TOKEN@github.com/ORGANIZATION/SUBMODULE_REPO.git"
git submodule update --init --recursive
```

**Option C — One submodule at a time:**
```powershell
cd path\to\your\repo\path\to\submodule
git remote set-url origin https://USERNAME:TOKEN@github.com/ORGANIZATION/submodule-repo-name.git
git pull origin main
cd ..\..\..
git submodule update --init --recursive
```

Replace `path\to\submodule` and `submodule-repo-name` with your actual submodule path and repo name.

---

## Quick reference

| Task              | Command |
|-------------------|--------|
| Set remote URL    | `git remote set-url origin https://USERNAME:TOKEN@github.com/ORG/REPO.git` |
| Pull branch       | `git pull origin BRANCH_NAME` |
| Clone with PAT    | `git clone https://USERNAME:TOKEN@github.com/ORG/REPO.git` |
| Submodule update  | `git submodule update --init --recursive` |

---

## Security notes

1. **Do not commit or share your PAT** — treat it like a password.
2. **Do not paste your PAT in chat or docs** — if you do, revoke it on GitHub and create a new one.
3. **Token in remote URL** — it is stored in `.git/config` in plain text. Prefer using Git Credential Manager and the normal HTTPS URL when possible:
   ```powershell
   git remote set-url origin https://github.com/ORG/REPO.git
   git config --global credential.helper manager
   ```
   Then use your username and PAT as the password when Git prompts you.
4. **Revoke and rotate** — if a token may have been exposed, revoke it at [GitHub → Settings → Developer settings → Personal access tokens](https://github.com/settings/tokens) and create a new one.

---

*Document version: 1.0 — Opportunity+ Scripts*
