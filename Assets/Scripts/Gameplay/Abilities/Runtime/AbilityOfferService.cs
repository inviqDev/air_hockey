using System.Collections.Generic;

public sealed class AbilityOfferService
{
    private const int MaxOfferCount = 3;

    public IReadOnlyList<AbilityOffer> BuildOffers(AbilityOfferRequest request)
    {
        var offers = new List<AbilityOffer>(MaxOfferCount);
        var catalog = request.Catalog;
        if (!catalog) return offers;

        var ownedAbilityIds = request.OwnedAbilityIds;
        var abilityConfigs = catalog.AbilityConfigs;

        for (var i = 0; i < abilityConfigs.Count; i++)
        {
            var config = abilityConfigs[i];
            if (!config) continue;

            var isOwned = ContainsOwnedAbilityId(ownedAbilityIds, config.Id);
            if (!request.HasEmptyAbilitySlot && !isOwned) continue;

            var kind = isOwned
                ? AbilityOfferKind.Upgrade
                : AbilityOfferKind.NewAbility;

            offers.Add(new AbilityOffer(config, kind));

            if (offers.Count >= MaxOfferCount)
                break;
        }

        return offers;
    }

    private static bool ContainsOwnedAbilityId(IReadOnlyCollection<string> ownedAbilityIds, string abilityId)
    {
        if (ownedAbilityIds == null) return false;
        if (string.IsNullOrEmpty(abilityId)) return false;

        foreach (var ownedId in ownedAbilityIds)
        {
            if (ownedId == abilityId)
                return true;
        }

        return false;
    }
}
