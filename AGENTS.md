# Repo Instructions

## Unity Style

- For `UnityEngine.Object` references, prefer Unity lifetime checks:
  - Use `if (!objectRef)` instead of `if (objectRef == null)`
  - Use `if (objectRef)` instead of `if (objectRef != null)`
- Apply this to Unity-backed references such as `GameObject`, `Component`, `MonoBehaviour`, `ScriptableObject`, `Button`, `Image`, `Canvas`, and `TextMeshProUGUI`.
- Do not force this style onto plain C# reference types or non-Unity objects.
