using UnityEngine;
using System.Collections;

public class BotTankController : BaseTankController
{
    [Header("Target")]
    public GameObject player;

    [Header("Navigation")]
    public float directionChangeInterval = 2.5f;
    public float shootRange = 8f;

    private float lastDirectionChange;
    private Quaternion targetRotation;

    protected override void Start()
    {
        base.Start();
        Spawn();
        lastDirectionChange = Time.time;
        targetRotation = transform.rotation;
    }

    protected override void Update()
    {
        base.Update();
        HandleMovement();
        HandleShoot();
    }

    protected override void Spawn()
    {
        Vector3 pos;
        do
        {
            float x = Random.Range(spawnMin.x, spawnMax.x);
            float y = Random.Range(spawnMin.y, spawnMax.y);
            pos = new Vector3(x, y, 0);
        }
        while (Vector2.Distance(Vector2.zero, pos) < centerSafeRadius ||
               (bordersTilemap != null && bordersTilemap.HasTile(bordersTilemap.WorldToCell(pos))));

        transform.position = pos;
    }

    private void HandleMovement()
    {
        if (shootEffectActive) return;

        RotateTowards(targetRotation, rotateSpeed);
        Vector3 direction = transform.up * moveSpeed;

        if (!TryMove(direction))
        {
            AvoidObstacle();
            return;
        }

        if (Time.time - lastDirectionChange > directionChangeInterval)
        {
            float randomTurn = Random.Range(-45f, 45f);
            targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + randomTurn);
            lastDirectionChange = Time.time;
        }
    }

    private void AvoidObstacle()
    {
        float turn = Random.Range(90f, 160f);
        targetRotation = Quaternion.Euler(0, 0, transform.eulerAngles.z + turn);
        lastDirectionChange = Time.time;
    }

    private void RotateTowardsPlayer()
    {
        if (player == null) return;
        Vector3 dir = player.transform.position - transform.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        targetRotation = Quaternion.Euler(0, 0, targetAngle);
    }

    private void HandleShoot()
    {
        if (bulletPrefab == null || shootEffectActive || player == null) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance > shootRange) return;

        RotateTowardsPlayer();
        Shoot();
    }

    protected override bool CanShoot()
    {
        return base.CanShoot() && player != null &&
               Vector3.Distance(transform.position, player.transform.position) <= shootRange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == player)
        {
            Time.timeScale = 0f;
            return;
        }

        if (other.CompareTag("DefeatBullet"))
        {
            Destroy(other.gameObject);
            OnDefeated();
        }
    }

    protected override void OnDefeated()
    {
        SpawnDefeatEffect();
        Destroy(gameObject);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1f);
        GameObject newBot = Instantiate(gameObject);
        newBot.GetComponent<BotTankController>().Spawn();
    }
}