# ArchitectureRules.md

Architecture and design rules for AI coding agents working on this Unity project.

## Main Principle

Keep architecture simple, explicit, and useful.

Do not add patterns because they look professional. Add a pattern only when it solves a real problem in the current task.

Good architecture should make the project easier to understand, test, extend, and debug.

Bad architecture adds layers, interfaces, inheritance, or indirection without a clear benefit.

## Single Responsibility

Each class should have one clear reason to change.

Avoid classes that simultaneously handle:

- input
- movement
- health
- UI
- audio
- spawning
- saving
- game state

Split classes only when it improves clarity.

Do not split tiny scripts into needless fragments.

## Dependencies

Prefer explicit dependencies.

Bad:

```csharp
private void Start()
{
    _player = GameObject.FindWithTag("Player").GetComponent<Player>();
}
```

Better:

```csharp
public void Initialize(Player player)
{
    _player = player;
}
```

Or:

```csharp
[SerializeField] private Player player;
```

A class should make it reasonably obvious what it needs to work.

## Dependency Direction

Prefer high-level systems coordinating lower-level systems.

Avoid random cross-references between unrelated objects.

Example preferred flow:

```text
GameManager / Composition Root
    -> creates or references Player
    -> creates or references Spawner
    -> passes Player into Spawner.Initialize(player)
```

Avoid:

```text
Spawner searches for Player
Player searches for UI
Enemy searches for GameManager
UI modifies EnemySpawner directly
```

## Composition Root

If the project has a manager, installer, bootstrapper, or scene initializer, use it as the place where systems are wired together.

The composition root may:

- create systems;
- connect dependencies;
- initialize gameplay systems;
- pass references between systems.

Do not scatter dependency lookup across many individual scripts.

## Patterns: When to Use Them

Use patterns only when they solve the current problem.

### Factory

Use a Factory when object creation has:

- dependencies;
- multiple variants;
- pooling;
- non-trivial setup;
- runtime config selection;
- spawn rules.

Do not introduce a Factory for simple one-off creation.

### State Machine

Use a State Machine when behavior has clear states and transitions.

Good examples:

- enemy AI states;
- boss phases;
- player movement states;
- game flow states;
- ability lifecycle.

Avoid a State Machine for simple two-branch logic.

### Observer / Events

Use events when systems need loose communication.

Good examples:

- health changed;
- player died;
- wave completed;
- ability became ready;
- UI needs to update after gameplay state changes.

Avoid events when a direct method call is clearer and there is no coupling problem.

### Strategy

Use Strategy when behavior must be swapped or configured.

Good examples:

- enemy attack behavior;
- movement behavior;
- ability targeting behavior;
- damage calculation variants.

Avoid Strategy for behavior that has only one implementation and no expected variation.

### Object Pool

Use Object Pool when objects are spawned and despawned frequently.

Good examples:

- enemies;
- bullets;
- VFX;
- particles;
- sounds;
- floating UI.

Avoid pooling rare or one-time objects unless the project already standardizes it.

### ScriptableObject Config

Use ScriptableObject configs for editable static data.

Good examples:

- enemy stats;
- player stats;
- ability settings;
- wave settings;
- item definitions;
- audio/particle keys.

Do not store normal runtime mutable state in shared ScriptableObject assets.

## Patterns: What Not to Add by Default

Do not add these unless clearly justified:

- service locator;
- global static access;
- abstract factory layers;
- unnecessary interfaces;
- generic base classes;
- reflection-based systems;
- event bus;
- full MVC/MVP/MVVM structure;
- dependency injection framework;
- custom serialization framework.

These can be valid in some projects, but they are not default solutions.

## Interfaces

Use interfaces when they provide real value:

- multiple implementations exist or are expected soon;
- testing/mocking is important;
- the caller should not depend on a concrete Unity component;
- the project already uses this abstraction style.

Do not create an interface for every class automatically.

Bad:

```csharp
public interface IPlayerHealth
{
    void TakeDamage(int damage);
}

public class PlayerHealth : MonoBehaviour, IPlayerHealth
{
    public void TakeDamage(int damage) { }
}
```

This is unnecessary if there is only one implementation and no abstraction need.

Better when abstraction is useful:

```csharp
public interface IDamageable
{
    void TakeDamage(int damage);
}
```

This can be useful because many things may be damageable: player, enemies, barrels, destructible props.

## Inheritance vs Composition

Prefer composition over deep inheritance.

Acceptable inheritance:

