using UnityEngine;
using UnityEngine.UI;

public class CompactModeController : MonoBehaviour
{
    [Header("References")]
    public CanvasScaler  canvasScaler;
    [Tooltip("Fixed-size container holding EggPanel, BottomPanel and the overlays.")]
    public RectTransform frame;
    public RectTransform eggPanel;
    public CanvasGroup   bottomPanelGroup;
    public Button        toggleButton;

    [Header("Layout")]
    [Tooltip("Window height divided by width below this value switches to egg-only layout.")]
    public float compactAspectThreshold = 1.3f;
    [Tooltip("Egg panel height divided by width. 480x480 = 1.")]
    public float eggAspect = 1f;

    private bool    _compact;
    private bool    _initialised;
    private Vector2 _eggAnchorMin, _eggAnchorMax, _eggOffsetMin, _eggOffsetMax;

    void Awake()
    {
        _eggAnchorMin = eggPanel.anchorMin;
        _eggAnchorMax = eggPanel.anchorMax;
        _eggOffsetMin = eggPanel.offsetMin;
        _eggOffsetMax = eggPanel.offsetMax;

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
        if (!_initialised || shouldBeCompact != _compact)
            ApplyLayout(shouldBeCompact);
    }

    private void ApplyLayout(bool compact)
    {
        _compact     = compact;
        _initialised = true;

        Vector2 reference = canvasScaler.referenceResolution;

        if (compact)
        {
            // Egg-only: fit to width so the egg keeps its size in the shorter window.
            canvasScaler.screenMatchMode    = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0f;
            frame.sizeDelta = new Vector2(reference.x, reference.x * eggAspect);

            eggPanel.anchorMin = Vector2.zero;
            eggPanel.anchorMax = Vector2.one;
            eggPanel.offsetMin = Vector2.zero;
            eggPanel.offsetMax = Vector2.zero;
        }
        else
        {
            // Full layout: the whole 480x854 frame always fits on screen at its true shape.
            // Any leftover screen space becomes empty bars instead of stretching the layout.
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            frame.sizeDelta = reference;

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
