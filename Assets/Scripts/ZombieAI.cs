using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using UnityEngine.UI;

public enum ZombieFaction { Enemy, Allied }

public class ZombieAI : MonoBehaviour
{
    public System.Action<ZombieAI> OnDeath;
    public System.Action<ZombieAI> OnVaccinated;
    public System.Action<ZombieAI> OnRevertedToEnemy;

    [Header("Stats")]
    public float maxHealth = 100f;
    public float damage = 10f;
    public float attackRange = 2f;
    public float minStopDistance = 1.5f;
    public float attackCooldown = 1.5f;
    public float damageBarSpeed = 2f;

    [Header("Allied")]
    public float alliedDuration = 8f;
    public float alliedSpeedMultiplier = 1.3f;
    public GameObject alliedVFX;
    public float alliedDamageMultiplier = 2f;
    public float alliedAttackCooldownMultiplier = 0.5f;
    public float alliedScaleMultiplier = 1.5f;
    public float blinkStartTime = 3f;
    public float blinkInterval = 0.3f;

    private Vector3 _baseScale;
    private float _baseDamage;
    private float _baseAttackCooldown;
    private Coroutine _blinkCoroutine;

    [Header("Effects")]
    public GameObject deathParticlesPrefab;
    public float timeBeforeSmoke = 1.5f;

    [Header("Components")]
    public Animator animator;
    public NavMeshAgent agent;
    public Transform target;
    public Image healthBarFill;
    public Image damageBarFill;
    public Canvas healthBarCanvas;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip[] growlSounds;
    [Range(3f, 10f)] public float growlIntervalMin = 4f;
    [Range(3f, 10f)] public float growlIntervalMax = 8f;
    public AudioClip[] deathSounds;
    public AudioClip puffSound;
    public AudioClip attackSound;

    public AudioSettingsData audioSettings;
    [Range(0f, 1f)] public float growlVolume = 0.3f;

    [Header("Loot Drop")]
    public PickupDataSO[] lootTable;
    public float lootSpreadRadius = 1f;

    public ZombieFaction Faction { get; private set; } = ZombieFaction.Enemy;
    public bool IsDead => _isDead;

    private float _currentHealth;
    private float _targetDamageFill;
    private bool _isDead;
    private bool _isAttacking;
    private float _nextAttackTime;
    private float _alliedTimer;
    private float _baseSpeed;

    private Transform _cachedCameraTransform;
    private Collider _collider;

    private Renderer[] _renderers;
    private Material[][] _cachedMaterials;
    private Color[] _originalColors;

    private static readonly int s_IsWalking = Animator.StringToHash("isWalking");
    private static readonly int s_Attack = Animator.StringToHash("attack");
    private static readonly int s_Die = Animator.StringToHash("die");

    private static Transform _cachedPlayerTransform;
    private static PlayerHealth _cachedPlayerHealth;

    private void Awake()
    {
        agent = agent ? agent : GetComponent<NavMeshAgent>();
        animator = animator ? animator : GetComponent<Animator>();
        _collider = GetComponent<Collider>();

        _baseScale = transform.localScale;
        _baseDamage = damage;
        _baseAttackCooldown = attackCooldown;
        if (alliedVFX) alliedVFX.SetActive(false);

        if (Camera.main != null)
            _cachedCameraTransform = Camera.main.transform;

        healthBarCanvas = healthBarCanvas ? healthBarCanvas : GetComponentInChildren<Canvas>();
        if (healthBarCanvas) SetupHealthBars();

        CacheRenderers();
    }

