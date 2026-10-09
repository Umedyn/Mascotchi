using UnityEngine;
using UnityEngine.UI;

public class CompactModeController : MonoBehaviour
{
    [Header("References")]
    public CanvasScaler  canvasScaler;
    public RectTransform eggPanel;
    public CanvasGroup   bottomPanelGroup;
    public Button        toggleButton;

    [Header("Layout")]
    [Tooltip("Window height divided by width below this value switches to egg-only layout.")]
    public float compactAspectThreshold = 1.3f;
    [Tooltip("Egg panel height divided by width. 480x480 = 1.")]
    public float eggAspect = 1f;

    private bool    _compact;
    private float   _originalMatch;
    private Vector2 _eggAnchorMin, _eggAnchorMax, _eggOffsetMin, _eggOffsetMax;

    void Awake()
    {
        _originalMatch = canvasScaler.matchWidthOrHeight;
        _eggAnchorMin  = eggPanel.anchorMin;
        _eggAnchorMax  = eggPanel.anchorMax;
        _eggOffsetMin  = eggPanel.offsetMin;
        _eggOffsetMax  = eggPanel.offsetMax;

        toggleButton.onClick.AddListener(Toggle);

#if UNITY_ANDROID && !UNITY_EDITOR
        toggleButton.gameObject.SetActive(false);
#endif
    }

    void Start()
    {
    #if UNITY_STANDALONE && !UNITY_EDITOR
        if ((float)Screen.height / Screen.width < compactAspectThreshold)
        {
            Vector2 reference = canvasScaler.referenceResolution;
            int fullHeight = Mathf.RoundToInt(Screen.width * reference.y / reference.x);
            Screen.SetResolution(Screen.width, fullHeight, FullScreenMode.Windowed);
        }
    #endif
    }

    void Update()
    {
        bool shouldBeCompact = (float)Screen.height / Screen.width < compactAspectThreshold;
        if (shouldBeCompact != _compact)
            ApplyLayout(shouldBeCompact);
    }

    private void ApplyLayout(bool compact)
    {
        _compact = compact;

        // Match width in compact mode so the egg keeps its size instead of shrinking to fit the shorter window.
        canvasScaler.matchWidthOrHeight = compact ? 0f : _originalMatch;

        if (compact)
        {
            eggPanel.anchorMin = Vector2.zero;
            eggPanel.anchorMax = Vector2.one;
            eggPanel.offsetMin = Vector2.zero;
            eggPanel.offsetMax = Vector2.zero;
        }
        else
        {
            eggPanel.anchorMin = _eggAnchorMin;
            eggPanel.anchorMax = _eggAnchorMax;
            eggPanel.offsetMin = _eggOffsetMin;
            eggPanel.offsetMax = _eggOffsetMax;
        }

        bottomPanelGroup.alpha          = compact ? 0f : 1f;
        bottomPanelGroup.interactable   = !compact;
        bottomPanelGroup.blocksRaycasts = !compact;

        if (PanelManager.Instance != null)
            PanelManager.Instance.settingsButton.gameObject.SetActive(!compact);
    }

    private void Toggle()
    {
#if UNITY_STANDALONE
        if (!_compact && PanelManager.Instance != null && !PanelManager.Instance.mainPanel.activeSelf)
            return;

        Vector2 reference = canvasScaler.referenceResolution;
        float   fullRatio = reference.y / reference.x;
        int     width     = Screen.width;
        int     height    = Mathf.RoundToInt(width * (_compact ? fullRatio : eggAspect));

        Screen.SetResolution(width, height, FullScreenMode.Windowed);
#endif
    }
}