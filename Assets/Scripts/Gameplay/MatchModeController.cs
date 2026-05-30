using UI.Enums;
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

    public void StartMatch(MainMenuSelection selection)
    {
        switch (selection)
        {
            case MainMenuSelection.AiOpponent:
                roundResetter?.SpawnGameItemsForAiOpponent();
                break;
            case MainMenuSelection.SecondPlayer:
                roundResetter?.SpawnGameItemsForSecondPlayer();
                break;
            default:
                Debug.LogError($"Unhandled match mode selection: {selection}", this);
                break;
        }
    }

    private void ValidateReferences()
    {
        if (!roundResetter)
        {
            Debug.LogError($"{nameof(MatchModeController)} requires a RoundResetter reference.", this);
        }
    }
}
