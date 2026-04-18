using UnityEngine;
using System.IO;
using System.Collections;
public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public PlayerProgressSO progress;

    [Tooltip("Минимальная задержка между реальными записями на диск (секунды)")]
    public float saveDelay = 1.0f;

    private string _savePath;
    private bool _isDirty;
    private float _saveTimer;
    private bool _saveScheduled;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _savePath = Path.Combine(Application.persistentDataPath, "playerSave.json");
        LoadGame();
    }

    private void Update()
    {
        if (_isDirty)
        {
            _saveTimer -= Time.unscaledDeltaTime;
            if (_saveTimer <= 0f)
            {
                FlushSave();
            }
        }
    }

    public void SaveGame()
    {
        _isDirty = true;
        if (!_saveScheduled)
        {
            _saveTimer = saveDelay;
            _saveScheduled = true;
        }
    }

    public void ForceSave()
    {
        FlushSave();
    }

    private void FlushSave()
    {
        if (!_isDirty) return;
        string json = JsonUtility.ToJson(progress, true);
        File.WriteAllText(_savePath, json);
        _isDirty = false;
        _saveScheduled = false;
        Debug.Log("[SaveManager] Сохранено");
    }

    public void LoadGame()
    {
        if (File.Exists(_savePath))
        {
            string json = File.ReadAllText(_savePath);
            JsonUtility.FromJsonOverwrite(json, progress);
            Debug.Log("[SaveManager] Загружено");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause) ForceSave();
    }

    private void OnApplicationQuit()
    {
        ForceSave();
    }
}