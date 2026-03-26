using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 12f;

    [Header("Lifetime")]
    public float maxDistance = 10f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        Move();
        CheckDistance();
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
            Destroy(gameObject);
        }
    }
}