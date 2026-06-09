using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class Pool
{
    private readonly Dictionary<Type, Stack<Component>> availableItemsByType = new();
    private readonly HashSet<int> availableInstanceIds = new();

    public T TryGetFromPool<T>(T prefab, Vector2 position, Quaternion rotation) where T : Component, IPoolable
    {
        if (!prefab) return null;

        var item = TryGetAvailableItem<T>(out var pooledItem)
            ? pooledItem
            : UnityEngine.Object.Instantiate(prefab);

        item.transform.SetPositionAndRotation(position, rotation);
        item.gameObject.SetActive(true);
        item.OnGetFromPool();

        return item;
    }

    public void ReturnToPool<T>(T item) where T : Component, IPoolable
    {
        if (!item) return;

        var instanceId = item.gameObject.GetInstanceID();
        if (availableInstanceIds.Contains(instanceId)) return;

        item.OnMoveToPool();
        item.gameObject.SetActive(false);

        var itemType = item.GetType();
        if (!availableItemsByType.TryGetValue(itemType, out var itemsOfType))
        {
            itemsOfType = new Stack<Component>();
            availableItemsByType[itemType] = itemsOfType;
        }

        itemsOfType.Push(item);
        availableInstanceIds.Add(instanceId);
    }

    private bool TryGetAvailableItem<T>(out T item) where T : Component, IPoolable
    {
        item = null;
        var itemType = typeof(T);

        if (!availableItemsByType.TryGetValue(itemType, out var itemsOfType))
            return false;

        while (itemsOfType.Count > 0)
        {
            var candidate = itemsOfType.Pop() as T;
            if (!candidate) continue;

            availableInstanceIds.Remove(candidate.gameObject.GetInstanceID());
            item = candidate;
            return true;
        }

        return false;
    }
}
