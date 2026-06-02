using UnityEngine;

public sealed class SideOwner : MonoBehaviour
{
    [SerializeField] private PlayerSide side = PlayerSide.Right;

    public PlayerSide Side
    {
        get => side;
        set => side = value;
    }
}