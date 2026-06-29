using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class ParticipantPreparationCoordinator : MonoBehaviour
{
    [SerializeField] private TurnController turnController;
    [SerializeField] private AbilityCatalog abilityCatalog;
    
    [SerializeField] private ParticipantAbilitySetup leftParticipantProgression = new();
    [SerializeField] private ParticipantAbilitySetup rightParticipantProgression = new();

    private readonly AbilityOfferService offerService = new();
    private readonly Dictionary<PlayerSide, ParticipantPreparationController> participantControllers = new();
    
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

        if (isInitialized) return;

        participantControllers.Clear();

        var leftParticipantController = CreateParticipantPreparationController(PlayerSide.Left, leftParticipantProgression);
        var rightParticipantController = CreateParticipantPreparationController(PlayerSide.Right, rightParticipantProgression);

        participantControllers.Add(PlayerSide.Left, leftParticipantController);
        participantControllers.Add(PlayerSide.Right, rightParticipantController);
        
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

    public void BindParticipantAbilityController(PlayerSide side, PlayerAbilityController abilityController)
    {
        if (!isInitialized) return;
        if (!TryGetParticipantController(side, out var participantController)) return;
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

    private ParticipantPreparationController CreateParticipantPreparationController(PlayerSide side, ParticipantAbilitySetup binding)
    {
        var abilitySelectionRuntime = CreateParticipantAbilitySelectionRuntime(side, binding);
        var readyStatusHandler = CreateParticipantReadyStatusHandler(side, binding, abilitySelectionRuntime);

        return new ParticipantPreparationController(side, abilitySelectionRuntime, readyStatusHandler);
    }

    private ParticipantAbilitySelectionRuntime CreateParticipantAbilitySelectionRuntime(PlayerSide side, ParticipantAbilitySetup binding)
    {
        var progression = new AbilityPointsProgression(binding.InitialDurationSeconds, binding.DurationMultiplier);

        var offerFlow = new AbilityOfferSelectionFlow(
            binding.ParticipantHud,
            binding.AbilitySelectionViewContainer,
            progression,
            abilityCatalog,
            offerService,
            () => matchManager && matchManager.IsAbilityMenuInteractionAllowed);

        offerFlow.SetCanOpenMenuPredicate(() => !matchManager || !matchManager.IsParticipantReady(side));

        return new ParticipantAbilitySelectionRuntime(binding.ParticipantHud, progression, offerFlow);
    }

    private ParticipantReadyStatusHandler CreateParticipantReadyStatusHandler(
        PlayerSide side,
        ParticipantAbilitySetup binding,
        ParticipantAbilitySelectionRuntime abilitySelectionRuntime)
    {
        return new ParticipantReadyStatusHandler(side, binding.ParticipantHud, abilitySelectionRuntime, matchManager);
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

    private bool TryGetParticipantController(PlayerSide side, out ParticipantPreparationController controller)
    {
        if (participantControllers.TryGetValue(side, out controller))
            return true;

        if (side != PlayerSide.Left && side != PlayerSide.Right)
        {
            Debug.LogError(
                $"{nameof(ParticipantPreparationCoordinator)} on {name} received unsupported {nameof(PlayerSide)} value: {side}.",
                this);
            return false;
        }

        Debug.LogError(
            $"{nameof(ParticipantPreparationCoordinator)} on {name} is missing a registered participant controller for valid side {side}.",
            this);
        return false;
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
