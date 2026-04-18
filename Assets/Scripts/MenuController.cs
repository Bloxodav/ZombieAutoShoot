using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject customizationPanel;
    public GameObject shopPanel;
    public GameObject locationPanel;

    [Header("Game")]
    public string gameSceneName = "GAME";

    private GameObject currentPanel;

    public bool IsUIOpen => currentPanel != null;

    private void Awake()
    {
        Instance = this;
        CloseAllPanels();
    }

    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ToggleSettings()
    {
        TogglePanel(settingsPanel);
    }

    public void ToggleCustomization()
    {
        TogglePanel(customizationPanel);
    }

    public void ToggleShop()
    {
        TogglePanel(shopPanel);
    }

    public void ToggleLocation()
    {
        TogglePanel(locationPanel);
    }
    private void TogglePanel(GameObject panel)
    {
        if (currentPanel == panel)
        {
            panel.SetActive(false);
            currentPanel = null;
            return;
        }

        CloseAllPanels();

        panel.SetActive(true);
        currentPanel = panel;
    }

    private void CloseAllPanels()
    {
        settingsPanel.SetActive(false);
        customizationPanel.SetActive(false);
        shopPanel.SetActive(false);
        locationPanel.SetActive(false);
        currentPanel = null;
    }
    public void SelectLocation(int id)
    {
        PlayerPrefs.SetInt("SelectedLocation", id);
        TogglePanel(locationPanel);
    }

    public void RemoveAds()
    {
        PlayerPrefs.SetInt("AdsRemoved", 1);
        Debug.Log("Ads removed");
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
