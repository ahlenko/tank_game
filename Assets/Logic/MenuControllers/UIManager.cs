using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public Canvas mainCanvas;
    public Canvas enemiesCanvas;
    public Canvas pauseCanvas;
    public Canvas endGameCanvas;
    public Canvas respownCanvas;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void DisableUI()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Menu"));
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(false);
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(false);
        if (pauseCanvas != null) pauseCanvas.gameObject.SetActive(false);
        if (endGameCanvas != null) endGameCanvas.gameObject.SetActive(false);
        if (respownCanvas != null) respownCanvas.gameObject.SetActive(false);
    }

    void Start()
    {
        ShowMainUI();
    }

    public void ShowMainUI()
    {
        DisableUI();
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(true);
    }

    public void ShowPauseUI()
    {
        DisableUI();
        if (pauseCanvas != null) pauseCanvas.gameObject.SetActive(true);
    }

    public void ShowRespawnUI()
    {
        DisableUI();
        if (respownCanvas != null) respownCanvas.gameObject.SetActive(true);
    }

    public void ShowEndGameUI()
    {
        DisableUI();
        if (endGameCanvas != null) endGameCanvas.gameObject.SetActive(true);
    }

    public void ShowEnemiesUI()
    {
        DisableUI();
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(true);
    }
}
