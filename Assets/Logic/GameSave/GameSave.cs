using UnityEngine;

[System.Serializable]
public class GameSave
{
    public int mapSeed;

    public Vector3 playerPosition;
    public float playerRotation;

    public int playerLives;
    public int botsKilled;
    public float gameTime;

    public EnemyData[] enemies;
    public int botCount;
}