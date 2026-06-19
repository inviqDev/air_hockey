using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySelectionCoordinator : MonoBehaviour
{
    [Serializable]
    private sealed class ParticipantProgressionBinding
    {
        [SerializeField] private AbilityHudView abilityHud;
        [SerializeField] private AbilitySelectionMenu abilitySelectionMenu;
        [SerializeField, Min(1f)] private float initialDurationSeconds = 30f;
        [SerializeField, Min(1f)] private float durationMultiplier = 1.5f;

        public event Action<ParticipantProgressionBinding> AbilitySelectionMenuRequested;
        private PlayerInputReader inputReader;

        public AbilitySelectionMenu AbilitySelectionMenu => abilitySelectionMenu;
        public PlayerAbilityController AbilityController => abilityHud ? abilityHud.AbilityController : null;

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
            abilityHud.SetAbilityMenuButtonEnabled(canOpenAbilityMenu);
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

        public void UpdateInputReaderSubscription()
        {
            var nextInputReader = AbilityController ? AbilityController.InputReader : null;
            if (inputReader == nextInputReader) return;

            if (inputReader)
                inputReader.AbilitySelectionMenuPressed -= HandleAbilitySelectionMenuPressed;

            inputReader = nextInputReader;

            if (inputReader)
                inputReader.AbilitySelectionMenuPressed += HandleAbilitySelectionMenuPressed;
        }

        public void ClearInputReaderSubscription()
        {
            if (!inputReader) return;

            inputReader.AbilitySelectionMenuPressed -= HandleAbilitySelectionMenuPressed;
            inputReader = null;
        }

        public void Validate(string fieldName, UnityEngine.Object context)
        {
            if (!abilityHud)
                Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {context.name} requires a {nameof(AbilityHudView)} reference for {fieldName}.", context);

            if (!abilitySelectionMenu)
                Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {context.name} requires an {nameof(AbilitySelectionMenu)} reference for {fieldName}.", context);
        }

        private void HandlePlusAbilityButtonClicked()
        {
            AbilitySelectionMenuRequested?.Invoke(this);
        }

        private void HandleAbilitySelectionMenuPressed()
        {
            AbilitySelectionMenuRequested?.Invoke(this);
        }
    }

    [SerializeField] private TurnController turnController;
    [SerializeField] private AbilityCatalog abilityCatalog;
    [SerializeField] private ParticipantProgressionBinding leftProgression = new();
    [SerializeField] private ParticipantProgressionBinding rightProgression = new();

    private readonly AbilityOfferService offerService = new();

    private ParticipantAbilityProgression leftParticipant;
    private ParticipantAbilityProgression rightParticipant;
    private MatchManager matchManager;

    private void Awake()
    {
        ValidateReferences();
        leftParticipant = leftProgression.CreateRuntimeProgression();
        rightParticipant = rightProgression.CreateRuntimeProgression();
        RefreshAllHud();
    }

    public void SetMatchManager(MatchManager manager)
    {
        matchManager = manager;
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
        CloseMenu(leftProgression);
        CloseMenu(rightProgression);
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ResetProgression()
    {
        leftParticipant.ResetProgression();
        rightParticipant.ResetProgression();
        CloseMenu(leftProgression);
        CloseMenu(rightProgression);
        RefreshAllHud();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        leftParticipant.Tick(deltaTime);
        rightParticipant.Tick(deltaTime);
        leftProgression.UpdateInputReaderSubscription();
        rightProgression.UpdateInputReaderSubscription();
        RefreshAllHud();
        CloseMenusWhenUnavailable();
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
        CloseMenu(leftProgression);
        CloseMenu(rightProgression);
        RefreshAllHud();
    }

    private void HandleTurnEnded()
    {
        leftParticipant.StopTurnProgression();
        rightParticipant.StopTurnProgression();
        RefreshAllHud();
    }

    private void SubscribeToHudEvents()
    {
        leftProgression.AbilitySelectionMenuRequested += HandleAbilitySelectionMenuRequested;
        rightProgression.AbilitySelectionMenuRequested += HandleAbilitySelectionMenuRequested;
        
        if (leftProgression.AbilitySelectionMenu)
            leftProgression.AbilitySelectionMenu.OfferSelected += HandleOfferSelected;
        
        if (rightProgression.AbilitySelectionMenu)
            rightProgression.AbilitySelectionMenu.OfferSelected += HandleOfferSelected;
        
        leftProgression.SubscribeToHud();
        rightProgression.SubscribeToHud();
    }

    private void UnsubscribeFromHudEvents()
    {
        leftProgression.UnsubscribeFromHud();
        rightProgression.UnsubscribeFromHud();
        leftProgression.ClearInputReaderSubscription();
        rightProgression.ClearInputReaderSubscription();
        
        leftProgression.AbilitySelectionMenuRequested -= HandleAbilitySelectionMenuRequested;
        rightProgression.AbilitySelectionMenuRequested -= HandleAbilitySelectionMenuRequested;
        
        if (leftProgression.AbilitySelectionMenu)
            leftProgression.AbilitySelectionMenu.OfferSelected -= HandleOfferSelected;
        
        if (rightProgression.AbilitySelectionMenu)
            rightProgression.AbilitySelectionMenu.OfferSelected -= HandleOfferSelected;
    }

    private void HandleAbilitySelectionMenuRequested(ParticipantProgressionBinding binding)
    {
        var participant = GetParticipantProgression(binding);
        if (participant == null) return;
        if (!CanOpenMenu(binding, participant)) return;

        var offers = BuildOffers(binding);
        if (binding.AbilitySelectionMenu)
            binding.AbilitySelectionMenu.Toggle(offers);
    }

    private void HandleOfferSelected(AbilityOffer offer)
    {
    }

    private void RefreshAllHud()
    {
        leftProgression.RefreshHud(leftParticipant, CanOpenMenu(leftProgression, leftParticipant));
        rightProgression.RefreshHud(rightParticipant, CanOpenMenu(rightProgression, rightParticipant));
    }

    private void ValidateReferences()
    {
        if (!turnController)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires a {nameof(TurnController)} reference.", this);

        if (!abilityCatalog)
            Debug.LogError($"{nameof(AbilitySelectionCoordinator)} on {name} requires an {nameof(AbilityCatalog)} reference.", this);

        leftProgression.Validate(nameof(leftProgression), this);
        rightProgression.Validate(nameof(rightProgression), this);
    }

    private IReadOnlyList<AbilityOffer> BuildOffers(ParticipantProgressionBinding binding)
    {
        if (!abilityCatalog)
            return Array.Empty<AbilityOffer>();

        var abilityController = binding.AbilityController;
        if (!abilityController)
            return Array.Empty<AbilityOffer>();

        var ownedAbilityIds = new List<string>(abilityController.AbilitySlotCount);
        var hasEmptyAbilitySlot = false;

        for (var i = 0; i < abilityController.AbilitySlotCount; i++)
        {
            var ability = abilityController.GetAbilityInSlot(i);
            if (ability == null)
            {
                hasEmptyAbilitySlot = true;
                continue;
            }

            if (!string.IsNullOrEmpty(ability.Id))
                ownedAbilityIds.Add(ability.Id);
        }

        return offerService.BuildOffers(new AbilityOfferRequest(abilityCatalog, ownedAbilityIds, hasEmptyAbilitySlot));
    }

    private ParticipantAbilityProgression GetParticipantProgression(ParticipantProgressionBinding binding)
    {
        if (ReferenceEquals(binding, leftProgression))
            return leftParticipant;

        if (ReferenceEquals(binding, rightProgression))
            return rightParticipant;

        return null;
    }

    private bool CanOpenMenu(ParticipantProgressionBinding binding, ParticipantAbilityProgression participant)
    {
        if (binding == null) return false;
        if (participant == null) return false;
        if (participant.AvailableAbilityPoints <= 0) return false;
        if (!binding.AbilityController) return false;
        if (!binding.AbilityController.InputReader) return false;
        if (!binding.AbilitySelectionMenu) return false;
        if (!matchManager) return false;

        return matchManager.IsAbilityMenuInteractionAllowed;
    }

    private void CloseMenusWhenUnavailable()
    {
        if (!CanOpenMenu(leftProgression, leftParticipant))
            CloseMenu(leftProgression);

        if (!CanOpenMenu(rightProgression, rightParticipant))
            CloseMenu(rightProgression);
    }

    private static void CloseMenu(ParticipantProgressionBinding binding)
    {
        if (binding == null) return;
        if (!binding.AbilitySelectionMenu) return;

        binding.AbilitySelectionMenu.Close();
    }
}
