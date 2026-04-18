using UnityEngine;

public enum PickupType { Money, Health, Ammo, Shield }

[CreateAssetMenu(fileName = "NewPickupData", menuName = "Loot/Pickup Data")]
public class PickupDataSO : ScriptableObject
{
    [Header("Тип предмета")]
    public PickupType pickupType;

    [Header("Шанс выпадения (0 = никогда, 1 = всегда)")]
    [Range(0f, 1f)] public float dropChance = 1f;

    [Header("Префаб")]
    public GameObject prefab;

    [Header("Звук сбора")]
    public AudioClip pickupSound;

    [Header("Эффект сбора")]
    public GameObject pickupEffect;

    [Header("Время жизни")]
    public float lifetime = 15f;

    [Header("Анимация")]
    public float hoverHeight = 0.5f;
    public float amplitude = 0.15f;
    public float frequency = 2.5f;
    public float rotationSpeed = 80f;

    [Header("Деньги")]
    public PlayerProgressSO progress;
    public int minMoney = 10;
    public int maxMoney = 50;

    [Header("Здоровье")]
    public float minHeal = 15f;
    public float maxHeal = 30f;

    [Header("Патроны")]
    public int minAmmo = 8;
    public int maxAmmo = 16;

    public int GetMoneyAmount(LevelConfigSO levelConfig)
    {
        int baseAmount = Random.Range(minMoney, maxMoney + 1);
        if (levelConfig == null || progress == null) return baseAmount;

        float multiplier = 1f + levelConfig.cashRewardMultiplierPerLevel * (progress.currentLevel - 1);
        return Mathf.RoundToInt(baseAmount * multiplier);
    }
}