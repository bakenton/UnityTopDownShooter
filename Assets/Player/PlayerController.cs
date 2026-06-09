using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Input (optional)")]
    public InputActionReference moveAction;
    public string moveActionName = "Move";

    [Header("Aiming")]
    public Camera cam;                 // if null -> Camera.main will be used
    [Tooltip("Degrees to add to final rotation (use if sprite faces right/forward differently).")]
    public float aimRotationOffset = 0f;
    [Tooltip("0 = instant, >0 = smoothing speed")]
    public float aimSmoothing = 0f;
    [Tooltip("Расстояние firePoint от центра персонажа")]
    public float firePointDistance = 0.5f;

    [Header("Sprint / Stamina")]
    [Tooltip("Перетащите Sprint action (Button) сюда или оставьте пустым — будет использован Left Shift")]
    public InputActionReference sprintAction;
    public string sprintActionName = "Sprint";
    public float sprintMultiplier = 1.8f;
    public float maxStamina = 5f;
    public float staminaDrainRate = 1.5f; // per second while sprinting
    public float staminaRegenRate = 1f;   // per second when not sprinting
    public float minSprintStamina = 0.1f; // minimum to allow sprint
    public Image staminaBarFill;

    Rigidbody2D rb;
    Vector2 moveInput;
    InputAction runtimeMove;
    InputAction runtimeSprint;

    float currentStamina;
    bool wantSprint;
    bool isSprinting;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerShooting playerShooting;
    private Vector2 lastLookDirection = Vector2.down;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        animator = GetComponent<Animator>();
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        playerShooting = GetComponent<PlayerShooting>();

        currentStamina = maxStamina;
        UpdateStaminaUI();
    }

    void OnEnable()
    {
        // move action resolution (existing logic)
        if (moveAction != null && moveAction.action != null)
        {
            runtimeMove = moveAction.action;
        }
        else
        {
            var pi = GetComponent<PlayerInput>();
            if (pi != null && pi.actions != null)
            {
                runtimeMove = pi.actions.FindAction(moveActionName) ?? pi.actions.FindAction(pi.defaultActionMap + "/" + moveActionName);
            }
        }

        // sprint action resolution (similar)
        if (sprintAction != null && sprintAction.action != null)
        {
            runtimeSprint = sprintAction.action;
        }
        else
        {
            var pi2 = GetComponent<PlayerInput>();
            if (pi2 != null && pi2.actions != null)
            {
                runtimeSprint = pi2.actions.FindAction(sprintActionName) ?? pi2.actions.FindAction(pi2.defaultActionMap + "/" + sprintActionName);
            }
        }

        if (runtimeMove != null) runtimeMove.Enable();
        if (runtimeSprint != null) runtimeSprint.Enable();
    }

    void OnDisable()
    {
        if (runtimeMove != null) runtimeMove.Disable();
        if (runtimeSprint != null) runtimeSprint.Disable();
    }

    void Update()
    {
        // movement input
        if (runtimeMove != null)
        {
            try
            {
                moveInput = runtimeMove.ReadValue<Vector2>();
            }
            catch (System.InvalidOperationException)
            {
                // fallback if binding not composite
                moveInput = ReadMoveFromKeyboardBindings();
            }
        }
        else
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

        // sprint input (new Input System or fallback)
        if (runtimeSprint != null)
        {
            // ReadValue<float>() works for Button/Key controls
            float v = 0f;
            try { v = runtimeSprint.ReadValue<float>(); }
            catch { v = 0f; }
            wantSprint = v > 0.5f;
        }
        else
        {
            var kb = Keyboard.current;
            if (kb != null)
                wantSprint = kb.leftShiftKey.isPressed;
            else
                wantSprint = Input.GetKey(KeyCode.LeftShift);
        }

        // handle stamina + aiming
        HandleStamina(Time.deltaTime);
        UpdateStaminaUI();
        HandleLook();
    }

    void FixedUpdate()
    {
        float speed = moveSpeed * (isSprinting ? sprintMultiplier : 1f);
        rb.MovePosition(rb.position + moveInput * speed * Time.fixedDeltaTime);
    }

    void HandleStamina(float dt)
    {
        bool moving = moveInput.sqrMagnitude > 0.001f;
        if (wantSprint && moving && currentStamina > minSprintStamina)
        {
            isSprinting = true;
            currentStamina -= staminaDrainRate * dt;
            if (currentStamina < 0f) currentStamina = 0f;
            // if exhausted, stop sprint immediately
            if (currentStamina <= 0f) isSprinting = false;
        }
        else
        {
            isSprinting = false;
            currentStamina += staminaRegenRate * dt;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }
    }

    void UpdateStaminaUI()
    {
        if (staminaBarFill == null) return;
        staminaBarFill.fillAmount = GetStaminaNormalized();
    }

    Vector2 ReadMoveFromKeyboardBindings()
    {
        var kb = Keyboard.current;
        if (kb == null) return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        float x = 0f, y = 0f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;

        var v = new Vector2(x, y);
        if (v.sqrMagnitude > 1f) v.Normalize();
        return v;
    }

    void HandleLook()
    {
        // get camera
        Camera useCam = cam != null ? cam : Camera.main;
        if (useCam == null) return;

        // read mouse position (new Input System if available, fallback legacy)
        Vector3 mouseScreen;
        if (Mouse.current != null)
        {
            var mv = Mouse.current.position.ReadValue();
            mouseScreen = new Vector3(mv.x, mv.y, 0f);
        }
        else
        {
            mouseScreen = Input.mousePosition;
        }

        float cameraToPlayerDistance = transform.position.z - useCam.transform.position.z;
        Vector3 mouseWorld = useCam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, cameraToPlayerDistance));
        Vector2 dir = (mouseWorld - transform.position);
        if (dir.sqrMagnitude < 0.0001f) return;

        dir.Normalize();
        lastLookDirection = dir;

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        targetAngle += aimRotationOffset;

        if (playerShooting != null && playerShooting.firePoint != null)
        {
            // Вращение firePoint
            playerShooting.firePoint.rotation = Quaternion.Euler(0f, 0f, targetAngle);
            
            // Движение firePoint вокруг персонажа на определённом расстоянии
            Vector3 firePointPos = transform.position + (Vector3)dir * firePointDistance;
            firePointPos.z = playerShooting.firePoint.position.z; // сохраняем Z
            playerShooting.firePoint.position = firePointPos;
        }

        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator == null) return;

        bool moving = moveInput.sqrMagnitude > 0.1f;
        
        // Переключаем параметр IsMoving в Animator
        animator.SetBool("IsMoving", moving);
        
        // Определяем направление движения для разворота спрайта
        Vector2 moveDirection = moveInput;
        if (moveDirection.sqrMagnitude < 0.1f)
        {
            // Если не движется, используем направление взгляда
            moveDirection = lastLookDirection;
        }
        
        // Разворот спрайта влево/вправо
        UpdateSpriteFlip(moveDirection);
    }
    
    void UpdateSpriteFlip(Vector2 direction)
    {
        if (spriteRenderer == null) return;
        
        // Если есть движение в стороны, разворачиваем спрайт
        if (Mathf.Abs(direction.x) > 0.1f)
        {
            spriteRenderer.flipX = direction.x < 0f;
        }
    }

    // публичный доступ к уровню стамины для UI
    public float GetStaminaNormalized()
    {
        return Mathf.Clamp01(currentStamina / maxStamina);
    }
}
