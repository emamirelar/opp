# Onboarding Guide — Joining a Shift-Left Team

**Version:** 1.2  
**Date:** March 9, 2026  
**Status:** Active  
**Audience:** New QA testers joining the team; new developers who need to understand how to work with QA in the shift-left model  
**Prerequisites:** Read the [Shift-Left Testing Manifesto](SHIFT_LEFT_TESTING_MANIFESTO.md) first. It explains what shift-left means, who does what, and why.

---

## Table of Contents

1. [Welcome — What You Need to Know on Day 1](#1-welcome--what-you-need-to-know-on-day-1)
2. [The 30-60-90 Day Framework](#2-the-30-60-90-day-framework)
3. [The Buddy System](#3-the-buddy-system)
4. [Shadowing and Reverse Shadowing](#4-shadowing-and-reverse-shadowing)
5. [QA Tester Onboarding Track](#5-qa-tester-onboarding-track)
6. [Developer Onboarding Track — Working with QA](#6-developer-onboarding-track--working-with-qa)
7. [Your First Sprint](#7-your-first-sprint)
8. [Common Questions from New Hires](#8-common-questions-from-new-hires)
9. [Onboarding Checklists](#9-onboarding-checklists)
10. [Related Documents](#10-related-documents)

---

## 1. Welcome — What You Need to Know on Day 1

You are joining a team that is adopting a "shift-left" testing approach. This means quality is built into the development process from the start, not bolted on at the end. Here is the one-paragraph version:

> **QA testers** write automated test specs early in the sprint using Cursor and Claude AI. **Developers** run those tests on their machines before creating Pull Requests. QA then performs exploratory testing and end-to-end regression on the QA environment. Defects caught early cost minutes to fix; defects caught late cost days.

You do not need to understand everything on Day 1. The 90-day plan below gives you a structured path from "observing" to "independently contributing." Your buddy will help you through the rough spots.

### The Advantage of Being New

The team is simultaneously adopting shift-left practices, transitioning from Katalon to Playwright, and learning AI-assisted test authoring. As a new joiner, you have no legacy habits to unlearn. You are learning the new way from Day 1. The rest of the team is transitioning — you are starting fresh.

Your questions are valuable. Every question that cannot be answered by existing documentation is a gap the team should close. Do not hesitate to ask.

---

## 2. The 30-60-90 Day Framework

### Overview

| Phase       | Days  | Theme                   | You will go from...                    | ...to                                    |
|-------------|-------|-------------------------|----------------------------------------|------------------------------------------|
| Foundations | 1-30  | "Learn the System"      | Setting up tools, reading docs         | Running tests independently, first commit |
| Collaboration | 31-60 | "Contribute with Support" | Paired work with buddy oversight     | Authoring tests for real stories          |
| Ownership   | 61-90 | "Work Independently"    | Taking on work with guidance           | Owning test coverage for stories end-to-end |

### Days 1-30: Foundations

**Goal:** Understand the system, the tools, and the process. Make your first small contribution.

#### Week 1: Setup and Orientation

| Day | Activity                                                         | Output                                |
|-----|------------------------------------------------------------------|---------------------------------------|
| 1   | Environment setup (see [Onboarding Checklists](#9-onboarding-checklists)) | All tools installed, repo cloned, tests run |
| 1   | Meet your buddy — introductory 30-minute call                    | Buddy relationship established         |
| 1   | Read the Shift-Left Manifesto (Sections 1-3)                    | Understand the testing pyramid and who does what |
| 2   | Read the QA Tester Playbook (Sections 1-3)                      | Understand the QA lifecycle            |
| 2   | Read the Playwright Quickstart (Sections 1-4)                   | Understand what Playwright is and how tests work |
| 3   | **Early win:** Run the Playwright test suite locally and screenshot the results (~1,629 tests across 108 specs) | Confidence that the environment works |
| 3   | **Early win:** Run `dotnet test --filter "Category=Smoke"` and screenshot results (~10,040 total C# test methods) | Familiarity with the C# test runner   |
| 4   | Walk through the application with buddy — all main features     | Mental map of the product              |
| 5   | Review one recent Jira story end-to-end: ticket, PR, tests, deployment | Understand the full workflow         |

**Why "early wins" matter:** Completing a small, concrete task in the first week builds confidence and confirms that your environment is working. It also gives you something tangible to discuss in standup.

#### Week 2: Observe and Shadow

| Activity                                                          | Output                                |
|-------------------------------------------------------------------|---------------------------------------|
| Shadow your buddy during an exploratory testing session            | See how experienced testers think      |
| Attend Three Amigos as an observer (do not feel pressure to speak) | See how QA, Dev, and PO collaborate    |
| Watch your buddy write a test spec with Claude AI                 | See the AI-assisted workflow in action |
| Review a recent post-mortem with buddy                            | Learn about the system's weak points   |

#### Week 3: First Contributions

| Activity                                                          | Output                                |
|-------------------------------------------------------------------|---------------------------------------|
| Write your first test spec with Claude, paired with your buddy    | First code committed to the repo       |
| Run QA integration tests for a feature area and report results    | Familiarity with `dotnet test` filtering |
| Ask one "what if?" question in a Three Amigos session             | First contribution to edge case thinking |

#### Week 4: Growing Independence

| Activity                                                          | Output                                |
|-------------------------------------------------------------------|---------------------------------------|
| Author a simple Playwright E2E test with Claude assistance        | First Playwright spec committed        |
| Run the full Playwright suite and review the HTML report          | Understand test reporting              |
| Participate in standup: report what you tested, what you found    | Team sees you as a contributing member |

**Day 30 Milestone:** You can navigate the codebase, run all test suites independently, and have committed at least one test spec to the repo.

### Days 31-60: Collaboration

**Goal:** Contribute real work on real stories, with buddy oversight.

| Week | Activity                                                         | Output                                |
|------|------------------------------------------------------------------|---------------------------------------|
| 5    | Author integration test specs for a low-risk story (buddy reviews before push) | First solo test spec for a real story |
| 6    | Perform your first solo exploratory testing session (30 minutes, with a charter) | Session notes logged in Jira          |
| 6    | Log your first defect in Jira with a DEF-XXX entry in the defect list | Understanding of defect workflow       |
| 7    | Author a Playwright E2E test for a completed feature             | Growing E2E test ownership            |
| 7    | Pair with a developer to walk them through running your tests    | QA/Dev collaboration experience        |
| 8    | Review another tester's test spec and provide feedback           | Peer review skills developing          |

**Day 60 Milestone:** You are independently authoring test specs for low-to-medium risk stories with buddy oversight. You have logged at least one defect and performed at least two exploratory testing sessions.

### Days 61-90: Ownership

**Goal:** Work independently on story-level test coverage.

| Week  | Activity                                                         | Output                                |
|-------|------------------------------------------------------------------|---------------------------------------|
| 9     | Own test spec authoring for a full story end-to-end (integration + E2E) | Full test ownership for a story       |
| 10    | Lead an exploratory testing session (write the charter, execute, report) | Testing leadership experience         |
| 10    | Create an edge case checklist for Three Amigos without buddy help | Independent analytical contribution    |
| 11    | Present test coverage and defect summary at sprint demo          | Visibility with the wider team         |
| 12    | Contribute to a post-mortem as an active participant             | Process improvement contribution       |
| 12    | Mentor: explain one process or tool to a colleague (teaching cements learning) | Knowledge multiplication              |

**Day 90 Milestone:** You are a fully contributing team member operating within the shift-left model without daily supervision. Your buddy relationship transitions from structured to informal.

---

## 3. The Buddy System

Every new team member is assigned a buddy — a peer (not a manager) who serves as a safe point of contact for tactical and cultural questions. The buddy is not your supervisor and does not evaluate your performance. They are your go-to person for "how do we actually do this?" questions.

### Buddy vs. Manager Responsibilities

| Aspect             | Buddy                                                      | Manager / QA Lead                  |
|--------------------|------------------------------------------------------------|------------------------------------|
| Tactical help      | "How do I run the Playwright tests?" "Where is this config?" | Career goals, performance reviews |
| Cultural norms     | "How do we usually handle this?" "Is this the right channel?" | Team structure, process decisions |
| Code/test review   | Informal review before formal PR/push                      | Not involved in code-level review  |
| Availability       | Quick questions anytime (Slack/Teams, desk)                | Scheduled 1:1s                     |
| Duration           | Structured: first 60 days. Informal: ongoing.              | Ongoing                           |

### Buddy Selection Criteria

- Same role: QA buddy for QA hire, Dev buddy for Dev hire
- Has been on the team for at least 3 months
- Willing to invest 30-60 minutes per day in weeks 1-2, tapering to 15 minutes/day by week 4
- Patient, approachable, and willing to explain "obvious" things without judgment

### What Makes a Good Buddy

**Do:**
- Check in proactively: "How are you doing? Stuck on anything?"
- Answer questions without making the new hire feel like they should already know
- Document answers to common questions (they will come up again with the next new hire)
- Celebrate early wins: "Great, your first test spec is committed!"

**Do not:**
- Overwhelm with information on Day 1 — drip-feed context as it becomes relevant
- Do the work for them — guide, do not take over
- Skip the reverse shadowing step — it is how you confirm understanding
- Assume silence means everything is fine — check in

---

## 4. Shadowing and Reverse Shadowing

### Shadowing (Weeks 2-3): Watch and Learn

The new hire watches a senior team member perform their shift-left activities. The goal is to see the full workflow in context, not to memorize steps.

**For QA testers, shadow these activities:**

| Activity                              | What to observe                                    | Questions to ask                       |
|---------------------------------------|----------------------------------------------------|-----------------------------------------|
| Writing a test spec with Claude AI    | How the tester describes scenarios to Claude, how they review the output, what they change | "How do you decide what to test?" "How do you know if the AI output is correct?" |
| Running Playwright tests              | How the tester runs tests, reads results, interprets failures | "What do you do when a test fails?" "How do you know if it is a real bug vs. a test issue?" |
| Exploratory testing session           | How the tester chooses what to explore, how they think about edge cases, how they log findings | "How do you decide where to explore?" "What makes a good session charter?" |
| Three Amigos session                  | How QA asks "what could go wrong?", how edge cases are surfaced, how the checklist is built | "How do you come up with edge cases?" "What patterns do you look for?" |
| Logging a defect                      | How the tester writes repro steps, assigns severity, links to Jira, updates the defect list | "How do you decide the severity?" "What makes a good defect report?" |

### Reverse Shadowing (Weeks 4-5): Drive and Be Coached

The new hire performs the activities while the buddy watches and provides real-time feedback. This is where you discover whether understanding has truly transferred.

**For QA testers, reverse-shadow these activities:**

| Activity                              | The new hire does...                               | The buddy watches for...               |
|---------------------------------------|----------------------------------------------------|-----------------------------------------|
| Writing a test spec with Claude AI    | Describes scenarios, prompts Claude, reviews and adjusts the output | Does the tester think about edge cases? Do they accept AI output uncritically or review it? |
| Running tests and interpreting results | Runs the test suite, identifies failures, determines if they are real bugs or test issues | Can they read error messages? Do they know where to look for details? |
| Exploratory testing                   | Writes a charter, explores for 30 minutes, logs findings | Are they creative? Do they go beyond the happy path? |
| Creating an edge case checklist       | Writes the checklist for a story without prompting | Are the edge cases relevant and non-obvious? |

**The reverse shadowing step is critical.** It is the difference between "I watched someone do it" and "I can do it myself." Do not skip it.

---

## 5. QA Tester Onboarding Track

This section is the detailed learning path for new QA testers. It builds on the 30-60-90 framework with specific skills and milestones.

### Skill Progression

```
Week 1-2          Week 3-4           Week 5-6          Week 7-8          Week 9-10
─────────          ─────────          ─────────          ─────────          ─────────
Cursor + Git       Playwright         AI-Assisted        C# Integration     Advanced
Basics             Fundamentals       E2E Authoring      Test Authoring     Topics

Can open IDE       Can run tests      Can describe a     Can describe an    Can debug a
Can commit a       Can read a test    scenario to        integration test   failing test
file               Can write a        Claude and get     scenario and get   Can read CI
Can push a         basic test with    a working spec     a working spec     logs
branch             a buddy                                                  Can update
                                                                           selectors
```

### Required Reading (In Order)

| Priority | Document                          | When to Read    | Time      |
|----------|-----------------------------------|-----------------|-----------|
| 1        | This Onboarding Guide             | Day 1           | 15 min    |
| 2        | Shift-Left Testing Manifesto      | Day 1           | 30 min    |
| 3        | QA Tester Playbook (Sections 1-7) | Days 1-3        | 60 min    |
| 4        | Playwright Quickstart for Testers | Week 2          | 45 min    |
| 5        | QA Tester Playbook (Sections 8-16) | Weeks 3-4      | 60 min    |
| 6        | Defect Management Standard        | Week 4          | 20 min    |
| 7        | Test Ratio Enforcement Rule       | Week 5          | 10 min    |

### Key Concepts to Understand by Day 30

| Concept                         | Why it matters                                       | Where to learn it                      |
|---------------------------------|------------------------------------------------------|----------------------------------------|
| The testing pyramid             | Know which tests are yours and which are the developer's | Manifesto, Section 2                  |
| Three Amigos and edge case checklists | Your primary shift-left activity before coding starts | Manifesto, Section 6                  |
| "QA writes tests early, dev runs them" | The core shift-left handshake                     | Manifesto, Sections 4-5               |
| Quality gates (Pre-Commit, PR, QA Handoff) | The three checkpoints code must pass              | Manifesto, Section 7                  |
| DEF-XXX vs. QA-XXX defect system | Know which defect list to use and when                | Manifesto, Section 8                  |
| The 3:1 test ratio standard     | Every test suite must have 3x more negative/edge/functional/integration tests than positive | Playbook, Section 9 |
| Never weaken tests              | If a test fails, log a defect — do not change the assertion to make it pass | Manifesto, Section 4       |
| QA write boundaries             | Never modify production source code — only touch files in QA Tests/ | Playbook / Cursor rules    |

---

## 6. Developer Onboarding Track — Working with QA

This section is for developers joining the team. It does not cover general development onboarding (IDE setup, architecture, coding standards) — that is covered by the project README and development documentation. This section focuses specifically on how developers work with QA under the shift-left model.

### What Developers Need to Know About QA

1. **QA writes test specs early in the sprint.** By Day 3-5, QA will push integration and E2E test specs to `QA Tests/`. You are expected to pull and run them before creating your PR.

2. **Running QA tests is part of your Definition of Done.** The specific commands are in the [Developer Testing Contract](SHIFT_LEFT_TESTING_MANIFESTO.md#4-the-developer-testing-contract) section of the manifesto.

3. **If a QA test fails, fix your code — not the test.** If you believe the test expectation is wrong, discuss it with QA. Do not silently change assertions.

4. **Edge case checklists are attached to your Jira tickets.** After Three Amigos, QA attaches a checklist of edge cases. Review it and ensure your unit tests cover them.

5. **CI is the enforced gate.** Even if you skip running tests locally, CI will catch failures. But running locally is faster feedback — for you.

### Developer's First Sprint Checklist

- [ ] Read Manifesto Section 4 (Developer Testing Contract)
- [ ] Run `dotnet test --filter "Category=Smoke"` and confirm it passes
- [ ] Run QA integration tests for your story's feature area
- [ ] Attend Three Amigos and listen to how QA thinks about edge cases
- [ ] Review the edge case checklist on your Jira ticket
- [ ] Include "QA tests run: [pass/fail]" in your PR description
- [ ] If a QA test fails, discuss with QA before the PR

### Common Developer Questions

**Q: Do I need to learn Playwright?**  
A: No. Playwright E2E tests are 100% QA-owned. You run C# tests using `dotnet test`.

**Q: What if QA tests are not ready when I need them?**  
A: Continue with your unit tests and raise it in standup. QA will prioritize.

**Q: What if I think a QA test is wrong?**  
A: Talk to QA. You may be right, or the test may be catching a real bug. Either way, do not change the test yourself.

**Q: Will this slow me down?**  
A: Running smoke tests takes ~30 seconds. Running feature-specific integration tests takes 1-5 minutes. This is far less time than the 2+ hours of rework when QA finds the bug 3 days later.

---

## 7. Your First Sprint

Here is what a typical 2-week sprint looks like from the perspective of a new team member. In your first sprint, you will mostly observe and do paired work. By your third sprint, you will be doing most of this independently.

### For a New QA Tester

| Sprint Day | Activity                                                         | Your Role (First Sprint) |
|------------|------------------------------------------------------------------|--------------------------|
| Day 1      | Sprint Planning — QA identifies test scenarios per story         | Observe, take notes      |
| Day 1-2    | Three Amigos — QA asks "what could go wrong?"                    | Observe, ask one question |
| Day 3-5    | QA authors test specs with Claude AI, pushes to repo             | Pair with buddy, co-author one spec |
| Daily      | Standup — QA reports test authoring progress                     | Report what you observed/learned |
| Day 6-8    | Dev marks "Ready for QA" — QA does exploratory testing           | Shadow buddy's exploratory session |
| Day 8-9    | Bug triage — QA Lead + Dev Lead review open defects              | Observe, learn severity classification |
| Day 10     | Sprint demo — QA presents test metrics and defect summary        | Watch, understand the format |
| Day 10     | Retrospective — team discusses shift-left friction               | Share one observation from your fresh perspective |

### For a New Developer

| Sprint Day | Activity                                                         | Your Role (First Sprint) |
|------------|------------------------------------------------------------------|--------------------------|
| Day 1      | Sprint Planning — agree on acceptance criteria with QA           | Participate, listen to QA input |
| Day 1-2    | Three Amigos — hear edge cases from QA                           | Ask clarifying questions  |
| Day 3-8    | Development — write code with unit tests                         | Write code, run QA tests before PR |
| Day 6-8    | Mark story "Ready for QA" — list which QA tests you ran          | Follow the checklist in Section 6 |
| Day 8-9    | Bug triage — review defects found by QA                          | Participate, own one defect fix |
| Day 10     | Sprint demo + Retrospective                                      | Share your shift-left experience |

---

## 8. Common Questions from New Hires

**Q: I am not a coder. How can I write automated tests?**  
A: You describe what to test in plain English. Claude AI writes the code. You review it, run it, and commit it. You are the test designer — Claude is the typist. Your value is knowing what to test, not how to code it.

**Q: What if Claude writes something wrong?**  
A: Claude's output should always be reviewed. Run the test. If it passes for the wrong reason or tests the wrong thing, adjust the prompt and try again. Your buddy can help you learn what "good" looks like.

**Q: I feel overwhelmed by all the tools.**  
A: Focus on one tool at a time, in this order: (1) Cursor IDE, (2) Git basics, (3) Playwright, (4) Claude AI for tests. The 90-day plan spaces these out intentionally.

**Q: What if I break something?**  
A: You are working in `QA Tests/` — you cannot break production code. The worst case is a test that does not compile or gives wrong results, which is caught in CI. This is a safe space to learn.

**Q: How long until I am "up to speed"?**  
A: By Day 30, you should be able to run tests independently and make small contributions. By Day 60, you should be authoring tests for real stories with buddy support. By Day 90, you should be working independently. Everyone learns at their own pace — the 90-day framework is a guide, not a deadline.

**Q: What if my buddy is busy?**  
A: Your buddy has committed to being available. If they are consistently unavailable, raise it with the QA Lead. In the meantime, other team members can help — the whole team benefits from a well-onboarded colleague.

**Q: What was the team doing before shift-left?**  
A: Previously, developers wrote code and handed it to QA for all testing. QA found bugs, logged them, and developers context-switched back to fix them. This was slow and expensive. Shift-left means QA writes tests earlier, developers run them before handoff, and QA focuses on exploratory and E2E testing.

---

## 9. Onboarding Checklists

### Day 1 Checklist — QA Tester

**Environment Setup:**
- [ ] Cursor IDE installed and configured
- [ ] Git access to the repository verified (clone, branch, push)
- [ ] Jira access with correct project permissions
- [ ] Node.js 20+ installed
- [ ] `npm install` completed in `QA Tests/` folder
- [ ] Playwright browsers installed (`npx playwright install`)
- [ ] .NET SDK installed
- [ ] Can run `npx playwright test home.spec.ts --project=chromium` successfully
- [ ] Can run `dotnet test --filter "Category=Smoke"` successfully

**Access and Accounts:**
- [ ] GitHub account added to the repository with correct permissions
- [ ] Jira account with access to the project board
- [ ] QA environment URL and credentials provided
- [ ] Staging environment URL and credentials provided
- [ ] Team communication channel (Slack/Teams) joined

**People:**
- [ ] Buddy assigned and introductory meeting completed
- [ ] QA Lead 1:1 scheduled (first week)
- [ ] Introduced to the development team

**Reading List Delivered:**
- [ ] This Onboarding Guide
- [ ] Shift-Left Testing Manifesto
- [ ] QA Tester Playbook
- [ ] Playwright Quickstart for Testers

### Day 1 Checklist — Developer

**Environment Setup:**
- [ ] Cursor IDE installed and configured
- [ ] Git access to the repository verified (clone, branch, push)
- [ ] Jira access with correct project permissions
- [ ] .NET SDK installed, solution builds without errors
- [ ] PostgreSQL client installed (pgAdmin or DBeaver)
- [ ] Can run `dotnet test --filter "Category=Smoke"` successfully
- [ ] Angular CLI and Node.js installed, frontend builds without errors

**Access and Accounts:**
- [ ] GitHub account added to the repository with correct permissions
- [ ] Jira account with access to the project board
- [ ] Dev environment URL and credentials provided
- [ ] Team communication channel (Slack/Teams) joined

**People:**
- [ ] Buddy assigned and introductory meeting completed
- [ ] Dev Lead 1:1 scheduled (first week)
- [ ] Introduced to the QA team (you will be working closely with them)

**Reading List Delivered:**
- [ ] This Onboarding Guide (Section 6 specifically)
- [ ] Shift-Left Testing Manifesto (Section 4 specifically)
- [ ] Project README and architecture overview

### 30-Day Check-In Template

The QA Lead or manager uses this template for the Day 30 check-in:

| Question                                                | Expected Answer                              |
|---------------------------------------------------------|----------------------------------------------|
| Can you run all test suites independently?               | Yes — Playwright, smoke, integration         |
| Have you committed at least one test to the repo?        | Yes — with buddy review                      |
| Can you describe the testing pyramid and your role in it? | Understands QA vs. Dev responsibilities      |
| Have you attended Three Amigos?                          | Yes — at least once, asked a question        |
| Do you understand the defect workflow (DEF-XXX)?         | Can explain the lifecycle                    |
| What has been the most challenging part so far?          | Open discussion — identify additional support needs |
| What would have made your first month easier?            | Feedback to improve onboarding for the next hire |

---

## 10. Related Documents

| Document                          | Location                                                   | Purpose                              |
|-----------------------------------|------------------------------------------------------------|--------------------------------------|
| Shift-Left Testing Manifesto      | `QA Tests/Documentation/SHIFT_LEFT_TESTING_MANIFESTO.md`   | Team strategy, role definitions, handshake points |
| Shift-Left Scorecard              | `QA Tests/Documentation/SHIFT_LEFT_SCORECARD.md`           | Measurement criteria, sprint dashboard, maturity model |
| Action Items (Dev + QA)           | `QA Tests/Documentation/ACTION_ITEMS.md`                   | Living to-do list for developers and QA |
| QA Tester Playbook                | `QA Tests/Documentation/QA_TESTER_PLAYBOOK.md`             | Day-to-day QA practices, test categories, templates |
| Playwright Quickstart for Testers | `QA Tests/Playwright Tests/QUICKSTART_FOR_TESTERS.md`      | Playwright setup, running tests, writing tests |
| Testing Structure                 | `QA Tests/Documentation/TESTING_STRUCTURE.md`              | Where tests live in the repository (test counts, CI pipeline) |
| Defect List for Developers        | `QA Tests/Defect List for Developers.md`                   | Production code defects (DEF-XXX) — ~135 open |
| Defect List for QA                | `QA Tests/Defect List for QA.md`                           | Test infrastructure issues (QA-XXX) — ~11 active |

---

## Version History

| Version | Date       | Author     | Changes                    |
|---------|------------|------------|----------------------------|
| 1.0     | 2026-03-05 | QA Lead    | Initial version — extracted and expanded from Shift-Left Manifesto Appendix E |
| 1.1     | 2026-03-06 | QA Lead    | Updated Related Documents with Scorecard, Action Items, and current defect counts |
| 1.2     | 2026-03-09 | QA Lead    | Updated test counts (10,040 C# methods, 1,629 Playwright tests). Updated defect counts (135 open DEF, 11 active QA). Added test data infrastructure context (TestEntityBuilder, Bogus, JSON fixtures, workflow-mocks.helper). |
