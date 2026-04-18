using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    public AudioSettingsData settingsData;

    [Header("Music UI")]
    public Slider musicSlider;
    public GameObject musicMuteObject;

    [Header("SFX UI")]
    public Slider sfxSlider;
    public GameObject sfxMuteObject;

    private bool _initialized;

    void Start()
    {
        settingsData.LoadSettings();

        _initialized = false;
        musicSlider.value = settingsData.musicVolume;
        sfxSlider.value = settingsData.sfxVolume;
        _initialized = true;

        UpdateMuteIcons();
    }

    public void OnMusicSliderChanged(float value)
    {
        if (!_initialized) return;
        settingsData.musicVolume = value;
        settingsData.NotifyVolumeChanged();
        UpdateMuteIcons();
        settingsData.SaveSettings();
    }

    public void OnSfxSliderChanged(float value)
    {
        if (!_initialized) return;
        settingsData.sfxVolume = value;
        settingsData.NotifyVolumeChanged();
        UpdateMuteIcons();
        settingsData.SaveSettings();
    }

    public void MuteMusic() => musicSlider.value = musicSlider.value > 0 ? 0 : 0.5f;
    public void MuteSfx() => sfxSlider.value = sfxSlider.value > 0 ? 0 : 0.5f;

    private void UpdateMuteIcons()
    {
        if (musicMuteObject) musicMuteObject.SetActive(settingsData.musicVolume <= 0.001f);
        if (sfxMuteObject) sfxMuteObject.SetActive(settingsData.sfxVolume <= 0.001f);
    }
}