# UnityCodingRules.md

Unity-specific rules for AI coding agents working on this Unity 3D project.

## Unity Version

- Use Unity 6-compatible APIs and practices.
- Do not assume old Unity behavior if Unity 6 has a newer recommended workflow.
- Prefer code that works cleanly with the Unity Editor and Inspector.

## References and Scene Access

Avoid these unless explicitly justified:

- `GameObject.Find`
- `GameObject.FindWithTag`
- `Object.FindFirstObjectByType`
- `Object.FindAnyObjectByType`
- `Object.FindObjectsByType`
- string-based scene object lookup
- hidden dependencies through object names, tags, or hierarchy paths

Prefer:

- `[SerializeField]` references assigned in the Inspector
- constructor or method injection for plain C# classes
- explicit initialization methods
- ScriptableObject configs for static/tunable data
- cached component references
- events/signals for communication between systems
- references passed from composition root, manager, installer, or initialization code

If a scene search API is used:

1. Explain why it is acceptable.
2. Keep it out of per-frame code.
3. Prefer using it only during setup, initialization, or editor tooling.

## Update / Performance

Do not put expensive work in `Update`, `FixedUpdate`, or `LateUpdate`.

Avoid per-frame:

- allocations
- LINQ
- string concatenation
- `new` collections
- repeated `GetComponent`
- scene searches
- repeated `Camera.main`
- repeated physics queries without need
- `Debug.Log` spam

Prefer:

- cache references in `Awake`, `Start`, or initialization methods
- event-driven logic where appropriate
- timers/coroutines only when they simplify the logic
- object pooling for frequently spawned objects
- clear separation between input reading and gameplay logic

`Update` is allowed for continuous gameplay behavior such as movement, camera follow, cooldown ticking, or simple state checks. The rule is not “never use Update”; the rule is “do not use Update for avoidable expensive polling.”

# C# Code Style Rules

- Do not pass complex conditional or nested expressions directly into method calls.
- Split that logic into local variables first, then pass the local variables into the method.

## Guard Clauses and Braces

Prefer short guard clauses to reduce nesting and keep code easier to scan.

Use the one-line form for simple return-only guards:

```csharp
if (!hasCurrentConfiguration) return;
```

Do not expand a simple return-only guard into a block:

```csharp
// Avoid
if (!hasCurrentConfiguration)
{
    return;
}
```

For a simple `if` with exactly one non-return statement, omit braces and put the statement on the next line:

```csharp
if (!isInitialized)
    Initialize();
```

Avoid putting non-return statements on the same line as the condition:

```csharp
// Avoid
if (!isInitialized) Initialize();
```

Use braces when the `if` body contains two or more statements:

```csharp
if (!isInitialized)
{
    Initialize();
    Debug.Log("System initialized.");
}
```

Use braces when the condition or body becomes visually complex, even if there is only one statement:

```csharp
if (playerHealth != null && playerHealth.CurrentValue <= 0)
{
    HandlePlayerDeath();
}
```

Use braces when removing them would make the code less readable, less safe, or inconsistent with nearby code.

Summary:

- Prefer `if (condition) return;` for simple return-only guards.
- Use no braces for simple one-statement guards.
- Put non-return one-statement guards on the next line.
- Do not write non-return one-statement guards on the same line.
- Use braces for multi-statement bodies.
- Use braces when readability or safety is better with braces.
- Do not expand simple return-only guards into a block with only `return;`.

## Unity Object Lifetime Checks

For references derived from `UnityEngine.Object`, use Unity-aware lifetime checks.

Preferred style:

- Use `if (!objectRef)` when checking for missing or destroyed Unity objects.
- Use `if (objectRef)` when checking that a Unity object exists and is not destroyed.

Also acceptable when clarity is better:

- `if (objectRef == null)`
- `if (objectRef != null)`

Do not use these for Unity lifetime checks:

- `objectRef?.SomeMethod()`
- `objectRef ?? fallback`
- `objectRef is null`
- `objectRef is not null`
- `ReferenceEquals(objectRef, null)`

These C# null mechanisms can bypass Unity's overloaded null/lifetime behavior.

Apply this only to types derived from `UnityEngine.Object`, such as:

- `GameObject`
- `Component`
- `MonoBehaviour`
- `ScriptableObject`
- `Transform`
- `Rigidbody`
- `Collider`
- `Button`
- `Image`
- `Canvas`
- `TextMeshProUGUI`

Do not apply this style to plain C# objects, services, models, collections, interfaces, delegates, strings, or non-Unity data classes.

## Instantiate / Destroy

Avoid frequent runtime `Instantiate` and `Destroy` for gameplay objects such as:

- bullets
- enemies
- VFX
- particles
- sounds
- damage numbers
- floating UI

Prefer pooling for repeatable gameplay objects.

`Instantiate` is acceptable for:

- one-time scene setup
- rare UI screens
- level initialization
- prototypes when explicitly requested
- objects that are created rarely and do not affect performance

## MonoBehaviour Rules

Keep `MonoBehaviour` classes focused.

A `MonoBehaviour` should usually handle:

- Unity lifecycle
- serialized references
- scene integration
- starting/stopping behavior

Avoid putting too much business logic directly into large `MonoBehaviour` scripts.

Prefer plain C# classes for:

- calculations
- rules
- state machines
- ability logic
- wave generation
- config-driven behavior
- data transformations

## Serialized Fields

Prefer private serialized fields:

```csharp
[SerializeField] private float moveSpeed = 5f;
```

