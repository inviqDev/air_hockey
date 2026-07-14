using System;

public sealed class ParticipantGameplayInputBinding : IDisposable
{
    private readonly InputReader inputReader;

    private PlayerStrikerMovement movement;
    private PlayerAbilityController abilityController;

    public ParticipantGameplayInputBinding(InputReader inputReader)
    {
        this.inputReader = inputReader ?? throw new ArgumentNullException(nameof(inputReader));
    }

    public void Bind(PlayerStrikerMovement nextMovement, PlayerAbilityController nextAbilityController)
    {
        if (!nextMovement)
            throw new ArgumentNullException(nameof(nextMovement));

        if (!nextAbilityController)
            throw new ArgumentNullException(nameof(nextAbilityController));

        Unbind();

        movement = nextMovement;
        abilityController = nextAbilityController;

        inputReader.MoveInputChanged += movement.SetMoveInput;
        inputReader.AbilitySlotPressed += abilityController.UseSlot;
        inputReader.RefreshMoveInput();
    }

    public void Unbind()
    {
        if (movement)
        {
            inputReader.MoveInputChanged -= movement.SetMoveInput;
            movement.ClearMoveInput();
        }

        if (abilityController)
            inputReader.AbilitySlotPressed -= abilityController.UseSlot;

        movement = null;
        abilityController = null;
    }

    public void Dispose()
    {
        Unbind();
    }
}
