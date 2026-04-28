using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public Image healthBarFill;
    public TMP_Text healthText;

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
        if (amount <= 0) return;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        UpdateHealthUI();
        Debug.Log($"[PlayerHealth] Took {amount} damage. Health: {currentHealth}/{maxHealth}");
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
