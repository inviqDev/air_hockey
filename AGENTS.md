# AGENTS.md

This file contains mandatory instructions for AI coding agents working on this Unity project.

## Project Context

- Engine: Unity 6.
- Project type: Unity 2D game.
- Target quality: readable, maintainable, beginner-intermediate friendly professional game code.
- Prefer small, safe, incremental changes over large rewrites.
- Do not introduce unnecessary architecture or patterns unless the task clearly requires them.

## Instruction Files

Before starting work, read the relevant instruction files:

If the task touches Unity scene logic, MonoBehaviours, prefabs, spawning, input, UI, physics, pooling, or performance - read `Docs/AI/UnityRules.md`.

If the task touches class design, managers, services, factories, state machines, events, ScriptableObjects, or system boundaries - read `Docs/AI/ArchitectureRules.md`.

Always follow `Docs/AI/AgentWorkflow.md` when making code changes.

- `Docs/AI/UnityCodingRules.md` — Unity-specific coding rules and anti-patterns.
- `Docs/AI/ArchitectureRules.md` — architecture, dependencies, patterns, and project structure rules.
- `Docs/AI/AgentWorkflow.md` — how to inspect, change, report, and verify work.

## Core Principles

1. Keep changes focused on the user’s request.
2. Reuse existing project style, naming, folders, and patterns where reasonable.
3. Do not rewrite unrelated systems.
4. Do not silently delete existing behavior.
5. Prefer readable, maintainable C# over clever code.
6. Prefer simple architecture first.
7. Make the smallest safe assumption if something is unclear and state that assumption.
8. Do not claim that code was tested unless it was actually tested.

## Learning-Oriented Rule

The user is learning C#, Unity, and game development.

When modifying code, do not only make it work. Preserve learning value:

- keep code readable for a beginner-to-intermediate Unity/C# developer;
- avoid clever abstractions;
- explain non-obvious decisions;
- prefer professional but understandable solutions;
- avoid adding patterns just to look advanced.

## Forbidden Defaults

Do not use these as default solutions:

- `GameObject.Find`, `GameObject.FindWithTag` and similar for dependency lookup 
- singleton for every manager
- static global access for gameplay systems
- public fields for Inspector data
- per-frame LINQ in gameplay
- per-frame allocations
- `Resources.Load` as a general architecture solution
- hardcoded scene object names
- hidden dependencies through tags, layers, names, or hierarchy paths
- large god classes
- unnecessary inheritance hierarchies
- unnecessary interfaces for one implementation
- premature design patterns

## Preferred Defaults

Prefer these by default:

- explicit references
- private serialized fields
- cached components
- small focused classes
- config-driven design with ScriptableObjects
- object pooling for repeated spawn/despawn
- event-driven communication where it reduces coupling
- plain C# classes for pure logic
- incremental changes
- clear validation and error messages

## Reporting Format

When reporting back, use this structure:

1. What changed
2. Files changed
3. Why this approach
4. Risks / assumptions
5. How to verify in Unity
