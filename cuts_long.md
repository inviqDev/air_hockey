# Codex Workflow Shortcuts

This file defines exact reusable commands for Codex.

Execute a shortcut only when the user's message exactly matches its command.
The user's latest direct instruction always has higher priority than this file.
Do not read or update `Docs/AI/Project/backlog.md` unless the user explicitly requests backlog work.

Supported commands:

- `__?!`
- `_prep!!`
- `_start!!`
- `_audit!!`
- `_diffaudit!!`
- `_recheck!!`
- `_done!!`
- `_close!!`
- `_handoff!!`

---

## Shortcut: `__?!`
Use when the user says exactly: `__?!`

### Purpose - Show a compact reference list of every shortcut currently defined in this file.

### Actions
1. Read the current root `cuts.md`.
2. Identify every section declared as `## Shortcut:`.
3. Preserve each shortcut's exact spelling, underscores, question marks, and exclamation marks.
4. Include `__?!` itself in the result.
5. Do not inspect project code, Git state, planning documents, context, or backlog.
6. Do not modify any file.
7. Do not execute any listed shortcut.
8. Do not add commands that are not defined in the current `cuts.md`.

### Response

Return only a numbered list in the same order the shortcuts appear in `cuts.md`.

Format each item as:
`1. <exact shortcut> — <very short purpose>`

The purpose must be a single concise phrase, normally no more than 10 words.

Example format:
1. `_prep!!` — analyze the current step
2. `_start!!` — implement the approved approach

Do not add an introduction, explanation, recommendation, footer, or next-step suggestion.

---

## Shortcut: `_prep!!`
Use when the user says exactly: `_prep!!`

### Purpose - Analyze the step marked `Current` in `Docs/AI/Project/master_plan.md` before implementation.

### Actions
1. Identify the current master-plan step.
2. Read only the relevant sections of:
   - `Docs/AI/Project/context.md`;
   - relevant subsystem context;
   - relevant coding, architecture, and workflow rules.
3. Inspect the actual code, assets, and runtime paths needed to understand the step.
4. Do not modify files.
5. Do not analyze or implement later master-plan steps.
6. Prefer the smallest safe and understandable implementation direction.

### Report
1. Current behavior relevant to the step.
2. Concrete missing behavior or intended outcome.
3. Ownership and dependency flow.
4. Proposed implementation approach.
5. Expected files to change or be created.
6. Risks and assumptions.
7. Required Unity verification.
8. Concise suggested Git branch name.

Wait for explicit user approval before implementation.

---

## Shortcut: `_start!!` - Use when the user says exactly: `_start!!`

### Meaning
The user approves the most recently proposed implementation approach and authorizes implementation.
If no clear approved approach exists in the current conversation, stop and ask the user to restate or confirm it.

### Actions
1. Confirm the active master-plan step.
2. Follow only the approved implementation approach.
3. Read only relevant instruction files and project files.
4. Modify only files required for the approved scope.
5. Keep the implementation small, readable, and understandable.
6. Do not implement later steps or unrelated refactors.
7. Do not update master-plan status.
8. Do not perform Git operations or begin finalization automatically.

### Report

1. What changed.
2. Files changed or created.
3. Resulting runtime flow.
4. Ownership, dependency, API, or lifecycle changes.
5. Risks and assumptions.
6. Exact Unity verification steps.
7. Anything not verified outside the Unity Editor.

Wait for `_audit!!`.

---

# Shared Deep Audit Standard - Use this standard for both `_audit!!` and `_diffaudit!!`.

Inspect the actual diff, changed production files, relevant unchanged call sites, ownership paths, lifecycle behavior, Unity wiring, assets, configuration, generated files, and serialization dependencies.
Do not rely only on an implementation summary.

For every meaningful changed class, method, component, dependency, public API, event, collection, cache, state field, lifecycle callback, fallback, and repeated operation, evaluate:
- purpose and current requirement;
- ownership and source of truth;
- execution frequency and lifecycle;
- assumptions and failure behavior;
- correctness and edge cases;
- simpler correct alternatives;
- proportionality of complexity;
- consistency with the surrounding subsystem;
- regression and maintenance impact.

Review for:
- incorrect state transitions, stale state, duplicate sources of truth, reentrancy, and partial initialization;
- unclear ownership, mixed responsibilities, excessive coupling, hidden dependencies, and unnecessary public APIs;
- avoidable per-frame work, repeated refresh, lookups, allocations, LINQ, closures, boxing, copying, and logging;
- Unity lifecycle, pooling, restart, reuse, scene transition, and serialization risks;
- duplicate or stale subscriptions, missing cleanup, and participant or instance cross-wiring;
- fragile names, hierarchy assumptions, magic strings, child order, and runtime discovery of required dependencies;
- ambiguous contracts, silent failure, misleading fallbacks, weak diagnostics, insufficient validation, and excessive defensive code;
- unnecessary abstractions, wrappers, services, interfaces, caches, flags, generic systems, and speculative extension points;
- poor naming, hidden mutation, duplication, excessive fragmentation, dead code, and misleading comments;
- deterministic ordering, identifiers, equality, collection integrity, and shared ScriptableObject mutation;
- input, UI, gameplay, prefab, scene, generated-file, restart, and pooling regressions;
- scope creep, future-step work, unrelated refactors, diagnostics, and accidental asset changes.

