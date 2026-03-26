using UnityEngine;
using UnityEngine.Tilemaps;

public class MapGenerator : MonoBehaviour
{
    [Header("Ground Tiles")]
    public TileBase[] grassTile;
    public TileBase[] sandTile;
    public TileBase[] transitionTile;
    public TileBase[] grassRoadTile;
    public TileBase[] sandRoadTile;
    public TileBase[] transitionRoadTile;

    [Header("Map Size")]
    public int width = 40;
    public int height = 40;

    [Header("Generation Settings")]
    public int seed = 0;
    public bool useRandomSeed = true;
    public float grassNoiseScale = 0.2f;
    public float grassThreshold = 0.7f;

    [Header("Tilemap References")]
    public Tilemap groundTilemap;
    public Tilemap routeTilemap;

    private TileBase[,] groundMapVithoutTransitions;
    private TileBase[,] groundMap;
    private TileBase[,] routeMap;
    private int[,] groundType;


    private void InitializeSeed()
    {
        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
            Random.InitState(seed);
            Debug.Log($"Using random seed: {seed}");
        }
        else
        {
            Random.InitState(seed);
            Debug.Log($"Using specified seed: {seed}");
        }
    }

    private TileBase GetRandomTile(TileBase[] tiles)
    {
        if (tiles == null || tiles.Length == 0) return null;
        return tiles[Random.Range(0, tiles.Length)];
    }

    // Generates the base ground layer with grass and sand biomes
    private void GenerateGround()
    {
        groundMap = new TileBase[width, height];
        groundType = new int[width, height];

        float seedOffsetX = seed % 1000f;
        float seedOffsetY = seed / 1000f % 1000f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float value = Mathf.PerlinNoise(x * grassNoiseScale + seedOffsetX, y * grassNoiseScale + seedOffsetY);
                if (value > grassThreshold)
                {
                    groundMap[x, y] = GetRandomTile(grassTile);
                    groundType[x, y] = 0;
                }
                else
                {
                    groundMap[x, y] = GetRandomTile(sandTile);
                    groundType[x, y] = 1;
                }
            }
        }

        groundMapVithoutTransitions = (TileBase[,])groundMap.Clone();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase t = GetGroundTransitionTile(x, y);
                if (t != null) groundMap[x, y] = t;
            }
        }
    }

    // Determines if a transition tile is needed based on neighboring tiles and returns the appropriate tile
    private TileBase GetGroundTransitionTile(int x, int y)
    {
        bool isGrass = groundType[x, y] == 0;

        bool grassLeft = x > 0 && groundType[x - 1, y] == 0;
        bool grassRight = x < width - 1 && groundType[x + 1, y] == 0;
        bool grassDown = y > 0 && groundType[x, y - 1] == 0;
        bool grassUp = y < height - 1 && groundType[x, y + 1] == 0;

        bool sandLeft = x > 0 && groundType[x - 1, y] == 1;
        bool sandRight = x < width - 1 && groundType[x + 1, y] == 1;
        bool sandDown = y > 0 && groundType[x, y - 1] == 1;
        bool sandUp = y < height - 1 && groundType[x, y + 1] == 1;

        bool undefinedLeft = x == 0;
        bool undefinedRight = x == width - 1;
        bool undefinedDown = y == 0;
        bool undefinedUp = y == height - 1;

        bool grassTopLeft = x > 0 && y < height - 1 && groundType[x - 1, y + 1] == 0;
        bool grassTopRight = x < width - 1 && y < height - 1 && groundType[x + 1, y + 1] == 0;
        bool grassBottomLeft = x > 0 && y > 0 && groundType[x - 1, y - 1] == 0;
        bool grassBottomRight = x < width - 1 && y > 0 && groundType[x + 1, y - 1] == 0;

        if (!isGrass)
        {
            if (!sandLeft && !sandRight && !sandDown && !sandUp)
                return GetRandomTile(grassTile);

            if (grassUp && grassRight && grassTopRight)
                return transitionTile[0];
            if (grassUp && grassLeft && grassTopLeft)
                return transitionTile[1];
            if (grassDown && grassRight && grassBottomRight)
                return transitionTile[2];
            if (grassDown && grassLeft && grassBottomLeft)
                return transitionTile[3];

            if (grassLeft) return transitionTile[8];
            if (grassDown) return transitionTile[9];
            if (grassUp) return transitionTile[10];
            if (grassRight) return transitionTile[11];

            if (grassBottomLeft && (sandUp || undefinedUp) && (sandRight || undefinedRight))
                return transitionTile[4];
            if (grassBottomRight && (sandUp || undefinedUp) && (sandLeft || undefinedLeft))
                return transitionTile[5];
            if (grassTopLeft && (sandDown || undefinedDown) && (sandRight || undefinedRight))
                return transitionTile[6];
            if (grassTopRight && (sandDown || undefinedDown) && (sandLeft || undefinedLeft))
                return transitionTile[7];
        }

        return null;
    }

    // Places the generated ground tiles onto the tilemap
    private void PlaceGroundTiles()
    {
        groundTilemap.ClearAllTiles();

        int groundTilesPlaced = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = groundMap[x, y];
                if (tile != null)
                {
                    groundTilemap.SetTile(pos, tile);
                    groundTilesPlaced++;
                }
            }
        }

        Debug.Log($"Map generated! Seed: {seed}, Ground tiles: {groundTilesPlaced}");
    }

    // Generates the main route from the left edge to the right edge of the map, ensuring it passes through both grass and sand biomes
    private void GenerateRoutes()
    {

    }

    // Places the generated route tiles onto the tilemap
    private void PlaceGroundTiles()
    {
        routeTilemap.ClearAllTiles();

        int routeTilesPlaced = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = routeMap[x, y];
                if (tile != null)
                {
                    routeTilemap.SetTile(pos, tile);
                    routeTilesPlaced++;
                }
            }
        }

        Debug.Log($"Map generated! Seed: {seed}, Route tiles: {routeTilesPlaced}");
    }

    public void GenerateMap()
    {
        InitializeSeed();
        GenerateGround();
        PlaceGroundTiles();
        GenerateRoutes();
        PlaceRouteTiles();
    }

    public void GenerateMapWithSeed(int newSeed)
    {
        seed = newSeed;
        useRandomSeed = false;
        GenerateMap();
    }

    public int GetCurrentSeed() => seed;

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
            GenerateMap();
        if (Input.GetKeyDown(KeyCode.R))
        {
            useRandomSeed = true;
            GenerateMap();
        }
        if (Input.GetKeyDown(KeyCode.S) && Input.GetKey(KeyCode.LeftControl))
            Debug.Log($"Current seed: {seed}");
#elif ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
            GenerateMap();
        if (UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            useRandomSeed = true;
            GenerateMap();
        }
        if (UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame && UnityEngine.InputSystem.Keyboard.current.ctrlKey.isPressed)
            Debug.Log($"Current seed: {seed}");
#endif
    }
}