using System;
using UnityEngine;

public sealed class NextAbilityMenu : MonoBehaviour
{
    [Serializable]
    private sealed class PlayerFreeAbilityTimer
    {
        [SerializeField] private PlayerAbilityHudView rootPlayerHUD;
        [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
        [SerializeField, Min(1f)] private float durationMultiplier = 1.3333f;

        private readonly Timer timer = new();
        private float nextDurationSeconds;
        private int availableAmount;

        public void Initialize()
        {
            nextDurationSeconds = Mathf.Max(1f, initialDurationSeconds);
            ResetForRoundEnd();
            UpdateAvailableAmount();
        }

        public void StartRound()
        {
            nextDurationSeconds = Mathf.Max(1f, initialDurationSeconds);
            timer.SetDecremental(nextDurationSeconds);
            timer.Restart();
            UpdateTimerDisplay();
        }

        public void StopRound()
        {
            ResetForRoundEnd();
        }

        public void Tick(float deltaTime)
        {
            if (!timer.IsRunning) return;

            var completed = timer.Tick(deltaTime);
            UpdateTimerDisplay();

            if (!completed) return;

            availableAmount++;
            UpdateAvailableAmount();

            nextDurationSeconds *= durationMultiplier;
            timer.SetDecremental(nextDurationSeconds);
            timer.Restart();
            UpdateTimerDisplay();
        }

        public void Validate(string fieldName, UnityEngine.Object context)
        {
            if (!rootPlayerHUD)
                Debug.LogError($"{nameof(NextAbilityMenu)} on {context.name} requires a {nameof(PlayerAbilityHudView)} reference for {fieldName}.", context);
        }

        private void ResetForRoundEnd()
        {
            timer.Stop();
            timer.SetDecremental(nextDurationSeconds);
            timer.Reset();
            UpdateTimerDisplay();
        }

        private void UpdateTimerDisplay()
        {
            if (!rootPlayerHUD) return;

            rootPlayerHUD.SetFreeAbilityTimerText(timer.GetFormattedMinutesSeconds());
        }

        private void UpdateAvailableAmount()
        {
            if (!rootPlayerHUD) return;

            rootPlayerHUD.SetAvailableAmount(availableAmount);
        }
    }

    [SerializeField] private TurnController turnController;
    [SerializeField] private PlayerFreeAbilityTimer leftPlayer = new();
    [SerializeField] private PlayerFreeAbilityTimer rightPlayer = new();

    private void Awake()
    {
        ValidateReferences();
        leftPlayer.Initialize();
        rightPlayer.Initialize();
    }

    private void OnEnable()
    {
        SubscribeToTurnEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromTurnEvents();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        leftPlayer.Tick(deltaTime);
        rightPlayer.Tick(deltaTime);
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
        leftPlayer.StartRound();
        rightPlayer.StartRound();
    }

    private void HandleTurnEnded()
    {
        leftPlayer.StopRound();
        rightPlayer.StopRound();
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(NextAbilityMenu)} on {name} requires a {nameof(TurnController)} reference.", this);

        leftPlayer.Validate(nameof(leftPlayer), this);
        rightPlayer.Validate(nameof(rightPlayer), this);
    }
}
