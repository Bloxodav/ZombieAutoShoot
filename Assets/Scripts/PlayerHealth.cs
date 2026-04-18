using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public Slider healthBar;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip damageSound;
    public AudioClip deathSound;

    [Header("Death")]
    public float deathDelay = 2f;

    private float currentHealth;
    private bool isDead = false;
    private bool _invincible = false;

    private void Start()
    {
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void SetInvincible(bool value)
    {
        _invincible = value;
    }

    public void TakeDamage(float amount)
    {
        if (isDead || _invincible) return;

        currentHealth -= amount;

        if (healthBar != null)
            healthBar.value = currentHealth;

        if (audioSource != null && damageSound != null)
            audioSource.PlayOneShot(damageSound);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (audioSource != null && deathSound != null)
            audioSource.PlayOneShot(deathSound);

        MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();
        foreach (var s in scripts)
            if (s != this) s.enabled = false;

        StartCoroutine(ReturnToMainMenu());
    }

    private IEnumerator ReturnToMainMenu()
    {
        yield return new WaitForSeconds(deathDelay);
        SceneManager.LoadScene("Menu");
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        if (healthBar != null) healthBar.value = currentHealth;
    }

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
}