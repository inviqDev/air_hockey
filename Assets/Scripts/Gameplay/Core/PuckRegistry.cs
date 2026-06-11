using UnityEngine;

public sealed class PuckRegistry : MonoBehaviour
{
    private Puck puck;

    public Puck CurrentPuck => puck;

    public bool IsPuck(Puck candidate)
    {
        return candidate && candidate == puck;
    }

    public void RegisterPuck(Puck newPuck)
    {
        puck = newPuck;
    }

    public void Clear()
    {
        puck = null;
    }
}
