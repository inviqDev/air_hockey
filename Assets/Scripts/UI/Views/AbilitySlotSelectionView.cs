using System.Collections.Generic;
using UnityEngine;

public sealed class AbilitySlotSelectionView : MonoBehaviour
{
    [SerializeField] private AbilitySelectionItemsContainer slotItemsContainer;
    [SerializeField] private Sprite emptySlotIcon;

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
        var slotCount = slotItemsContainer ? slotItemsContainer.ItemCount : 0;
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

        var itemView = slotItemsContainer.GetItemView(slotIndex);
        if (!itemView) return;

        itemView.SetVisible(true);
        itemView.SetIcon(icon);
        itemView.SetSelected(isSelected);
    }

    private void ValidateReferences()
    {
        if (!slotItemsContainer)
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires an {nameof(AbilitySelectionItemsContainer)} reference.", this);

        if (!emptySlotIcon)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires an empty slot icon reference.", this);
        }
    }
}
