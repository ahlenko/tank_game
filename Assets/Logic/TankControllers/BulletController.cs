using UnityEngine;
using UnityEngine.Tilemaps;

public class BulletProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;

    [Header("Lifetime")]
    public float maxDistance = 10f;

    [Header("Prefabs")]
    public GameObject defeatEffectPrefab;
    public GameObject destroyEffectPrefab;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Move();
        CheckDistance();
        CheckBorderCollision();
    }

    void Move()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    void CheckDistance()
    {
        float dist = Vector3.Distance(startPosition, transform.position);

        if (dist >= maxDistance)
        {
            SpawnDestroyEffect();
            Destroy(gameObject);
        }
    }

    void CheckBorderCollision()
    {
        Collider2D hit = Physics2D.OverlapPoint(transform.position);

        if (hit != null && hit.CompareTag("Border"))
        {
            SpawnDefeatEffect();
            Destroy(gameObject);
        }
    }

    protected void SpawnDefeatEffect()
    {
        if (defeatEffectPrefab != null)
        {
            GameObject effect = Instantiate(defeatEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    protected void SpawnDestroyEffect()
    {
        if (destroyEffectPrefab != null)
        {
            GameObject effect = Instantiate(destroyEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }
}