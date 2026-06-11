using UnityEngine;

public abstract class AbilityDefinition : ScriptableObject
{
    [SerializeField] private string abilityId;
    [SerializeField] private string displayName;

    public string AbilityId => string.IsNullOrWhiteSpace(abilityId) ? name : abilityId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;

    public abstract IAbility CreateAbility();
}