    private void CacheRenderers()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _cachedMaterials = new Material[_renderers.Length][];
        _originalColors = new Color[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _cachedMaterials[i] = _renderers[i].materials;
            if (_cachedMaterials[i].Length > 0 && _cachedMaterials[i][0].HasProperty("_Color"))
                _originalColors[i] = _cachedMaterials[i][0].color;
        }
    }

    private void OnEnable()
    {
        ResetState();
        ZombieFactionRegistry.Register(this);
        StartCoroutine(UpdateAI());
        StartCoroutine(GrowlLoop());
    }

    private void OnDisable()
    {
        ZombieFactionRegistry.Unregister(this);
    }

    private IEnumerator GrowlLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, growlIntervalMax));

        while (!_isDead)
        {
            if (growlSounds != null && growlSounds.Length > 0 && audioSource != null)
            {
                var clip = growlSounds[Random.Range(0, growlSounds.Length)];
                float vol = growlVolume * (audioSettings != null ? audioSettings.sfxVolume : 1f);
                audioSource.PlayOneShot(clip, vol);
            }
            yield return new WaitForSeconds(Random.Range(growlIntervalMin, growlIntervalMax));
        }
    }

    public void ResetState()
    {
        OnDeath = null;
        OnVaccinated = null;
        OnRevertedToEnemy = null;

        _isDead = false;
        _isAttacking = false;
        _nextAttackTime = 0f;
        _currentHealth = maxHealth;
        _targetDamageFill = 1f;
        target = null;
        _alliedTimer = 0f;

        Faction = ZombieFaction.Enemy;
        gameObject.layer = LayerMask.NameToLayer("Zombie");

        if (_collider) _collider.enabled = true;

        if (agent)
        {
            agent.enabled = true;
            agent.isStopped = false;
            agent.stoppingDistance = minStopDistance;
            agent.autoBraking = false;
            _baseSpeed = agent.speed;
        }

        if (healthBarCanvas) healthBarCanvas.gameObject.SetActive(false);
        if (healthBarFill) healthBarFill.fillAmount = 1f;
        if (damageBarFill) damageBarFill.fillAmount = 1f;

        if (animator)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (_blinkCoroutine != null) { StopCoroutine(_blinkCoroutine); _blinkCoroutine = null; }
        if (alliedVFX) alliedVFX.SetActive(false);
        transform.localScale = _baseScale;
        damage = _baseDamage;
        attackCooldown = _baseAttackCooldown;
    }

    private void SetupHealthBars()
    {
        if (healthBarFill && damageBarFill) return;
        foreach (var img in healthBarCanvas.GetComponentsInChildren<Image>())
        {
            if (img.name == "HealthFill") healthBarFill = img;
            else if (img.type == Image.Type.Filled && img != healthBarFill) damageBarFill = img;
        }
    }

    private void Update()
    {
        if (healthBarCanvas && healthBarCanvas.gameObject.activeInHierarchy && _cachedCameraTransform != null)
            healthBarCanvas.transform.rotation = _cachedCameraTransform.rotation;

        if (healthBarFill)
            healthBarFill.fillAmount = Mathf.MoveTowards(healthBarFill.fillAmount, _targetDamageFill, Time.deltaTime * damageBarSpeed * 10f);

        if (damageBarFill)
            damageBarFill.fillAmount = Mathf.MoveTowards(damageBarFill.fillAmount, _targetDamageFill, Time.deltaTime * damageBarSpeed);

        if (Faction == ZombieFaction.Allied)
        {
            _alliedTimer -= Time.deltaTime;
            if (_alliedTimer <= 0f) RevertToEnemy();
        }
    }

    private IEnumerator UpdateAI()
    {
        var waitShort = new WaitForSeconds(0.1f);
        var waitLong = new WaitForSeconds(0.3f);

        while (!_isDead)
        {
            RefreshTarget();

            if (!target) { yield return waitLong; continue; }

            float sqrDist = (transform.position - target.position).sqrMagnitude;
            float sqrRange = attackRange * attackRange;

            RotateTowardsTarget();

            if (sqrDist > sqrRange)
            {
                if (agent && agent.enabled)
                {
                    agent.isStopped = false;
                    agent.stoppingDistance = minStopDistance;
                    agent.SetDestination(target.position);
                }
                animator.SetBool(s_IsWalking, true);
            }
            else
            {
                if (agent && agent.enabled) agent.isStopped = true;
                animator.SetBool(s_IsWalking, false);

                if (Time.time >= _nextAttackTime && !_isAttacking)
                    StartCoroutine(Attack());
            }

            yield return waitShort;
        }
    }

    private void RefreshTarget()
    {
        if (Faction == ZombieFaction.Allied)
        {
            target = ZombieFactionRegistry.GetNearestEnemy(transform);
            return;
        }

        EnsurePlayerCached();

        Transform nearestAlly = ZombieFactionRegistry.GetNearestAlly(transform);

        if (nearestAlly == null) { target = _cachedPlayerTransform; return; }
        if (_cachedPlayerTransform == null) { target = nearestAlly; return; }

        float distToAlly = (nearestAlly.position - transform.position).sqrMagnitude;
        float distToPlayer = (_cachedPlayerTransform.position - transform.position).sqrMagnitude;
        target = distToAlly < distToPlayer ? nearestAlly : _cachedPlayerTransform;
    }

    private void EnsurePlayerCached()
    {
        if (_cachedPlayerTransform != null) return;
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _cachedPlayerTransform = playerObj.transform;
            _cachedPlayerHealth = playerObj.GetComponent<PlayerHealth>();
        }
    }

    private void RotateTowardsTarget()
    {
        if (!target || _isDead) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 40f);
    }

    private IEnumerator Attack()
    {
        if (_isAttacking || _isDead) yield break;
        _isAttacking = true;
        animator.SetTrigger(s_Attack);
        audioSource?.PlayOneShot(attackSound);
        yield return new WaitForSeconds(attackCooldown);
        _nextAttackTime = Time.time + attackCooldown;
        _isAttacking = false;
    }

    public void DealDamageToTarget()
    {
        if (_isDead || !target) return;
        if ((transform.position - target.position).sqrMagnitude > (attackRange + 0.5f) * (attackRange + 0.5f)) return;

        if (Faction == ZombieFaction.Allied)
        {
            target.GetComponent<ZombieAI>()?.TakeDamage(damage);
            return;
        }

        if (target == _cachedPlayerTransform)
            _cachedPlayerHealth?.TakeDamage(damage);
        else
            target.GetComponent<ZombieAI>()?.TakeDamage(damage);
    }

    public void DealDamageToPlayer() => DealDamageToTarget();

    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        _currentHealth = Mathf.Max(_currentHealth - amount, 0f);

        if (healthBarCanvas)
        {
            healthBarCanvas.gameObject.SetActive(true);
            _targetDamageFill = _currentHealth / maxHealth;
        }

        if (_currentHealth <= 0f)
            StartCoroutine(DieDelayed());
    }

    public void SyncHealthToMax()
    {
        _currentHealth = maxHealth;
        _targetDamageFill = 1f;
        if (healthBarFill) healthBarFill.fillAmount = 1f;
        if (damageBarFill) damageBarFill.fillAmount = 1f;
        if (healthBarCanvas) healthBarCanvas.gameObject.SetActive(false);
    }

    public void SyncBaseSpeed()
    {
        if (agent) _baseSpeed = agent.speed;
    }

    public void Vaccinate()
    {
        if (_isDead) return;

        ZombieFactionRegistry.Unregister(this);
        Faction = ZombieFaction.Allied;
        ZombieFactionRegistry.Register(this);

        _alliedTimer = alliedDuration;
        target = null;

        int alliedLayer = LayerMask.NameToLayer("AlliedZombie");
        if (alliedLayer >= 0) gameObject.layer = alliedLayer;

        if (agent && _baseSpeed > 0f)
            agent.speed = _baseSpeed * alliedSpeedMultiplier;

        transform.localScale = _baseScale * alliedScaleMultiplier;
        damage = _baseDamage * alliedDamageMultiplier;
        attackCooldown = _baseAttackCooldown * alliedAttackCooldownMultiplier;
        if (alliedVFX) alliedVFX.SetActive(true);
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(AlliedVFXBlink());
        OnVaccinated?.Invoke(this);
    }

    private void RevertToEnemy()
    {
        ZombieFactionRegistry.Unregister(this);
        Faction = ZombieFaction.Enemy;
        ZombieFactionRegistry.Register(this);

        target = null;
        _alliedTimer = 0f;

        int zombieLayer = LayerMask.NameToLayer("Zombie");
        if (zombieLayer >= 0) gameObject.layer = zombieLayer;

        if (agent && _baseSpeed > 0f)
            agent.speed = _baseSpeed;

        transform.localScale = _baseScale;
        damage = _baseDamage;
        attackCooldown = _baseAttackCooldown;
        if (alliedVFX) alliedVFX.SetActive(false);
        OnRevertedToEnemy?.Invoke(this);
    }
    private IEnumerator AlliedVFXBlink()
    {
        yield return new WaitForSeconds(alliedDuration - blinkStartTime);

        if (alliedVFX == null) yield break;

        float elapsed = 0f;
        while (elapsed < blinkStartTime)
        {
            alliedVFX.SetActive(!alliedVFX.activeSelf);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        alliedVFX.SetActive(false);
        _blinkCoroutine = null;
    }

    private static void PlaySoundAtPoint(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        var go = new GameObject("ZombieDeathSound");
        go.transform.position = position;
        var src = go.AddComponent<AudioSource>();
        src.spatialBlend = 1f;
        src.volume = volume;
        src.clip = clip;
        src.Play();
        Object.Destroy(go, clip.length);
    }

    private IEnumerator DieDelayed()
    {
        _isDead = true;

        int defaultLayer = LayerMask.NameToLayer("Default");
        if (defaultLayer >= 0) gameObject.layer = defaultLayer;

        animator.SetTrigger(s_Die);

        if (deathSounds != null && deathSounds.Length > 0)
        {
            var clip = deathSounds[Random.Range(0, deathSounds.Length)];
            float vol = audioSettings != null ? audioSettings.sfxVolume : 1f;
            PlaySoundAtPoint(clip, transform.position, vol);
        }

        if (_collider) _collider.enabled = false;
        if (agent) { agent.isStopped = true; agent.enabled = false; }
        healthBarCanvas?.gameObject.SetActive(false);

        yield return new WaitForSeconds(timeBeforeSmoke);

        if (deathParticlesPrefab != null)
        {
            var ps = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);
            var p = ps.GetComponent<ParticleSystem>();
            float lifetime = p != null ? p.main.duration + p.main.startLifetime.constantMax : 3f;
            Destroy(ps, lifetime);
        }

        if (puffSound != null)
        {
            float vol = audioSettings != null ? audioSettings.sfxVolume : 1f;
            PlaySoundAtPoint(puffSound, transform.position, vol);
        }

        DropLoot();
        OnDeath?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void DropLoot()
    {
        if (Faction == ZombieFaction.Allied) return;
        if (lootTable == null || lootTable.Length == 0) return;

        for (int i = 0; i < lootTable.Length; i++)
        {
            var entry = lootTable[i];
            if (entry == null || entry.prefab == null) continue;
            if (Random.value > entry.dropChance) continue;

            float angle = (360f / lootTable.Length) * i * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * lootSpreadRadius;
            var go = Instantiate(entry.prefab, transform.position + offset, Quaternion.identity);
            var pickup = go.GetComponent<PickupItem>();
            if (pickup) pickup.data = entry;
        }
    }

    public static void ClearPlayerCache()
    {
        _cachedPlayerTransform = null;
        _cachedPlayerHealth = null;
    }
}