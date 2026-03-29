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


    public void StartNewGame(int enemyCount)
    {
        UIManager.Instance.DisableUI();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));

        PlayerPrefs.SetInt("SelectedEnemyCount", enemyCount);
        PlayerPrefs.Save();

        botsKilled = 0;
        startTime = Time.time;
        SetupGameScene();
    }

    private void SetupGameScene()
    {
        int enemyCount = PlayerPrefs.GetInt("SelectedEnemyCount", 2);

        if (playerTank != null)
        {
            playerTank.SetActive(true);
            var playerScripts = playerTank.GetComponents<MonoBehaviour>();
            foreach (var script in playerScripts)
            {
                if (script is IInitializable init)
                    init.Initialize();
            }
        }
        else
            Debug.LogError("playerTank not assigned in GameManager!");

        void SetupBot(GameObject bot, int requiredCount)
        {
            if (bot == null) return;

            bot.SetActive(enemyCount >= requiredCount);

            var botScripts = bot.GetComponents<MonoBehaviour>();
            foreach (var script in botScripts)
            {
                if (script is IInitializable init)
                    init.Initialize();
            }
        }

        SetupBot(botTank2, 2);
        SetupBot(botTank3, 3);
        SetupBot(botTank4, 4);

        if (mapGeneratorPrefab != null)
        {
            mapGeneratorPrefab.SetActive(true);
            var mapScripts = mapGeneratorPrefab.GetComponents<MonoBehaviour>();
            foreach (var script in mapScripts)
            {
                if (script is IInitializable init)
                    init.Initialize();
            }
        }
        else
            Debug.LogError("mapGeneratorPrefab not assigned in GameManager!");

        startTime = Time.time;
    }

    public void ContinueGame()
    {
        UIManager.Instance.DisableUI();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
    }



    public void ViewSave()
    {
        string savePath = Application.persistentDataPath + "/save.json";
        if (System.IO.File.Exists(savePath))
            Application.OpenURL(savePath);
    }

    private void ShowMainMenu()
    {

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
}