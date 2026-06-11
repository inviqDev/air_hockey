using UnityEngine;

public readonly struct EffectContext
{
    public EffectContext(EffectReceiver receiver, GameObject target, Transform targetTransform)
    {
        Receiver = receiver;
        Target = target;
        TargetTransform = targetTransform;
    }

    public EffectReceiver Receiver { get; }
    public GameObject Target { get; }
    public Transform TargetTransform { get; }
}
