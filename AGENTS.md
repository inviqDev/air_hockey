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
