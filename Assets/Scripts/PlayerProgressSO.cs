using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerProgress", menuName = "Game/Player Progress")]
public class PlayerProgressSO : ScriptableObject
{
    [Header("Currencies")]
    public int cash;
    public int gems;

    [Header("Level")]
    public int currentLevel = 1;

    [Header("Unlocked Content")]
    public List<int> unlockedCharacterIds = new List<int>();
    public List<int> unlockedLocationIds = new List<int>();

    [Header("Selected")]
    public int selectedLocationId = 1;

    [Header("Upgrade Levels")]
    public int ammoLevel = 0;
    public int speedLevel = 0;
    public int fireRateLevel = 0;

    public event System.Action OnCashChanged;
    public event System.Action OnLevelChanged;

    public void NotifyCashChanged() => OnCashChanged?.Invoke();
    public void NotifyLevelChanged() => OnLevelChanged?.Invoke();

    public bool IsCharacterUnlocked(int id) => unlockedCharacterIds.Contains(id);
    public bool IsLocationUnlocked(int id) => unlockedLocationIds.Contains(id);

    public void UnlockCharacter(int id)
    {
        if (!unlockedCharacterIds.Contains(id))
            unlockedCharacterIds.Add(id);
    }

    public void UnlockLocation(int id)
    {
        if (!unlockedLocationIds.Contains(id))
            unlockedLocationIds.Add(id);
    }
}