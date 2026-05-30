using UnityEngine;

public static class SideUtility
{
    public static Vector2 DashDirection(PlayerSide side)
    {
        return side == PlayerSide.Left ? Vector2.right : Vector2.left;
    }
}
