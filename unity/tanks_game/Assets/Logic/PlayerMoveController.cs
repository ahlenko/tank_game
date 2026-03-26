using UnityEngine;
using UnityEngine.Tilemaps;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using System.Collections;

public class PlayerMoveController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 120f;

    [Header("Rotation Pivot")]
    public float rotationPivotOffset = 0.4f;

    [Header("Spawn")]
    public Vector2 spawnMin;
    public Vector2 spawnMax;

    [Header("References")]
    public Tilemap bordersTilemap;
    public Camera mainCamera;

    [Header("Prefabs")]
    public GameObject trackPrefab;
    public GameObject bulletPrefab;

    [Header("Track Settings")]
    public float trackSpawnDistance = 0.8f;
    public float trackLifetime = 10f;

    [Header("Shoot Settings")]
    public GameObject shootEffectPrefab;
    public float shootEffectOffset = 0.5f;
    public float shootCooldown = 0.5f;
    private float lastShootTime = -Mathf.Infinity;

    private Vector3 lastTrackPos;

    private bool shootEffectActive = false;

    void Start()
    {
        SpawnRandom();
        lastTrackPos = transform.position;
    }

    void Update()
    {
        HandleRotation();
        HandleMovement();
        HandleShoot();
        UpdateCamera();
    }

    void SpawnRandom()
    {
        float x = Random.Range(spawnMin.x, spawnMax.x);
        float y = Random.Range(spawnMin.y, spawnMax.y);
        transform.position = new Vector3(x, y, 0);
    }

    void HandleRotation()
    {
        if (shootEffectActive)
            return;

        float rotate = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A))
            rotate = 1f;
        if (Input.GetKey(KeyCode.D))
            rotate = -1f;
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current.aKey.isPressed)
            rotate = 1f;
        if (Keyboard.current.dKey.isPressed)
            rotate = -1f;
#endif

        Vector3 pivot = transform.position - transform.up * rotationPivotOffset;
        transform.RotateAround(pivot, Vector3.forward, rotate * rotateSpeed * Time.deltaTime);
    }

    void HandleMovement()
    {
        if (shootEffectActive)
            return;

        float move = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.W))
            move = 1f;
        if (Input.GetKey(KeyCode.S))
            move = -1f;
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current.wKey.isPressed)
            move = 1f;
        if (Keyboard.current.sKey.isPressed)
            move = -1f;
#endif

        if (move == 0)
            return;

        Vector3 direction = transform.up * move;
        Vector3 nextPos = transform.position + direction * moveSpeed * Time.deltaTime;

        if (!IsBlocked(nextPos))
        {
            transform.position = nextPos;
            SpawnTrack();
        }
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
        if (bulletPrefab == null || shootEffectActive)
            return;

        if (Time.time - lastShootTime < shootCooldown)
            return;

        bool shoot = false;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Space))
            shoot = true;
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            shoot = true;
#endif

        if (shoot)
        {
            lastShootTime = Time.time;

            // Створення ефекту запуску
            if (shootEffectPrefab != null)
            {
                Vector3 effectPos = transform.position + transform.up * shootEffectOffset;
                GameObject effect = Instantiate(shootEffectPrefab, effectPos, transform.rotation);
                shootEffectActive = true;
                Destroy(effect, 0.2f);
                StartCoroutine(ResetShootEffect(0.2f));
            }

            // Створення снаряду зі зміщенням
            Vector3 bulletPos = transform.position + transform.up * shootEffectOffset;
            Instantiate(bulletPrefab, bulletPos, transform.rotation);
        }
    }

    private System.Collections.IEnumerator ResetShootEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        shootEffectActive = false;
    }

    void UpdateCamera()
    {
        if (mainCamera == null)
            return;

        Vector3 pos = transform.position;
        pos.z = mainCamera.transform.position.z;

        mainCamera.transform.position = pos;
    }
}
