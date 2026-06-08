using UnityEngine;

[RequireComponent(typeof(AICommandSource))]
public sealed class AiStriker : StrikerBase
{
    [SerializeField] private AICommandSource aiCommandSource;

    private void Reset()
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();
    }

    protected override void ApplyStrikerSetup(StrikerSetupContext setupContext)
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();

        if (aiCommandSource)
        {
            aiCommandSource.SetStrikerSide(setupContext.Side);
            aiCommandSource.SetCurrentPuck(setupContext.Puck);
        }
    }

    protected override void ResetCustomStrikerState()
    {
        if (aiCommandSource)
            aiCommandSource.ResetState();
    }

    protected override IMovementCommandSource GetMovementCommandSource()
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();

        return aiCommandSource;
    }
}
