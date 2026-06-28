using UnityEngine;

public sealed class AbilitySelectionItemsContainer : MonoBehaviour
{
    [SerializeField] private AbilitySelectionItemView[] itemViews;

    public int ItemCount
    {
        get
        {
            EnsureItemViewsCached();
            return itemViews?.Length ?? 0;
        }
    }

    private void Awake()
    {
        EnsureItemViewsCached();
        ValidateReferences();
    }

    private void OnValidate()
    {
        RebuildDirectChildItemCache();
        ValidateReferences();
    }

    public AbilitySelectionItemView GetItemView(int index)
    {
        EnsureItemViewsCached();

        if (itemViews == null) return null;
        if (index < 0 || index >= itemViews.Length) return null;

        return itemViews[index];
    }

    private void EnsureItemViewsCached()
    {
        if (HasValidDirectChildCache()) return;

        RebuildDirectChildItemCache();
    }

    private bool HasValidDirectChildCache()
    {
        if (itemViews == null) return false;
        if (itemViews.Length != transform.childCount) return false;

        for (var i = 0; i < itemViews.Length; i++)
        {
            var itemView = itemViews[i];
            if (!itemView) return false;
            if (itemView.transform.parent != transform) return false;
            if (itemView.transform.GetSiblingIndex() != i) return false;
        }

        return true;
    }

    private void RebuildDirectChildItemCache()
    {
        var directChildCount = transform.childCount;
        itemViews = new AbilitySelectionItemView[directChildCount];

        for (var i = 0; i < directChildCount; i++)
        {
            var child = transform.GetChild(i);
            child.TryGetComponent(out itemViews[i]);
        }
    }

    private void ValidateReferences()
    {
        if (itemViews == null || itemViews.Length == 0)
        {
            Debug.LogError($"{nameof(AbilitySelectionItemsContainer)} on {name} requires direct child {nameof(AbilitySelectionItemView)} components.", this);
            return;
        }

        for (var i = 0; i < itemViews.Length; i++)
        {
            var itemView = itemViews[i];
            if (!itemView)
            {
                Debug.LogError($"{nameof(AbilitySelectionItemsContainer)} on {name} requires a direct child {nameof(AbilitySelectionItemView)} at sibling index {i}.", this);
                continue;
            }

            if (itemView.transform.parent != transform)
            {
                Debug.LogError($"{nameof(AbilitySelectionItemsContainer)} on {name} requires {nameof(AbilitySelectionItemView)} at index {i} to be a direct child.", this);
            }

            if (itemView.transform.GetSiblingIndex() != i)
            {
                Debug.LogError($"{nameof(AbilitySelectionItemsContainer)} on {name} requires cached item order to match direct-child sibling order at index {i}.", this);
            }
        }
    }
}
