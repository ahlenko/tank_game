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
        playerLives = 3;
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

        UIManager.Instance.SetPlayerLives(playerLives);
        UIManager.Instance.SetKilledCount(botsKilled);
    }

    public void SaveGame()
    {
        GameSave state = new GameSave
        {
            playerLives = playerLives,
            botsKilled = botsKilled,
            mapSeed = MapGenerator.Instance != null ? MapGenerator.Instance.seed : 0,
            playerPosition = playerTank != null ? playerTank.transform.position : Vector3.zero,
            playerRotation = playerTank != null ? playerTank.transform.rotation.eulerAngles.z : 0,
            botCount = PlayerPrefs.GetInt("SelectedEnemyCount", 2),
            enemies = new EnemyData[]
            {
                botTank1 != null ? new EnemyData { position = botTank1.transform.position, rotation = botTank1.transform.rotation.eulerAngles.z } : null,
                botTank2 != null ? new EnemyData { position = botTank2.transform.position, rotation = botTank2.transform.rotation.eulerAngles.z } : null,
                botTank3 != null ? new EnemyData { position = botTank3.transform.position, rotation = botTank3.transform.rotation.eulerAngles.z } : null,
                botTank4 != null ? new EnemyData { position = botTank4.transform.position, rotation = botTank4.transform.rotation.eulerAngles.z } : null
            }
        };

        string json = JsonUtility.ToJson(state);
        string savePath = Application.persistentDataPath + "/save.json";
        System.IO.File.WriteAllText(savePath, json);
        playerTank.SetActive(false);
        UIManager.Instance.ShowMainUI();
    }

    public void ContinueGame()
    {
        botsKilled = 0;
        playerLives = 3;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));

        string savePath = Application.persistentDataPath + "/save.json";
        if (!System.IO.File.Exists(savePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = System.IO.File.ReadAllText(savePath);
        GameSave state = JsonUtility.FromJson<GameSave>(json);

        playerLives = state.playerLives;
        botsKilled = state.botsKilled;

        if (mapGeneratorPrefab != null)
        {
            mapGeneratorPrefab.SetActive(true);
            var mapScripts = mapGeneratorPrefab.GetComponents<MonoBehaviour>();

            if (MapGenerator.Instance != null)
                MapGenerator.Instance.setMapSeed(state.mapSeed);
        }

        if (playerTank != null)
        {
            playerTank.SetActive(true);

            var playerScripts = playerTank.GetComponents<MonoBehaviour>();
            foreach (var script in playerScripts)
            {
                if (script is IInitializable init)
                    init.InitializeWithPosition(state.playerPosition, state.playerRotation);
            }
        }

        GameObject[] bots = { botTank1, botTank2, botTank3, botTank4 };
        for (int i = 0; i < bots.Length; i++)
        {
            if (bots[i] != null && i < state.enemies.Length && state.enemies[i] != null)
            {
                bots[i].SetActive(true);

                var botScripts = bots[i].GetComponents<MonoBehaviour>();
                foreach (var script in botScripts)
                {
                    if (script is IInitializable init)
                        init.InitializeWithPosition(state.enemies[i].position, state.enemies[i].rotation);
                }
            }
            else if (bots[i] != null)
            {
                bots[i].SetActive(false);
            }
        }

        // When user save game at the moment when some bots are defeated
        if (bots.Length < state.botCount)
        {
            for (int i = bots.Length; i < state.botCount; i++)
            {
                if (bots[i] != null)
                {
                    bots[i].SetActive(true);

                    var botScripts = bots[i].GetComponents<MonoBehaviour>();
                    foreach (var script in botScripts)
                    {
                        if (script is IInitializable init)
                            init.Initialize();
                    }
                }
            }
        }

        UIManager.Instance.DisableUI();
        UIManager.Instance.SetPlayerLives(playerLives);
        UIManager.Instance.SetKilledCount(botsKilled);
    }

    public void ContinuePlay()
    {
        UIManager.Instance.DisableUI();
    }


    public void OnBotDefeated()
    {
        botsKilled++;
        UIManager.Instance.SetKilledCount(botsKilled);
    }

    public void OnPlayerDefeated()
    {
        playerLives--;
        UIManager.Instance.SetPlayerLives(playerLives);
        if (playerLives > 0)
        {
            UIManager.Instance.ShowRespawnUI();
        }
        else
        {
            UIManager.Instance.ShowEndGameUI();
        }
    }

    public void ViewSave()
    {
        string savePath = Application.persistentDataPath + "/save.json";
        if (System.IO.File.Exists(savePath))
        {
            string url = "file://" + savePath;
            Application.OpenURL(url);
        }
    }


    public void RespawnPlayer()
    {
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
        UIManager.Instance.DisableUI();
    }

    public void EndGame()
    {
        UIManager.Instance.SetPlayerLives(3);
        UIManager.Instance.SetKilledCount(0);
        playerTank.SetActive(false);
        UIManager.Instance.ShowMainUI();
    }
}