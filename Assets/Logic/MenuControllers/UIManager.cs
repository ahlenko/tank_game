using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public TMP_Text killedText;
    public TMP_Text playerStatsText;
    public TMP_Text playerLivesText;
    public Canvas mainCanvas;
    public Canvas enemiesCanvas;
    public Canvas pauseCanvas;
    public Canvas endGameCanvas;
    public Canvas respownCanvas;
    public Canvas playerStatsCanvas;
    public bool gameOnPause = false;


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

    public void SetKilledCount(int value)
    {
        killedText.text = "You have destroyed %killed enemies".Replace("%killed", value.ToString());
        playerStatsText.text = "Enemy destroyed: %killed".Replace("%killed", value.ToString());
    }

    public void SetPlayerLives(int value)
    {
        playerLivesText.text = "Lives: %lives".Replace("%lives", value.ToString());
    }

    public void DisableUI()
    {
        gameOnPause = false;
        playerStatsCanvas.gameObject.SetActive(true);
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
        playerStatsCanvas.gameObject.SetActive(false);
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(true);
    }

    public void ShowPauseUI()
    {
        if (mainCanvas != null && mainCanvas.gameObject.activeSelf) return;
        if (pauseCanvas != null && pauseCanvas.gameObject.activeSelf) return;
        if (respownCanvas != null && respownCanvas.gameObject.activeSelf) return;
        if (endGameCanvas != null && endGameCanvas.gameObject.activeSelf) return;
        if (enemiesCanvas != null && enemiesCanvas.gameObject.activeSelf) return;

        DisableUI();
        gameOnPause = true;
        if (pauseCanvas != null) pauseCanvas.gameObject.SetActive(true);
    }

    public void ShowRespawnUI()
    {
        DisableUI();
        gameOnPause = true;
        if (respownCanvas != null) respownCanvas.gameObject.SetActive(true);
    }

    public void ShowEndGameUI()
    {
        SetKilledCount(GameManager.Instance.botsKilled);
        DisableUI();
        gameOnPause = true;
        if (endGameCanvas != null) endGameCanvas.gameObject.SetActive(true);
    }

    public void ShowEnemiesUI()
    {
        DisableUI();
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(true);
    }
}
