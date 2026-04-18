using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioVolumeLinker : MonoBehaviour
{
    public enum AudioType { Music, SFX }
    public AudioType type;
    public AudioSettingsData settingsData;

    private AudioSource _source;

    void Awake() => _source = GetComponent<AudioSource>();

    void OnEnable()
    {
        settingsData.OnVolumeChanged += ApplyVolume;
        ApplyVolume();
    }

    void OnDisable() => settingsData.OnVolumeChanged -= ApplyVolume;

    public void ApplyVolume()
    {
        _source.volume = type == AudioType.Music
            ? settingsData.musicVolume
            : settingsData.sfxVolume;
    }
}