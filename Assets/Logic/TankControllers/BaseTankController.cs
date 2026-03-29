using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public abstract class BaseTankController : MonoBehaviour
{
    [Header("Tank Settings")]
    [SerializeField] protected bool isPlayer = false;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotateSpeed = 90f;

    [Header("Spawn")]
    public Vector2 spawnMin;
    public Vector2 spawnMax;
    public float centerSafeRadius = 15f;

    [Header("Navigation")]
    public Tilemap bordersTilemap;

    [Header("Prefabs")]
    public GameObject trackPrefab;
    public GameObject bulletPrefab;
    public GameObject shootEffectPrefab;

    [Header("Track Settings")]
    public float trackSpawnDistance = 0.2f;
    public float trackLifetime = 2f;

    [Header("Shoot Settings")]
    public float shootEffectOffset = 1f;
    public float shootCooldown = 1.2f;

    [Header("Defeat bullet")]
    public GameObject defeatBulletPrefab;
    public GameObject defeatEffectPrefab;

    protected Vector3 lastTrackPos;
    protected bool shootEffectActive = false;
    protected float lastShootTime = -Mathf.Infinity;

    protected virtual void Start()
    {
        lastTrackPos = transform.position;
    }

    protected virtual void Update() { }

    protected abstract void Spawn();

    protected void SpawnTrack()
    {
        if (trackPrefab == null) return;
        if (Vector3.Distance(transform.position, lastTrackPos) < trackSpawnDistance) return;

        GameObject track = Instantiate(trackPrefab, transform.position, transform.rotation);
        Destroy(track, trackLifetime);
        lastTrackPos = transform.position;
    }

    protected bool IsBlocked(Vector3 position)
    {
        if (bordersTilemap == null) return false;
        Vector3Int cell = bordersTilemap.WorldToCell(position);
        return bordersTilemap.HasTile(cell);
    }

    protected IEnumerator ResetShootEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        shootEffectActive = false;
    }

    protected bool TryMove(Vector3 direction)
    {
        if (shootEffectActive) return false;

        Vector3 nextPos = transform.position + direction * moveSpeed * Time.deltaTime;
        if (IsBlocked(nextPos)) return false;

        transform.position = nextPos;
        SpawnTrack();
        return true;
    }

    protected void RotateTowards(Quaternion targetRotation, float speed)
    {
        if (shootEffectActive) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
    }

    protected virtual bool CanShoot()
    {
        return !shootEffectActive && Time.time - lastShootTime >= shootCooldown;
    }

    protected void Shoot()
    {
        if (!CanShoot()) return;

        lastShootTime = Time.time;

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

    protected void SpawnDefeatEffect()
    {
        if (defeatEffectPrefab != null)
        {
            GameObject effect = Instantiate(defeatEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    protected virtual void OnDefeated()
    {
        Destroy(gameObject);
    }
}