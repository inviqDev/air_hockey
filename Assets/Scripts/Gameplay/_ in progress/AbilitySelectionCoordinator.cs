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
        public event Action<ParticipantProgressionBinding, int> OfferClicked;
        private PlayerInputReader inputReader;
        private bool isRoundBreakInputEnabled;

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

        public void UpdateInputReaderSubscription(bool shouldEnableRoundBreakInput)
        {
            var nextInputReader = AbilityController ? AbilityController.InputReader : null;
            if (inputReader == nextInputReader && isRoundBreakInputEnabled == shouldEnableRoundBreakInput)
                return;

            if (inputReader)
            {
                inputReader.AbilitySelectionMenuPressed -= HandleAbilitySelectionMenuPressed;

                if (inputReader != nextInputReader)
                    inputReader.SetRoundBreakInputEnabled(false);
            }

            inputReader = nextInputReader;
            isRoundBreakInputEnabled = shouldEnableRoundBreakInput;

            if (inputReader)
            {
                inputReader.AbilitySelectionMenuPressed += HandleAbilitySelectionMenuPressed;
                inputReader.SetRoundBreakInputEnabled(isRoundBreakInputEnabled);
            }
        }

        public void ClearInputReaderSubscription()
        {
            if (!inputReader) return;

            inputReader.SetRoundBreakInputEnabled(false);
            inputReader.AbilitySelectionMenuPressed -= HandleAbilitySelectionMenuPressed;
            inputReader = null;
            isRoundBreakInputEnabled = false;
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

        public void SubscribeToMenu()
        {
            if (!abilitySelectionMenu) return;

            abilitySelectionMenu.OfferClicked += HandleOfferClicked;
        }

        public void UnsubscribeFromMenu()
        {
            if (!abilitySelectionMenu) return;

            abilitySelectionMenu.OfferClicked -= HandleOfferClicked;
        }

        private void HandleOfferClicked(int offerIndex)
        {
            OfferClicked?.Invoke(this, offerIndex);
        }
    }

    [SerializeField] private TurnController turnController;
    [SerializeField] private AbilityCatalog abilityCatalog;
    [SerializeField] private ParticipantProgressionBinding leftProgression = new();
    [SerializeField] private ParticipantProgressionBinding rightProgression = new();

    private readonly AbilityOfferService offerService = new();

    private ParticipantAbilityProgression leftParticipant;
    private ParticipantAbilityProgression rightParticipant;
    private ParticipantAbilitySelectionSession leftSelectionSession;
    private ParticipantAbilitySelectionSession rightSelectionSession;
    private MatchManager matchManager;

    private void Awake()
    {
        ValidateReferences();
        leftParticipant = leftProgression.CreateRuntimeProgression();
        rightParticipant = rightProgression.CreateRuntimeProgression();
        leftSelectionSession = new ParticipantAbilitySelectionSession();
        rightSelectionSession = new ParticipantAbilitySelectionSession();
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
        CloseAllMenus();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void ResetProgression()
    {
        leftParticipant.ResetProgression();
        rightParticipant.ResetProgression();
        CloseAllMenus();
        RefreshAllHud();
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        leftParticipant.Tick(deltaTime);
        rightParticipant.Tick(deltaTime);
        leftProgression.UpdateInputReaderSubscription(CanOpenMenu(leftProgression, leftParticipant));
        rightProgression.UpdateInputReaderSubscription(CanOpenMenu(rightProgression, rightParticipant));
        RefreshAllHud();
        CloseMenusWhenLifecycleRequiresIt();
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
        CloseAllMenus();
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
        leftProgression.OfferClicked += HandleOfferClicked;
        rightProgression.OfferClicked += HandleOfferClicked;
        leftProgression.SubscribeToHud();
        rightProgression.SubscribeToHud();
        leftProgression.SubscribeToMenu();
        rightProgression.SubscribeToMenu();
    }

    private void UnsubscribeFromHudEvents()
    {
        leftProgression.UnsubscribeFromHud();
        rightProgression.UnsubscribeFromHud();
        leftProgression.UnsubscribeFromMenu();
        rightProgression.UnsubscribeFromMenu();
        leftProgression.ClearInputReaderSubscription();
        rightProgression.ClearInputReaderSubscription();
        leftProgression.AbilitySelectionMenuRequested -= HandleAbilitySelectionMenuRequested;
        rightProgression.AbilitySelectionMenuRequested -= HandleAbilitySelectionMenuRequested;
        leftProgression.OfferClicked -= HandleOfferClicked;
        rightProgression.OfferClicked -= HandleOfferClicked;
    }

    private void HandleAbilitySelectionMenuRequested(ParticipantProgressionBinding binding)
    {
        if (!TryGetParticipantContext(binding, out var participant, out var session)) return;

        if (session.State == ParticipantAbilitySelectionSession.SessionState.SelectingOffer)
        {
            CloseMenu(binding, session);
            return;
        }

        if (!CanOpenMenu(binding, participant)) return;

        var offers = BuildOffers(binding);
        if (!session.TryOpen(offers)) return;

        RenderMenu(binding, session);
    }

    private void HandleOfferClicked(ParticipantProgressionBinding binding, int offerIndex)
    {
        if (!TryGetParticipantContext(binding, out _, out var session)) return;
        if (!session.TrySelectOffer(offerIndex)) return;

        RenderMenu(binding, session);
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

    private ParticipantAbilitySelectionSession GetSelectionSession(ParticipantProgressionBinding binding)
    {
        if (ReferenceEquals(binding, leftProgression))
            return leftSelectionSession;

        if (ReferenceEquals(binding, rightProgression))
            return rightSelectionSession;

        return null;
    }

    private bool TryGetParticipantContext(
        ParticipantProgressionBinding binding,
        out ParticipantAbilityProgression participant,
        out ParticipantAbilitySelectionSession session)
    {
        participant = GetParticipantProgression(binding);
        session = GetSelectionSession(binding);
        return participant != null && session != null;
    }

    private bool CanOpenMenu(ParticipantProgressionBinding binding, ParticipantAbilityProgression participant)
    {
        if (binding == null) return false;
        if (participant == null) return false;
        if (participant.AvailableAbilityPoints <= 0) return false;
        if (!binding.AbilitySelectionMenu) return false;
        if (!matchManager) return false;

        var abilityController = binding.AbilityController;
        if (!abilityController) return false;

        var inputReader = abilityController.InputReader;
        if (!inputReader) return false;

        return matchManager.IsAbilityMenuInteractionAllowed;
    }

    private void CloseMenusWhenLifecycleRequiresIt()
    {
        if (ShouldCloseMenuForLifecycle(leftProgression, leftSelectionSession))
            CloseMenu(leftProgression, leftSelectionSession);

        if (ShouldCloseMenuForLifecycle(rightProgression, rightSelectionSession))
            CloseMenu(rightProgression, rightSelectionSession);
    }

    private bool ShouldCloseMenuForLifecycle(ParticipantProgressionBinding binding, ParticipantAbilitySelectionSession session)
    {
        if (binding == null || session == null) return false;
        if (session.State != ParticipantAbilitySelectionSession.SessionState.SelectingOffer) return false;
        if (!binding.AbilitySelectionMenu) return true;
        if (!matchManager) return true;

        var abilityController = binding.AbilityController;
        if (!abilityController) return true;

        var inputReader = abilityController.InputReader;
        if (!inputReader) return true;

        return !matchManager.IsAbilityMenuInteractionAllowed;
    }

    private void CloseAllMenus()
    {
        CloseMenu(leftProgression, leftSelectionSession);
        CloseMenu(rightProgression, rightSelectionSession);
    }

    private static void CloseMenu(ParticipantProgressionBinding binding, ParticipantAbilitySelectionSession session)
    {
        if (session == null) return;

        session.Close();

        if (binding == null) return;
        if (!binding.AbilitySelectionMenu) return;
        binding.AbilitySelectionMenu.Close();
    }

    private static void RenderMenu(ParticipantProgressionBinding binding, ParticipantAbilitySelectionSession session)
    {
        if (binding == null || session == null) return;
        if (!binding.AbilitySelectionMenu) return;

        if (session.State != ParticipantAbilitySelectionSession.SessionState.SelectingOffer)
        {
            binding.AbilitySelectionMenu.Close();
            return;
        }

        binding.AbilitySelectionMenu.Show(session.Offers, session.SelectedOfferIndex);
    }
}
