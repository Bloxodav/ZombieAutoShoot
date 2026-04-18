using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    [Header("Модель щита")]
    [Tooltip("Перетащи сюда GameObject с моделью щита. Он будет включаться/выключаться.")]
    public GameObject shieldModel;

    [Header("Длительность")]
    public float duration = 3f;

    [Header("Вращение")]
    public float rotationSpeed = 90f;

    [Header("Пульсация масштаба")]
    public float pulseSpeed = 2f;
    public float pulseAmplitude = 0.06f;

    [Header("Звук")]
    public AudioSource audioSource;
    public AudioClip activateSound;
    public AudioClip deactivateSound;

    private PlayerHealth _playerHealth;
    private Vector3 _baseScale;
    private float _timer;
    private bool _active;

    private void Awake()
    {
        _playerHealth = GetComponentInParent<PlayerHealth>();

        if (shieldModel != null)
        {
            _baseScale = shieldModel.transform.localScale;
            shieldModel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!_active) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f) { Deactivate(); return; }

        if (shieldModel != null)
        {
            shieldModel.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            shieldModel.transform.localScale = _baseScale * pulse;
        }
    }

    public void Activate()
    {
        _timer = duration;
        _active = true;

        if (_playerHealth == null)
            _playerHealth = GetComponentInParent<PlayerHealth>();

        if (shieldModel != null)
        {
            shieldModel.transform.localScale = _baseScale;
            shieldModel.SetActive(true);
        }

        _playerHealth?.SetInvincible(true);
        audioSource?.PlayOneShot(activateSound);
    }

    private void Deactivate()
    {
        _active = false;
        if (shieldModel != null) shieldModel.SetActive(false);
        _playerHealth?.SetInvincible(false);
        audioSource?.PlayOneShot(deactivateSound);
    }

    public float TimeRemaining => _timer;
    public bool IsActive => _active;
}