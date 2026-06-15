using System;
using UnityEngine;

public sealed class AbilitySelectionCoordinator : MonoBehaviour
{
    [Serializable]
    private sealed class ParticipantProgression
    {
        [SerializeField] private AbilityHudView abilityHud;
        [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
        [SerializeField, Min(1f)] private float durationMultiplier = 1.5f;

        private readonly AbilityProgressState progressState = new();

        public event Action<ParticipantProgression> AbilityMenuRequested;

        public void Initialize()
        {
            progressState.ConfigureFreeAbilityTimer(initialDurationSeconds, durationMultiplier);
            RefreshHud(false);
        }

        public void StartRound()
        {
            progressState.StartTurnProgression();
            RefreshHud(false);
        }

        public void StopRound()
        {
            progressState.StopTurnProgression();
            RefreshHud(true);
        }

        public void Tick(float deltaTime, bool canOpenAbilityMenu)
        {
            progressState.Tick(deltaTime);
            RefreshHud(canOpenAbilityMenu);
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

        private void RefreshHud(bool canOpenAbilityMenu)
        {
            if (!abilityHud) return;

            abilityHud.SetFreeAbilityTimerText(progressState.FreeAbilityTimerText);
            abilityHud.SetAvailableAmount(progressState.AvailableAbilityPoints);
            abilityHud.SetAbilityMenuButtonEnabled(canOpenAbilityMenu && progressState.AvailableAbilityPoints > 0);
        }

        private void HandlePlusAbilityButtonClicked()
        {
            AbilityMenuRequested?.Invoke(this);
        }
    }

    [SerializeField] private TurnController turnController;
    [SerializeField] private ParticipantProgression leftProgression = new();
    [SerializeField] private ParticipantProgression rightProgression = new();

    private void Awake()
    {
        ValidateReferences();
        leftProgression.Initialize();
        rightProgression.Initialize();
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

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        var canOpenAbilityMenu = turnController && !turnController.IsTurnActive;
        leftProgression.Tick(deltaTime, canOpenAbilityMenu);
        rightProgression.Tick(deltaTime, canOpenAbilityMenu);
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
        leftProgression.StartRound();
        rightProgression.StartRound();
    }

    private void HandleTurnEnded()
    {
        leftProgression.StopRound();
        rightProgression.StopRound();
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

    private void HandleAbilityMenuRequested(ParticipantProgression progression)
    {
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires a {nameof(TurnController)} reference.", this);

        leftProgression.Validate(nameof(leftProgression), this);
        rightProgression.Validate(nameof(rightProgression), this);
    }
}
