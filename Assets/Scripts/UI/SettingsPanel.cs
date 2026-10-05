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

    void OnEnable()
    {
        if (GameManager.Instance?.CurrentSave != null)
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

        GameManager gm = GameManager.Instance;
        if (gm == null || gm.CurrentSave == null || gm.LoadedMascots == null || gm.LoadedMascots.Count == 0)
        {
            creditsText.text = "";
            return;
        }

        MascotData mascot = gm.CurrentSave.isEvolved
            ? gm.LoadedMascots.Find(m => m.Definition.mascotName == gm.CurrentSave.activeMascotId)
            : gm.LoadedMascots[0];

        ArtistCredits c = mascot?.Definition.artistCredits;
        if (c == null)
        {
            creditsText.text = "";
            return;
        }

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[ {mascot.Definition.mascotName} ]");
        if (!string.IsNullOrEmpty(c.likenessArtist))
            sb.AppendLine($"  Likeness: {c.likenessArtist}");
        if (!string.IsNullOrEmpty(c.spriteArtist))
            sb.AppendLine($"  Sprites: {c.spriteArtist}");
        if (!string.IsNullOrEmpty(c.stingerArtist))
            sb.AppendLine($"  Stinger: {c.stingerArtist}");

        creditsText.text = sb.ToString().TrimEnd();
    }
}