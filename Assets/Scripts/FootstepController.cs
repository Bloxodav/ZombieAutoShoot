using UnityEngine;
using System.Collections;

public class FootstepController : MonoBehaviour
{
    [Header("Audio Sources (ноги)")]
    public AudioSource rightFoot;
    public AudioSource leftFoot;

    [Header("Звуки шагов")]
    public AudioClip[] footstepSounds;

    [Header("Громкость")]
    public AudioSettingsData audioSettings;
    [Range(0f, 0.3f)] public float volumeVariation = 0.1f;
    [Range(0f, 0.2f)] public float pitchVariation = 0.1f;

    [Header("Пыль")]
    public ParticleSystem dustPrefab;

    [Header("Кости ног")]
    public Transform rightFootBone;
    public Transform leftFootBone;

    private AudioClip _lastClip;
    private float _lastStepTime; // ← добавь поле
    private const float MinStepInterval = 0.15f; // минимум между шагами


    private ParticleSystem[] _rightPool = new ParticleSystem[3];
    private ParticleSystem[] _leftPool = new ParticleSystem[3];
    private int _rightIdx;
    private int _leftIdx;

    private void Awake()
    {
        if (dustPrefab == null) return;
        for (int i = 0; i < 3; i++)
        {
            _rightPool[i] = Instantiate(dustPrefab);
            _rightPool[i].gameObject.SetActive(false);
            _leftPool[i] = Instantiate(dustPrefab);
            _leftPool[i].gameObject.SetActive(false);
        }
    }

    public void OnRightFootstep()
    {
        if (Time.time - _lastStepTime < MinStepInterval) return; // ← блокируем дубли
        _lastStepTime = Time.time;
        PlaySound(isRight: true);
        PlayDust(isRight: true);
    }

    public void OnLeftFootstep()
    {
        if (Time.time - _lastStepTime < MinStepInterval) return;
        _lastStepTime = Time.time;
        PlaySound(isRight: false);
        PlayDust(isRight: false);
    }

    private void PlaySound(bool isRight)
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;

        float baseVolume = audioSettings != null ? audioSettings.sfxVolume : 0.7f;
        if (baseVolume <= 0.001f) return;

        AudioSource source = isRight ? rightFoot : leftFoot;
        if (source == null) return;

        float volume = Mathf.Clamp01(baseVolume + Random.Range(-volumeVariation, volumeVariation));
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.PlayOneShot(GetRandomClip(), volume);
    }

    private void PlayDust(bool isRight)
    {
        if (dustPrefab == null) return;

        ParticleSystem ps;
        Transform bone;

        if (isRight)
        {
            ps = _rightPool[_rightIdx];
            _rightIdx = (_rightIdx + 1) % 3;
            bone = rightFootBone;
        }
        else
        {
            ps = _leftPool[_leftIdx];
            _leftIdx = (_leftIdx + 1) % 3;
            bone = leftFootBone;
        }

        ps.transform.position = bone != null ? bone.position : transform.position;
        ps.transform.rotation = Quaternion.identity;
        ps.gameObject.SetActive(true);
        ps.Play();
        StartCoroutine(ReturnToPoolAfterPlay(ps));
    }

    private IEnumerator ReturnToPoolAfterPlay(ParticleSystem ps)
    {
        float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        yield return new WaitForSeconds(lifetime);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
    }

    private AudioClip GetRandomClip()
    {
        if (footstepSounds.Length == 1) return footstepSounds[0];
        AudioClip clip;
        int attempts = 0;
        do
        {
            clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
            attempts++;
        }
        while (clip == _lastClip && attempts < 5);
        _lastClip = clip;
        return clip;
    }
}