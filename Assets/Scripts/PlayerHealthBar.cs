using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    [Header("Ссылки")]
    public PlayerHealth playerHealth;
    public Camera cam;
    public Image greenFill;       // зелёная полоска (быстрая)
    public Image damageFill;      // белая полоска (медленная, отстаёт)
    public TextMeshProUGUI hpText;

    [Header("Скорость анимации")]
    public float greenSpeed = 10f;   // как быстро зелёная догоняет цель
    public float damageSpeed = 2f;   // как медленно белая уменьшается

    private float _targetFill = 1f;  // целевое значение HP

    private void Update()
    {
        // Бар смотрит на камеру
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                         cam.transform.rotation * Vector3.up);

        if (playerHealth == null) return;

        _targetFill = playerHealth.CurrentHealth / playerHealth.maxHealth;

        // Зелёная — быстро догоняет цель
        if (greenFill != null)
            greenFill.fillAmount = Mathf.MoveTowards(
                greenFill.fillAmount, _targetFill, Time.deltaTime * greenSpeed);

        // Белая — медленно отстаёт (анимация урона)
        if (damageFill != null)
            damageFill.fillAmount = Mathf.MoveTowards(
                damageFill.fillAmount, _targetFill, Time.deltaTime * damageSpeed);

        // Текст обновляется сразу
        if (hpText != null)
            hpText.text = ((int)playerHealth.CurrentHealth).ToString();
    }
}