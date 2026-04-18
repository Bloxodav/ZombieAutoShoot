using UnityEngine;

public class PlayerWallet : MonoBehaviour
{
    [Header("Data")]
    public PlayerProgressSO playerProgress;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;

    public void AddCash(int amount)
    {
        if (playerProgress != null)
        {
            playerProgress.cash += amount;

            if (audioSource && pickupSound)
            {
                audioSource.PlayOneShot(pickupSound);
            }
        }
    }
}