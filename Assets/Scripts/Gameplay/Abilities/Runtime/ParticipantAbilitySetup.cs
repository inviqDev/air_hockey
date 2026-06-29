using System;
using UnityEngine;

[Serializable]
public sealed class ParticipantAbilitySetup
{
    [SerializeField] private ParticipantHudView participantHud;
    [SerializeField] private AbilitySelectionViewContainer abilitySelectionViewContainer;

    [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
    [SerializeField, Min(1f)] private float durationMultiplier = 1.5f;

    public ParticipantHudView ParticipantHud => participantHud;
    public AbilitySelectionViewContainer AbilitySelectionViewContainer => abilitySelectionViewContainer;
    public float InitialDurationSeconds => initialDurationSeconds;
    public float DurationMultiplier => durationMultiplier;

    public void Validate(string fieldName, UnityEngine.Object context)
    {
        if (!participantHud)
            Debug.LogError($"{nameof(ParticipantPreparationCoordinator)} on {context.name} requires a {nameof(ParticipantHudView)} reference for {fieldName}.", context);

        if (!abilitySelectionViewContainer)
            Debug.LogError($"{nameof(ParticipantPreparationCoordinator)} on {context.name} requires a {nameof(AbilitySelectionViewContainer)} reference for {fieldName}.", context);
    }
}
