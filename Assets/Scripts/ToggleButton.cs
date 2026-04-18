using UnityEngine;
using UnityEngine.UI;

public class AudioSettingController : MonoBehaviour
{
    [Header("Тип настройки")]
    public bool isMusicControl = false;

    [Header("UI элементы")]
    public Slider audioSlider;
    public Image audioIcon;
    public GameObject crossLine;

    [Header("Спрайты")]
    public Sprite iconOn; 
    public Sprite iconOff;

    [Header("Настройки")]
    public float muteThreshold = 0.01f;

    private AudioSource audioSource;
    private string volumeKey;

    void Start()
    {
        volumeKey = isMusicControl ? "MusicVolume" : "SoundVolume";

        if (audioSlider == null)
            audioSlider = GetComponentInParent<Slider>();

        if (audioIcon == null)
            audioIcon = GetComponent<Image>();

        if (audioSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat(volumeKey, 0.8f);
            audioSlider.value = savedVolume;

            audioSlider.onValueChanged.AddListener(OnVolumeChanged);

            OnVolumeChanged(audioSlider.value);
        }

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(OnIconClick);
        }

        SetupAudioSource();
    }

    void SetupAudioSource()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = isMusicControl;
        }

        if (audioSlider != null)
        {
            ApplyVolume(audioSlider.value);
        }
    }

    void OnVolumeChanged(float volume)
    {
        bool isMuted = volume <= muteThreshold;

        UpdateIcon(!isMuted);

        ApplyVolume(volume);

        SaveVolume(volume);
    }

    void OnIconClick()
    {
        if (audioSlider == null) return;

        if (audioSlider.value > muteThreshold)
        {
            audioSlider.value = 0f;
        }
        else
        {
            float savedVolume = PlayerPrefs.GetFloat(volumeKey + "_Last", 0.8f);
            audioSlider.value = savedVolume;
        }
    }

    void UpdateIcon(bool isOn)
    {
        if (audioIcon == null) return;

        if (iconOn != null && iconOff != null)
        {
            audioIcon.sprite = isOn ? iconOn : iconOff;
        }

        audioIcon.color = isOn ? Color.white : Color.gray;

        if (crossLine != null)
        {
            crossLine.SetActive(!isOn);
        }

        if (audioSlider != null)
        {
            audioSlider.interactable = isOn;

            var colors = audioSlider.colors;
            colors.disabledColor = isOn ? Color.gray : new Color(0.5f, 0.5f, 0.5f, 0.5f);
            audioSlider.colors = colors;
        }
    }

    void ApplyVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;

            if (isMusicControl && volume > muteThreshold && !audioSource.isPlaying)
            {
            }
            else if (volume <= muteThreshold && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        else
        {
            if (isMusicControl)
            {
            }
            else
            {
            }
        }
    }

    void SaveVolume(float volume)
    {
        PlayerPrefs.SetFloat(volumeKey, volume);

        if (volume > muteThreshold)
        {
            PlayerPrefs.SetFloat(volumeKey + "_Last", volume);
        }

        PlayerPrefs.Save();
    }

    public void SetVolume(float volume)
    {
        if (audioSlider != null)
        {
            audioSlider.value = Mathf.Clamp01(volume);
        }
    }

    public float GetVolume()
    {
        return audioSlider != null ? audioSlider.value : 0f;
    }

    public bool IsMuted()
    {
        return audioSlider != null && audioSlider.value <= muteThreshold;
    }

    void OnDestroy()
    {
        if (audioSlider != null)
        {
            audioSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}