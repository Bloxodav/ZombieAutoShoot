using UnityEngine;
using UnityEngine.UI;

public class LocationCard : MonoBehaviour
{
    public PlayerProgressSO progress;
    public int locationId;
    private SelectedLocationLabel _locationLabel;

    [Tooltip("Рамка выделения — выключена по умолчанию")]
    public GameObject selectedBorder;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
        _locationLabel = FindObjectOfType<SelectedLocationLabel>();
        RefreshBorder();
    }

    private void OnEnable() => RefreshBorder();

    private void OnClick()
    {
        progress.selectedLocationId = locationId;
        SaveManager.instance?.SaveGame();

        foreach (var card in transform.parent.GetComponentsInChildren<LocationCard>())
            card.RefreshBorder();

        _locationLabel?.Refresh();
    }

    public void RefreshBorder()
    {
        if (selectedBorder != null)
            selectedBorder.SetActive(progress.selectedLocationId == locationId);
    }
}