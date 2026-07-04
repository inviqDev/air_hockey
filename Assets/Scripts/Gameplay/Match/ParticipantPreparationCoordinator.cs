using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ParticipantPreparationCoordinator : MonoBehaviour
{
    [SerializeField] private TurnController turnController;
    [SerializeField] private AbilityCatalog abilityCatalog;

    [SerializeField] private ParticipantAbilitySetup leftParticipantProgression = new();
    [SerializeField] private ParticipantAbilitySetup rightParticipantProgression = new();

    private readonly AbilityOfferService offerService = new();
    private readonly Dictionary<ParticipantId, ParticipantPreparationController> participantControllers = new();

    private MatchManager matchManager;

    private bool isInitialized;
    private bool isActivated;
    private bool isRuntimeActive;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnEnable()
    {
        if (!isActivated) return;
        BeginRuntimeActivation();
    }

    private void OnDisable()
    {
        EndRuntimeActivation();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Update()
    {
        if (!isRuntimeActive) return;

        var deltaTime = Time.deltaTime;
        foreach (var participantController in participantControllers.Values)
        {
            participantController.Tick(deltaTime);
        }
    }

    public void Initialize(MatchManager manager)
    {
        ValidateReferences();
        matchManager = manager;

        if (!manager)
            throw new ArgumentNullException(nameof(manager));

        isInitialized = true;
    }

    public void Activate()
    {
        if (!isInitialized) return;
        if (isActivated) return;

        isActivated = true;
        BeginRuntimeActivation();
    }

    public void Deactivate()
    {
        if (!isInitialized) return;
        if (!isActivated) return;

        isActivated = false;
        EndRuntimeActivation();
    }

    public void ConfigureParticipants(MatchConfiguration configuration)
    {
        if (!isInitialized)
        {
            throw new InvalidOperationException(
                $"{nameof(ParticipantPreparationCoordinator)} must be initialized before configuring participants.");
        }

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        EndRuntimeActivation();
        participantControllers.Clear();

        var allParticipants = configuration.Roster.Participants.ToArray();
        foreach (var participant in allParticipants)
        {
            var slotId = configuration.SlotAssignments.GetSlotForParticipant(participant.ParticipantId);
            var participantController = CreateParticipantPreparationController(participant, slotId);
            participantControllers.Add(participant.ParticipantId, participantController);
        }

        BeginRuntimeActivation();
    }

    public void ClearParticipants()
    {
        if (!isInitialized) return;

        EndRuntimeActivation();
        participantControllers.Clear();
    }

    public void BindParticipantAbilityController(ParticipantId participantId, PlayerAbilityController abilityController)
    {
        if (!isInitialized) return;

        if (!TryGetParticipantController(participantId, out var participantController)) return;
        participantController.BindAbilityController(abilityController);
    }

    public void ClearParticipantAbilityControllers()
    {
        if (!isInitialized) return;
        ForEachParticipantController(controller => controller.BindAbilityController(null));
    }

    public void ResetProgression()
    {
        if (!isInitialized) return;
        ForEachParticipantController(controller => controller.ResetProgression());
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
        if (!isInitialized) return;
        ForEachParticipantController(controller => controller.StartTurnProgression());
    }

    private void HandleTurnEnded()
    {
        if (!isInitialized) return;
        ForEachParticipantController(controller => controller.StopTurnProgression());
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(ParticipantPreparationCoordinator)} on {name} requires a {nameof(TurnController)} reference.", this);

        if (!abilityCatalog)
            Debug.LogError($"{nameof(ParticipantPreparationCoordinator)} on {name} requires an {nameof(AbilityCatalog)} reference.", this);

        leftParticipantProgression.Validate(nameof(leftParticipantProgression), this);
        rightParticipantProgression.Validate(nameof(rightParticipantProgression), this);
    }

    private ParticipantPreparationController CreateParticipantPreparationController(
        MatchParticipantSetup participant,
        ArenaSlotId slotId)
    {
        var binding = GetSetupForSlot(slotId);
        var abilitySelectionRuntime = CreateParticipantAbilitySelectionRuntime(participant.ParticipantId, binding);
        var readyStatusHandler = CreateParticipantReadyStatusHandler(participant.ParticipantId, binding, abilitySelectionRuntime);

        return new ParticipantPreparationController(abilitySelectionRuntime, readyStatusHandler);
    }

    private ParticipantAbilitySelectionRuntime CreateParticipantAbilitySelectionRuntime(ParticipantId participantId, ParticipantAbilitySetup binding)
    {
        var progression = new AbilityPointsProgression(binding.InitialDurationSeconds, binding.DurationMultiplier);

        var offerFlow = new AbilityOfferSelectionFlow(
            binding.ParticipantHud,
            binding.AbilitySelectionViewContainer,
            progression,
            abilityCatalog,
            offerService,
            () => matchManager && matchManager.CanParticipantOpenAbilityMenu(participantId));

        return new ParticipantAbilitySelectionRuntime(binding.ParticipantHud, progression, offerFlow);
    }

    private ParticipantReadyStatusHandler CreateParticipantReadyStatusHandler(
        ParticipantId participantId,
        ParticipantAbilitySetup binding,
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime)
    {
        return new ParticipantReadyStatusHandler(participantId, binding.ParticipantHud, abilitySelectionRuntime, matchManager);
    }

    private void BeginRuntimeActivation()
    {
        if (!isInitialized) return;
        if (!isActivated) return;
        if (!isActiveAndEnabled) return;
        if (isRuntimeActive) return;

        SubscribeToTurnEvents();
        ForEachParticipantController(controller => controller.Enable());
        isRuntimeActive = true;
    }

    private void EndRuntimeActivation()
    {
        if (!isRuntimeActive) return;

        ForEachParticipantController(controller => controller.Disable());
        UnsubscribeFromTurnEvents();
        isRuntimeActive = false;
    }

    private bool TryGetParticipantController(ParticipantId participantId, out ParticipantPreparationController controller)
    {
        if (participantControllers.TryGetValue(participantId, out controller)) return true;

        Debug.LogError($"{nameof(ParticipantPreparationCoordinator)} on {name} " +
                       $"is missing a registered participant controller for participant {participantId}.", this);

        return false;
    }

    private ParticipantAbilitySetup GetSetupForSlot(ArenaSlotId slotId)
    {
        if (slotId == TemporaryTwoSideArena.LeftSlot)
            return leftParticipantProgression;

        if (slotId == TemporaryTwoSideArena.RightSlot)
            return rightParticipantProgression;

        throw new ArgumentOutOfRangeException(
            nameof(slotId), slotId, $"{nameof(ParticipantPreparationCoordinator)} only supports the temporary two-side arena slots.");
    }

    private void ForEachParticipantController(Action<ParticipantPreparationController> action)
    {
        if (action == null) return;

        foreach (var participantController in participantControllers.Values)
        {
            if (participantController == null) continue;
            action(participantController);
        }
    }
}
