using UnityEngine;
using System.Collections;
using UnityEngine.Tilemaps;
using System.ComponentModel;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerTankController : BaseTankController, IInitializable
{


    [Header("Camera")]
    public Camera mainCamera;

    private bool isRespawning = false;

    public void Initialize()
    {
        isRespawning = false;
        base.Start();
        Spawn();
    }

    public void InitializeWithPosition(Vector3 position, float rotation = 0f)
    {
        base.Start();
        transform.position = position;
        transform.rotation = Quaternion.Euler(0, 0, rotation);
    }

    protected override void Start()
    {
        Initialize();
    }

    protected override void Update()
    {
        base.Update();
        HandleRotation();
        HandleMovement();
        HandleShoot();
        UpdateCamera();

        foreach (var bot in GameObject.FindGameObjectsWithTag("BotTank"))
        {

            if (Vector2.Distance(transform.position, bot.transform.position) < 1f)
            {
                if (!isRespawning)
                {
                    isRespawning = true;
                    GameManager.Instance.OnPlayerDefeated();
                    SpawnDefeatEffect();

                }
            }
        }
    }

    protected override void Spawn()
    {
        float x = Random.Range(spawnMin.x, spawnMax.x);
        float y = Random.Range(spawnMin.y, spawnMax.y);
        transform.position = new Vector3(x, y, 0);
    }

    private void HandleRotation()
    {
        if (shootEffectActive || UIManager.Instance.gameOnPause) return;

        float rotate = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A)) rotate = 1f;
        if (Input.GetKey(KeyCode.D)) rotate = -1f;
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current.aKey.isPressed) rotate = 1f;
        if (Keyboard.current.dKey.isPressed) rotate = -1f;
#endif

        Vector3 pivot = transform.position - transform.up * rotationPivotOffset;
        transform.RotateAround(pivot, Vector3.forward, rotate * rotateSpeed * Time.deltaTime);
    }

    private void HandleMovement()
    {
        if (shootEffectActive || UIManager.Instance.gameOnPause) return;

        float move = 0f;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.W)) move = 1f;
        if (Input.GetKey(KeyCode.S)) move = -1f;
#elif ENABLE_INPUT_SYSTEM
        if (Keyboard.current.wKey.isPressed) move = 1f;
        if (Keyboard.current.sKey.isPressed) move = -1f;
#endif

        if (move == 0) return;
        Vector3 direction = transform.up * move * moveSpeed;
        TryMove(direction);
    }

    private void HandleShoot()
    {
        if (bulletPrefab == null || shootEffectActive || UIManager.Instance.gameOnPause) return;

        bool shoot = false;

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetMouseButtonDown(0)) shoot = true;
#elif ENABLE_INPUT_SYSTEM
        if (Mouse.current.leftButton.wasPressedThisFrame) shoot = true;
#endif

        if (shoot) Shoot();
    }

    private void UpdateCamera()
    {
        if (mainCamera == null) return;
        Vector3 pos = transform.position;
        pos.z = mainCamera.transform.position.z;
        mainCamera.transform.position = pos;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("BotBullet"))
        {
            GameManager.Instance.OnPlayerDefeated();
            SpawnDefeatEffect();
        }

    }
}
