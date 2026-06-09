using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire")]
    public GameObject bulletPrefab;
    public Transform firePoint; // place at muzzle, oriented so its up (or forward) is shooting dir
    public float bulletSpeed = 20f;
    public float fireRate = 6f; // rounds per second
    public bool holdToFire = false; // true = auto while hold, false = single-shot on press

    [Header("Input (optional)")]
    public InputActionReference fireAction; // assign "Attack" action reference or use PlayerInput
    public string fireActionName = "Attack";
    public InputActionReference reloadAction; // assign "Reload" action reference or use PlayerInput
    public string reloadActionName = "Reload";

    [Header("Ammo")]
    public int maxAmmo = 30;
    public int currentAmmo = 30;
    public int reserveAmmo = 90;
    public float reloadTime = 1.5f;
    public TMP_Text ammoText;

    InputAction runtimeFire;
    InputAction runtimeReload;
    float nextFireTime;
    bool runtimeHoldFire;
    bool runtimeFireTriggered;
    bool runtimeReloadRequested;
    bool isReloading;
    float reloadEndTime;

    void OnEnable()
    {
        // resolve runtimeFire: prefer explicit reference, else find in PlayerInput
        var pi = GetComponent<PlayerInput>();

        if (fireAction != null && fireAction.action != null)
            runtimeFire = fireAction.action;
        else if (pi != null && pi.actions != null)
            runtimeFire = pi.actions.FindAction(fireActionName) ?? pi.actions.FindAction(pi.defaultActionMap + "/" + fireActionName);

        if (reloadAction != null && reloadAction.action != null)
            runtimeReload = reloadAction.action;
        else if (pi != null && pi.actions != null)
            runtimeReload = pi.actions.FindAction(reloadActionName) ?? pi.actions.FindAction(pi.defaultActionMap + "/" + reloadActionName);

        if (runtimeFire != null)
        {
            runtimeFire.started += OnFireStarted;
            runtimeFire.canceled += OnFireCanceled;
            runtimeFire.performed += OnFirePerformed;
            runtimeFire.Enable();
        }
        else
        {
            Debug.LogWarning("[PlayerShooting] Fire action not found. Assign an InputActionReference or set up PlayerInput with an 'Attack' action.");
        }

        if (runtimeReload != null)
        {
            runtimeReload.performed += OnReloadPerformed;
            runtimeReload.Enable();
        }
        else
        {
            Debug.LogWarning("[PlayerShooting] Reload action not found. Assign an InputActionReference or set up PlayerInput with a 'Reload' action.");
        }

        runtimeHoldFire = false;
        runtimeFireTriggered = false;
        runtimeReloadRequested = false;
        isReloading = false;
        reloadEndTime = 0f;
    }

    void OnDisable()
    {
        if (runtimeFire != null)
        {
            runtimeFire.started -= OnFireStarted;
            runtimeFire.canceled -= OnFireCanceled;
            runtimeFire.performed -= OnFirePerformed;
            runtimeFire.Disable();
        }

        if (runtimeReload != null)
        {
            runtimeReload.performed -= OnReloadPerformed;
            runtimeReload.Disable();
        }
    }

    void OnFireStarted(InputAction.CallbackContext context)
    {
        if (holdToFire)
            runtimeHoldFire = true;
        else
            runtimeFireTriggered = true;
    }

    void OnFireCanceled(InputAction.CallbackContext context)
    {
        if (holdToFire)
            runtimeHoldFire = false;
    }

    void OnFirePerformed(InputAction.CallbackContext context)
    {
        if (!holdToFire)
            runtimeFireTriggered = true;
    }

    void OnReloadPerformed(InputAction.CallbackContext context)
    {
        runtimeReloadRequested = true;
    }

    void Start()
    {
        if (currentAmmo <= 0)
            currentAmmo = maxAmmo;

        UpdateAmmoText();
    }

    void UpdateAmmoText()
    {
        if (ammoText == null) return;
        ammoText.text = $"Ammo: {currentAmmo}/{maxAmmo} | Reserve: {reserveAmmo}";
    }

    public void AddAmmo(int amount)
    {
        if (amount <= 0) return;

        // First fill current clip if needed, then add to reserve.
        int missing = maxAmmo - currentAmmo;
        int toCurrent = Mathf.Min(missing, amount);
        currentAmmo += toCurrent;
        amount -= toCurrent;
        reserveAmmo += amount;

        UpdateAmmoText();
        Debug.Log($"[PlayerShooting] Picked up ammo. Current: {currentAmmo}/{maxAmmo}, Reserve: {reserveAmmo}");
    }

    void Update()
    {
        if (runtimeFire == null && runtimeReload == null)
            return;

        if (isReloading)
        {
            if (Time.time >= reloadEndTime)
            {
                FinishReload();
            }
            return;
        }

        if (runtimeReloadRequested)
        {
            TryStartReload();
        }

        bool firedThisFrame = runtimeFireTriggered;

        if (!holdToFire && firedThisFrame)
        {
            TryFire();
        }

        runtimeFireTriggered = false;
        runtimeReloadRequested = false;
    }

    void TryFire()
    {
        if (Time.time < nextFireTime) return;
        if (isReloading) return;
        if (currentAmmo <= 0)
        {
            Debug.Log("[PlayerShooting] Out of ammo. Press Reload to refill.");
            return;
        }

        nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);

        if (bulletPrefab == null || firePoint == null)
        {
            if (bulletPrefab == null)
                Debug.LogWarning("[PlayerShooting] Bullet prefab is missing in inspector. Assign BulletPrefab to fire bullets.");
            if (firePoint == null)
                Debug.LogWarning("[PlayerShooting] Fire point is missing in inspector. Assign a FirePoint transform.");
            return;
        }

        currentAmmo--;
        UpdateAmmoText();
        Debug.Log($"[PlayerShooting] Fired at {Time.time:F2}. Ammo: {currentAmmo}/{maxAmmo} (reserve {reserveAmmo})");

        var b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        // Force bullet to move along firePoint's local Y axis
        var rb2 = b.GetComponent<Rigidbody2D>();
        if (rb2 != null)
        {
            Vector2 dir = firePoint.up;
            rb2.gravityScale = 0f;
            rb2.freezeRotation = true;
            rb2.linearVelocity = dir * bulletSpeed;
            rb2.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        else
        {
            var rb3 = b.GetComponent<Rigidbody>();
            if (rb3 != null)
            {
                Vector3 dir3 = firePoint.up;
                rb3.useGravity = false;
                rb3.freezeRotation = true;
                rb3.linearVelocity = dir3 * bulletSpeed;
                rb3.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }
    }

    void TryStartReload()
    {
        if (isReloading) return;
        if (currentAmmo >= maxAmmo)
        {
            Debug.Log("[PlayerShooting] Ammo full, reload not needed.");
            return;
        }
        if (reserveAmmo <= 0)
        {
            Debug.Log("[PlayerShooting] No reserve ammo left.");
            return;
        }

        isReloading = true;
        reloadEndTime = Time.time + Mathf.Max(0.01f, reloadTime);
        Debug.Log($"[PlayerShooting] Reloading... will finish at {reloadEndTime:F2}");
    }

    void FinishReload()
    {
        int needed = maxAmmo - currentAmmo;
        int used = Mathf.Min(needed, reserveAmmo);
        currentAmmo += used;
        reserveAmmo -= used;
        isReloading = false;
        UpdateAmmoText();
        Debug.Log($"[PlayerShooting] Reload complete. Ammo: {currentAmmo}/{maxAmmo}, reserve: {reserveAmmo}");
    }

}
