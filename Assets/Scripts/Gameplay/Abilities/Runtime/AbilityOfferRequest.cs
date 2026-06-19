using System.Collections.Generic;

public readonly struct AbilityOfferRequest
{
    public AbilityOfferRequest(
        AbilityCatalog catalog,
        IReadOnlyCollection<string> ownedAbilityIds,
        bool hasEmptyAbilitySlot)
    {
        Catalog = catalog;
        OwnedAbilityIds = ownedAbilityIds;
        HasEmptyAbilitySlot = hasEmptyAbilitySlot;
    }

    public AbilityCatalog Catalog { get; }
    public IReadOnlyCollection<string> OwnedAbilityIds { get; }
    public bool HasEmptyAbilitySlot { get; }
}
