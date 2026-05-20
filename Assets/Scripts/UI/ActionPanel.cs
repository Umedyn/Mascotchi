using UnityEngine;
using UnityEngine.UI;

public class ActionPanel : MonoBehaviour
{
    [Header("Action Buttons")]
    public Button feedButton;
    public Button restButton;
    public Button cleanButton;
    public Button activityButton;

    [Header("Activity Buttons")]
    public Button karaokeButton;
    public Button gamingButton;
    public Button streamingButton;
    public Button marblesButton;
    public Button codingButton;

    [Header("Cooldown")]
    [Tooltip("Cooldown in seconds. GDD specifies 2-3 minutes (120-180). Keep low for testing.")]
    public float cooldownDuration = 10f;

    private float _cooldownRemaining;
    private bool  _onCooldown;

    void Start()
    {
        feedButton.onClick.AddListener(()     => DoAction(ActionType.Feed));
        restButton.onClick.AddListener(()     => DoAction(ActionType.Rest));
        cleanButton.onClick.AddListener(()    => DoAction(ActionType.Clean));
        activityButton.onClick.AddListener(() => PanelManager.Instance.ShowPanel(
            PanelManager.Instance.activityPicker));

        karaokeButton.onClick.AddListener(()   => DoActivity(ActionType.Karaoke));
        gamingButton.onClick.AddListener(()    => DoActivity(ActionType.Gaming));
        streamingButton.onClick.AddListener(() => DoActivity(ActionType.Streaming));
        marblesButton.onClick.AddListener(()   => DoActivity(ActionType.Marbles));
        codingButton.onClick.AddListener(()    => DoActivity(ActionType.Coding));
    }

    void Update()
    {
        if (!_onCooldown) return;
        _cooldownRemaining -= Time.deltaTime;
        if (_cooldownRemaining <= 0f)
        {
            _onCooldown = false;
            SetButtonsInteractable(true);
        }
    }

    private void DoAction(ActionType action)
    {
        if (_onCooldown) return;
        GameManager.Instance.PerformAction(action);
        MainUIController.Instance.RefreshDisplay();
        StartCooldown();
    }

    private void DoActivity(ActionType activity)
    {
        if (_onCooldown) return;
        PanelManager.Instance.ShowPanel(PanelManager.Instance.mainPanel);
        GameManager.Instance.PerformAction(activity);
        MainUIController.Instance.RefreshDisplay();
        StartCooldown();
    }

    private void StartCooldown()
    {
        _cooldownRemaining = cooldownDuration;
        _onCooldown        = true;
        SetButtonsInteractable(false);
    }

    private void SetButtonsInteractable(bool state)
    {
        feedButton.interactable     = state;
        restButton.interactable     = state;
        cleanButton.interactable    = state;
        activityButton.interactable = state;
    }
}