using UnityEngine;

public class LocationManager : MonoBehaviour
{
    public PlayerProgressSO progress;

    [System.Serializable]
    public struct LocationEntry
    {
        public int id;
        public GameObject sceneObject;
    }

    public LocationEntry[] locations;

    private void Start()
    {
        bool found = false;

        foreach (var entry in locations)
        {
            if (entry.sceneObject == null) continue;

            bool isSelected = (entry.id == progress.selectedLocationId);
            entry.sceneObject.SetActive(isSelected);
            if (isSelected) found = true;
        }

        if (!found && locations.Length > 0 && locations[0].sceneObject != null)
        {
            locations[0].sceneObject.SetActive(true);
            progress.selectedLocationId = locations[0].id;
        }
    }
}