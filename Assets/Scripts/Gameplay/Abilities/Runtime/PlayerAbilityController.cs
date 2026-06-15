using UnityEngine;
using System;

public sealed class PlayerAbilityController : MonoBehaviour
{
    private const int SlotCount = 4;

    [Header("References")]
    [SerializeField] private StrikerMovement strikerMovement;
    [SerializeField] private PlayerInputReader inputReader;

    private readonly IAbility[] abilitySlots = new IAbility[SlotCount];

    private AbilityFactory abilityFactory;
    
    private IStrikerMovementOverride movementOverride;
    private IPuckScaleController puckScaleController;
    private bool isSubscribedToInput;

    public event Action<int> AbilitySlotChanged;

    public int AbilitySlotCount => SlotCount;

    public void SetPuckScaleController(IPuckScaleController controller)
    {
        puckScaleController = controller;
    }

    private void Reset()
    {
        if (!strikerMovement)
            strikerMovement = GetComponent<StrikerMovement>();

        if (!strikerMovement)
            strikerMovement = GetComponentInParent<StrikerMovement>();

        if (!inputReader)
            inputReader = GetComponent<PlayerInputReader>();

        if (!inputReader)
            inputReader = GetComponentInParent<PlayerInputReader>();
    }

    private void Awake()
    {
        abilityFactory = new AbilityFactory();
        CacheReferences();
    }

    private void OnEnable()
    {
        SubscribeToInput();
    }

    private void OnDisable()
    {
        UnsubscribeFromInput();
    }

    public void AddAbilityToSlot(AbilityConfig config, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;
        if (!config) return;
        if (!EnsureReadyToCreateAbility()) return;

        var ability = abilityFactory.CreateAbility(config, movementOverride, puckScaleController);
        if (ability == null) return;

        SetAbilityToSlot(ability, slotIndex);
    }

    public void SetAbilityToSlot(IAbility ability, int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var previousAbility = abilitySlots[slotIndex];
        if (previousAbility == ability) return;

        previousAbility?.Dispose();
        abilitySlots[slotIndex] = ability;
        AbilitySlotChanged?.Invoke(slotIndex);
    }

    public void UseSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var ability = abilitySlots[slotIndex];
        if (ability == null) return;
        if (!ability.CanActivate) return;

        ability.Activate();
    }

    public IAbility GetAbilityInSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return null;

        return abilitySlots[slotIndex];
    }

    public AbilitySlotData GetAbilitySlotData(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex))
        {
            Debug.LogError($"{nameof(PlayerAbilityController)} on {name} cannot provide ability slot data for invalid slot index {slotIndex}.", this);
            return CreateEmptyAbilitySlotData();
        }

        var ability = abilitySlots[slotIndex];
        if (ability == null) return CreateEmptyAbilitySlotData();

        var cooldown = ability as IHasCooldown;
        var hasCooldown = cooldown != null;
        var cooldownDuration = 0f;
        var cooldownRemaining = 0f;

        if (hasCooldown)
        {
            cooldownDuration = cooldown.CooldownDuration;
            cooldownRemaining = cooldown.CooldownRemaining;
        }

        return new AbilitySlotData(
            ability.Config,
            ability.CanActivate,
            hasCooldown,
            cooldownDuration,
            cooldownRemaining);
    }

    private void FixedUpdate()
    {
        var deltaTime = Time.fixedDeltaTime;

        for (var i = 0; i < abilitySlots.Length; i++)
        {
            var ability = abilitySlots[i];
            if (ability == null) continue;

            ability.Tick(deltaTime);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromInput();
        DisposeAbilities();
    }

    private bool EnsureReadyToCreateAbility()
    {
        if (abilityFactory == null)
            abilityFactory = new AbilityFactory();

        return CacheReferences();
    }

    private bool CacheReferences()
    {
        if (!strikerMovement && !TryGetComponent(out strikerMovement))
            strikerMovement = GetComponentInParent<StrikerMovement>();

        if (!strikerMovement)
        {
            Debug.LogError($"{nameof(PlayerAbilityController)} on {name} requires a {nameof(StrikerMovement)} component on this GameObject or a parent.", this);
            movementOverride = null;
            return false;
        }

        if (!inputReader && !TryGetComponent(out inputReader))
            inputReader = GetComponentInParent<PlayerInputReader>();

        movementOverride = strikerMovement;
        return true;
    }

    private void SubscribeToInput()
    {
        if (isSubscribedToInput) return;
        if (!CacheReferences()) return;

        if (!inputReader)
        {
            Debug.LogError($"{nameof(PlayerAbilityController)} on {name} requires a {nameof(PlayerInputReader)} component on this GameObject or a parent.", this);
            return;
        }

        inputReader.AbilitySlotPressed += UseSlot;
        isSubscribedToInput = true;
    }

    private void UnsubscribeFromInput()
    {
        if (!isSubscribedToInput) return;
        if (inputReader)
            inputReader.AbilitySlotPressed -= UseSlot;

        isSubscribedToInput = false;
    }

    private void DisposeAbilities()
    {
        for (var i = 0; i < abilitySlots.Length; i++)
        {
            var ability = abilitySlots[i];
            if (ability == null) continue;

            ability.Dispose();
            abilitySlots[i] = null;
            AbilitySlotChanged?.Invoke(i);
        }
    }

    private AbilitySlotData CreateEmptyAbilitySlotData()
    {
        return new AbilitySlotData(
            null,
            false,
            false,
            0f,
            0f);
    }

    private static bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < SlotCount;
    }
}
