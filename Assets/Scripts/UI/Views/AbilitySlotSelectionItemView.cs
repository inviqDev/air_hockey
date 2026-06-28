using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public sealed class AbilitySlotSelectionItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Outline selectedOutline;

    private void Awake()
    {
        CacheReferences();
        ValidateReferences();
    }

    private void OnValidate()
    {
        CacheReferences();
        ValidateReferences();
    }

    public void Render(Sprite icon, bool isOccupied, bool isSelected, Color occupiedColor, Color emptyColor)
    {
        if (iconImage)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon;
            iconImage.color = isOccupied ? occupiedColor : emptyColor;
        }

        if (selectedOutline)
            selectedOutline.enabled = isSelected;
    }

    private void CacheReferences()
    {
        if (!iconImage)
            iconImage = GetComponent<Image>();

        if (!selectedOutline)
            selectedOutline = GetComponent<Outline>();
    }

    private void ValidateReferences()
    {
        if (!iconImage)
            Debug.LogError($"{nameof(AbilitySlotSelectionItemView)} on {name} requires an {nameof(Image)} reference.", this);

        if (!selectedOutline)
            Debug.LogError($"{nameof(AbilitySlotSelectionItemView)} on {name} requires an {nameof(Outline)} reference.", this);
    }
}
