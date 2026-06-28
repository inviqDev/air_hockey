using UnityEngine;

public sealed class AbilitySlotSelectionItemsContainer : MonoBehaviour
{
    [SerializeField] private AbilitySlotSelectionItemView[] slotViews;

    public int SlotCount
    {
        get
        {
            EnsureSlotViewsCached();
            return slotViews?.Length ?? 0;
        }
    }

    private void Awake()
    {
        EnsureSlotViewsCached();
        ValidateReferences();
    }

    private void OnValidate()
    {
        RebuildDirectChildSlotCache();
        ValidateReferences();
    }

    public AbilitySlotSelectionItemView GetSlotView(int index)
    {
        EnsureSlotViewsCached();

        if (slotViews == null) return null;
        if (index < 0 || index >= slotViews.Length) return null;

        return slotViews[index];
    }

    private void EnsureSlotViewsCached()
    {
        if (HasValidDirectChildCache()) return;

        RebuildDirectChildSlotCache();
    }

    private bool HasValidDirectChildCache()
    {
        if (slotViews == null) return false;
        if (slotViews.Length != transform.childCount) return false;

        for (var i = 0; i < slotViews.Length; i++)
        {
            var slotView = slotViews[i];
            if (!slotView) return false;
            if (slotView.transform.parent != transform) return false;
            if (slotView.transform.GetSiblingIndex() != i) return false;
        }

        return true;
    }

    private void RebuildDirectChildSlotCache()
    {
        var directChildCount = transform.childCount;
        slotViews = new AbilitySlotSelectionItemView[directChildCount];

        for (var i = 0; i < directChildCount; i++)
        {
            var child = transform.GetChild(i);
            child.TryGetComponent(out slotViews[i]);
        }
    }

    private void ValidateReferences()
    {
        if (slotViews == null || slotViews.Length == 0)
        {
            Debug.LogError($"{nameof(AbilitySlotSelectionItemsContainer)} on {name} requires direct child {nameof(AbilitySlotSelectionItemView)} components.", this);
            return;
        }

        for (var i = 0; i < slotViews.Length; i++)
        {
            var slotView = slotViews[i];
            if (!slotView)
            {
                Debug.LogError($"{nameof(AbilitySlotSelectionItemsContainer)} on {name} requires a direct child {nameof(AbilitySlotSelectionItemView)} at sibling index {i}.", this);
                continue;
            }

            if (slotView.transform.parent != transform)
            {
                Debug.LogError($"{nameof(AbilitySlotSelectionItemsContainer)} on {name} requires {nameof(AbilitySlotSelectionItemView)} at index {i} to be a direct child.", this);
            }

            if (slotView.transform.GetSiblingIndex() != i)
            {
                Debug.LogError($"{nameof(AbilitySlotSelectionItemsContainer)} on {name} requires cached slot order to match direct-child sibling order at index {i}.", this);
            }
        }
    }
}
