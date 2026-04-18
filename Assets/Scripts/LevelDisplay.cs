using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LevelDisplay : MonoBehaviour
{
    public PlayerProgressSO progress;

    private TextMeshProUGUI _label;

    private void Awake() => _label = GetComponent<TextMeshProUGUI>();

    private void OnEnable()
    {
        if (progress != null)
            progress.OnLevelChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (progress != null)
            progress.OnLevelChanged -= Refresh;
    }

    private void Refresh()
    {
        if (progress != null)
            _label.text = $"LVL {progress.currentLevel}";
    }
}