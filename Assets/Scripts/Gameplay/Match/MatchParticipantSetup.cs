using System;

public sealed class MatchParticipantSetup
{
    private readonly InputLayout inputLayout;

    public ParticipantId ParticipantId { get; }
    public ParticipantControlSource ControlSource { get; }
    public bool IsHuman => ControlSource == ParticipantControlSource.Human;
    public bool IsAi => ControlSource == ParticipantControlSource.Ai;

    private MatchParticipantSetup(ParticipantId participantId, ParticipantControlSource controlSource, InputLayout inputLayout)
    {
        ParticipantId = participantId;
        ControlSource = controlSource;
        this.inputLayout = inputLayout;
    }


    public InputLayout GetRequiredHumanInputLayout()
    {
        if (!IsHuman)
        {
            throw new InvalidOperationException(
                $"{nameof(MatchParticipantSetup)} for {ParticipantId} is not human-controlled.");
        }

        return inputLayout;
    }

    public static MatchParticipantSetup CreateHumanSetup(ParticipantId participantId, InputLayout inputLayout)
    {
        return new MatchParticipantSetup(participantId, ParticipantControlSource.Human, inputLayout);
    }

    public static MatchParticipantSetup CreateAiSetup(ParticipantId participantId)
    {
        return new MatchParticipantSetup(participantId, ParticipantControlSource.Ai, default);
    }
}
