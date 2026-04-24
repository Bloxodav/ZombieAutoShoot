using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;
    public PlayerProgressSO progress;

    private const string SaveKey = "PlayerSave";
    private bool _isDirty;
    private float _saveTimer;
    public float saveDelay = 1f;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        LoadGame();
    }

    private void Update()
    {
        if (!_isDirty) return;
        _saveTimer -= Time.unscaledDeltaTime;
        if (_saveTimer <= 0f) FlushSave();
    }

    public void SaveGame()
    {
        _isDirty = true;
        _saveTimer = saveDelay;
    }

    public void ForceSave() => FlushSave();

    private void FlushSave()
    {
        if (!_isDirty) return;
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(progress));
        PlayerPrefs.Save();
        _isDirty = false;
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SaveKey))
            JsonUtility.FromJsonOverwrite(PlayerPrefs.GetString(SaveKey), progress);
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) ForceSave();
    }

    private void OnApplicationQuit() => ForceSave();
}