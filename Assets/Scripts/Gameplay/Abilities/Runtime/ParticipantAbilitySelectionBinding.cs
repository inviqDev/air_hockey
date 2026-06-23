using System;
using UnityEngine;

[Serializable]
public sealed class ParticipantAbilitySelectionBinding
{
    [SerializeField] private AbilityHudView abilityHud;
    [SerializeField] private AbilitySelectionMenu abilitySelectionMenu;
    
    [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
    [SerializeField, Min(1f)] private float durationMultiplier = 1.5f;

    public AbilityHudView AbilityHud => abilityHud;
    public AbilitySelectionMenu AbilitySelectionMenu => abilitySelectionMenu;
    public float InitialDurationSeconds => initialDurationSeconds;
    public float DurationMultiplier => durationMultiplier;

    public void Validate(string fieldName, UnityEngine.Object context)
    {
        if (!abilityHud)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {context.name} requires a {nameof(AbilityHudView)} reference for {fieldName}.", context);

        if (!abilitySelectionMenu)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {context.name} requires an {nameof(AbilitySelectionMenu)} reference for {fieldName}.", context);
    }
}
