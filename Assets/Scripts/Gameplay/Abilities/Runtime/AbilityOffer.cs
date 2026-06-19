public enum AbilityOfferKind
{
    NewAbility,
    Upgrade
}

public readonly struct AbilityOffer
{
    public AbilityOffer(AbilityConfig config, AbilityOfferKind kind)
    {
        Config = config;
        Kind = kind;
    }

    public AbilityConfig Config { get; }
    public AbilityOfferKind Kind { get; }
}
