using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public PlayerProgressSO progress;
    public CharacterDataSO characterData;
    public UpgradeUI upgradeUI;

    [Header("Settings - Ammo")]
    public int ammoStep = 20;
    public int ammoBaseCost = 100;

    [Header("Settings - Speed")]
    public float speedStep = 0.5f;
    public int speedBaseCost = 150;

    [Header("Settings - Fire Rate")]
    public float fireRateStep = 0.05f;
    public int fireRateBaseCost = 200;

    public void UpgradeAmmo()
    {
        int cost = ammoBaseCost + (progress.ammoLevel * 50);
        if (progress.cash >= cost)
        {
            progress.cash -= cost;
            progress.ammoLevel++;
            AfterUpgrade();
        }
    }

    public void UpgradeSpeed()
    {
        int cost = speedBaseCost + (progress.speedLevel * 70);
        if (progress.cash >= cost)
        {
            progress.cash -= cost;
            progress.speedLevel++;
            AfterUpgrade();
        }
    }

    public void UpgradeFireRate()
    {
        int cost = fireRateBaseCost + (progress.fireRateLevel * 100);
        if (progress.cash >= cost)
        {
            progress.cash -= cost;
            progress.fireRateLevel++;
            AfterUpgrade();
        }
    }

    private void AfterUpgrade()
    {
        progress.NotifyCashChanged();

        if (upgradeUI != null)
            upgradeUI.UpdateAllUI();

        if (SaveManager.instance != null)
            SaveManager.instance.SaveGame();
    }
}