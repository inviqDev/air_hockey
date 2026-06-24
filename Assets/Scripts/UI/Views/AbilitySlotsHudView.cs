using UnityEngine;

public sealed class AbilitySlotsHudView : MonoBehaviour
{
    [SerializeField] private AbilitySlot[] abilitySlots;

    private PlayerAbilityController abilityController;
    
    private bool isInitialized;
    private bool isSubscribed;

    public void Initialize()
    {
        if (isInitialized) return;

        ValidateReferences();
        SetAllSlotsEmpty();
        isInitialized = true;

        if (isActiveAndEnabled)
        {
            SubscribeToController();
            RefreshAllSlots();
        }
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        Initialize();

        if (abilityController == controller) return;

        UnsubscribeFromController();
        abilityController = controller;

        if (isActiveAndEnabled)
            SubscribeToController();

        ValidateControllerSlotCompatibility();
        RefreshAllSlots();
    }

    private void OnEnable()
    {
        if (!isInitialized) return;

        SubscribeToController();
        RefreshAllSlots();
    }

    private void OnDisable()
    {
        if (!isInitialized) return;
        UnsubscribeFromController();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    private void Update()
    {
        if (!isInitialized) return;
        RefreshCooldowns();
    }

    private void SubscribeToController()
    {
        if (isSubscribed) return;
        if (!abilityController) return;

        abilityController.AbilitySlotChanged += HandleAbilitySlotChanged;
        isSubscribed = true;
    }

    private void UnsubscribeFromController()
    {
        if (!isSubscribed) return;

        if (abilityController)
            abilityController.AbilitySlotChanged -= HandleAbilitySlotChanged;

        isSubscribed = false;
    }

    private void HandleAbilitySlotChanged(int slotIndex)
    {
        RefreshSlot(slotIndex);
    }

    private void RefreshAllSlots()
    {
        if (!abilityController)
        {
            SetAllSlotsEmpty();
            return;
        }

        var slotCount = GetVisibleSlotCount();
        for (var i = 0; i < slotCount; i++)
        {
            RefreshSlot(i);
        }

        for (var i = slotCount; i < abilitySlots.Length; i++)
        {
            abilitySlots[i].SetEmpty();
        }
    }

    private void RefreshSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var slotData = abilityController.GetAbilitySlotData(slotIndex);
        ApplySlotData(abilitySlots[slotIndex], slotData);
    }

    private void RefreshCooldowns()
    {
        if (!abilityController) return;

        var slotCount = GetVisibleSlotCount();
        for (var i = 0; i < slotCount; i++)
        {
            var slotData = abilityController.GetAbilitySlotData(i);
            if (slotData.IsEmpty) continue;

            abilitySlots[i].SetCooldown(slotData.ShouldUseActiveCooldownBackground, slotData.CooldownVisualNormalized);
        }
    }

    private void ApplySlotData(AbilitySlot slotView, AbilitySlotData slotData)
    {
        if (slotData.IsEmpty)
        {
            slotView.SetEmpty();
            return;
        }

        slotView.SetAbility(slotData.Config);
        slotView.SetCooldown(slotData.ShouldUseActiveCooldownBackground, slotData.CooldownVisualNormalized);
    }

    private void SetAllSlotsEmpty()
    {
        foreach (var slot in abilitySlots)
        {
            if (!slot) continue;
            slot.SetEmpty();
        }
    }

    private void ValidateReferences()
    {
        ValidateConfiguredSlotViews();

        if (abilitySlots == null) return;

        for (var i = 0; i < abilitySlots.Length; i++)
        {
            if (abilitySlots[i]) continue;

            Debug.LogError(
                $"{nameof(AbilitySlotsHudView)} on {name} requires an {nameof(AbilitySlot)} reference for slot index {i}.",
                this);
        }
    }

    private void ValidateConfiguredSlotViews()
    {
        if (abilitySlots == null)
        {
            Debug.LogError($"{nameof(AbilitySlotsHudView)} on {name} requires ability slot view references.", this);
            return;
        }

        if (abilitySlots.Length == 0)
        {
            Debug.LogError(
                $"{nameof(AbilitySlotsHudView)} on {name} requires at least one {nameof(AbilitySlot)} reference.",
                this);
        }
    }

    private void ValidateControllerSlotCompatibility()
    {
        if (!abilityController) return;
        if (abilitySlots == null) return;

        var runtimeSlotCount = abilityController.AbilitySlotCount;
        var configuredSlotCount = abilitySlots.Length;

        if (configuredSlotCount != runtimeSlotCount)
        {
            Debug.LogError(
                $"{nameof(AbilitySlotsHudView)} on {name} has {configuredSlotCount} configured slot views, but {nameof(PlayerAbilityController)} expects {runtimeSlotCount} slots.",
                this);
        }
    }

    private int GetVisibleSlotCount()
    {
        return Mathf.Min(abilityController.AbilitySlotCount, abilitySlots.Length);
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < GetVisibleSlotCount();
    }
}
