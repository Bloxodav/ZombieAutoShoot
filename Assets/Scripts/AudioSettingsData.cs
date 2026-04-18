using UnityEngine;

[CreateAssetMenu(fileName = "AudioSettings", menuName = "Settings/AudioData")]
public class AudioSettingsData : ScriptableObject
{
    public float musicVolume = 0.5f;
    public float sfxVolume = 0.5f;

    public event System.Action OnVolumeChanged;

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVol", musicVolume);
        PlayerPrefs.SetFloat("SfxVol", sfxVolume);
    }

    public void LoadSettings()
    {
        musicVolume = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SfxVol", 0.5f);
    }

    public void NotifyVolumeChanged() => OnVolumeChanged?.Invoke();
}