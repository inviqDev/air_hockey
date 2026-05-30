using UnityEngine;

public sealed class MatchModeController : MonoBehaviour
{
    [SerializeField] private RoundResetter roundResetter;

    private void Awake()
    {
        ValidateReferences();
    }

    private void OnValidate()
    {
        ValidateReferences();
    }

    public void StartMatch(MatchConfiguration configuration)
    {
        roundResetter?.SpawnGameItems(configuration);
    }

    private void ValidateReferences()
    {
        if (!roundResetter)
        {
            Debug.LogError($"{nameof(MatchModeController)} requires a RoundResetter reference.", this);
        }
    }
}
