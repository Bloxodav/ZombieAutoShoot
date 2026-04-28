using UnityEngine;

public enum ShootMode { Bullet, Syringe }

public class PlayerCombat : MonoBehaviour
{
    public bool HasTarget => true; // всегда смотрим на курсор

    [Header("Camera")]
    public Camera mainCamera;

    [Header("References")]
    public Weapon currentWeapon;
    public SyringeWeapon syringeWeapon;
    public PlayerAimController aimController;
    public PlayerAmmo playerAmmo;
    public SyringeAmmo syringeAmmo;
    public Animator animator;

    [Header("Layers")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Weapon Switch Sound")]
    public AudioSource audioSource;
    public AudioClip weaponSwitchSound;

    [Header("Auto Reload")]
    public float autoReloadDelay = 0.3f;

    private Vector3 _aimWorldPoint;
    private bool _isSwitching;
    private bool _pendingAutoReload;

    private int _isFireHash;
    private int _isRifleHash;
    private int _isPistolHash;
    private int _switchToSyringeHash;
    private int _switchToRifleHash;

    private ShootMode _currentMode = ShootMode.Bullet;
    public ShootMode CurrentShootMode => _currentMode;

    private void Start()
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!playerAmmo) playerAmmo = GetComponent<PlayerAmmo>();
        if (!syringeAmmo) syringeAmmo = GetComponent<SyringeAmmo>();
        if (!mainCamera) mainCamera = Camera.main;

        _isFireHash = Animator.StringToHash("isFire");
        _isRifleHash = Animator.StringToHash("isRifle");
        _isPistolHash = Animator.StringToHash("isPistol");
        _switchToSyringeHash = Animator.StringToHash("switchToSyringe");
        _switchToRifleHash = Animator.StringToHash("switchToRifle");
    }

    private void Update()
    {
        UpdateWeaponState();

        if (_isSwitching) return;

        UpdateAimPoint();
        HandleInput();
        HandleAutoReload();
    }

    private void UpdateAimPoint()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float dist))
        {
            _aimWorldPoint = ray.GetPoint(dist);

            // ¬–≈ћ≈ЌЌќ Ч рисуй сферу в точке прицела
            Debug.DrawLine(mainCamera.transform.position, _aimWorldPoint, Color.red);
        }

        if (aimController != null)
            aimController.SetAimPoint(_aimWorldPoint);
    }

    private void HandleInput()
    {
        // Q Ч смена оружи€
        if (Input.GetKeyDown(KeyCode.Q))
            ToggleShootMode();

        // R Ч ручна€ перезар€дка
        if (Input.GetKeyDown(KeyCode.R))
            TryReload();

        // LMB Ч стрельба
        bool isShooting = Input.GetMouseButton(0);
        animator.SetBool(_isFireHash, isShooting);

        if (isShooting)
            TryShoot();
    }

    private void TryShoot()
    {
        if (_currentMode == ShootMode.Bullet)
        {
            if (currentWeapon != null && currentWeapon.CanFire())
                currentWeapon.FireAtPoint(_aimWorldPoint, targetMask, obstacleMask);
        }
        else
        {
            if (syringeWeapon != null && syringeWeapon.CanFire())
                syringeWeapon.FireAtPoint(_aimWorldPoint, targetMask, obstacleMask);
        }
    }

    private void HandleAutoReload()
    {
        if (_currentMode == ShootMode.Bullet)
        {
            if (playerAmmo != null && playerAmmo.CurrentMagazine <= 0 && !playerAmmo.IsReloading)
                playerAmmo.StartReload();
        }
        else
        {
            if (syringeAmmo != null && syringeAmmo.CurrentMagazine <= 0 && !syringeAmmo.IsReloading)
                syringeAmmo.StartReload();
        }
    }

    private void TryReload()
    {
        if (_currentMode == ShootMode.Bullet)
            playerAmmo?.StartReload();
        else
            syringeAmmo?.StartReload();
    }

    public void ToggleShootMode()
    {
        if (_isSwitching) return;

        _isSwitching = true;
        animator.SetBool(_isFireHash, false);

        if (audioSource && weaponSwitchSound)
            audioSource.PlayOneShot(weaponSwitchSound);

        if (_currentMode == ShootMode.Bullet)
        {
            _currentMode = ShootMode.Syringe;
            animator.SetTrigger(_switchToSyringeHash);
        }
        else
        {
            _currentMode = ShootMode.Bullet;
            animator.SetTrigger(_switchToRifleHash);
        }

        playerAmmo?.SetDisplayMode(_currentMode);
    }

    public void OnWeaponSwitchComplete()
    {
        _isSwitching = false;
        if (currentWeapon != null)
            currentWeapon.nextFireTime = Time.time + 0.15f;
    }

    private void UpdateWeaponState()
    {
        if (!currentWeapon || !currentWeapon.data) return;
        WeaponType type = currentWeapon.data.weaponType;
        animator.SetBool(_isRifleHash, type == WeaponType.Rifle);
        animator.SetBool(_isPistolHash, type == WeaponType.Pistol);
    }
}