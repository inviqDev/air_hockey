# Repo Instructions

## Unity Style

- For `UnityEngine.Object` references, prefer Unity lifetime checks:
  - Use `if (!objectRef)` instead of `if (objectRef == null)`
  - Use `if (objectRef)` instead of `if (objectRef != null)`
- Apply this to Unity-backed references such as `GameObject`, `Component`, `MonoBehaviour`, `ScriptableObject`, `Button`, `Image`, `Canvas`, and `TextMeshProUGUI`.
- Do not force this style onto plain C# reference types or non-Unity objects.
- For a single guard that only returns, prefer the one-line form:
  - Write `if (!hasCurrentConfiguration) return;`
  - Do not expand it to a multi-line block with only `return;`
- For a single guard with only one statement inside, do not use braces.
  - Write it in two lines.
  - Exception: if the only statement is `return;`, keep it on one line, for example `if (!hasCurrentConfiguration) return;`
- If an `if` statement contains two or more lines inside its body, use braces.
- Do not pass complex conditional or nested expressions directly into method calls.
  - Split that logic into local variables first, then pass the local variables into the method.
- Do not use scene-wide or hierarchy-search lookup methods as routine dependency resolution.
  - Avoid APIs that search the scene, object tree, tags, or global object collections at runtime.
  - Prefer explicit serialized references, constructor-style setup, registration, or deliberate wiring from an owning composition/root object.
  - If a lookup is truly unavoidable, treat it as an exceptional case and document why the dependency cannot be wired directly.
