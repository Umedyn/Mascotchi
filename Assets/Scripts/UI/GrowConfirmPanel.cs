using UnityEngine;
using UnityEngine.UI;

public class GrowConfirmPanel : MonoBehaviour
{
    public Button confirmButton;
    public Button cancelButton;

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        cancelButton.onClick.AddListener(OnCancel);
    }

    private void OnConfirm()
    {
        GameManager.Instance.StartNewGame();
        PanelManager.Instance.ShowPanel(
            PanelManager.Instance.nicknamePrompt);
    }

    private void OnCancel()
    {
        PanelManager.Instance.ShowPanel(
            PanelManager.Instance.settingsPanel);
    }
}