Avoid public fields unless they are intentionally part of the public API.

Use properties or methods for controlled public access:

```csharp
[SerializeField] private int maxHealth = 100;

public int MaxHealth => maxHealth;
```

Useful attributes are allowed when they improve Inspector clarity:

```csharp
[Header("Movement")]
[SerializeField, Min(0f)] private float moveSpeed = 5f;
[SerializeField, Tooltip("How quickly the character turns toward movement direction.")]
private float rotationSpeed = 720f;
```

Do not add attributes mechanically to every field.

## Component Caching

Avoid repeated component lookups.

Bad:

```csharp
private void Update()
{
    GetComponent<Rigidbody>().AddForce(Vector3.forward);
}
```

Better:

```csharp
private Rigidbody _rigidbody;

private void Awake()
{
    _rigidbody = GetComponent<Rigidbody>();
}

private void FixedUpdate()
{
    _rigidbody.AddForce(Vector3.forward);
}
```

## Input System

Use Unity’s New Input System if input work is needed.

Separate:

- input reading
- gameplay interpretation
- movement/attack execution

Do not directly mix complex gameplay behavior into input callback methods.

Input callbacks should usually:

- update input state;
- raise clear events;
- call small intent methods.

Example:

```csharp
private void OnMove(InputAction.CallbackContext context)
{
    _moveInput = context.ReadValue<Vector2>();
}
```

Then apply movement elsewhere:

```csharp
private void Update()
{
    _movement.Move(_moveInput);
}
```

## Physics and Movement

When using physics:

- Use `Rigidbody` / `Rigidbody2D` movement in the physics flow.
- Use `FixedUpdate` for physics-related movement.
- Do not move physics objects through `transform.position` unless intentionally bypassing physics.

When not using physics:

- `transform.position`, `CharacterController`, or custom movement can be used.
- Keep movement logic deterministic and easy to inspect.

For 3D character movement:

- `CharacterController` is acceptable for non-physics character movement.
- `NavMeshAgent` is acceptable for enemy AI navigation.
- Direct transform movement is acceptable for simple prototypes or non-physics objects.

## UI Rules

Do not mix core gameplay logic into UI scripts.

UI should:

- display state;
- forward user actions;
- subscribe/unsubscribe safely;
- not own gameplay rules.

Gameplay systems should work without UI where possible.

Bad:

```csharp
public class HealthBar : MonoBehaviour
{
    public void DamagePlayer()
    {
        player.Health -= 10;
    }
}
```

Better:

```csharp
public class HealthBar : MonoBehaviour
{
    public void SetValue(float normalizedHealth)
    {
        slider.value = normalizedHealth;
    }
}
```

## Events and Subscriptions

Always unsubscribe from events.

Typical pattern:

```csharp
private void OnEnable()
{
    source.SomeEvent += OnSomeEvent;
}

private void OnDisable()
{
    source.SomeEvent -= OnSomeEvent;
}
```

For manually initialized dependencies, unsubscribe in a matching cleanup method or `OnDestroy`.

Avoid anonymous lambdas for event subscriptions if they need to be unsubscribed later.

Bad:

```csharp
button.onClick.AddListener(() => StartGame());
```

Acceptable for one-time UI setup if removal is not needed, but prefer named methods when lifecycle matters:

```csharp
private void OnEnable()
{
    button.onClick.AddListener(StartGame);
}

private void OnDisable()
{
    button.onClick.RemoveListener(StartGame);
}
```

## ScriptableObjects

Use ScriptableObjects for static configuration:

- enemy stats
- player stats
- ability settings
- wave settings
- audio/particle keys
- balance data

Do not store runtime mutable state in shared ScriptableObject assets unless explicitly intended.

Runtime state belongs in:

- scene objects;
- runtime models;
- plain C# state containers;
- instantiated runtime copies.

## Prefabs and Inspector Safety

When adding serialized fields:

1. Keep names clear.
2. Add `[Header]`, `[Tooltip]`, or `[Min]` only when useful.
3. Validate required references.
4. Do not assume references are assigned.
5. Avoid changing prefab/scene structure unless the task asks for it.

Use clear validation for required references:

```csharp
private void Awake()
{
    if (player == null)
    {
        Debug.LogError($"{nameof(MyComponent)} requires a Player reference.", this);
        enabled = false;
        return;
    }
}
```

## Layers, Tags, and Names

Do not rely on object names or hierarchy paths for gameplay logic.

Tags and layers are acceptable for Unity systems such as:

- collision filtering;
- physics queries;
- camera culling;
- broad category checks.

Do not use tags as a replacement for explicit dependencies when a direct reference is possible.

## Logging

Use logs carefully.

Allowed:

- setup validation errors;
- important state transitions during debugging;
- temporary logs while implementing a feature.

Avoid:

- logs every frame;
- noisy success logs;
- leaving temporary debug spam in final code.

Prefer contextual logs:

```csharp
Debug.LogError($"Missing {nameof(EnemyConfig)} on {name}.", this);
```

## Coroutines and Timers

Coroutines are acceptable for simple time-based sequences.

Avoid using many scattered coroutines when a clear timer/state machine would be easier to reason about.

Stop coroutines or clean up timers when the object is disabled/destroyed if needed.

## Object Pooling

Use pooling for repeatable objects:

- enemies
- projectiles
- VFX
- particles
- audio sources
- damage numbers

A pooled object should have a clear reset lifecycle:

- `OnGetFromPool`
- `OnReturnToPool`
- or project-consistent equivalents

Do not leave old state on pooled objects.
