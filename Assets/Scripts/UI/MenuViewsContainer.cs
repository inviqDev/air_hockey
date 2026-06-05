using System.Collections.Generic;
using UnityEngine;

public sealed class MenuViewsContainer : MonoBehaviour
{
    [SerializeField] private List<MenuViewBase> views = new();

    public void Initialize()
    {
        foreach (var view in views)
        {
            if (!view) continue;
            view.Initialize();
        }
    }

    private void OnValidate()
    {
        for (var i = views.Count - 1; i >= 0; i--)
        {
            if (views[i]) continue;
            views.RemoveAt(i);
        }
    }
}
