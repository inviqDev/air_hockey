using UnityEngine;

public static class ReferenceGuard
{
    public static bool Missing(Object reference, string referenceName, Object context)
    {
        if (reference) return false;

        Debug.LogError($"{referenceName} reference is missing.", context);
        return true;
    }
}