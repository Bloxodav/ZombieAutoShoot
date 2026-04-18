using UnityEngine;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public PlayerProgressSO progress;
    public CharacterDataSO characterData;
    public UpgradeManager upgradeManager;

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoLvlText;
    public TextMeshProUGUI ammoCostText;
    public TextMeshProUGUI ammoValueText;

    [Header("Speed UI")]
    public TextMeshProUGUI speedLvlText;
    public TextMeshProUGUI speedCostText;
    public TextMeshProUGUI speedValueText;

    [Header("Fire Rate UI")]
    public TextMeshProUGUI fireRateLvlText;
    public TextMeshProUGUI fireRateCostText;
    public TextMeshProUGUI fireRateValueText;

    private void Start()
    {
        UpdateAllUI();
    }

    public void UpdateAllUI()
    {
        int currentAmmoPrice = upgradeManager.ammoBaseCost + (progress.ammoLevel * 50);
        int totalAmmo = characterData.startAmmo + (progress.ammoLevel * upgradeManager.ammoStep);

        ammoLvlText.text = "LVL: " + progress.ammoLevel;
        ammoCostText.text = currentAmmoPrice + "$";
        ammoValueText.text =  totalAmmo.ToString();

        int currentSpeedPrice = upgradeManager.speedBaseCost + (progress.speedLevel * 70);
        float totalSpeed = characterData.moveSpeed + (progress.speedLevel * upgradeManager.speedStep);

        speedLvlText.text = "LVL: " + progress.speedLevel;
        speedCostText.text = currentSpeedPrice + "$";
        speedValueText.text = totalSpeed.ToString("F1");

        int currentFRPrice = upgradeManager.fireRateBaseCost + (progress.fireRateLevel * 100);
        float currentDelay = characterData.weapon.fireRate - (progress.fireRateLevel * upgradeManager.fireRateStep);

        fireRateLvlText.text = "LVL: " + progress.fireRateLevel;
        fireRateCostText.text = currentFRPrice + "$";
        fireRateValueText.text = Mathf.Max(currentDelay, 0.05f).ToString("F4");
    }
}