using UnityEngine;

public sealed class HalfFieldAreaLimiter : MonoBehaviour
{
    [SerializeField] private float centerX = 0f;

    public float GetAllowedCenterLineX(PlayerSide side, float worldRadius)
    {
        return side == PlayerSide.Left ? centerX - worldRadius : centerX + worldRadius;
    }

    public bool IsPastCenterLine(Vector2 position, PlayerSide side, float worldRadius)
    {
        var allowedX = GetAllowedCenterLineX(side, worldRadius);
        return side == PlayerSide.Left ? position.x > allowedX : position.x < allowedX;
    }

    public Vector2 ClampToCenterLine(Vector2 position, PlayerSide side, float worldRadius)
    {
        var allowedX = GetAllowedCenterLineX(side, worldRadius);

        if (side == PlayerSide.Left && position.x > allowedX)
        {
            position.x = allowedX;
        }
        else if (side == PlayerSide.Right && position.x < allowedX)
        {
            position.x = allowedX;
        }

        return position;
    }
}
