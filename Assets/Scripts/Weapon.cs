using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Data")]
    public WeaponData data;

    [Header("References")]
    public Transform bulletSpawnPoint;
    public GameObject bulletTrailPrefab;
    public AudioSource audioSource;
    public PlayerProgressSO progress;

    [Header("Layers")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Ammo System")]
    public PlayerAmmo playerAmmo;

    [Header("Pool Sizes")]
    public int trailPoolSize = 10;
    public int muzzlePoolSize = 5;
    public int hitPoolSize = 8;

    [Header("Audio Settings")]
    public AudioSettingsData audioSettings;
    [Range(0f, 1f)] public float shootVolume = 1f;

    [HideInInspector] public float nextFireTime;

    private Queue<TrailRenderer> _trailPool = new Queue<TrailRenderer>();
    private Queue<ParticleSystem> _muzzlePool = new Queue<ParticleSystem>();
    private Queue<ParticleSystem> _hitPool = new Queue<ParticleSystem>();

    private static readonly Collider[] _targetBuffer = new Collider[1];

    public float CurrentFireRate
    {
        get
        {
            if (progress == null) return data.fireRate;
            return Mathf.Max(data.fireRate - progress.fireRateLevel * 0.05f, 0.05f);
        }
    }

    private void Start()
    {
        if (!playerAmmo) playerAmmo = GetComponentInParent<PlayerAmmo>();
        if (!playerAmmo)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) playerAmmo = player.GetComponent<PlayerAmmo>();
        }

        if (!progress)
        {
            var um = FindFirstObjectByType<UpgradeManager>();
            if (um) progress = um.progress;
        }

        WarmupPools();
    }

    private void WarmupPools()
    {
        if (bulletTrailPrefab != null)
        {
            for (int i = 0; i < trailPoolSize; i++)
            {
                var go = Instantiate(bulletTrailPrefab);
                go.SetActive(false);
                var trail = go.GetComponent<TrailRenderer>();
                if (trail) _trailPool.Enqueue(trail);
            }
        }

        if (data.muzzleFlash != null)
        {
            for (int i = 0; i < muzzlePoolSize; i++)
            {
                var ps = Instantiate(data.muzzleFlash);
                ps.gameObject.SetActive(false);
                _muzzlePool.Enqueue(ps);
            }
        }

        if (data.hitEffect != null)
        {
            for (int i = 0; i < hitPoolSize; i++)
            {
                var ps = Instantiate(data.hitEffect);
                ps.gameObject.SetActive(false);
                _hitPool.Enqueue(ps);
            }
        }
    }

    public bool CanFire()
    {
        bool hasAmmo = playerAmmo ? playerAmmo.CanShoot() : true;
        return data && Time.time >= nextFireTime && hasAmmo;
    }

    public void Fire(Transform target)
    {
        if (!CanFire()) return;

        nextFireTime = Time.time + CurrentFireRate;
        if (playerAmmo) playerAmmo.ConsumeAmmo();

        PlayMuzzleFlash();

        var shootSound = data.GetRandomShootSound();
        if (audioSource && shootSound)
        {
            float vol = shootVolume * (audioSettings != null ? audioSettings.sfxVolume : 1f);
            audioSource.PlayOneShot(shootSound, vol);
        }

        switch (data.weaponType)
        {
            case WeaponType.Shotgun: FireShotgun(target); break;
            default: FireSingle(target); break;
        }
    }

    private void PlayMuzzleFlash()
    {
        if (data.muzzleFlash == null || bulletSpawnPoint == null) return;

        ParticleSystem muzzle = GetFromMuzzlePool();
        muzzle.transform.SetPositionAndRotation(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        muzzle.transform.SetParent(bulletSpawnPoint);
        muzzle.gameObject.SetActive(true);
        muzzle.Play();
        StartCoroutine(ReturnMuzzleToPool(muzzle));
    }

    private ParticleSystem GetFromMuzzlePool()
    {
        while (_muzzlePool.Count > 0)
        {
            var c = _muzzlePool.Dequeue();
            if (c != null) return c;
        }
        return Instantiate(data.muzzleFlash);
    }

    private IEnumerator ReturnMuzzleToPool(ParticleSystem muzzle)
    {
        yield return new WaitForSeconds(muzzle.main.duration);
        muzzle.gameObject.SetActive(false);
        muzzle.transform.SetParent(null);
        _muzzlePool.Enqueue(muzzle);
    }

    private void FireSingle(Transform target)
    {
        var col = target.GetComponent<Collider>();
        if (col == null) return;
        Vector3 hitPoint = col.bounds.center;
        StartCoroutine(SpawnTrail(hitPoint));
        DealDamage(target, hitPoint);
    }

    private void FireShotgun(Transform target)
    {
        var col = target.GetComponent<Collider>();
        if (col == null) return;
        Vector3 center = col.bounds.center;

        for (int i = 0; i < data.pellets; i++)
        {
            Vector3 dir = (center - bulletSpawnPoint.position).normalized;
            dir = Quaternion.Euler(
                Random.Range(-data.spreadAngle, data.spreadAngle),
                Random.Range(-data.spreadAngle, data.spreadAngle),
                0f) * dir;

            if (Physics.Raycast(bulletSpawnPoint.position, dir, out RaycastHit hit, data.range, targetMask | obstacleMask))
            {
                StartCoroutine(SpawnTrail(hit.point));
                if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                    DealDamage(hit.transform, hit.point);
            }
        }
    }

    private void DealDamage(Transform target, Vector3 hitPoint)
    {
        var zombie = target.GetComponent<ZombieAI>();
        if (!zombie) return;

        zombie.TakeDamage(data.damage);

        if (audioSource && data.hitSound)
            audioSource.PlayOneShot(data.hitSound);

        PlayHitEffect(hitPoint);
    }

    private void PlayHitEffect(Vector3 hitPoint)
    {
        if (data.hitEffect == null) return;

        ParticleSystem ps = null;
        while (_hitPool.Count > 0)
        {
            var c = _hitPool.Dequeue();
            if (c != null) { ps = c; break; }
        }
        if (ps == null) ps = Instantiate(data.hitEffect);

        ps.transform.position = hitPoint;
        ps.gameObject.SetActive(true);
        ps.Play();
        StartCoroutine(ReturnHitToPool(ps));
    }

    private IEnumerator ReturnHitToPool(ParticleSystem ps)
    {
        float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        yield return new WaitForSeconds(lifetime);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        _hitPool.Enqueue(ps);
    }
    public void FireAtPoint(Vector3 worldPoint, LayerMask targetMask, LayerMask obstacleMask)
    {
        if (!CanFire()) return;

        nextFireTime = Time.time + CurrentFireRate;
        if (playerAmmo) playerAmmo.ConsumeAmmo();

        PlayMuzzleFlash();

        var shootSound = data.GetRandomShootSound();
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);

        if (data.weaponType == WeaponType.Shotgun)
        {
            FireAtPointShotgun(worldPoint, targetMask, obstacleMask);
            return;
        }

        Vector3 origin = bulletSpawnPoint.position;
        Vector3 dir = (worldPoint - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, data.range, targetMask | obstacleMask))
        {
            StartCoroutine(SpawnTrail(hit.point));
            if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                DealDamage(hit.transform, hit.point);
        }
        else
        {
            StartCoroutine(SpawnTrail(origin + dir * data.range));
        }
    }

    private void FireAtPointShotgun(Vector3 worldPoint, LayerMask targetMask, LayerMask obstacleMask)
    {
        Vector3 origin = bulletSpawnPoint.position;
        Vector3 baseDir = (worldPoint - origin).normalized;

        for (int i = 0; i < data.pellets; i++)
        {
            Vector3 dir = Quaternion.Euler(
                Random.Range(-data.spreadAngle, data.spreadAngle),
                Random.Range(-data.spreadAngle, data.spreadAngle),
                0f) * baseDir;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, data.range, targetMask | obstacleMask))
            {
                StartCoroutine(SpawnTrail(hit.point));
                if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
                    DealDamage(hit.transform, hit.point);
            }
            else
            {
                StartCoroutine(SpawnTrail(origin + dir * data.range));
            }
        }
    }
    private IEnumerator SpawnTrail(Vector3 end)
    {
        TrailRenderer trail = null;
        while (_trailPool.Count > 0)
        {
            var c = _trailPool.Dequeue();
            if (c != null) { trail = c; break; }
        }
        if (trail == null)
            trail = Instantiate(bulletTrailPrefab).GetComponent<TrailRenderer>();

        Vector3 start = bulletSpawnPoint.position;
        float dist = Vector3.Distance(start, end);

        trail.Clear();
        trail.transform.position = start;
        trail.gameObject.SetActive(true);
        trail.AddPosition(start);

        if (dist < 0.01f)
        {
            yield return new WaitForSeconds(trail.time);
            trail.Clear();
            trail.gameObject.SetActive(false);
            _trailPool.Enqueue(trail);
            yield break;
        }

        float speed = 120f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * speed / dist;
            trail.transform.position = Vector3.Lerp(start, end, Mathf.Min(t, 1f));
            yield return null;
        }

        yield return new WaitForSeconds(trail.time);
        trail.Clear();
        trail.gameObject.SetActive(false);
        _trailPool.Enqueue(trail);
    }
}