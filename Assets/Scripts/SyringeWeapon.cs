using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SyringeWeapon : MonoBehaviour
{
    [Header("Data")]
    public SyringeWeaponData data;

    [Header("References")]
    public Transform bulletSpawnPoint;
    public SyringeAmmo syringeAmmo;
    public AudioSource audioSource;

    [Header("Layers")]
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("Pool Sizes")]
    public int trailPoolSize = 10;
    public int muzzlePoolSize = 3;
    public int hitPoolSize = 6;

    private float _nextFireTime;
    private bool _isReloading;

    [Header("Animation")]
    public Animator animator;

    private readonly Queue<TrailRenderer> _trailPool = new Queue<TrailRenderer>();
    private readonly Queue<ParticleSystem> _muzzlePool = new Queue<ParticleSystem>();
    private readonly Queue<ParticleSystem> _hitPool = new Queue<ParticleSystem>();

    private void Start()
    {
        if (!syringeAmmo) syringeAmmo = GetComponentInParent<SyringeAmmo>();
        if (!animator) animator = GetComponentInParent<Animator>();
        WarmupPools();
    }

    private void WarmupPools()
    {
        if (data.trailPrefab != null)
        {
            for (int i = 0; i < trailPoolSize; i++)
            {
                var go = Instantiate(data.trailPrefab);
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
        bool hasAmmo = syringeAmmo ? syringeAmmo.CanShoot() : true;
        return data && !_isReloading && Time.time >= _nextFireTime && hasAmmo;
    }

    public void Fire(Transform target)
    {
        if (!CanFire()) return;

        _nextFireTime = Time.time + data.fireRate;
        if (syringeAmmo) syringeAmmo.ConsumeAmmo();

        if (audioSource && data.shootSound)
            audioSource.PlayOneShot(data.shootSound);

        PlayMuzzleFlash();

        var col = target.GetComponent<Collider>();
        Vector3 hitPoint = col != null ? col.bounds.center : target.position;

        StartCoroutine(FireRaycast(target, hitPoint));
        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        _isReloading = true;

        if (animator) animator.SetTrigger(Animator.StringToHash("isReloading"));

        yield return new WaitForSeconds(0.3f);

        if (audioSource && data.reloadSound)
            audioSource.PlayOneShot(data.reloadSound);

        yield return new WaitForSeconds(data.reloadTime - 0.3f);

        _isReloading = false;
    }
    public void FireAtPoint(Vector3 worldPoint, LayerMask targetMask, LayerMask obstacleMask)
    {
        if (!CanFire()) return;

        _nextFireTime = Time.time + data.fireRate;
        if (syringeAmmo) syringeAmmo.ConsumeAmmo();

        if (audioSource && data.shootSound)
            audioSource.PlayOneShot(data.shootSound);

        PlayMuzzleFlash();

        Vector3 origin = bulletSpawnPoint.position;
        Vector3 dir = (worldPoint - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, data.range, targetMask | obstacleMask))
        {
            Vector3 hitPoint = hit.point;
            StartCoroutine(SpawnTrail(hitPoint));

            if (((1 << hit.collider.gameObject.layer) & targetMask) != 0)
            {
                var zombie = hit.transform.GetComponent<ZombieAI>();
                if (zombie != null && !zombie.IsDead && zombie.Faction == ZombieFaction.Enemy)
                {
                    zombie.alliedDuration = data.alliedDuration;
                    zombie.Vaccinate();
                    PlayHitEffect(hitPoint);
                    if (audioSource && data.hitSound)
                        audioSource.PlayOneShot(data.hitSound);
                }
            }
        }
        else
        {
            StartCoroutine(SpawnTrail(origin + dir * data.range));
        }

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator FireRaycast(Transform target, Vector3 hitPoint)
    {
        yield return null;

        if (target == null || !target.gameObject.activeInHierarchy)
        {
            StartCoroutine(SpawnTrail(hitPoint));
            yield break;
        }

        var zombie = target.GetComponent<ZombieAI>();
        if (zombie == null || zombie.IsDead || zombie.Faction != ZombieFaction.Enemy)
        {
            StartCoroutine(SpawnTrail(hitPoint));
            yield break;
        }

        Vector3 origin = bulletSpawnPoint.position + Vector3.up * 0.1f;
        Vector3 dir = (hitPoint - origin).normalized;
        float dist = Vector3.Distance(origin, hitPoint);

        bool blocked = Physics.Raycast(origin, dir, dist, obstacleMask);

        StartCoroutine(SpawnTrail(hitPoint));

        if (!blocked)
        {
            zombie.alliedDuration = data.alliedDuration;
            zombie.Vaccinate();
            PlayHitEffect(hitPoint);

            if (audioSource && data.hitSound)
                audioSource.PlayOneShot(data.hitSound);
        }
    }

    private void PlayMuzzleFlash()
    {
        if (data.muzzleFlash == null || bulletSpawnPoint == null) return;

        ParticleSystem muzzle = DequeueOrCreate(_muzzlePool, data.muzzleFlash);
        muzzle.transform.SetPositionAndRotation(bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        muzzle.transform.SetParent(bulletSpawnPoint);
        muzzle.gameObject.SetActive(true);
        muzzle.Play();
        StartCoroutine(ReturnToPool(muzzle, muzzle.main.duration, _muzzlePool, bulletSpawnPoint));
    }

    private void PlayHitEffect(Vector3 point)
    {
        if (data.hitEffect == null) return;

        ParticleSystem ps = DequeueOrCreate(_hitPool, data.hitEffect);
        ps.transform.position = point;
        ps.gameObject.SetActive(true);
        ps.Play();
        float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
        StartCoroutine(ReturnToPool(ps, lifetime, _hitPool, null));
    }

    private IEnumerator ReturnToPool(ParticleSystem ps, float delay, Queue<ParticleSystem> pool, Transform resetParent)
    {
        yield return new WaitForSeconds(delay);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.gameObject.SetActive(false);
        if (resetParent != null) ps.transform.SetParent(null);
        pool.Enqueue(ps);
    }

    private IEnumerator SpawnTrail(Vector3 end)
    {
        if (data.trailPrefab == null) yield break;

        TrailRenderer trail = null;
        while (_trailPool.Count > 0)
        {
            var c = _trailPool.Dequeue();
            if (c != null) { trail = c; break; }
        }
        if (trail == null)
            trail = Instantiate(data.trailPrefab).GetComponent<TrailRenderer>();

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

        float speed = data.projectileSpeed;
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

    private static T DequeueOrCreate<T>(Queue<T> pool, T prefab) where T : Object
    {
        while (pool.Count > 0)
        {
            var c = pool.Dequeue();
            if (c != null) return c;
        }
        return Instantiate(prefab);
    }
}