- common base class with genuinely shared behavior;
- Unity-specific base class when the project already uses it;
- small abstract base for related gameplay entities.

Avoid:

- deep inheritance chains;
- base classes full of unrelated features;
- inheritance used only to share a few fields;
- forcing unrelated objects into the same hierarchy.

Prefer composition:

```text
Enemy
    -> Health
    -> Movement
    -> Attack
    -> Detection
```

instead of:

```text
Character
    -> EnemyCharacter
        -> FlyingEnemy
            -> FireFlyingEnemy
```

## Managers

Do not create a new manager for everything.

A manager is acceptable when it has a clear coordination responsibility:

- spawning;
- game state;
- scene loading;
- audio playback;
- pooling;
- UI screen coordination.

Avoid vague managers with unclear ownership.

Bad names:

- `GameStuffManager`
- `HelperManager`
- `DataManager` without clear responsibility
- `ObjectManager`

Prefer specific names:

- `WaveSpawner`
- `EnemyPool`
- `SceneTransitionService`
- `AbilityController`
- `PlayerInputReader`

## Singletons

Do not use singleton as the default solution.

A singleton may be acceptable for project-level services when:

- only one instance should exist;
- global access is intentionally part of the project style;
- lifecycle is controlled;
- testing and hidden dependencies are not harmed too much.

Possible examples:

- AudioManager;
- Pool;
- SceneTransitionService;
- GameManager in a small project.

Avoid using singleton for normal gameplay entities:

- Player;
- Enemy;
- Weapon;
- Ability;
- UI element;
- Spawner instance when direct reference is possible.

If using a singleton, do not hide too much behavior behind it.

## Static Access

Avoid static access for mutable gameplay state.

Bad:

```csharp
GameState.PlayerHealth -= 10;
```

Better:

```csharp
_playerHealth.TakeDamage(10);
```

Static constants and pure utility methods are acceptable when they are genuinely stateless.

## Plain C# Classes

Prefer plain C# classes for logic that does not need Unity lifecycle or Inspector integration.

Good candidates:

- ability logic;
- cooldown timers;
- damage calculations;
- wave generation;
- save data models;
- state machines;
- targeting rules;
- config parsing.

Benefits:

- easier to test;
- less coupled to scene objects;
- easier to reason about;
- fewer Unity lifecycle problems.

## Folder and File Organization

Follow existing project structure first.

If no clear structure exists, prefer simple feature-based folders:

```text
Assets/Scripts/Player
Assets/Scripts/Enemies
Assets/Scripts/Abilities
Assets/Scripts/UI
Assets/Scripts/Spawning
Assets/Scripts/Configs
Assets/Scripts/Infrastructure
```

Do not reorganize the project unless explicitly requested.

Do not move files just to make the structure look nicer.

## Naming

Use standard C# naming:

- Classes, structs, enums, methods, properties: `PascalCase`
- Interfaces: `IInterfaceName`
- Private fields: `_camelCase`
- Local variables and parameters: `camelCase`
- Constants: project-consistent style, usually `PascalCase`

Use names that reveal intent.

Avoid vague names:

- `Manager` when the responsibility is unclear;
- `Controller` for everything;
- `Data` for complex behavior;
- `Temp`;
- `Stuff`;
- `Thing`;
- `Helper` without context.

## C# Code Style

Write simple, readable C#.

Prefer:

- early returns;
- small methods;
- clear guard clauses;
- explicit access modifiers;
- readonly fields where appropriate;
- properties for controlled public access;
- meaningful method names.

Avoid:

- clever one-liners;
- unnecessary LINQ in gameplay code;
- magic numbers without named fields/configs;
- deeply nested conditionals;
- hidden side effects;
- large methods;
- premature generics;
- reflection;
- dynamic typing.

## Error Handling

Do not hide errors silently.

Use clear fail-fast validation for required dependencies:

```csharp
if (dependency == null)
{
    Debug.LogError($"{nameof(MyClass)} requires {nameof(dependency)}.", this);
    enabled = false;
    return;
}
```

Do not spam logs every frame.

## Data vs Behavior

Keep static data, runtime state, and behavior separate when practical.

Example:

- `EnemyConfig` — static data in ScriptableObject;
- `Enemy` — MonoBehaviour scene/prefab component;
- `EnemyRuntimeStats` — runtime mutable values;
- `EnemyMovement` — movement behavior;
- `EnemyAttack` — attack behavior.

Do not force this split for tiny prototypes, but prefer it as systems grow.
