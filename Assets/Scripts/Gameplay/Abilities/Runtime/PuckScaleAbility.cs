public sealed class PuckScaleAbility : AbilityBase
{
    public readonly struct Context
    {
        public Context(IPuckScaleController puckScaleController)
        {
            PuckScaleController = puckScaleController;
        }

        public IPuckScaleController PuckScaleController { get; }
    }

    private readonly PuckScaleAbilityConfig puckScaleConfig;
    private readonly Context context;

    public PuckScaleAbility(PuckScaleAbilityConfig config, Context context) : base(config)
    {
        puckScaleConfig = config;
        this.context = context;
    }

    protected override void ActivateCore()
    {
        context.PuckScaleController.ToggleScale(
            puckScaleConfig.DownscaledScaleMultiplier,
            puckScaleConfig.AnimationDuration,
            puckScaleConfig.AnimationEase);
    }

    protected override bool CanActivateCore()
    {
        if (context.PuckScaleController == null) return false;

        return context.PuckScaleController.CanToggleScale;
    }
}
