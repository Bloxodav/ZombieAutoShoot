using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PickupItem : MonoBehaviour
{
    public PickupDataSO data;
    public LevelConfigSO levelConfig;

    private Vector3 _startPos;
    private float _timer;
    private Renderer _renderer;
    private bool _collected;
    private bool _isMoneyCoin;

    private void Start()
    {
        if (data == null) { Destroy(gameObject); return; }

        _isMoneyCoin = (data.pickupType == PickupType.Money);
        _startPos = transform.position + Vector3.up * data.hoverHeight;
        _timer = data.lifetime;
        _renderer = GetComponent<Renderer>();

        if (_isMoneyCoin)
            AutoCollectMoney();
    }

    private void Update()
    {
        if (_isMoneyCoin) return;

        transform.Rotate(Vector3.up, data.rotationSpeed * Time.deltaTime, Space.World);

        float y = _startPos.y + Mathf.Sin(Time.time * data.frequency) * data.amplitude;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);

        _timer -= Time.deltaTime;
        if (_timer <= 0f) { Destroy(gameObject); return; }

        if (_timer <= 3f && _renderer != null)
            _renderer.enabled = Mathf.Sin(Time.time * 15f) > 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_collected || _isMoneyCoin) return;

        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        _collected = true;

        PlaySound(other);
        SpawnEffect();

        switch (data.pickupType)
        {
            case PickupType.Health: CollectHealth(other); break;
            case PickupType.Ammo: CollectAmmo(other); break;
            case PickupType.Shield: CollectShield(other); break;
        }
    }

    private void AutoCollectMoney()
    {
        int amount = data.GetMoneyAmount(levelConfig);

        if (CoinFlyManager.Instance != null)
        {
            CoinFlyManager.Instance.SpawnCoins(transform.position, amount);
        }
        else
        {
            if (data.progress != null)
            {
                data.progress.cash += amount;
                data.progress.NotifyCashChanged();
            }
            SaveManager.instance?.SaveGame();
        }

        Destroy(gameObject);
    }

    private void CollectHealth(Collider player)
    {
        var ph = player.GetComponent<PlayerHealth>();
        if (ph == null) { _collected = false; return; }
        ph.Heal(Random.Range(data.minHeal, data.maxHeal));
        Destroy(gameObject);
    }

    private void CollectAmmo(Collider player)
    {
        var pa = player.GetComponent<PlayerAmmo>();
        if (pa == null) { _collected = false; return; }
        pa.AddAmmo(Random.Range(data.minAmmo, data.maxAmmo + 1));
        Destroy(gameObject);
    }

    private void CollectShield(Collider player)
    {
        var shield = player.GetComponentInChildren<ShieldEffect>(true);

        if (shield == null)
        {
            _collected = false;
            return;
        }

        shield.Activate();
        Destroy(gameObject);
    }

    private void PlaySound(Collider player)
    {
        if (data.pickupSound == null) return;
        player.GetComponent<AudioSource>()?.PlayOneShot(data.pickupSound);
    }

    private void SpawnEffect()
    {
        if (data.pickupEffect == null) return;
        var go = Instantiate(data.pickupEffect, transform.position, transform.rotation);
        var ps = go.GetComponent<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax : 3f;
        Destroy(go, lifetime);
    }
}