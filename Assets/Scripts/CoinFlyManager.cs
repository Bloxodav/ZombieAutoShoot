using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinFlyManager : MonoBehaviour
{
    public static CoinFlyManager Instance { get; private set; }

    [Header("Ссылки")]
    public GameObject coinFlyPrefab;
    public RectTransform targetIcon;
    public Canvas canvas;

    [Header("Текст заработка")]
    public TMP_Text sessionEarningsText;
    public string earningsPrefix = "+";

    [Header("Звук")]
    public AudioClip coinArriveSound;
    public AudioSource audioSource;

    [Header("Полёт")]
    [Range(1, 15)] public int coinCount = 6;
    public float flyDuration = 1.2f;
    public float scatterRadius = 80f;
    public float spawnInterval = 0.08f;

    [Header("Пульсация иконки")]
    public float iconPunchScale = 1.25f;
    public float iconPunchDuration = 0.15f;

    private Vector3 _iconOriginalScale;
    private int _sessionEarnings;

    private Queue<CoinFlyEffect> _coinPool = new Queue<CoinFlyEffect>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        _iconOriginalScale = targetIcon ? targetIcon.localScale : Vector3.one;

        WarmupPool();
        UpdateSessionText();
    }

    private void WarmupPool()
    {
        if (coinFlyPrefab == null) return;
        for (int i = 0; i < coinCount * 2; i++)
        {
            var go = Instantiate(coinFlyPrefab, canvas.transform);
            go.SetActive(false);
            var effect = go.GetComponent<CoinFlyEffect>();
            if (effect) _coinPool.Enqueue(effect);
        }
    }

    private CoinFlyEffect GetFromPool()
    {
        while (_coinPool.Count > 0)
        {
            var e = _coinPool.Dequeue();
            if (e != null) { e.gameObject.SetActive(true); return e; }
        }
        var go = Instantiate(coinFlyPrefab, canvas.transform);
        return go.GetComponent<CoinFlyEffect>();
    }

    private void ReturnToPool(CoinFlyEffect effect)
    {
        if (effect == null) return;
        effect.gameObject.SetActive(false);
        _coinPool.Enqueue(effect);
    }

    public void SpawnCoins(Vector3 worldPos, int coinValue)
    {
        if (coinFlyPrefab == null || targetIcon == null || canvas == null)
        {
            if (SaveManager.instance != null)
            {
                SaveManager.instance.progress.cash += coinValue;
                SaveManager.instance.progress.NotifyCashChanged();
                SaveManager.instance.SaveGame();
            }
            return;
        }
        StartCoroutine(SpawnSequence(worldPos, coinValue));
    }

    public void ResetSessionEarnings()
    {
        _sessionEarnings = 0;
        UpdateSessionText();
    }

    private IEnumerator SpawnSequence(Vector3 worldPos, int totalValue)
    {
        int[] values = SplitValue(totalValue, coinCount);

        for (int i = 0; i < coinCount; i++)
        {
            var effect = GetFromPool();
            if (effect == null) { yield return new WaitForSeconds(spawnInterval); continue; }

            int captured = values[i];
            effect.Play(
                startWorldPos: worldPos,
                targetUI: targetIcon,
                canvas: canvas,
                flyDuration: flyDuration,
                scatterRadius: scatterRadius,
                delay: 0f,
                onComplete: () =>
                {
                    OnCoinArrived(captured);
                    ReturnToPool(effect);
                }
            );

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void OnCoinArrived(int value)
    {
        if (SaveManager.instance == null) return;

        SaveManager.instance.progress.cash += value;
        SaveManager.instance.progress.NotifyCashChanged();
        SaveManager.instance.SaveGame();

        _sessionEarnings += value;
        UpdateSessionText();

        audioSource?.PlayOneShot(coinArriveSound);
        StopCoroutine(nameof(PunchIcon));
        StartCoroutine(PunchIcon());
    }

    private void UpdateSessionText()
    {
        if (sessionEarningsText == null) return;
        sessionEarningsText.text = _sessionEarnings > 0 ? $"{earningsPrefix}{_sessionEarnings}" : "0";
    }

    private IEnumerator PunchIcon()
    {
        if (targetIcon == null) yield break;
        targetIcon.localScale = _iconOriginalScale * iconPunchScale;
        float e = 0f;
        while (e < iconPunchDuration)
        {
            e += Time.deltaTime;
            targetIcon.localScale = Vector3.Lerp(
                _iconOriginalScale * iconPunchScale,
                _iconOriginalScale,
                Mathf.Clamp01(e / iconPunchDuration));
            yield return null;
        }
        targetIcon.localScale = _iconOriginalScale;
    }

    private static int[] SplitValue(int total, int count)
    {
        var parts = new int[count];
        int perCoin = total / count;
        int remainder = total - perCoin * count;
        for (int i = 0; i < count; i++)
            parts[i] = perCoin + (i < remainder ? 1 : 0);
        return parts;
    }
}