Do not mechanically require:
- replacing every poll with an event;
- removing every allocation;
- adding validation everywhere;
- splitting every large class;
- adding interfaces for every dependency;
- adding caching, pooling, or zero-allocation infrastructure without evidence.

Classify findings as:
- blocking correctness issue;
- justified cleanup before `_recheck!!`;
- acceptable current tradeoff;
- future profiling or refactor item.

An audit may apply blocking fixes, small justified cleanups, and removal of unnecessary complexity introduced by the audited work.
It must not start unrelated work, silently redesign approved architecture, update roadmap status, finalize work, or perform Git operations.
If a larger redesign is needed, report it and wait for user direction.

---

## Shortcut: `_audit!!` - Use when the user says exactly: `_audit!!`

### Scope
Apply the Shared Deep Audit Standard only to the implementation for the current master-plan step.
Review the actual current diff and all relevant runtime paths.
Do not include unrelated pre-existing changes.

### Report
1. Files and runtime paths reviewed.
2. Blocking issues.
3. Justified cleanups.
4. Runtime-work and allocation findings.
5. Ownership, state, lifecycle, and subscription risks.
6. Contract, validation, and failure-behavior risks.
7. Unity, serialization, prefab, scene, asset, and generated-file risks.
8. Complexity, naming, readability, and maintenance findings.
9. Regression and scope risks.
10. Exact fixes applied.
11. Larger work deliberately deferred.
12. Manual Unity verification still required.
13. Whether the implementation is ready for `_recheck!!`.

If relevant production or Unity files change after `_audit!!`, the audit becomes stale.

---

## Shortcut: `_diffaudit!!` - Use when the user says exactly: `_diffaudit!!`

### Purpose
Audit the entire current Git working-tree change set, regardless of the active master-plan step.
This shortcut is standalone and does not advance or finalize master-plan work.

### Scope

1. Inspect:
   - `git status --short --branch`;
   - unstaged diff;
   - staged diff;
   - relevant untracked files.
2. Apply the Shared Deep Audit Standard to all current changes relative to `HEAD`.
3. Do not assume all changes were created by the latest Codex action.
4. Report uncertainty about authorship or pre-existing changes.
5. Inspect unchanged code only when needed to understand the current diff.
6. Do not turn the task into a repository-wide audit.

### Modifications

A clean file may be changed only when a small direct fix is required to make the current diff correct.
Report any expansion of the change set.
Do not perform commits, pushes, branch switches, resets, rebases, merges, roadmap updates, or unrelated cleanup.

### Report
1. Repository state and exact change set audited.
2. Staged, unstaged, and untracked files reviewed.
3. Relevant runtime and ownership paths.
4. Blocking issues and justified cleanups.
5. Runtime, allocation, lifecycle, subscription, contract, naming, and failure-behavior findings.
6. Unity, serialization, prefab, scene, asset, and generated-file risks.
7. Complexity, regression, and accidental-change risks.
8. Exact fixes applied.
9. Larger or unrelated work deliberately deferred.
10. Manual Unity verification required.
11. Whether the current change set is acceptable for the user's next intended action.

If relevant files change after `_diffaudit!!`, the audit becomes stale.
If `_diffaudit!!` changes implementation-affecting files, any earlier `_audit!!`, `_recheck!!`, or `_done!!` covering those files becomes stale.

---

## Shortcut: `_recheck!!` - Use when the user says exactly: `_recheck!!`

### Purpose
Perform the final read-only check of all current uncommitted changes belonging to the active master-plan step.

### Actions
1. Confirm the active master-plan step.
2. Inspect Git status and the relevant diff.
3. Compare the implementation with:
   - the active master-plan step;
   - the approved approach;
   - relevant project instructions;
   - existing architecture.
4. Do not modify files.
5. Do not review unrelated pre-existing changes.
6. Do not advance the master plan.

### Check For
- scope creep or missing behavior;
- incorrect ownership or duplicated state;
- unnecessary dependencies or abstractions;
- lifecycle, cleanup, subscription, pooling, pause, and restart risks;
- Unity serialization and Inspector risks;
- naming and consistency problems;
- regressions;
- missing verification coverage;
- work belonging to a later step.

### Report
Separate:
- blocking issues;
- non-blocking issues;
- optional future improvements;
- manual Unity verification still required.

If no meaningful issue exists, state that clearly.
If implementation-affecting files change after `_recheck!!`, both `_audit!!` and `_recheck!!` become stale.
After `_recheck!!`, the user must complete the required Unity Editor and Play Mode verification before `_done!!`.

---

## Shortcut: `_done!!` - Use when the user says exactly: `_done!!`

