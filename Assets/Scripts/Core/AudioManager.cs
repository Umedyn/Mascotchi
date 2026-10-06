using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    public AudioMixer audioMixer;

    [Header("Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip mainTheme;

    [Header("UI SFX")]
    public AudioClip buttonTap;

    [Header("Action SFX")]
    public AudioClip feedSFX;
    public AudioClip restSFX;
    public AudioClip cleanSFX;

    [Header("Activity SFX")]
    public AudioClip karaokeSFX;
    public AudioClip gamingSFX;
    public AudioClip streamingSFX;
    public AudioClip marblesSFX;
    public AudioClip codingSFX;
    public AudioClip activityCompleteSFX;

    [Header("Evolution SFX")]
    public AudioClip evolutionSting;

    private const string MusicParam = "MusicVolume";
    private const string SFXParam   = "SFXVolume";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start() => PlayMainTheme();

    public void SetMusicVolume(float sliderValue)
    {
        float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        audioMixer.SetFloat(MusicParam, db);
    }

    public void SetSFXVolume(float sliderValue)
    {
        float db = sliderValue > 0.001f ? Mathf.Log10(sliderValue) * 20f : -80f;
        audioMixer.SetFloat(SFXParam, db);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void PlayMainTheme()        => PlayMusic(mainTheme);
    public void PlayButtonTap()        => PlaySFX(buttonTap);
    public void PlayActivityComplete() => PlaySFX(activityCompleteSFX);
    public void PlayEvolutionSting()   => PlaySFX(evolutionSting);

    public void PlayActionSFX(ActionType action) => PlaySFX(GetActionClip(action));

    // Single lookup point so v3.0 per-mascot overrides can slot in here later.
    private AudioClip GetActionClip(ActionType action)
    {
        switch (action)
        {
            case ActionType.Feed:      return feedSFX;
            case ActionType.Rest:      return restSFX;
            case ActionType.Clean:     return cleanSFX;
            case ActionType.Karaoke:   return karaokeSFX;
            case ActionType.Gaming:    return gamingSFX;
            case ActionType.Streaming: return streamingSFX;
            case ActionType.Marbles:   return marblesSFX;
            case ActionType.Coding:    return codingSFX;
            default:                   return null;
        }
    }
}