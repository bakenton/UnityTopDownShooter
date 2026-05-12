using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public Image healthBarFill;
    public TMP_Text healthText;

    [Header("Death")]
    public float deathDelay = 1f;
    public string deathSceneName = "DeathScreen";

    bool isDead = false;

    void Awake()
    {
        if (currentHealth <= 0)
            currentHealth = maxHealth;

        UpdateHealthUI();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
        Debug.Log($"[PlayerHealth] Healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || isDead) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthUI();
        Debug.Log($"[PlayerHealth] Took {amount} damage. Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        gameObject.SetActive(false);
        Debug.Log("[PlayerHealth] Player died!");
        Invoke(nameof(LoadDeathScene), deathDelay);
    }

    void LoadDeathScene()
    {
        SceneManager.LoadScene(deathSceneName);
    }

    void UpdateHealthUI()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = Mathf.Clamp01((float)currentHealth / maxHealth);

        if (healthText != null)
            healthText.text = $"Health: {currentHealth}/{maxHealth}";
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }
}
