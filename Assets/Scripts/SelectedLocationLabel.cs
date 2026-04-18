using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class SelectedLocationLabel : MonoBehaviour
{
    public PlayerProgressSO progress;

    [System.Serializable]
    public struct LocationName
    {
        public int id;
        public string name;
    }

    public LocationName[] locationNames;

    private TextMeshProUGUI _label;

    private void Awake()
    {
        _label = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        foreach (var entry in locationNames)
        {
            if (entry.id == progress.selectedLocationId)
            {
                _label.text = entry.name;
                return;
            }
        }

        _label.text = "—";
    }
    public void Refresh() => UpdateLabel();
}