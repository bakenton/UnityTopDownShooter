using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Fire")]
    public GameObject bulletPrefab;
    public Transform firePoint; // place at muzzle, oriented so its up (or forward) is shooting dir
    public float bulletSpeed = 20f;
    public float fireRate = 6f; // rounds per second
    public bool holdToFire = false; // true = auto while hold, false = single-shot on press

    [Header("Input (optional)")]
    public InputActionReference fireAction; // assign "Fire" action reference or use PlayerInput
    public string fireActionName = "Fire";

    InputAction runtimeFire;
    float nextFireTime;

    void OnEnable()
    {
        // resolve runtimeFire: prefer explicit reference, else find in PlayerInput
        if (fireAction != null && fireAction.action != null)
            runtimeFire = fireAction.action;
        else
        {
            var pi = GetComponent<PlayerInput>();
            if (pi != null && pi.actions != null)
                runtimeFire = pi.actions.FindAction(fireActionName) ?? pi.actions.FindAction(pi.defaultActionMap + "/" + fireActionName);
        }

        if (runtimeFire != null) runtimeFire.Enable();
    }

    void OnDisable()
    {
        if (runtimeFire != null) runtimeFire.Disable();
    }

    void Update()
    {
        bool wantFire = false;
        bool firedThisFrame = false;

        if (runtimeFire != null)
        {
            if (holdToFire)
            {
                // ReadValue<float>() works for Button controls; treat >0.5 as pressed
                float v = 0f;
                try { v = runtimeFire.ReadValue<float>(); } catch { v = 0f; }
                wantFire = v > 0.5f;
            }
            else
            {
                // single-press: use triggered
                try { firedThisFrame = runtimeFire.triggered; }
                catch { firedThisFrame = false; }
            }
        }
        else
        {
            var mb = Mouse.current;
            if (holdToFire)
            {
                if (mb != null) wantFire = mb.leftButton.isPressed;
                else wantFire = Input.GetMouseButton(0);
            }
            else
            {
                if (mb != null) firedThisFrame = mb.leftButton.wasPressedThisFrame;
                else firedThisFrame = Input.GetMouseButtonDown(0);
            }
        }

        // decide firing
        if (holdToFire && wantFire)
        {
            TryFire();
        }
        else if (!holdToFire && firedThisFrame)
        {
            TryFire();
        }
    }

    void TryFire()
    {
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + 1f / Mathf.Max(0.0001f, fireRate);

        if (bulletPrefab == null || firePoint == null) return;

        // instantiate bullet and set its velocity
        var b = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        var rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // assume firePoint.up is forward for top-down; change to right if your sprite points right
            Vector2 dir = firePoint.up; 
            rb.linearVelocity = dir.normalized * bulletSpeed;
        }
    }
}
