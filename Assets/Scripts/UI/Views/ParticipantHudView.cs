using System;
using UnityEngine;

public sealed class ParticipantHudView : MonoBehaviour
{
    [Header("HUD's children refs")]
    [SerializeField] private AbilitySlotsHudView abilitySlotsHudView;
    [SerializeField] private AbilityOffersHudView abilityOffersHudView;
    [SerializeField] private FreeAbilityTimerHudView freeAbilityTimerHudView;

    private bool isInitialized;

    public event Action PlusAbilityButtonClicked
    {
        add
        {
            Initialize();
            if (abilityOffersHudView)
                abilityOffersHudView.OpenOffersButtonClicked += value;
        }
        remove
        {
            if (!abilityOffersHudView) return;
            abilityOffersHudView.OpenOffersButtonClicked -= value;
        }
    }

    public void Initialize()
    {
        if (isInitialized) return;

        ValidateReferences();
        
        if (!abilityOffersHudView || !freeAbilityTimerHudView || !abilitySlotsHudView)
        {
            isInitialized = false;
            return;
        }

        freeAbilityTimerHudView.Initialize();
        abilityOffersHudView.Initialize();
        abilitySlotsHudView.Initialize();
        
        isInitialized = true;
    }

    public void SetFreeAbilityTimerText(string value)
    {
        if (!isInitialized || !freeAbilityTimerHudView) return;
        freeAbilityTimerHudView.SetFreeAbilityTimerText(value);
    }

    public void SetAvailableAmount(int amount)
    {
        if (!isInitialized || !abilityOffersHudView) return;
        abilityOffersHudView.SetAvailableAmount(amount);
    }

    public void SetAbilityMenuButtonEnabled(bool isEnabled)
    {
        if (!isInitialized || !abilityOffersHudView) return;
        abilityOffersHudView.SetAbilityMenuButtonEnabled(isEnabled);
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        if (!isInitialized || !abilitySlotsHudView) return;
        abilitySlotsHudView.BindAbilityController(controller);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void ValidateReferences()
    {
        if (!abilityOffersHudView)
            Debug.LogError($"{nameof(ParticipantHudView)} on {name} requires an {nameof(AbilityOffersHudView)} reference.", this);

        if (!freeAbilityTimerHudView)
            Debug.LogError($"{nameof(ParticipantHudView)} on {name} requires a {nameof(FreeAbilityTimerHudView)} reference.", this);

        if (!abilitySlotsHudView)
            Debug.LogError($"{nameof(ParticipantHudView)} on {name} requires a {nameof(AbilitySlotsHudView)} reference.", this);
    }
}
