using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Data")]
    public PlayerProgressSO playerProgress;


    public void AddCash(int amount)
    {
        if (playerProgress != null)
        {
            playerProgress.cash += amount;

        }
    }
}