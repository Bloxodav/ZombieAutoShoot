using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
public class LevelConfigSO : ScriptableObject
{
    [Header("Зомби за волну")]
    [Tooltip("Зомби на первом уровне")]
    public int baseZombiesPerWave = 5;
    [Tooltip("Прибавляется к кол-ву зомби каждые N уровней")]
    public int zombiesIncreasePerLevels = 1;
    [Tooltip("Каждые сколько уровней добавляется зомби")]
    public int zombiesIncreaseInterval = 1;
    [Tooltip("Максимальное кол-во зомби за волну")]
    public int maxZombiesPerWave = 100;

    [Header("Здоровье зомби")]
    public float baseZombieHealth = 100f;
    [Tooltip("Множитель здоровья за каждый уровень (напр. 0.05 = +5% за уровень)")]
    public float healthMultiplierPerLevel = 0.05f;
    [Tooltip("Максимальный множитель здоровья")]
    public float maxHealthMultiplier = 10f;

    [Header("Урон зомби")]
    public float baseZombieDamage = 10f;
    [Tooltip("Множитель урона за каждый уровень")]
    public float damageMultiplierPerLevel = 0.03f;
    [Tooltip("Максимальный множитель урона")]
    public float maxDamageMultiplier = 5f;

    [Header("Скорость зомби")]
    public float baseZombieSpeed = 3.5f;
    [Tooltip("Прибавка к скорости за каждый уровень")]
    public float speedIncreasePerLevel = 0.02f;
    [Tooltip("Максимальная скорость зомби")]
    public float maxZombieSpeed = 7f;

    [Header("Количество волн")]
    public int baseWaveCount = 3;
    [Tooltip("Добавлять волну каждые N уровней")]
    public int waveIncreaseInterval = 5;
    [Tooltip("Максимальное кол-во волн")]
    public int maxWaveCount = 10;

    [Header("Награда")]
    public int baseCashReward = 50;
    [Tooltip("Множитель награды за каждый уровень")]
    public float cashRewardMultiplierPerLevel = 0.1f;

    public int GetZombiesPerWave(int level)
    {
        int bonus = (level / zombiesIncreaseInterval) * zombiesIncreasePerLevels;
        return Mathf.Min(baseZombiesPerWave + bonus, maxZombiesPerWave);
    }

    public float GetZombieHealth(int level)
    {
        float mult = 1f + healthMultiplierPerLevel * (level - 1);
        return baseZombieHealth * Mathf.Min(mult, maxHealthMultiplier);
    }

    public float GetZombieDamage(int level)
    {
        float mult = 1f + damageMultiplierPerLevel * (level - 1);
        return baseZombieDamage * Mathf.Min(mult, maxDamageMultiplier);
    }

    public float GetZombieSpeed(int level)
    {
        return Mathf.Min(baseZombieSpeed + speedIncreasePerLevel * (level - 1), maxZombieSpeed);
    }

    public int GetWaveCount(int level)
    {
        int bonus = (level / waveIncreaseInterval);
        return Mathf.Min(baseWaveCount + bonus, maxWaveCount);
    }

    public int GetCashReward(int level)
    {
        return Mathf.RoundToInt(baseCashReward * (1f + cashRewardMultiplierPerLevel * (level - 1)));
    }
}