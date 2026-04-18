using UnityEngine;

[CreateAssetMenu(fileName = "SoundSettings", menuName = "Settings/Sound Settings")]
public class SoundSettingsSO : ScriptableObject
{
    public AudioSettingsData audioSettings;

    [System.Serializable]
    public struct SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 2f)] public float volumeScale;
    }

    public SoundEntry[] sounds;

    public void Play(string soundName, AudioSource source)
    {
        if (audioSettings == null || audioSettings.sfxVolume <= 0.001f) return;

        foreach (var entry in sounds)
        {
            if (entry.name != soundName || entry.clip == null) continue;
            float volume = Mathf.Clamp01(audioSettings.sfxVolume * entry.volumeScale);
            source.PlayOneShot(entry.clip, volume);
            return;
        }
    }

    public void PlayMusic(string soundName, AudioSource source)
    {
        if (audioSettings == null || audioSettings.musicVolume <= 0.001f) return;

        foreach (var entry in sounds)
        {
            if (entry.name != soundName || entry.clip == null) continue;
            float volume = Mathf.Clamp01(audioSettings.musicVolume * entry.volumeScale);
            source.PlayOneShot(entry.clip, volume);
            return;
        }
    }

    public float GetFinalVolume(string soundName)
    {
        float global = audioSettings != null ? audioSettings.sfxVolume : 1f;
        foreach (var entry in sounds)
            if (entry.name == soundName)
                return Mathf.Clamp01(global * entry.volumeScale);
        return global;
    }
}