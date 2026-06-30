using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ParticipantReadyView : MonoBehaviour
{
    [Header("Ready state settings")]
    [SerializeField] private Image stateIcon;
    [SerializeField] private Sprite notReadySprite;
    [SerializeField] private Sprite readySprite;
    
    [Header("Ready state text field")]
    [SerializeField] private TMP_Text readyText;

    [Header("Ready state text colors")]
    [SerializeField] private Color notReadyTextColor;
    [SerializeField] private Color readyTextColor;

    private void Awake()
    {
        ValidateReferences();
        ApplyVisualState(false);
    }

    public void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    public void SetReady(bool isReady)
    {
        ApplyVisualState(isReady);
    }

    private void ApplyVisualState(bool isReady)
    {
        if (stateIcon)
            stateIcon.sprite = isReady ? readySprite : notReadySprite;

        if (readyText)
        {
            var color = isReady ? readyTextColor : notReadyTextColor;
            color.a = 1f;
            readyText.color = color;
        }
    }

    private void ValidateReferences()
    {
        if (!stateIcon)
            Debug.LogError($"{nameof(ParticipantReadyView)} on {name} requires a {nameof(stateIcon)} reference.", this);

        if (!readyText)
            Debug.LogError($"{nameof(ParticipantReadyView)} on {name} requires a {nameof(readyText)} reference.", this);

        if (!notReadySprite)
            Debug.LogError($"{nameof(ParticipantReadyView)} on {name} requires a {nameof(notReadySprite)} reference.", this);

        if (!readySprite)
            Debug.LogError($"{nameof(ParticipantReadyView)} on {name} requires a {nameof(readySprite)} reference.", this);
    }
}
