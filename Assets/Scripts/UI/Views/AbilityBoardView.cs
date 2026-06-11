using TMPro;
using UnityEngine;

public sealed class AbilityBoardView : MonoBehaviour
{
    private const string EmptySlotText = "EMPTY";

    [SerializeField] private TextMeshProUGUI slotOneLabel;
    [SerializeField] private TextMeshProUGUI slotTwoLabel;
    [SerializeField] private TextMeshProUGUI slotThreeLabel;

    private AbilityController boundController;

    private void OnEnable()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        Unbind();
    }

    public void Bind(AbilityController controller)
    {
        if (boundController == controller)
        {
            Refresh();
            return;
        }

        Unbind();
        boundController = controller;

        if (boundController != null)
            boundController.AbilitiesChanged += Refresh;

        Refresh();
    }

    public void ClearBoard()
    {
        Unbind();
        SetSlotText(slotOneLabel, string.Empty);
        SetSlotText(slotTwoLabel, string.Empty);
        SetSlotText(slotThreeLabel, string.Empty);
    }

    private void Unbind()
    {
        if (boundController != null)
            boundController.AbilitiesChanged -= Refresh;

        boundController = null;
    }

    private void Refresh()
    {
        SetSlotText(slotOneLabel, GetDisplayName(0));
        SetSlotText(slotTwoLabel, GetDisplayName(1));
        SetSlotText(slotThreeLabel, GetDisplayName(2));
    }

    private string GetDisplayName(int slotIndex)
    {
        return boundController != null
            ? boundController.GetDisplayNameForSlot(slotIndex)
            : string.Empty;
    }

    private static void SetSlotText(TextMeshProUGUI label, string abilityName)
    {
        if (!label)
            return;

        label.text = string.IsNullOrWhiteSpace(abilityName) ? EmptySlotText : abilityName;
    }
}
