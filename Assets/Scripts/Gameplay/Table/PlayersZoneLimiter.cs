using UnityEngine;

public sealed class PlayersZoneLimiter : MonoBehaviour
{
    [SerializeField] private float maxOffset = 0.75f;

    private Vector3 defaultLocalPosition;
    private bool isInitialized;

    private void Awake()
    {
        CacheDefaultPosition();
    }

    public void ShiftTowardGoal(PlayerSide goalSide, float requestedOffset)
    {
        CacheDefaultPosition();

        var direction = goalSide == PlayerSide.Right ? 1f : -1f;
        var clampedOffset = Mathf.Clamp(Mathf.Abs(requestedOffset), 0f, maxOffset);
        var localPosition = defaultLocalPosition;
        localPosition.x += direction * clampedOffset;
        transform.localPosition = localPosition;
    }

    public void ResetState()
    {
        CacheDefaultPosition();
        transform.localPosition = defaultLocalPosition;
    }

    private void CacheDefaultPosition()
    {
        if (isInitialized)
            return;

        defaultLocalPosition = transform.localPosition;
        isInitialized = true;
    }
}