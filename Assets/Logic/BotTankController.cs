using UnityEngine;
using UnityEngine.Tilemaps;

public class BotTankController : MonoBehaviour
{
    [Header("Target")]
    public GameObject player;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotateSpeed = 120f;

    [Header("Spawn")]
    public Vector2 spawnMin;
    public Vector2 spawnMax;
    public float centerSafeRadius = 6f;

    [Header("Navigation")]
    public Tilemap bordersTilemap;
    public float directionChangeInterval = 2.5f;

    [Header("References")]
    public GameObject trackPrefab;
    public GameObject bulletPrefab;
    public GameObject shootEffectPrefab;

    [Header("Track Settings")]
    public float trackSpawnDistance = 0.8f;
    public float trackLifetime = 10f;

    [Header("Shoot Settings")]
    public float shootEffectOffset = 0.5f;
    public float shootCooldown = 1.2f;
    public float shootRange = 8f;

    [Header("Defeat bullet")]
    public GameObject defeatBulletPrefab;
    public GameObject defeatEffectPrefab;

    private float lastShootTime = -Mathf.Infinity;
    private float lastDirectionChange;
    private Vector3 lastTrackPos;

    private bool shootEffectActive = false;
    private Quaternion targetRotation;

    void Start()
    {
        SpawnRandomEdge();
        lastTrackPos = transform.position;
        lastDirectionChange = Time.time;
        targetRotation = transform.rotation;
    }

    void Update()
    {
        HandleMovement();

        if (player != null)
        {
            HandleShoot();
        }
    }

    void SpawnRandomEdge()
    {
        Vector3 pos;

        do
        {
            float x = Random.Range(spawnMin.x, spawnMax.x);
            float y = Random.Range(spawnMin.y, spawnMax.y);
            pos = new Vector3(x, y, 0);
        }
        while (
            Vector2.Distance(Vector2.zero, pos) < centerSafeRadius ||
            (bordersTilemap != null && bordersTilemap.HasTile(bordersTilemap.WorldToCell(pos)))
        );

        transform.position = pos;
    }

    void RotateTowardsPlayer()
    {
        if (shootEffectActive)
            return;

        Vector3 dir = player.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            rotateSpeed * Time.deltaTime
        );
    }

    void HandleMovement()
    {
        if (shootEffectActive)
            return;

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotateSpeed * Time.deltaTime
        );

        Vector3 nextPos = transform.position + transform.up * moveSpeed * Time.deltaTime;

        if (IsBlocked(nextPos))
        {
            AvoidObstacle();
            return;
        }

        transform.position = nextPos;
        SpawnTrack();

        if (Time.time - lastDirectionChange > directionChangeInterval)
        {
            float randomTurn = Random.Range(-45f, 45f);
            targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + randomTurn);
            lastDirectionChange = Time.time;
        }
    }

    void AvoidObstacle()
    {
        float turn = Random.Range(90f, 160f);
        targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + turn);
        lastDirectionChange = Time.time;
    }

    bool IsBlocked(Vector3 position)
    {
        if (bordersTilemap == null)
            return false;

        Vector3Int cell = bordersTilemap.WorldToCell(position);
        return bordersTilemap.HasTile(cell);
    }

    void SpawnTrack()
    {
        if (trackPrefab == null)
            return;

        if (Vector3.Distance(transform.position, lastTrackPos) < trackSpawnDistance)
            return;

        GameObject track = Instantiate(trackPrefab, transform.position, transform.rotation);
        Destroy(track, trackLifetime);

        lastTrackPos = transform.position;
    }

    void HandleShoot()
    {
        if (bulletPrefab == null || shootEffectActive || player == null)
            return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > shootRange)
            return;

        if (Time.time - lastShootTime < shootCooldown)
            return;

        lastShootTime = Time.time;

        Vector3 dir = player.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0, 0, targetAngle);

        if (shootEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + transform.up * shootEffectOffset;
            GameObject effect = Instantiate(shootEffectPrefab, effectPos, transform.rotation);
            shootEffectActive = true;
            Destroy(effect, 0.2f);
            StartCoroutine(ResetShootEffect(0.2f));
        }

        Vector3 bulletPos = transform.position + transform.up * shootEffectOffset;
        Instantiate(bulletPrefab, bulletPos, transform.rotation);
    }

    System.Collections.IEnumerator ResetShootEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        shootEffectActive = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == player)
        {
            Time.timeScale = 0f;
            return;
        }

        if (other.CompareTag("DefeatBullet"))
        {
            if (defeatEffectPrefab != null)
            {
                GameObject effect = Instantiate(defeatEffectPrefab, transform.position, Quaternion.identity);
                Destroy(effect, 0.5f);
            }

            Destroy(other.gameObject);

            GameObject newBot = Instantiate(gameObject);
            newBot.GetComponent<BotTankController>().SpawnRandomEdge();

            Destroy(gameObject);
        }
    }

    public void DestroyBot()
    {
        StartCoroutine(RespawnRoutine());
        Destroy(gameObject);
    }

    System.Collections.IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1f);

        GameObject newBot = Instantiate(gameObject);
        newBot.GetComponent<BotTankController>().SpawnRandomEdge();
    }
}
