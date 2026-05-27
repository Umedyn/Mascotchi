using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsPanel : MonoBehaviour
{
    [Header("Volume")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Nickname")]
    public TMP_InputField nicknameField;

    [Header("Buttons")]
    public Button rosterButton;
    public Button growNewMascotButton;

    [Header("Credits")]
    public TextMeshProUGUI creditsText;

    void Awake()
    {
        musicSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        sfxSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
        nicknameField.onEndEdit.AddListener(OnNicknameEdited);

        rosterButton.onClick.AddListener(() =>
            PanelManager.Instance.ShowPanel(PanelManager.Instance.rosterGalleryPanel));

        growNewMascotButton.onClick.AddListener(() =>
            PanelManager.Instance.ShowPanel(PanelManager.Instance.growConfirmPanel));
    }

    void Start()
    {
        musicSlider.value = 1f;
        sfxSlider.value   = 1f;
        if (GameManager.Instance?.CurrentSave != null)
            RefreshNickname();
        RefreshCredits();
    }

    public void RefreshNickname()
    {
        nicknameField.text = GameManager.Instance.CurrentSave.nickname;
    }

    private void OnNicknameEdited(string value)
    {
        string trimmed = value.Trim();
        if (!string.IsNullOrEmpty(trimmed))
            GameManager.Instance.SetNickname(trimmed);
    }
    public void RefreshCredits()
    {
        if (creditsText == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        List<string> unlocked = GameManager.Instance.CurrentSave.unlockedMascots;

        foreach (MascotData mascot in GameManager.Instance.LoadedMascots)
        {
            if (!unlocked.Contains(mascot.Definition.mascotName)) continue;
            var c = mascot.Definition.artistCredits;
            if (c == null) continue;

            sb.AppendLine($"[ {mascot.Definition.mascotName} ]");
            if (!string.IsNullOrEmpty(c.likenessArtist))
                sb.AppendLine($"  Likeness: {c.likenessArtist}");
            if (!string.IsNullOrEmpty(c.spriteArtist))
                sb.AppendLine($"  Background: {c.spriteArtist}");
            if (!string.IsNullOrEmpty(c.stingerArtist))
                sb.AppendLine($"  Stinger: {c.stingerArtist}");
            sb.AppendLine();
        }

        creditsText.text = sb.Length > 0 ? sb.ToString().TrimEnd() : "";
    }
}