using UnityEngine;

public sealed class PuckRegistry : MonoBehaviour
{
    private Rigidbody2D puck;

    public bool IsPuck(Rigidbody2D candidate)
    {
        return candidate && candidate == puck;
    }

    public void RegisterPuck(Rigidbody2D newPuck)
    {
        puck = newPuck;
    }

    public void Clear()
    {
        puck = null;
    }
}