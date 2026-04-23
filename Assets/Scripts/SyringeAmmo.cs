using UnityEngine;
using System.Collections;

public class SyringeAmmo : MonoBehaviour
{
    [Header("Settings")]
    public int maxSyringes = 20;
    public int startSyringes = 10;
    public float reloadTime = 2f;
    public int magazineSize = 5;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip reloadSound;

    private int _inMagazine;
    private int _inReserve;
    private bool _isReloading;

    public System.Action OnAmmoChanged;

    private void Start()
    {
        _inReserve = Mathf.Clamp(startSyringes, 0, maxSyringes);
        _inMagazine = Mathf.Min(magazineSize, _inReserve);
        _inReserve -= _inMagazine;
        OnAmmoChanged?.Invoke();
    }

    public bool CanShoot() => !_isReloading && _inMagazine > 0;

    public void ConsumeAmmo()
    {
        if (!CanShoot()) return;
        _inMagazine--;
        OnAmmoChanged?.Invoke();
    }

    public void StartReload()
    {
        if (_isReloading || _inReserve <= 0 || _inMagazine >= magazineSize) return;
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;
        if (audioSource && reloadSound) audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - _inMagazine;
        int toLoad = Mathf.Min(needed, _inReserve);
        _inMagazine += toLoad;
        _inReserve -= toLoad;

        _isReloading = false;
        OnAmmoChanged?.Invoke();
    }

    public void AddSyringes(int amount)
    {
        _inReserve = Mathf.Clamp(_inReserve + amount, 0, maxSyringes);
        OnAmmoChanged?.Invoke();
    }

    public int CurrentMagazine => _inMagazine;
    public int CurrentReserve => _inReserve;
}