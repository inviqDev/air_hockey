using UnityEngine;

[RequireComponent(typeof(AICommandSource))]
[RequireComponent(typeof(AiStrikerMovement))]
public sealed class AiStriker : StrikerBase
{
    [SerializeField] private AICommandSource aiCommandSource;

    private void Reset()
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();
    }

    protected override void ApplySetup(StrikerSetupContext setupContext)
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();

        if (aiCommandSource)
        {
            aiCommandSource.SetStrikerSide(setupContext.Side);
            aiCommandSource.SetCurrentPuck(setupContext.Puck);
        }
    }

    protected override void ResetCustomState()
    {
        if (aiCommandSource)
            aiCommandSource.ResetState();
    }

    protected override bool TryInitializeMovement()
    {
        if (!aiCommandSource)
            aiCommandSource = GetComponent<AICommandSource>();

        var aiMovement = Movement as AiStrikerMovement;
        if (!aiMovement)
        {
            Debug.LogError($"{nameof(AiStriker)} on {name} requires a {nameof(AiStrikerMovement)} component.", this);
            return false;
        }

        return aiMovement.InitializeAiStrikerMovement();
    }
}
