using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Handles the volume sliders and saves/loads their values with PlayerPrefs.
/// No singleton needed: the mixer keeps the values across scene loads on its own,
/// and this re-applies the saved values on startup (menu is the first scene).
/// </summary>
public class VolumeSettings : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Sliders (range 0 to 1)")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    // Must match the exposed parameter names on the mixer.
    private const string MASTER_PARAM = "MasterVolume";
    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    // PlayerPrefs keys.
    private const string MASTER_KEY = "vol_master";
    private const string MUSIC_KEY = "vol_music";
    private const string SFX_KEY = "vol_sfx";

    private void Start()
    {
        // Load saved values (default to full volume on first ever run).
        float master = PlayerPrefs.GetFloat(MASTER_KEY, 1f);
        float music = PlayerPrefs.GetFloat(MUSIC_KEY, 1f);
        float sfx = PlayerPrefs.GetFloat(SFX_KEY, 1f);

        // Set slider positions without firing the listeners, then apply to mixer.
        masterSlider.SetValueWithoutNotify(master);
        musicSlider.SetValueWithoutNotify(music);
        sfxSlider.SetValueWithoutNotify(sfx);

        ApplyToMixer(MASTER_PARAM, master);
        ApplyToMixer(MUSIC_PARAM, music);
        ApplyToMixer(SFX_PARAM, sfx);

        // Wire slider changes.
        masterSlider.onValueChanged.AddListener(SetMaster);
        musicSlider.onValueChanged.AddListener(SetMusic);
        sfxSlider.onValueChanged.AddListener(SetSfx);
    }

    public void SetMaster(float v) => Save(MASTER_PARAM, MASTER_KEY, v);
    public void SetMusic(float v) => Save(MUSIC_PARAM, MUSIC_KEY, v);
    public void SetSfx(float v) => Save(SFX_PARAM, SFX_KEY, v);

    private void Save(string param, string key, float linear)
    {
        linear = Mathf.Clamp01(linear);
        ApplyToMixer(param, linear);
        PlayerPrefs.SetFloat(key, linear);
    }

    /// <summary>
    /// Linear 0..1 slider value -> decibels. Log scale matches perceived loudness;
    /// 0 is floored to -80 dB (silent).
    /// </summary>
    private void ApplyToMixer(string param, float linear)
    {
        float dB = linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
        mixer.SetFloat(param, dB);
    }
}
