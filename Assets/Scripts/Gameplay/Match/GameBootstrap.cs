using System;
using UnityEngine;

public sealed class GameBootstrap : MonoBehaviour
{
    [SerializeField] private MatchManager matchManager;
    [SerializeField] private UIManager uiManager;

    private void Start()
    {
        ValidateReferencesOrThrow();
        InitializeSceneStartup();
    }

    private void OnValidate()
    {
        if (TryGetValidationError(out var error))
        {
            Debug.LogError(error, this);
        }
    }

    private void InitializeSceneStartup()
    {
        uiManager.InitializeGameStart();
        matchManager.InitializeGameStart(uiManager);
    }

    private void ValidateReferencesOrThrow()
    {
        if (TryGetValidationError(out var error))
        {
            throw new InvalidOperationException(error);
        }
    }

    private bool TryGetValidationError(out string error)
    {
        if (!matchManager)
        {
            error = $"{nameof(GameBootstrap)} requires a {nameof(MatchManager)} reference.";
            return true;
        }

        if (!uiManager)
        {
            error = $"{nameof(GameBootstrap)} requires a {nameof(UIManager)} reference.";
            return true;
        }

        error = string.Empty;
        return false;
    }
}