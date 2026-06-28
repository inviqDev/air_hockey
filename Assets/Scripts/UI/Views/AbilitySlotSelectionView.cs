using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySlotSelectionView : MonoBehaviour
{
    [SerializeField] private AbilitySlotSelectionItemsContainer slotItemsContainer;
    [SerializeField] private Sprite emptySlotIcon;
    [SerializeField] private Color occupiedSlotColor = Color.white;
    [SerializeField] private Color emptySlotColor = new Color(0.6f, 0.6f, 0.6f, 1f);

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void Show(IReadOnlyList<AbilitySlotData> slots, int selectedSlotIndex)
    {
        gameObject.SetActive(true);
        Render(slots, selectedSlotIndex);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Render(IReadOnlyList<AbilitySlotData> slots, int selectedSlotIndex)
    {
        var slotCount = slotItemsContainer ? slotItemsContainer.SlotCount : 0;
        for (var i = 0; i < slotCount; i++)
        {
            var hasSlotData = slots != null && i < slots.Count;
            var isOccupied = hasSlotData && slots[i].HasAbility;
            var icon = isOccupied && slots[i].Config
                ? slots[i].Config.Icon
                : emptySlotIcon;

            ApplySlotVisual(i, icon, isOccupied, hasSlotData && i == selectedSlotIndex);
        }
    }

    private void ApplySlotVisual(int slotIndex, Sprite icon, bool isOccupied, bool isSelected)
    {
        if (!slotItemsContainer) return;

        var slotView = slotItemsContainer.GetSlotView(slotIndex);
        if (!slotView) return;

        slotView.Render(icon, isOccupied, isSelected, occupiedSlotColor, emptySlotColor);
    }

    private void ValidateReferences()
    {
        if (!slotItemsContainer)
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires an {nameof(AbilitySlotSelectionItemsContainer)} reference.", this);

        if (!emptySlotIcon)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires an empty slot icon reference.", this);
        }
    }
}
