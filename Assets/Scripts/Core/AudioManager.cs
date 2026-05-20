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

    private const string MusicParam = "MusicVolume";
    private const string SFXParam   = "SFXVolume";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMusicVolume(float sliderValue)
    {
        // Slider 0-1 mapped to -80 to 0 dB
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
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();
}