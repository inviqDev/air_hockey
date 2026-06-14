using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PlayerAbilityHudView : MonoBehaviour
{
    private const int ExpectedAbilitySlotCount = 4;

    [Header("Free Ability Timer")]
    [SerializeField] private TextMeshProUGUI freeAbilityTimerText;
    
    [Header("Add Ability Button")]
    [SerializeField] private Button addAbilityButton;
    [SerializeField] private TextMeshProUGUI availableAmountText;

    [Header("Ability Slots")]
    [SerializeField] private AbilitySlotHudView[] abilitySlotViews;

    private PlayerAbilityController abilityController;
    private bool isSubscribed;

    public void SetFreeAbilityTimerText(string value)
    {
        if (!freeAbilityTimerText) return;
        freeAbilityTimerText.text = value;
    }

    public void SetAvailableAmount(int amount)
    {
        if (!availableAmountText) return;

        var clampedAmount = Mathf.Max(0, amount);
        availableAmountText.text = clampedAmount.ToString();

        if (!addAbilityButton) return;

        addAbilityButton.interactable = clampedAmount > 0;
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
        SetAllSlotsEmpty();
    }

    private void OnEnable()
    {
        SubscribeToController();
        RefreshAllSlots();
    }

    private void OnDisable()
    {
        UnsubscribeFromController();
    }

    private void Update()
    {
        RefreshCooldowns();
    }

    private void ValidateReferences()
    {
        if (!freeAbilityTimerText)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires a free ability timer text reference.", this);

        if (!addAbilityButton)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires an add ability button reference.", this);

        if (!availableAmountText)
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires an available amount text reference.", this);

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

        for (var i = slotCount; i < abilitySlotViews.Length; i++)
        {
            abilitySlotViews[i].SetEmpty();
        }
    }

    private void RefreshSlot(int slotIndex)
    {
        if (!IsValidSlotIndex(slotIndex)) return;

        var slotData = abilityController.GetAbilitySlotData(slotIndex);
        ApplySlotData(abilitySlotViews[slotIndex], slotData);
    }

    private void RefreshCooldowns()
    {
        if (!abilityController) return;

        var slotCount = GetVisibleSlotCount();
        for (var i = 0; i < slotCount; i++)
        {
            var slotData = abilityController.GetAbilitySlotData(i);
            abilitySlotViews[i].SetCooldown(slotData.HasCooldown, slotData.CooldownNormalized);
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
        slotView.SetCooldown(slotData.HasCooldown, slotData.CooldownNormalized);
    }

    private void SetAllSlotsEmpty()
    {
        for (var i = 0; i < abilitySlotViews.Length; i++)
        {
            abilitySlotViews[i].SetEmpty();
        }
    }

    private void CacheAbilitySlotViews()
    {
        if (abilitySlotViews != null && abilitySlotViews.Length == ExpectedAbilitySlotCount) return;

        abilitySlotViews = GetComponentsInChildren<AbilitySlotHudView>(true);
    }

    private void ValidateSlotCount()
    {
        if (abilitySlotViews == null)
        {
            Debug.LogError($"{nameof(PlayerAbilityHudView)} on {name} requires ability slot view references.", this);
            return;
        }

        var expectedSlotCount = abilityController
            ? abilityController.AbilitySlotCount
            : ExpectedAbilitySlotCount;

        if (abilitySlotViews.Length != expectedSlotCount)
        {
            Debug.LogError(
                $"{nameof(PlayerAbilityHudView)} on {name} has {abilitySlotViews.Length} ability slot views, but expected {expectedSlotCount}.",
                this);
        }
    }

    private int GetVisibleSlotCount()
    {
        return Mathf.Min(abilityController.AbilitySlotCount, abilitySlotViews.Length);
    }

    private bool IsValidSlotIndex(int slotIndex)
    {
        return slotIndex >= 0 && slotIndex < GetVisibleSlotCount();
    }
}
