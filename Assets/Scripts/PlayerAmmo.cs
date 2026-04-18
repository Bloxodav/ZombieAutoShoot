using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerAmmo : MonoBehaviour
{
    [Header("References")]
    public PlayerProgressSO progress;
    public CharacterDataSO characterData;
    public Weapon weapon;

    [Header("Syringe Ammo")]
    public SyringeAmmo syringeAmmo;

    [Header("Ammo Settings")]
    public int maxAmmo = 300;

    [Header("UI")]
    public TextMeshProUGUI mainAmmoText;
    public TextMeshProUGUI secondaryAmmoText;

    [Header("Reload Sound")]
    public AudioSource audioSource;

    private int _currentAmmo;
    private int _currentInMagazine;
    private int _magazineSize;
    private float _reloadTime;
    private bool _isReloading;

    private ShootMode _displayMode = ShootMode.Bullet;

    private void Start()
    {
        _magazineSize = weapon && weapon.data ? weapon.data.magazineSize : 30;
        _reloadTime = weapon && weapon.data ? weapon.data.reloadTime : 1.5f;

        int bonusAmmo = progress.ammoLevel * 20;
        int startAmmo = characterData.startAmmo + bonusAmmo;

        _currentAmmo = Mathf.Clamp(startAmmo, 0, maxAmmo);
        _currentInMagazine = _magazineSize;

        if (syringeAmmo != null)
            syringeAmmo.OnAmmoChanged += UpdateUI;

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (syringeAmmo != null)
            syringeAmmo.OnAmmoChanged -= UpdateUI;
    }

    private void Update()
    {
        if (_displayMode != ShootMode.Bullet) return;

        if (!_isReloading && _currentInMagazine <= 0 && _currentAmmo > 0)
            StartReload();

        if (Input.GetKeyDown(KeyCode.R) && CanReload())
            StartReload();
    }

    public void SetDisplayMode(ShootMode mode)
    {
        _displayMode = mode;
        UpdateUI();
    }

    public bool CanShoot() => !_isReloading && _currentInMagazine > 0;

    public void ConsumeAmmo()
    {
        if (!CanShoot()) return;
        _currentInMagazine--;
        UpdateUI();
    }

    private bool CanReload() =>
        !_isReloading && _currentAmmo > 0 && _currentInMagazine < _magazineSize;

    public void StartReload()
    {
        if (!CanReload()) return;
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;

        var reloadSound = weapon && weapon.data ? weapon.data.reloadSound : null;
        if (audioSource && reloadSound)
            audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(_reloadTime);

        int needed = _magazineSize - _currentInMagazine;
        int toLoad = Mathf.Min(needed, _currentAmmo);

        _currentAmmo -= toLoad;
        _currentInMagazine += toLoad;

        _isReloading = false;
        UpdateUI();
    }

    public void AddAmmo(int amount)
    {
        _currentAmmo = Mathf.Clamp(_currentAmmo + amount, 0, maxAmmo);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (_displayMode == ShootMode.Syringe && syringeAmmo != null)
        {
            if (mainAmmoText)
            {
                mainAmmoText.text = syringeAmmo.CurrentMagazine.ToString();
                mainAmmoText.color = Color.cyan;
            }
            if (secondaryAmmoText)
                secondaryAmmoText.text = syringeAmmo.CurrentReserve.ToString();
            return;
        }

        if (mainAmmoText)
        {
            mainAmmoText.text = _currentInMagazine.ToString();
            mainAmmoText.color = GetAmmoColor();
        }

        if (secondaryAmmoText)
            secondaryAmmoText.text = _currentAmmo.ToString();
    }

    private Color GetAmmoColor()
    {
        float ratio = (float)_currentInMagazine / _magazineSize;
        if (ratio <= 0.25f) return Color.red;
        if (ratio <= 0.375f) return new Color(1f, 0.4f, 0f);
        if (ratio <= 0.5f) return Color.yellow;
        return Color.white;
    }

    public int CurrentMagazine => _currentInMagazine;
    public int CurrentAmmo => _currentAmmo;
}