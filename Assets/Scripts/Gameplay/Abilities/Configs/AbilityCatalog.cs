using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability Catalog", menuName = "Abilities/Ability Catalog")]
public sealed class AbilityCatalog : ScriptableObject
{
    [SerializeField] private AbilityConfig[] abilityConfigs = Array.Empty<AbilityConfig>();

    public IReadOnlyList<AbilityConfig> AbilityConfigs => abilityConfigs ?? Array.Empty<AbilityConfig>();
    public int Count => abilityConfigs?.Length ?? 0;
}
