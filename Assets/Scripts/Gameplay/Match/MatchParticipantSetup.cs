using System;

public enum ParticipantControlSource
{
    Human,
    Ai
}

public sealed class MatchParticipantSetup
{
    private readonly PlayerControlScheme humanControlScheme;

    private MatchParticipantSetup(
        ParticipantId participantId,
        ParticipantControlSource controlSource,
        PlayerControlScheme humanControlScheme)
    {
        ParticipantId = participantId;
        ControlSource = controlSource;
        this.humanControlScheme = humanControlScheme;
    }

    public ParticipantId ParticipantId { get; }
    public ParticipantControlSource ControlSource { get; }
    public bool IsHuman => ControlSource == ParticipantControlSource.Human;
    public bool IsAi => ControlSource == ParticipantControlSource.Ai;

    public PlayerControlScheme GetRequiredHumanControlScheme()
    {
        if (!IsHuman)
        {
            throw new InvalidOperationException(
                $"{nameof(MatchParticipantSetup)} for {ParticipantId} is not human-controlled.");
        }

        return humanControlScheme;
    }

    public static MatchParticipantSetup CreateHumanSetup(
        ParticipantId participantId,
        PlayerControlScheme controlScheme)
    {
        return new MatchParticipantSetup(participantId, ParticipantControlSource.Human, controlScheme);
    }

    public static MatchParticipantSetup CreateAiSetup(ParticipantId participantId)
    {
        return new MatchParticipantSetup(participantId, ParticipantControlSource.Ai, default);
    }
}
