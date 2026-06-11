using UnityEngine;

public sealed class AbilityContext
{
    public AbilityContext(
        GameObject owner,
        Transform ownerTransform,
        EffectReceiver effectReceiver)
    {
        Owner = owner;
        OwnerTransform = ownerTransform;
        EffectReceiver = effectReceiver;
    }

    public GameObject Owner { get; }
    public Transform OwnerTransform { get; }
    public EffectReceiver EffectReceiver { get; }

    public bool TryGetOwnerComponent<T>(out T component)
    {
        component = default;
        return Owner && Owner.TryGetComponent(out component);
    }

    public bool TryFindFirstObject<T>(out T instance) where T : Object
    {
        instance = Object.FindFirstObjectByType<T>();
        return instance;
    }

    public bool TryFindFirstObjectIncludingInactive<T>(out T instance) where T : Object
    {
        instance = Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        return instance;
    }
}
