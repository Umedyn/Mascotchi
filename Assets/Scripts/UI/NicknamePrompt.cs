using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknamePrompt : MonoBehaviour
{
    public TMP_InputField inputField;
    public Button         confirmButton;
    public TextMeshProUGUI errorText;

    // Hard blacklist — extend this list as needed
    private static readonly string[] Blacklist = { };

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        errorText.text = "";
    }

    private void OnConfirm()
    {
        string nickname = inputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
        {
            errorText.text = "Please enter a nickname.";
            return;
        }

        foreach (string banned in Blacklist)
        {
            if (nickname.IndexOf(banned, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                errorText.text = "That nickname is not allowed.";
                return;
            }
        }

        GameManager.Instance.SetNickname(nickname);
        PanelManager.Instance.ShowPanel(PanelManager.Instance.mainPanel);
        PanelManager.Instance.settingsPanel.GetComponent<SettingsPanel>().RefreshNickname();
    }
}