### Meaning
This command means:
- the user personally completed the required Unity Editor and Play Mode verification;
- the user confirms that the full active master-plan step works correctly;
- the user accepts the runtime result;
- the full active step is ready for finalization.

Do not use `_done!!` for an unfinished slice when the active master-plan step is still in progress.
Codex does not perform or simulate Unity verification.

### Preconditions
- `_audit!!` completed successfully;
- `_recheck!!` completed successfully;
- no blocking issue remains;
- no implementation-affecting files changed after `_recheck!!`;
- the user verified the full active step, not only an incomplete slice.

### Actions
- accept the user-reported verification;
- do not modify files;
- do not update the master plan;
- do not perform Git operations;
- do not start the next step.

### Response
Briefly confirm:
- Unity verification was accepted;
- the full active step is ready for finalization;
- the next command is `_close!!`.

---

## Shortcut: `_close!!` - Use when the user says exactly: `_close!!`

### Meaning
Finalize the entire active master-plan step.
This command must not be used to close only one unfinished slice of a still-active step.

### Preconditions
- `_audit!!` completed successfully for the final step implementation.
- `_recheck!!` completed successfully.
- `_done!!` completed successfully.
- The user confirmed that the entire active step is complete and accepted.
- No implementation-affecting files changed after `_done!!`.

### Actions
1. Identify the step marked `Current`.
2. Inspect final Git status and the relevant diff.
3. Confirm that the complete change set belongs to the full active step.
4. Do not modify gameplay code during finalization.
5. Update `Docs/AI/Project/master_plan.md`:
   - mark the current step `Done`;
   - mark the next planned step `Current`;
   - update Current Position;
   - do not start the next step.
6. Update `Docs/AI/Project/context.md` and relevant subsystem context only for durable changes to architecture, ownership, dependency flow, lifecycle, stable behavior, or confirmed decisions.
7. Do not add temporary notes, chronological history, or full change logs to durable context.
8. Update `Docs/AI/Project/current_handoff.md` with the resulting current position and exact next action.
9. Do not read or update backlog unless explicitly requested.
10. Do not commit, push, switch branches, create a PR, or start the next step unless explicitly requested.

### Commit and Pull Request Summary

Provide:
1. One short imperative commit subject.
2. A numbered list of only the most important completed changes.

Do not include deferred work, unchanged behavior, low-value file noise, branch names, or commit hashes.

### Report

1. Completed master-plan step.
2. Next step marked `Current`.
3. Documentation files updated.
4. Final working-tree files.
5. Commit subject.
6. Numbered commit or PR summary.
7. Remaining known limitations or verification notes.
8. Confirmation that the next step was not started.
9. Which ChatGPT Project sources became stale and should be replaced.

---

## Shortcut: `_handoff!!` - Use when the user says exactly: `_handoff!!`

### Purpose
Create a compact continuity snapshot for ChatGPT and a future Codex conversation.

This shortcut is standalone and works for:
- active master-plan work;
- a narrow slice inside a step;
- side work unrelated to the current master plan;
- debugging, refactoring, documentation, or architecture discussion;
- clean or dirty working trees;
- important decisions or approved approaches made without file changes.

### Information Boundary

Record information available in the current Codex conversation and workspace.
If an important decision was made only outside the current Codex conversation, the user must provide that decision before invoking `_handoff!!`.

### Actions
1. Read only the planning, context, and instruction files relevant to the current work.
2. Inspect:
   - current branch and upstream relation when available;
   - `git status --short --branch`;
   - staged, unstaged, and relevant untracked changes;
   - applicable master-plan position.
3. Summarize the current Codex conversation, including important decisions, approved approaches, unresolved questions, and exact next actions, even when Git is clean.
4. Determine the latest implementation, audit, recheck, compile/import, and user-verified state.
5. Create `Docs/AI/Project/current_handoff.md` if it does not exist.
6. Replace its previous content instead of appending history.
7. Modify no other files.
8. Do not claim compilation or Unity verification unless explicitly recorded.
9. Do not assume the work belongs to the master plan; describe standalone or side work accurately.
10. Do not advance roadmap status or perform Git operations.

### Handoff Content

Record:
1. Timestamp.
2. Branch, upstream, and working-tree state.
3. Current initiative, master-plan step, or standalone task.
4. Current narrow goal and explicit exclusions.
5. Important conversation decisions and approved approach.
6. Confirmed ownership and runtime path relevant to resuming.
7. Changed files and their purpose, or explicitly state that the tree is clean.
8. Audit, recheck, compile/import, and Unity verification state.
9. Remaining blockers, uncertainties, unresolved questions, and documentation drift.
10. Exact next action.
11. Actions that must not happen automatically.
12. ChatGPT Project sources that are now stale and should be replaced.

### Response

Return:
- brief confirmation that `current_handoff.md` was created or replaced;
- one compact `Paste into ChatGPT` block;
- one compact `Resume in Codex` block.

Do not commit, push, switch branches, start another task, or advance the master plan.
