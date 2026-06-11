using System.Collections.Generic;
using System;
using UnityEngine;

public sealed class AbilityController : MonoBehaviour
{
    [SerializeField] private List<AbilityDefinition> initialAbilities = new();

    private readonly List<IAbility> abilities = new();
    private AbilityContext context;

    public event Action AbilitiesChanged;

    public IReadOnlyList<IAbility> Abilities => abilities;

    public bool HasActiveAbility
    {
        get
        {
            for (var i = 0; i < abilities.Count; i++)
            {
                if (abilities[i].IsActive)
                    return true;
            }

            return false;
        }
    }

    public void Initialize(AbilityContext abilityContext)
    {
        context = abilityContext;
        RebuildAbilities();
    }

    public bool AddAbility(AbilityDefinition definition)
    {
        if (!TryCreateAbility(definition, out var ability))
            return false;

        abilities.Add(ability);
        NotifyAbilitiesChanged();
        return true;
    }

    public void RequestAbilityActivation(in AbilityFrameContext frameContext)
    {
        for (var i = 0; i < abilities.Count; i++)
        {
            if (!IsSlotRequested(frameContext.ActivationTriggers, i))
                continue;

            if (abilities[i] is IAbilityActivationHandler activationHandler)
                activationHandler.TryActivate(frameContext);
        }
    }

    public void TickAbilities(in AbilityFrameContext frameContext)
    {
        for (var i = 0; i < abilities.Count; i++)
            abilities[i].Tick(frameContext);
    }

    public void ResetState()
    {
        for (var i = 0; i < abilities.Count; i++)
            abilities[i].ResetState();
    }

    public string GetDisplayNameForSlot(int slotIndex)
    {
        if (slotIndex < 0)
            return string.Empty;

        if (slotIndex < abilities.Count)
        {
            var runtimeDefinition = abilities[slotIndex].Definition;
            if (runtimeDefinition)
                return runtimeDefinition.DisplayName;
        }

        if (slotIndex >= initialAbilities.Count)
            return string.Empty;

        var configuredDefinition = initialAbilities[slotIndex];
        return configuredDefinition ? configuredDefinition.DisplayName : string.Empty;
    }

    private bool TryCreateAbility(AbilityDefinition definition, out IAbility ability)
    {
        ability = null;

        if (!definition)
            return false;

        if (context == null)
        {
            Debug.LogError($"{nameof(AbilityController)} on {name} must be initialized before adding abilities.", this);
            return false;
        }

        ability = definition.CreateAbility();
        if (ability == null)
        {
            Debug.LogError($"{definition.name} did not create a runtime ability.", definition);
            return false;
        }

        ability.Initialize(context, definition);
        return true;
    }

    private void OnDestroy()
    {
        ClearRuntimeAbilities();
    }

    private static bool IsSlotRequested(AbilityActivationTrigger activationTriggers, int slotIndex)
    {
        return slotIndex switch
        {
            0 => (activationTriggers & AbilityActivationTrigger.SlotOne) != 0,
            1 => (activationTriggers & AbilityActivationTrigger.SlotTwo) != 0,
            2 => (activationTriggers & AbilityActivationTrigger.SlotThree) != 0,
            _ => false
        };
    }

    private void NotifyAbilitiesChanged()
    {
        AbilitiesChanged?.Invoke();
    }

    private void RebuildAbilities()
    {
        ClearRuntimeAbilities();

        for (var i = 0; i < initialAbilities.Count; i++)
        {
            if (!TryCreateAbility(initialAbilities[i], out var ability))
                continue;

            abilities.Add(ability);
        }

        NotifyAbilitiesChanged();
    }

    private void ClearRuntimeAbilities()
    {
        for (var i = 0; i < abilities.Count; i++)
            abilities[i].Dispose();

        abilities.Clear();
    }
}
