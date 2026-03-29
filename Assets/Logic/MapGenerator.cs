using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class MapGenerator : MonoBehaviour, IInitializable

{
    public static MapGenerator Instance;
    [Header("Ground Tiles")]
    public TileBase[] grassTile;
    public TileBase[] sandTile;
    public TileBase[] transitionTile;
    public TileBase[] grassRoadTile;
    public TileBase[] sandRoadTile;
    public TileBase[] transitionRoadTile;

    public TileBase[] bordersUniversal;
    public TileBase[] bordersGrass;
    public TileBase[] bordersSand;
    public int borderIntensity = 3;

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
    public Tilemap borderTilemap;

    private TileBase[,] groundMapVithoutTransitions;
    private TileBase[,] groundMap;
    private TileBase[,] routeMap;
    private TileBase[,] borderMap;
    private int[,] groundType;

    public void setMapSeed(int newSeed)
    {
        seed = newSeed;
        useRandomSeed = false;
        GenerateMap();
    }


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
        routeTilemap.ClearAllTiles();

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
        routeMap = new TileBase[width, height];
        HashSet<Vector2Int> roadCells = new HashSet<Vector2Int>();

        // ---------------------------
        // 1. Main road from left edge to right edge
        // ---------------------------
        Vector2Int start = new Vector2Int(0, Random.Range(0, height));
        Vector2Int end = new Vector2Int(width - 1, Random.Range(0, height));

        List<Vector2Int> mainPath = new List<Vector2Int> { start };
        roadCells.Add(start);
        Vector2Int current = start;
        int maxSteps = width * height * 2;
        int steps = 0;
        while (current != end && steps < maxSteps)
        {
            steps++;
            Vector2Int[] dirs = { Vector2Int.right, Vector2Int.up, Vector2Int.down };
            List<Vector2Int> possibleMoves = new List<Vector2Int>();
            List<float> weights = new List<float>();

            foreach (var dir in dirs)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height && !roadCells.Contains(next))
                {
                    possibleMoves.Add(next);
                    int newDistX = Mathf.Abs(next.x - end.x);
                    int oldDistX = Mathf.Abs(current.x - end.x);
                    if (newDistX < oldDistX)
                        weights.Add(10f);
                    else if (newDistX == oldDistX)
                        weights.Add(5f);
                    else
                        weights.Add(1f);
                }
            }

            if (possibleMoves.Count == 0)
            {
                // Stuck – try a different start (simple fallback: restart)
                start = new Vector2Int(0, Random.Range(0, height));
                end = new Vector2Int(width - 1, Random.Range(0, height));
                roadCells.Clear();
                mainPath.Clear();
                current = start;
                roadCells.Add(start);
                mainPath.Add(start);
                steps = 0;
                continue;
            }

            // Choose a random move based on weights
            int idx = 0;
            float totalWeight = 0;
            foreach (float w in weights) totalWeight += w;
            float rand = Random.Range(0f, totalWeight);
            float cumulative = 0;
            for (int i = 0; i < possibleMoves.Count; i++)
            {
                cumulative += weights[i];
                if (rand <= cumulative)
                {
                    idx = i;
                    break;
                }
            }
            Vector2Int nextPos = possibleMoves[idx];
            roadCells.Add(nextPos);
            mainPath.Add(nextPos);
            current = nextPos;
        }

        // ---------------------------
        // 2. Side roads from top and bottom edges
        // ---------------------------

        int topX = Random.Range(0, width);
        int topY = height - 1;
        for (int y = topY - 1; y >= 0; y--)
        {
            Vector2Int pos = new Vector2Int(topX, y);
            if (roadCells.Contains(pos))
            {
                for (int yy = topY; yy > y; yy--)
                {
                    Vector2Int p = new Vector2Int(topX, yy);
                    if (!roadCells.Contains(p))
                    {
                        roadCells.Add(p);
                    }
                }
                break;
            }
        }

        int bottomX = Random.Range(0, width);
        int bottomY = 0;
        for (int y = bottomY + 1; y < height; y++)
        {
            Vector2Int pos = new Vector2Int(bottomX, y);
            if (roadCells.Contains(pos))
            {
                for (int yy = bottomY; yy < y; yy++)
                {
                    Vector2Int p = new Vector2Int(bottomX, yy);
                    if (!roadCells.Contains(p))
                    {
                        roadCells.Add(p);
                    }
                }
                break;
            }
        }

        // ---------------------------
        // 3. Determine road tile indices for each road cell
        // ---------------------------
        Vector2Int[] dirs4 = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        foreach (Vector2Int pos in roadCells)
        {
            List<Vector2Int> neighborDirs = new List<Vector2Int>();
            foreach (Vector2Int dir in dirs4)
            {
                Vector2Int neighbor = pos + dir;
                if (neighbor.x >= 0 && neighbor.x < width && neighbor.y >= 0 && neighbor.y < height && roadCells.Contains(neighbor))
                {
                    neighborDirs.Add(dir);
                }
            }

            int count = neighborDirs.Count;
            int tileIndex = -1;

            if (count == 1)
            {
                Vector2Int dir = neighborDirs[0];
                if (dir.x != 0)
                    tileIndex = 6;
                else
                    tileIndex = 7;
            }
            else if (count == 2)
            {
                Vector2Int dirA = neighborDirs[0];
                Vector2Int dirB = neighborDirs[1];

                if ((dirA == Vector2Int.left && dirB == Vector2Int.right) ||
                    (dirA == Vector2Int.right && dirB == Vector2Int.left))
                {
                    tileIndex = 6;
                }
                else if ((dirA == Vector2Int.up && dirB == Vector2Int.down) ||
                         (dirA == Vector2Int.down && dirB == Vector2Int.up))
                {
                    tileIndex = 7;
                }
                else
                {
                    if ((dirA == Vector2Int.left && dirB == Vector2Int.up) ||
                        (dirA == Vector2Int.up && dirB == Vector2Int.left))
                        tileIndex = 2; // left turn
                    else if ((dirA == Vector2Int.left && dirB == Vector2Int.down) ||
                             (dirA == Vector2Int.down && dirB == Vector2Int.left))
                        tileIndex = 0; // right turn
                    else if ((dirA == Vector2Int.right && dirB == Vector2Int.up) ||
                             (dirA == Vector2Int.up && dirB == Vector2Int.right))
                        tileIndex = 3; // inverted left turn 
                    else if ((dirA == Vector2Int.right && dirB == Vector2Int.down) ||
                             (dirA == Vector2Int.down && dirB == Vector2Int.right))
                        tileIndex = 1; // inverted right turn 
                    else
                        tileIndex = 6;
                }
            }
            else if (count == 3)
            {
                bool hasLeft = false, hasRight = false, hasUp = false, hasDown = false;
                foreach (var d in neighborDirs)
                {
                    if (d == Vector2Int.left) hasLeft = true;
                    if (d == Vector2Int.right) hasRight = true;
                    if (d == Vector2Int.up) hasUp = true;
                    if (d == Vector2Int.down) hasDown = true;
                }
                if (!hasLeft) tileIndex = 8;   // T pointing right
                else if (!hasUp) tileIndex = 10;    // T pointing up
                else if (!hasDown) tileIndex = 9;  // T pointing down
                else if (!hasRight) tileIndex = 11; // T pointing left
            }
            else if (count == 4)
            {
                tileIndex = 4;
            }

            if (tileIndex == -1) continue;

            // ---------------------------
            // 4. Choose correct tile array (grass, sand, or transition)
            // ---------------------------
            int ground = groundType[pos.x, pos.y]; // 0 = grass, 1 = sand
            TileBase chosenTile = null;

            bool isStraight = (count == 2 &&
                               ((neighborDirs[0] == Vector2Int.left && neighborDirs[1] == Vector2Int.right) ||
                                (neighborDirs[0] == Vector2Int.right && neighborDirs[1] == Vector2Int.left) ||
                                (neighborDirs[0] == Vector2Int.up && neighborDirs[1] == Vector2Int.down) ||
                                (neighborDirs[0] == Vector2Int.down && neighborDirs[1] == Vector2Int.up)));

            if (isStraight)
            {

                Vector2Int roadDir = neighborDirs[0];
                Vector2Int neighborPos = pos + roadDir;
                int neighborGround = groundType[neighborPos.x, neighborPos.y];
                if (ground != neighborGround)
                {
                    int transIndex = -1;
                    if (roadDir == Vector2Int.right)
                    {
                        if (ground == 0 && neighborGround == 1) transIndex = 0;
                        else if (ground == 1 && neighborGround == 0) transIndex = 6;
                    }
                    else if (roadDir == Vector2Int.left)
                    {
                        if (ground == 0 && neighborGround == 1) transIndex = 6;
                        else if (ground == 1 && neighborGround == 0) transIndex = 0;
                    }
                    else if (roadDir == Vector2Int.up)
                    {
                        if (ground == 0 && neighborGround == 1) transIndex = 2;
                        else if (ground == 1 && neighborGround == 0) transIndex = 4;
                    }
                    else if (roadDir == Vector2Int.down)
                    {
                        if (ground == 0 && neighborGround == 1) transIndex = 4;
                        else if (ground == 1 && neighborGround == 0) transIndex = 2;
                    }

                    if (transIndex != -1 && transitionRoadTile != null && transitionRoadTile.Length > transIndex)
                        chosenTile = transitionRoadTile[transIndex];
                }
            }

            if (chosenTile == null)
            {
                if (ground == 0)
                {
                    if (grassRoadTile != null && tileIndex < grassRoadTile.Length)
                        chosenTile = grassRoadTile[tileIndex];
                }
                else
                {
                    if (sandRoadTile != null && tileIndex < sandRoadTile.Length)
                        chosenTile = sandRoadTile[tileIndex];
                }
            }

            if (chosenTile != null)
                routeMap[pos.x, pos.y] = chosenTile;
        }
    }

    // Places the generated route tiles onto the tilemap
    private void PlaceRouteTiles()
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



    private void GenerateBorders()
    {
        borderMap = new TileBase[width, height];
        TileBase frameTile = GetRandomTile(bordersUniversal);
        int maxBorder = Mathf.Clamp(borderIntensity, 0, 10);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                bool isFrame = x == 0 || y == 0 || x == width - 1 || y == height - 1;

                if (isFrame)
                {
                    borderMap[x, y] = frameTile;
                    continue;
                }

                if (maxBorder <= 0) continue;

                int chance = Random.Range(0, 50);
                if (chance > maxBorder) continue;

                if (routeMap[x, y] != null)
                {
                    TileBase roadBorder = GetRandomTile(bordersUniversal);
                    if (roadBorder != null)
                        borderMap[x, y] = roadBorder;
                    continue;
                }

                int ground = groundType[x, y];

                TileBase chosen = null;

                if (ground == 0)
                {
                    chosen = GetRandomTile(bordersGrass);
                }
                else
                {
                    chosen = GetRandomTile(bordersSand);
                }

                if (chosen == null)
                    chosen = GetRandomTile(bordersUniversal);

                if (chosen != null)
                    borderMap[x, y] = chosen;

                if (chosen != null && maxBorder > 3)
                {
                    int extend = Random.Range(1, maxBorder / 2 + 1);

                    if (Random.value > 0.5f)
                    {
                        for (int i = 1; i <= extend && x + i < width - 1; i++)
                        {
                            if (routeMap[x + i, y] == null)
                                borderMap[x + i, y] = chosen;
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= extend && y + i < height - 1; i++)
                        {
                            if (routeMap[x, y + i] == null)
                                borderMap[x, y + i] = chosen;
                        }
                    }
                }
            }
        }
    }

    // Places the generated route tiles onto the tilemap
    private void PlaceBorderTiles()
    {
        borderTilemap.ClearAllTiles();

        int borderTilesPlaced = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = borderMap[x, y];
                if (tile != null)
                {
                    borderTilemap.SetTile(pos, tile);
                    borderTilesPlaced++;
                }
            }
        }

        Debug.Log($"Map generated! Seed: {seed}, Border tiles: {borderTilesPlaced}");
    }


    public void GenerateMap()
    {
        InitializeSeed();
        GenerateGround();
        PlaceGroundTiles();
        GenerateRoutes();
        PlaceRouteTiles();
        GenerateBorders();
        PlaceBorderTiles();
    }

    public void GenerateMapWithSeed(int newSeed)
    {
        seed = newSeed;
        useRandomSeed = false;
        GenerateMap();
    }

    public int GetCurrentSeed() => seed;

    public void InitializeWithPosition(Vector3 position, float rotation = 0f)
    {
        return;
    }

    public void Initialize()
    {
        useRandomSeed = true;
        GenerateMap();
    }

    void Start()
    {
        GenerateMap();
    }

    void Update()
    {
#if ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape)) UIManager.Instance.ShowPauseUI();
#elif ENABLE_INPUT_SYSTEM
            if (Keyboard.current.escapeKey.wasPressedThisFrame) UIManager.Instance.ShowPauseUI();
#endif
    }
}