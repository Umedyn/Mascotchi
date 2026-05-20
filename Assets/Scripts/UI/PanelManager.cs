using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject activityPicker;
    public GameObject nicknamePrompt;
    public GameObject settingsPanel;
    public GameObject rosterGalleryPanel;
    public GameObject growConfirmPanel;

    [Header("Settings Button")]
    public Button   settingsButton;
    public Image    settingsButtonImage;
    public Sprite   menuSprite;
    public Sprite   backSprite;

    private GameObject _currentPanel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Debug.Log($"[PanelManager] Start. GameManager instance null: {GameManager.Instance == null}, CurrentSave null: {GameManager.Instance?.CurrentSave == null}, Nickname: '{GameManager.Instance?.CurrentSave?.nickname}'");
        mainPanel.SetActive(true);
        activityPicker.SetActive(false);
        nicknamePrompt.SetActive(false);
        settingsPanel.SetActive(false);
        rosterGalleryPanel.SetActive(false);
        growConfirmPanel.SetActive(false);

        _currentPanel = mainPanel;
        UpdateSettingsButton();

        settingsButton.onClick.AddListener(OnSettingsButtonPressed);

        // Show nickname prompt if no nickname set yet
        if (string.IsNullOrEmpty(GameManager.Instance.CurrentSave.nickname))
            ShowPanel(nicknamePrompt);
    }

    public void ShowPanel(GameObject panel)
    {
        _currentPanel.SetActive(false);
        _currentPanel = panel;
        _currentPanel.SetActive(true);
        UpdateSettingsButton();

        if (panel == settingsPanel)
            settingsPanel.GetComponent<SettingsPanel>().RefreshNickname();
        if (panel == settingsPanel)
            settingsPanel.GetComponent<SettingsPanel>().RefreshNickname();

        if (panel == rosterGalleryPanel)
            rosterGalleryPanel.GetComponent<RosterGallery>().Refresh();
    }

    public void OnSettingsButtonPressed()
    {
        if (_currentPanel == mainPanel)
        {
            ShowPanel(settingsPanel);
        }
        else if (_currentPanel == settingsPanel)
        {
            ShowPanel(mainPanel);
        }
        else if (_currentPanel == rosterGalleryPanel
              || _currentPanel == growConfirmPanel)
        {
            ShowPanel(settingsPanel);
        }
        else if (_currentPanel == activityPicker)
        {
            // Cancel activity picker, return to main
            ShowPanel(mainPanel);
        }
        // NicknamePrompt has no back — player must enter a name
    }

    private void UpdateSettingsButton()
    {
        bool onMain = _currentPanel == mainPanel;
        settingsButtonImage.sprite = onMain ? menuSprite : backSprite;
    }

    public bool IsMainPanelActive() => _currentPanel == mainPanel;
}