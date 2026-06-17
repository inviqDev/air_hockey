using System;
using UnityEngine;

public sealed class AbilitySelectionCoordinator : MonoBehaviour
{
    [Serializable]
    private sealed class ParticipantProgressionBinding
    {
        [SerializeField] private AbilityHudView abilityHud;
        [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
        [SerializeField, Min(1f)] private float durationMultiplier = 1.5f;

        public event Action<ParticipantProgressionBinding> AbilityMenuRequested;

        public ParticipantAbilityProgression CreateRuntimeProgression()
        {
            return new ParticipantAbilityProgression(initialDurationSeconds, durationMultiplier);
        }

        public void RefreshHud(ParticipantAbilityProgression progression, bool canOpenAbilityMenu)
        {
            if (progression == null) return;
            if (!abilityHud) return;

            abilityHud.SetFreeAbilityTimerText(progression.FreeAbilityTimerText);
            abilityHud.SetAvailableAmount(progression.AvailableAbilityPoints);
            abilityHud.SetAbilityMenuButtonEnabled(canOpenAbilityMenu && progression.AvailableAbilityPoints > 0);
        }

        public void SubscribeToHud()
        {
            if (!abilityHud) return;

            abilityHud.PlusAbilityButtonClicked += HandlePlusAbilityButtonClicked;
        }

        public void UnsubscribeFromHud()
        {
            if (!abilityHud) return;

            abilityHud.PlusAbilityButtonClicked -= HandlePlusAbilityButtonClicked;
        }

        public void Validate(string fieldName, UnityEngine.Object context)
        {
            if (!abilityHud)
                Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {context.name} requires a {nameof(AbilityHudView)} reference for {fieldName}.", context);
        }

        private void HandlePlusAbilityButtonClicked()
        {
            AbilityMenuRequested?.Invoke(this);
        }
    }

    [SerializeField] private TurnController turnController;
    [SerializeField] private ParticipantProgressionBinding leftProgression = new();
    [SerializeField] private ParticipantProgressionBinding rightProgression = new();

    private ParticipantAbilityProgression leftParticipant;
    private ParticipantAbilityProgression rightParticipant;

    private void Awake()
    {
        ValidateReferences();
        leftParticipant = leftProgression.CreateRuntimeProgression();
        rightParticipant = rightProgression.CreateRuntimeProgression();
        RefreshAllHud(false);
    }

    private void OnEnable()
    {
        SubscribeToTurnEvents();
        SubscribeToHudEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromHudEvents();
        UnsubscribeFromTurnEvents();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ResetProgression()
    {
        leftParticipant.ResetProgression();
        rightParticipant.ResetProgression();
        RefreshAllHud(false);
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var canOpenAbilityMenu = turnController && !turnController.IsTurnActive;
        leftParticipant.Tick(deltaTime);
        rightParticipant.Tick(deltaTime);
        RefreshAllHud(canOpenAbilityMenu);
    }

    private void SubscribeToTurnEvents()
    {
        if (!turnController) return;

        turnController.TurnStarted += HandleTurnStarted;
        turnController.TurnEnded += HandleTurnEnded;
    }

    private void UnsubscribeFromTurnEvents()
    {
        if (!turnController) return;

        turnController.TurnStarted -= HandleTurnStarted;
        turnController.TurnEnded -= HandleTurnEnded;
    }

    private void HandleTurnStarted()
    {
        leftParticipant.StartTurnProgression();
        rightParticipant.StartTurnProgression();
        RefreshAllHud(false);
    }

    private void HandleTurnEnded()
    {
        leftParticipant.StopTurnProgression();
        rightParticipant.StopTurnProgression();
        RefreshAllHud(true);
    }

    private void SubscribeToHudEvents()
    {
        leftProgression.AbilityMenuRequested += HandleAbilityMenuRequested;
        rightProgression.AbilityMenuRequested += HandleAbilityMenuRequested;
        leftProgression.SubscribeToHud();
        rightProgression.SubscribeToHud();
    }

    private void UnsubscribeFromHudEvents()
    {
        leftProgression.UnsubscribeFromHud();
        rightProgression.UnsubscribeFromHud();
        leftProgression.AbilityMenuRequested -= HandleAbilityMenuRequested;
        rightProgression.AbilityMenuRequested -= HandleAbilityMenuRequested;
    }

    private void HandleAbilityMenuRequested(ParticipantProgressionBinding progression)
    {
    }

    private void RefreshAllHud(bool canOpenAbilityMenu)
    {
        leftProgression.RefreshHud(leftParticipant, canOpenAbilityMenu);
        rightProgression.RefreshHud(rightParticipant, canOpenAbilityMenu);
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires a {nameof(TurnController)} reference.", this);

        leftProgression.Validate(nameof(leftProgression), this);
        rightProgression.Validate(nameof(rightProgression), this);
    }
}
