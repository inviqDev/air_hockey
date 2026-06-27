using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class AbilitySlotSelectionView : MonoBehaviour
{
    [SerializeField] private Image[] slotIconImages;
    [SerializeField] private Outline[] selectedOutlines;
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
        var slotCount = GetConfiguredSlotCount();
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
        var slotIconImage = slotIconImages[slotIndex];
        if (slotIconImage)
        {
            slotIconImage.sprite = icon;
            slotIconImage.enabled = icon;
            slotIconImage.color = isOccupied ? occupiedSlotColor : emptySlotColor;
        }

        var selectedOutline = selectedOutlines[slotIndex];
        if (selectedOutline)
            selectedOutline.enabled = isSelected;
    }

    private int GetConfiguredSlotCount()
    {
        if (slotIconImages == null || selectedOutlines == null) return 0;
        return Mathf.Min(slotIconImages.Length, selectedOutlines.Length);
    }

    private void ValidateReferences()
    {
        if (slotIconImages == null || slotIconImages.Length == 0)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires slot icon image references.", this);
        }

        if (selectedOutlines == null || selectedOutlines.Length == 0)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires selected outline references.", this);
        }

        if (slotIconImages != null && selectedOutlines != null && slotIconImages.Length != selectedOutlines.Length)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires matching slot icon and outline array sizes.", this);
        }

        var slotCount = GetConfiguredSlotCount();
        for (var i = 0; i < slotCount; i++)
        {
            if (!slotIconImages[i])
            {
                Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires a slot icon image reference at index {i}.", this);
            }

            if (!selectedOutlines[i])
            {
                Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires a selected outline reference at index {i}.", this);
            }
        }

        if (!emptySlotIcon)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionView)} on {name} requires an empty slot icon reference.", this);
        }
    }
}
