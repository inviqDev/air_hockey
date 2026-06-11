# AgentWorkflow.md

Workflow rules for AI coding agents working on this Unity project.

## General Behavior

Work like a careful developer, not like a code generator.

Before changing code:

1. Inspect the existing project structure and nearby code.
2. Reuse existing patterns, naming, folders, and conventions where reasonable.
3. Identify the smallest safe change that solves the user’s request.
4. Avoid unrelated rewrites.
5. Avoid changing public APIs unless necessary.
6. Avoid changing prefab/scene structure unless the task asks for it.

After changing code:

1. Summarize what changed.
2. List files changed.
3. Explain why this approach was chosen.
4. Mention risks, assumptions, or follow-up work.
5. Explain how the user can verify the result in Unity.

## Scope Control

Keep changes focused.

Do not:

- rewrite unrelated systems;
- rename unrelated files/classes;
- reformat entire files without need;
- introduce a new architecture for a small bug fix;
- delete existing behavior silently;
- change project settings unless requested;
- modify generated files unless necessary.

If a better larger refactor is visible, mention it as a recommendation instead of doing it automatically.

## Reading Instructions

Always read root `AGENTS.md` first.

Then read additional files only when relevant:

- Unity gameplay / scene / prefab / input / UI / physics / pooling / performance work:
  - `Docs/AI/UnityCodingRules.md`

- architecture / class design / patterns / dependencies / managers / services:
  - `Docs/AI/ArchitectureRules.md`

- code editing / reporting / validation workflow:
  - `Docs/AI/AgentWorkflow.md`

If unsure, read the relevant file before making changes.

## Planning

For non-trivial tasks, briefly state the plan before implementation.

A useful plan has:

- what files/systems will be inspected;
- what change is intended;
- what will not be touched;
- what assumption is being made.

Do not over-plan simple changes.

## Making Changes

Prefer small incremental changes.

When editing code:

1. Preserve existing behavior unless the task says otherwise.
2. Keep names consistent with the project.
3. Keep public APIs stable when possible.
4. Validate required serialized references.
5. Avoid unnecessary abstractions.
6. Avoid hidden dependencies.
7. Avoid per-frame allocations in gameplay code.
8. Add comments only when they clarify non-obvious logic.

## Comments

Do not add comments that simply repeat the code.

Bad:

```csharp
// Set health to max health
health = maxHealth;
```

Good:

```csharp
// Reset is needed because this object may be reused from the pool.
health = maxHealth;
```

## Testing and Validation

When possible, validate changes with:

- Unity Console;
- Play Mode;
- existing tests;
- small isolated test scenes;
- pure C# unit tests for non-Unity logic.

Do not claim that validation was done unless it was actually done.

Use honest language:

- "Not run: Unity Editor is not available in this environment."
- "Not tested: requires Play Mode validation."
- "Checked by reading the code only."

## Unity Verification Suggestions

When reporting a Unity change, include practical verification steps such as:

1. Open the target scene.
2. Press Play.
3. Trigger the relevant gameplay action.
4. Watch the Console for errors.
5. Check the Inspector references if something does not work.

Keep verification steps specific to the task.

## Error Reporting

If something cannot be completed, say exactly why.

Useful format:

```text
I could not complete X because Y.
I changed A and B.
Remaining work: C.
```

Do not pretend a task is finished when it is only partially done.

## Git and Repository Safety

Do not modify:

- `Library/`
- `Temp/`
- `Obj/`
- `Build/`
- generated IDE files
- generated solution/project files unless required

Do not manually edit `.meta` files unless Unity created/updated them or the task specifically requires it.

Do not create large binary files unless requested.

Do not reformat unrelated files.

## File Creation Rules

When creating new scripts:

1. Put them in the existing relevant folder if one exists.
2. Use a clear class name matching the file name.
3. Add a namespace only if the project already uses namespaces consistently.
4. Keep the first version small and focused.
5. Avoid creating multiple new layers unless necessary.

## Refactoring Rules

Before refactoring:

1. Identify the concrete problem.
2. Keep behavior equivalent unless requested.
3. Prefer safe intermediate steps.
4. Avoid broad rename/move operations unless useful.
5. Mention any manual Unity Inspector updates that may be needed.

Do not refactor only for style if the user asked for a bug fix.

## Reporting Format

Use this final format after making changes:

```text
What changed
- ...

Files changed
- ...

Why this approach
- ...

Risks / assumptions
- ...

How to verify in Unity
- ...
```

Keep the report concise.

## Prompt Handling

If the user asks for code, provide code.

If the user asks for explanation, explain the principle first and then show practical Unity/C# examples.

If the request is ambiguous, make a reasonable assumption and state it instead of blocking progress with too many questions.

## Do Not Over-Optimize

Do not optimize prematurely.

Optimize when:

- the code is in a hot path;
- the object is spawned frequently;
- the system runs every frame;
- the user reports performance problems;
- Unity Profiler or obvious code structure suggests a real issue.

Otherwise prefer clarity.
