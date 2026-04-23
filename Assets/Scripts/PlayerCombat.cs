using UnityEngine;

public enum ShootMode { Bullet, Syringe }

public class PlayerCombat : MonoBehaviour
{
    public PlayerProgressSO progress;
    public bool HasTarget => _currentTarget != null;

    [Header("Targeting")]
    public float detectDistance = 20f;
    public float aimDelay = 0.15f;
    public float searchInterval = 0.15f;
    public float hysteresisMargin = 3f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("References")]
    public Weapon currentWeapon;
    public SyringeWeapon syringeWeapon;
    public PlayerAimController aimController;
    public PlayerAmmo playerAmmo;
    public Animator animator;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Weapon Switch Sound")]
    public AudioSource audioSource;
    public AudioClip weaponSwitchSound;

    private Rigidbody _rb;
    private Transform _currentTarget;
    private ZombieAI _currentTargetAI;
    private float _aimTimer;
    private bool _isAimed;
    private float _searchTimer;
    private bool _isSwitching = false;

    private int _switchToSyringeHash;
    private int _switchToRifleHash;

    private ShootMode _currentMode = ShootMode.Bullet;

    private static readonly Collider[] _overlapBuffer = new Collider[32];

    private int _isFireHash;
    private int _isRifleHash;
    private int _isPistolHash;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!playerAmmo) playerAmmo = GetComponent<PlayerAmmo>();

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

        bool hasActiveWeapon = _currentMode == ShootMode.Bullet
            ? (currentWeapon != null && currentWeapon.data != null)
            : (syringeWeapon != null && syringeWeapon.data != null);

        if (!hasActiveWeapon)
        {
            animator.SetBool(_isFireHash, false);
            ClearTarget();
            return;
        }

        if (_currentTarget != null && IsTargetInvalid(_currentTarget, _currentTargetAI))
        {
            animator.SetBool(_isFireHash, false);
            ClearTarget();
            _searchTimer = 0f;
        }

        _searchTimer -= Time.deltaTime;
        if (_searchTimer <= 0f)
        {
            _searchTimer = searchInterval;
            UpdateTarget();
        }

        if (_currentTarget == null) return;

        RotateToTargetViaRigidbody(_currentTarget);

        if (!_isAimed)
        {
            _aimTimer += Time.deltaTime;
            if (_aimTimer >= aimDelay)
                _isAimed = true;
        }

        animator.SetBool(_isFireHash, _isAimed);

        if (!_isAimed) return;

        if (_currentMode == ShootMode.Bullet)
        {
            if (currentWeapon.CanFire())
                currentWeapon.Fire(_currentTarget);
        }
        else
        {
            if (syringeWeapon.CanFire())
                syringeWeapon.Fire(_currentTarget);
        }
    }

    public void ToggleShootMode()
    {
        if (_isSwitching) return;

        _isSwitching = true;
        ClearTarget();
        _searchTimer = 0f;
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
    }

    public void SetShootMode(ShootMode mode)
    {
        if (_currentMode == mode) return;
        _currentMode = mode;
        ClearTarget();
        _searchTimer = 0f;
        playerAmmo?.SetDisplayMode(_currentMode);
    }

    public ShootMode CurrentShootMode => _currentMode;

    private bool IsTargetInvalid(Transform t, ZombieAI ai)
    {
        if (t == null) return true;
        if (!t.gameObject.activeInHierarchy) return true;
        if (ai == null) return true;
        if (ai.IsDead) return true;
        if (ai.Faction == ZombieFaction.Allied) return true;
        return false;
    }

    private void UpdateTarget()
    {
        Transform found = FindNearestValidTarget(out float foundDist);

        if (found == null)
        {
            animator.SetBool(_isFireHash, false);
            ClearTarget();
            return;
        }

        if (_currentTarget == found) return;

        if (_currentTarget == null)
        {
            SetTarget(found);
            return;
        }

        float currentDist = Vector3.Distance(transform.position, _currentTarget.position);
        if (foundDist < currentDist - hysteresisMargin)
            SetTarget(found);
    }

    private void SetTarget(Transform t)
    {
        _currentTarget = t;
        _currentTargetAI = t.GetComponent<ZombieAI>();
        _aimTimer = 0f;
        _isAimed = false;
        aimController.SetTarget(t);
    }

    private void ClearTarget()
    {
        _currentTarget = null;
        _currentTargetAI = null;
        _aimTimer = 0f;
        _isAimed = false;
        if (aimController) aimController.ClearTarget();
    }

    private void RotateToTargetViaRigidbody(Transform target)
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion desired = Quaternion.LookRotation(dir);
        _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, desired, rotationSpeed * Time.deltaTime));
    }

    private void UpdateWeaponState()
    {
        if (!currentWeapon || !currentWeapon.data) return;
        WeaponType type = currentWeapon.data.weaponType;
        animator.SetBool(_isRifleHash, type == WeaponType.Rifle);
        animator.SetBool(_isPistolHash, type == WeaponType.Pistol);
    }

    private Transform FindNearestValidTarget(out float nearestDist)
    {
        int count = Physics.OverlapSphereNonAlloc(
            transform.position, detectDistance, _overlapBuffer, targetMask);

        Transform nearest = null;
        nearestDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var col = _overlapBuffer[i];
            if (!col.gameObject.activeInHierarchy) continue;

            var zombie = col.GetComponent<ZombieAI>();
            if (zombie == null) continue;
            if (zombie.IsDead) continue;
            if (zombie.Faction == ZombieFaction.Allied) continue;

            Vector3 point = col.bounds.center;
            Vector3 dir = (point - transform.position).normalized;

            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit,
                detectDistance, obstacleMask | targetMask))
            {
                if (((1 << hit.collider.gameObject.layer) & targetMask) == 0) continue;
            }

            float d = (point - transform.position).sqrMagnitude;
            if (d < nearestDist)
            {
                nearestDist = d;
                nearest = col.transform;
            }
        }

        nearestDist = nearest != null
            ? Mathf.Sqrt(nearestDist)
            : float.MaxValue;

        return nearest;
    }
}