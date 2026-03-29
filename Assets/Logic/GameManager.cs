using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player")]
    public GameObject playerTank;
    public int playerLives = 3;

    [Header("Bots")]
    public GameObject botTank1;
    public GameObject botTank2;
    public GameObject botTank3;
    public GameObject botTank4;

    [Header("Map")]
    public GameObject mapGeneratorPrefab;

    [Header("UI")]
    public Canvas mainCanvas;
    public Canvas enemiesCanvas;
    public Canvas pauseCanvas;
    public Canvas endGameCanvas;
    public Canvas respownCanvas;

    [Header("Stats")]
    public int botsKilled = 0;
    private float startTime;

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

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Game")
        {
            SetupGameScene();
        }
        else if (scene.name == "Menu")
        {
            ShowMainMenu();
        }
    }

    public void ShowEnemiesSelection()
    {
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(false);
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(true);
    }

    public void StartNewGame(int enemyCount)
    {
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(false);

        SceneManager.LoadScene("Game");
        PlayerPrefs.SetInt("SelectedEnemyCount", enemyCount);
        PlayerPrefs.Save();

        botsKilled = 0;
        startTime = Time.time;
    }

    private void SetupGameScene()
    {
        int enemyCount = PlayerPrefs.GetInt("SelectedEnemyCount", 2);

        if (playerTank != null)
            playerTank.SetActive(true);
        else
            Debug.LogError("playerTank not assigned in GameManager!");

        if (botTank2 != null) botTank2.SetActive(enemyCount >= 2);
        if (botTank3 != null) botTank3.SetActive(enemyCount >= 3);
        if (botTank4 != null) botTank4.SetActive(enemyCount >= 4);

        if (mapGeneratorPrefab != null)
        {
            Destroy(mapGeneratorPrefab);
            Instantiate(mapGeneratorPrefab);
        }
        else
        {
            Debug.LogError("mapGeneratorPrefab not assigned in GameManager!");
        }

        startTime = Time.time;
    }

    public void ContinueGame()
    {
        if (System.IO.File.Exists(Application.persistentDataPath + "/save.json"))
        {
            SceneManager.LoadScene("Game");
        }
        else
        {
            Debug.Log("No save file found");
        }
    }

    public void ViewSave()
    {
        string savePath = Application.persistentDataPath + "/save.json";
        if (System.IO.File.Exists(savePath))
            Application.OpenURL(savePath);
    }

    private void ShowMainMenu()
    {
        if (mainCanvas != null) mainCanvas.gameObject.SetActive(true);
        if (enemiesCanvas != null) enemiesCanvas.gameObject.SetActive(false);

        string savePath = Application.persistentDataPath + "/save.json";
        bool hasSave = System.IO.File.Exists(savePath);
    }

    public float GetGameTime()
    {
        return Time.time - startTime;
    }

    public void BotKilled()
    {
        botsKilled++;
    }

    public void PlayerDefeated()
    {
        playerLives--;
        if (playerLives > 0)
        {

        }
        else
        {
            GameOver();
        }
    }

    void GameOver()
    {
        // Показати екран завершення гри
        // UIManager.Instance.ShowGameOver(botsKilled, GetGameTime());
    }

    public void SaveGame()
    {

    }

    public void RespawnPlayer()
    {
        // Реалізуйте логіку респавну гравця
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}