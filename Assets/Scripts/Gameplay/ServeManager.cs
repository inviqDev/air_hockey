using UnityEngine;

public sealed class ServeManager : MonoBehaviour
{
    [SerializeField] private Vector2 tableCenterPosition = Vector2.zero;
    [SerializeField] private float puckDistanceFromAttacker = 1.25f;

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

    public Vector2 GetPuckStartPosition(Vector2 leftStrikerPosition, Vector2 rightStrikerPosition)
    {
        var attackerPosition = GetNextAttackingSide() == PlayerSide.Left
            ? leftStrikerPosition
            : rightStrikerPosition;
        var distanceToCenter = Vector2.Distance(attackerPosition, tableCenterPosition);
        var offsetFromAttacker = Mathf.Min(puckDistanceFromAttacker, distanceToCenter * 0.5f);

        return Vector2.MoveTowards(attackerPosition, tableCenterPosition, offsetFromAttacker);
    }

    private PlayerSide GetNextAttackingSide()
    {
        if (hasChosenFirstAttacker) return nextAttackingSide;
        
        nextAttackingSide = Random.value < 0.5f ? PlayerSide.Left : PlayerSide.Right;
        hasChosenFirstAttacker = true;

        return nextAttackingSide;
    }
}
