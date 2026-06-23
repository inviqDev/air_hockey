using System;
using UnityEngine;

public sealed class AbilitySelectionCoordinator : MonoBehaviour
{
    [SerializeField] private TurnController turnController;
    [SerializeField] private AbilityCatalog abilityCatalog;
    [SerializeField] private ParticipantAbilitySelectionBinding leftParticipantProgression = new();
    [SerializeField] private ParticipantAbilitySelectionBinding rightParticipantProgression = new();

    private readonly AbilityOfferService offerService = new();

    private ParticipantAbilitySelectionRuntime leftParticipant;
    private ParticipantAbilitySelectionRuntime rightParticipant;
    private MatchManager matchManager;

    private void Awake()
    {
        ValidateReferences();
        leftParticipant = CreateParticipantRuntime(leftParticipantProgression);
        rightParticipant = CreateParticipantRuntime(rightParticipantProgression);
    }

    private void OnEnable()
    {
        SubscribeToTurnEvents();
        ForEachParticipant(participant => participant.Enable());
    }

    private void OnDisable()
    {
        ForEachParticipant(participant => participant.Disable());
        UnsubscribeFromTurnEvents();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        ForEachParticipant(participant => participant.Tick(deltaTime));
    }

    public void SetMatchManager(MatchManager manager)
    {
        matchManager = manager;
    }

    public void BindParticipantAbilityController(PlayerSide side, PlayerAbilityController abilityController)
    {
        GetParticipantRuntime(side)?.BindAbilityController(abilityController);
    }

    public void ClearParticipantAbilityControllers()
    {
        ForEachParticipant(participant => participant.BindAbilityController(null));
    }

    public void ResetProgression()
    {
        ForEachParticipant(participant => participant.ResetProgression());
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
        ForEachParticipant(participant => participant.StartTurnProgression());
    }

    private void HandleTurnEnded()
    {
        ForEachParticipant(participant => participant.StopTurnProgression());
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires a {nameof(TurnController)} reference.", this);

        if (!abilityCatalog)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires an {nameof(AbilityCatalog)} reference.", this);

        leftParticipantProgression.Validate(nameof(leftParticipantProgression), this);
        rightParticipantProgression.Validate(nameof(rightParticipantProgression), this);
    }

    private ParticipantAbilitySelectionRuntime CreateParticipantRuntime(ParticipantAbilitySelectionBinding binding)
    {
        var progression = new ParticipantAbilityProgression(binding.InitialDurationSeconds, binding.DurationMultiplier);
        var offerFlow = new ParticipantAbilityOfferFlow(
            binding.AbilityHud,
            binding.AbilitySelectionMenu,
            progression,
            abilityCatalog,
            offerService,
            () => matchManager && matchManager.IsAbilityMenuInteractionAllowed);

        return new ParticipantAbilitySelectionRuntime(binding.AbilityHud, progression, offerFlow);
    }

    private ParticipantAbilitySelectionRuntime GetParticipantRuntime(PlayerSide side)
    {
        return side == PlayerSide.Left
            ? leftParticipant
            : rightParticipant;
    }

    private void ForEachParticipant(Action<ParticipantAbilitySelectionRuntime> action)
    {
        if (action == null) return;

        if (leftParticipant != null)
            action(leftParticipant);

        if (rightParticipant != null)
            action(rightParticipant);
    }
}
