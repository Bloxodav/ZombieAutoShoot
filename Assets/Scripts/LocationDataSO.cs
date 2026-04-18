using UnityEngine;

[CreateAssetMenu(fileName = "LocationData", menuName = "Game/Location")]
public class LocationDataSO : ScriptableObject
{
    public int locationId;
    public string locationName;
    public Sprite previewImage;
}