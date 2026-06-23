using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilityHudView : MonoBehaviour
{
    private const int SlotCount = 4;

    [Header("Free Ability Timer")]
    [SerializeField] private TextMeshProUGUI freeTimerText;
    
    [Header("Add Ability Button")]
    [SerializeField] private Button plusButton;
    [SerializeField] private TextMeshProUGUI availablePointsText;

    [Header("Ability Slots")]
    [SerializeField] private AbilitySlotHudView[] abilitySlots;

    private PlayerAbilityController abilityController;
    private bool isSubscribed;

    public event Action PlusAbilityButtonClicked;

    public void SetFreeAbilityTimerText(string value)
    {
        if (!freeTimerText) return;
        freeTimerText.text = value;
    }

    public void SetAvailableAmount(int amount)
    {
        if (!availablePointsText) return;

        var clampedAmount = Mathf.Max(0, amount);
        availablePointsText.text = clampedAmount.ToString();
    }

    public void SetAbilityMenuButtonEnabled(bool isEnabled)
    {
        if (!plusButton) return;

        plusButton.interactable = isEnabled;
    }

    public void BindAbilityController(PlayerAbilityController controller)
    {
        if (abilityController == controller) return;

        UnsubscribeFromController();
        abilityController = controller;

        if (isActiveAndEnabled)
            SubscribeToController();

        ValidateSlotCount();
        RefreshAllSlots();
    }

    private void OnValidate()
    {
        CacheAbilitySlotViews();
        ValidateReferences();
    }

    private void Awake()
    {
        CacheAbilitySlotViews();
        ValidateReferences();
        SetAbilityMenuButtonEnabled(false);
        SetAllSlotsEmpty();
    }

    private void OnEnable()
    {
        if (plusButton)
            plusButton.onClick.AddListener(HandleAbilityMenuButtonClicked);

        SubscribeToController();
        RefreshAllSlots();
    }

    private void OnDisable()
    {
        if (plusButton)
            plusButton.onClick.RemoveListener(HandleAbilityMenuButtonClicked);

        UnsubscribeFromController();
    }

    private void Update()
    {
        RefreshCooldowns();
    }

    private void ValidateReferences()
    {
        if (!freeTimerText)
            Debug.LogError($"{nameof(AbilityHudView)} on {name} requires a free ability timer text reference.", this);

        if (!plusButton)
            Debug.LogError($"{nameof(AbilityHudView)} on {name} requires an add ability button reference.", this);

        if (!availablePointsText)
            Debug.LogError($"{nameof(AbilityHudView)} on {name} requires an available amount text reference.", this);

        ValidateSlotCount();
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

    private void HandleAbilityMenuButtonClicked()
    {
        PlusAbilityButtonClicked?.Invoke();
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
            abilitySlots[i].SetCooldown(slotData.ShouldUseActiveCooldownBackground, slotData.CooldownVisualNormalized);
        }
    }

    private void ApplySlotData(AbilitySlotHudView slotView, AbilitySlotData slotData)
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
            slot.SetEmpty();
        }
    }

    private void CacheAbilitySlotViews()
    {
        if (abilitySlots != null && abilitySlots.Length == SlotCount) return;

        abilitySlots = GetComponentsInChildren<AbilitySlotHudView>(true);
    }

    private void ValidateSlotCount()
    {
        if (abilitySlots == null)
        {
            Debug.LogError($"{nameof(AbilityHudView)} on {name} requires ability slot view references.", this);
            return;
        }

        var expectedSlotCount = abilityController
            ? abilityController.AbilitySlotCount
            : SlotCount;

        if (abilitySlots.Length != expectedSlotCount)
        {
            Debug.LogError(
                $"{nameof(AbilityHudView)} on {name} has {abilitySlots.Length} ability slot views, but expected {expectedSlotCount}.",
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
