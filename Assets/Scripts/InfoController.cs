using UnityEngine;

public class InfoController : MonoBehaviour
{
    public static InfoController Instance;

    [Header("Info Panels (добавл€й сколько нужно)")]
    public GameObject[] infoPanels;

    [Header("Close Button")]
    public GameObject closeButton;

    private GameObject _currentInfoPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        CloseAllInfoPanels();
        if (closeButton != null) closeButton.SetActive(false);
    }
    public void OpenInfo(int index)
    {
        if (infoPanels == null || index < 0 || index >= infoPanels.Length)
        {
            Debug.LogWarning($"[InfoController] ѕанель с индексом {index} не существует!");
            return;
        }

        GameObject panel = infoPanels[index];
        if (panel == null) return;

        if (_currentInfoPanel == panel && panel.activeSelf)
        {
            CloseCurrentInfo();
            return;
        }

        CloseAllInfoPanels();
        panel.SetActive(true);
        _currentInfoPanel = panel;

        if (closeButton != null) closeButton.SetActive(true);
    }

    public void CloseCurrentInfo()
    {
        if (_currentInfoPanel != null)
        {
            _currentInfoPanel.SetActive(false);
            _currentInfoPanel = null;
        }
        if (closeButton != null) closeButton.SetActive(false);
    }

    private void CloseAllInfoPanels()
    {
        if (infoPanels == null) return;
        foreach (var p in infoPanels)
            if (p != null) p.SetActive(false);
        _currentInfoPanel = null;
    }

    public bool IsAnyInfoOpen() => _currentInfoPanel != null;
}