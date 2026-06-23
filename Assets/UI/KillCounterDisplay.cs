using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KillCounterDisplay : MonoBehaviour
{
    [Tooltip("Текст UI для отображения количества убийств.")]
    public Text uiText;

    [Tooltip("Текст TMP для отображения количества убийств.")]
    public TMP_Text uiTMPText;

    [Tooltip("Формат текста. Используйте {0} для подстановки количества убийств.")]
    public string labelFormat = "Kills: {0}";

    void Start()
    {
        UpdateKillDisplay();
    }

    public void UpdateKillDisplay()
    {
        string text = string.Format(labelFormat, GameManager.Instance.KillCount);

        if (uiText != null)
            uiText.text = text;

        if (uiTMPText != null)
            uiTMPText.text = text;
    }
}
