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
    private bool  _actionInProgress;

    // True while any action (fan or streamer) is animating. Streamer commands are ignored while busy.
    public bool IsBusy => _actionInProgress;

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
            if (!_actionInProgress) SetButtonsInteractable(true);
        }
    }

    // Fan pressed Feed, Rest or Clean.
    private void DoAction(ActionType action)
    {
        if (_onCooldown || _actionInProgress) return;
        RunAction(action, startFanCooldown: true);
    }

    // Fan picked an activity from the activity picker.
    private void DoActivity(ActionType activity)
    {
        if (_onCooldown || _actionInProgress) return;
        PanelManager.Instance.ShowPanel(PanelManager.Instance.mainPanel);
        RunAction(activity, startFanCooldown: true);
    }

    // Called by TwitchListener. Plays exactly like a fan action but leaves the fan's cooldown alone.
    public void RunStreamerAction(ActionType action)
    {
        if (_actionInProgress) return;
        RunAction(action, startFanCooldown: false);
    }

    private void RunAction(ActionType action, bool startFanCooldown)
    {
        _actionInProgress = true;
        SetButtonsInteractable(false);
        GameManager.Instance.creatureAnimator.ResetPosition();

        System.Action finish = () =>
        {
            GameManager.Instance.PerformAction(action);
            MainUIController.Instance.RefreshDisplay();
            _actionInProgress = false;

            if (startFanCooldown) StartCooldown();
            else if (!_onCooldown) SetButtonsInteractable(true);

            GameManager.Instance.creatureAnimator.PlayAnimation("Idle");
        };

        if (action == ActionType.Feed)
        {
            GameManager.Instance.creatureAnimator.ShowFood(() =>
            {
                AudioManager.Instance?.PlayActionSFX(ActionType.Feed);
                GameManager.Instance.creatureAnimator.PlayOnce("Feed", finish);
            });
            return;
        }

        if (action == ActionType.Rest || action == ActionType.Clean)
        {
            AudioManager.Instance?.PlayActionSFX(action);
            GameManager.Instance.creatureAnimator.PlayOnce(action.ToString(), finish);
            return;
        }

        // Activities: activity animation, then the Win animation.
        AudioManager.Instance?.PlayActionSFX(action);
        GameManager.Instance.creatureAnimator.PlayOnce(action.ToString(), () =>
        {
            AudioManager.Instance?.PlayActivityComplete();
            GameManager.Instance.creatureAnimator.PlayOnce("Win", finish);
        });
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
