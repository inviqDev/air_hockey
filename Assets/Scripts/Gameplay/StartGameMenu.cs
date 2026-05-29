using System;
using UnityEngine;
using UnityEngine.UI;

public sealed class StartGameMenu : MonoBehaviour
{
    [SerializeField] private GameObject choosingVsMenu;
    [SerializeField] private Button aiOpponentButton;
    [SerializeField] private Button secondPlayerButton;

    public event Action AiOpponentSelected;

    private void Awake()
    {
        ValidateReferences();
        DisableUnsupportedOptions();
    }

    private void OnEnable()
    {
        if (aiOpponentButton == null) return;

        aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
        aiOpponentButton.onClick.AddListener(SelectAiOpponent);
    }

    private void OnDisable()
    {
        if (aiOpponentButton != null)
        {
            aiOpponentButton.onClick.RemoveListener(SelectAiOpponent);
        }
    }

    private void OnValidate()
    {
        ValidateReferences();
        DisableUnsupportedOptions();
    }

    public void ShowChoosingVsMenu()
    {
        if (choosingVsMenu != null)
        {
            choosingVsMenu.SetActive(true);
        }
    }

    public void HideChoosingVsMenu()
    {
        if (choosingVsMenu != null)
        {
            choosingVsMenu.SetActive(false);
        }
    }

    private void SelectAiOpponent()
    {
        HideChoosingVsMenu();
        AiOpponentSelected?.Invoke();
    }

    private void ValidateReferences()
    {
        if (choosingVsMenu == null)
        {
            Debug.LogError($"{nameof(StartGameMenu)} requires a ChoosingVsMenu reference.", this);
        }

        if (aiOpponentButton == null)
        {
            Debug.LogError($"{nameof(StartGameMenu)} requires an AiOpponentButton reference.", this);
        }
    }

    private void DisableUnsupportedOptions()
    {
        if (secondPlayerButton != null)
        {
            secondPlayerButton.interactable = false;
        }
    }
}
