using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ZombieWaveSpawner : MonoBehaviour
{
    [Header("Zombie")]
    public GameObject zombiePrefab;
    public Transform player;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Level Config")]
    public LevelConfigSO levelConfig;
    public PlayerProgressSO progress;

    [Header("Waves")]
    public float breakBetweenWaves = 5f;
    public float spawnInterval = 0.2f;

    [Header("Pool")]
    public int poolWarmupSize = 15;

    [Header("UI")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI levelText;
    public GameObject victoryPanel;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip waveStartSound;

    private int _currentWave;
    private int _totalWaves;
    private int _aliveEnemyZombies;
    private int _currentLevel;
    private bool _isSpawning;

    private readonly Queue<ZombieAI> _pool = new Queue<ZombieAI>();

    private void Start()
    {
        if (victoryPanel) victoryPanel.SetActive(false);

        ZombieFactionRegistry.Clear();
        ZombieAI.ClearPlayerCache();

        _currentLevel = progress != null ? progress.currentLevel : 1;

        for (int i = 0; i < poolWarmupSize; i++)
        {
            var go = Instantiate(zombiePrefab);
            go.SetActive(false);
            var ai = go.GetComponent<ZombieAI>();
            if (ai) _pool.Enqueue(ai);
        }

        UpdateLevelText();
        StartCoroutine(WaveLoop());
    }

    private void OnDestroy()
    {
        ZombieFactionRegistry.Clear();
    }

    private ZombieAI GetFromPool(Vector3 position, Quaternion rotation)
    {
        while (_pool.Count > 0)
        {
            var ai = _pool.Dequeue();
            if (ai != null)
            {
                ai.transform.SetPositionAndRotation(position, rotation);
                ai.gameObject.SetActive(true);
                return ai;
            }
        }
        return Instantiate(zombiePrefab, position, rotation).GetComponent<ZombieAI>();
    }

    private void ReturnToPool(ZombieAI ai)
    {
        if (ai != null) _pool.Enqueue(ai);
    }

    private IEnumerator WaveLoop()
    {
        _totalWaves = levelConfig != null ? levelConfig.GetWaveCount(_currentLevel) : 3;
        _currentWave = 0;

        while (_currentWave < _totalWaves)
        {
            _currentWave++;
            yield return new WaitForSeconds(breakBetweenWaves);
            StartWave();
            yield return new WaitUntil(() => _aliveEnemyZombies <= 0 && !_isSpawning);
            ShowWaveCleared();
            yield return new WaitForSeconds(2f);
        }

        Victory();
    }

    private void StartWave()
    {
        if (audioSource && waveStartSound)
            audioSource.PlayOneShot(waveStartSound);

        int count = levelConfig != null ? levelConfig.GetZombiesPerWave(_currentLevel) : 5;

        ShowWaveStart();
        StartCoroutine(SpawnWave(count));
    }

    private IEnumerator SpawnWave(int count)
    {
        _isSpawning = true;
        var freePoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < count; i++)
        {
            if (freePoints.Count == 0)
                freePoints.AddRange(spawnPoints);

            int idx = Random.Range(0, freePoints.Count);
            Transform point = freePoints[idx];
            freePoints.RemoveAt(idx);

            ZombieAI ai = GetFromPool(point.position, point.rotation);

            if (ai != null)
            {
                if (levelConfig != null)
                {
                    ai.maxHealth = levelConfig.GetZombieHealth(_currentLevel);
                    ai.damage = levelConfig.GetZombieDamage(_currentLevel);
                    if (ai.agent != null)
                    {
                        ai.agent.speed = levelConfig.GetZombieSpeed(_currentLevel);
                        ai.SyncBaseSpeed();
                    }
                        
                    ai.SyncHealthToMax();
                }

                ai.target = player;

                ai.OnDeath -= OnZombieDeath;
                ai.OnDeath += OnZombieDeath;
                ai.OnVaccinated -= OnZombieVaccinated;
                ai.OnVaccinated += OnZombieVaccinated;
                ai.OnRevertedToEnemy -= OnZombieReverted;
                ai.OnRevertedToEnemy += OnZombieReverted;
            }

            _aliveEnemyZombies++;
            yield return new WaitForSeconds(spawnInterval);
        }

        _isSpawning = false;
    }

    private void OnZombieDeath(ZombieAI zombie)
    {
        if (zombie.Faction == ZombieFaction.Enemy)
            _aliveEnemyZombies--;

        ReturnToPool(zombie);
    }

    private void OnZombieVaccinated(ZombieAI zombie)
    {
        _aliveEnemyZombies--;
    }

    private void OnZombieReverted(ZombieAI zombie)
    {
        _aliveEnemyZombies++;
    }

    private void Victory()
    {
        if (progress != null)
        {
            int reward = levelConfig != null ? levelConfig.GetCashReward(_currentLevel) : 50;
            progress.cash += reward;
            progress.currentLevel++;
            progress.NotifyCashChanged();
            progress.NotifyLevelChanged();
            SaveManager.instance?.SaveGame();
        }

        if (victoryPanel) victoryPanel.SetActive(true);
    }

    private void ShowWaveStart()
    {
        if (!infoText) return;
        infoText.text = $"WAVE {_currentWave}/{_totalWaves}";
        infoText.gameObject.SetActive(true);
        Invoke(nameof(HideInfoText), 2f);
    }

    private void ShowWaveCleared()
    {
        if (!infoText) return;
        infoText.text = $"WAVE {_currentWave} CLEARED";
        infoText.gameObject.SetActive(true);
        Invoke(nameof(HideInfoText), 2f);
    }

    private void HideInfoText()
    {
        if (infoText) infoText.gameObject.SetActive(false);
    }

    private void UpdateLevelText()
    {
        if (levelText) levelText.text = $"LVL {_currentLevel}";
    }
}