using System.Collections.Generic;
using UnityEngine;

public sealed class EffectReceiver : MonoBehaviour
{
    private readonly List<IEffect> activeEffects = new();
    private EffectContext context;

    public bool HasActiveEffects => activeEffects.Count > 0;

    private void Awake()
    {
        context = new EffectContext(this, gameObject, transform);
    }

    public void AddEffect(IEffect effect)
    {
        if (effect == null)
            return;

        EnsureContext();
        activeEffects.Add(effect);
        effect.StartEffect(context);
    }

    public void ReplaceEffects<T>(T replacement) where T : class, IEffect
    {
        RemoveEffects<T>();
        AddEffect(replacement);
    }

    public void RemoveEffects<T>() where T : class, IEffect
    {
        EnsureContext();

        for (var i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (!(activeEffects[i] is T))
                continue;

            activeEffects[i].EndEffect(context);
            activeEffects.RemoveAt(i);
        }
    }

    public void GetEffects<T>(List<T> results) where T : class, IEffect
    {
        if (results == null)
            return;

        results.Clear();

        for (var i = 0; i < activeEffects.Count; i++)
        {
            if (activeEffects[i] is T effect)
                results.Add(effect);
        }
    }

    public void TickEffects(float deltaTime)
    {
        EnsureContext();

        for (var i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];
            effect.TickEffect(context, deltaTime);

            if (!effect.IsFinished)
                continue;

            effect.EndEffect(context);
            activeEffects.RemoveAt(i);
        }
    }

    public void ClearEffects()
    {
        EnsureContext();

        for (var i = activeEffects.Count - 1; i >= 0; i--)
            activeEffects[i].EndEffect(context);

        activeEffects.Clear();
    }

    private void OnDisable()
    {
        ClearEffects();
    }

    private void EnsureContext()
    {
        if (context.Receiver != null)
            return;

        context = new EffectContext(this, gameObject, transform);
    }
}
