using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Outline))]
public sealed class AbilitySelectionItemView : MonoBehaviour
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

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void SetIcon(Sprite icon)
    {
        if (!iconImage) return;

        iconImage.sprite = icon;
        iconImage.enabled = icon;
    }

    public void SetSelected(bool isSelected)
    {
        if (!selectedOutline) return;

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
            Debug.LogError($"{nameof(AbilitySelectionItemView)} on {name} requires an {nameof(Image)} reference.", this);

        if (!selectedOutline)
            Debug.LogError($"{nameof(AbilitySelectionItemView)} on {name} requires an {nameof(Outline)} reference.", this);
    }
}
