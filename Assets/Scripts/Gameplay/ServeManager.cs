using UnityEngine;

public sealed class ServeManager : MonoBehaviour
{
    private bool hasChosenFirstAttacker;
    private PlayerSide nextAttackingSide;

    public void ResetMatch()
    {
        hasChosenFirstAttacker = false;
    }

    public void SetNextAttacker(PlayerSide side)
    {
        nextAttackingSide = side;
        hasChosenFirstAttacker = true;
    }

    public Vector2 GetPuckStartPosition(Vector2 leftPuckDefaultPosition, Vector2 rightPuckDefaultPosition)
    {
        return GetNextAttackingSide() == PlayerSide.Left
            ? leftPuckDefaultPosition
            : rightPuckDefaultPosition;
    }

    private PlayerSide GetNextAttackingSide()
    {
        if (hasChosenFirstAttacker) return nextAttackingSide;
        
        nextAttackingSide = Random.value < 0.5f ? PlayerSide.Left : PlayerSide.Right;
        hasChosenFirstAttacker = true;

        return nextAttackingSide;
    }
}
