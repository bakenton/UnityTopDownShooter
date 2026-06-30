using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class LevelExitTrigger : MonoBehaviour
{
    [Tooltip("Требуемое количество убийств для выхода из уровня. Если указано 0, будет использоваться GameManager.requiredKills.")]
    public int requiredKills = 0;

    [Tooltip("Имя сцены, которая загружается при успешном выходе из уровня.")]
    public string victorySceneName = "MainMenu";

    [Header("UI Message")]
    [Tooltip("Текстовое поле UI для вывода сообщения о состоянии выхода.")]
    public Text messageText;

    [Tooltip("TMP-текстовое поле для вывода сообщения о состоянии выхода.")]
    public TMP_Text messageTMPText;

    [Tooltip("Сообщение, когда убийств недостаточно.")]
    public string notEnoughKillsMessage = "Убийств недостаточно.";

    [Tooltip("Сообщение при успешном выходе из уровня.")]
    public string successMessage = "Уровень пройден!";

    [Tooltip("Формат строки для показа текущего счёта убийств, после сообщения об отсутствии убийств.")]
    public string currentKillsFormat = "Текущий счёт: {0}";

    [Tooltip("Время, в течение которого показывается сообщение, сек.")]
    public float messageDisplayTime = 2f;

    private float messageTimer;

    void Reset()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.isTrigger = true;
    }

    void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
            if (messageTimer <= 0f)
                ClearMessage();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        int killTarget = requiredKills > 0 ? requiredKills : GameManager.Instance.requiredKills;

        if (GameManager.Instance.KillCount >= killTarget)
        {
            ShowSuccessMessage();
            LoadVictoryScene();
            return;
        }

        ShowInsufficientKillsMessage();
    }

    private void ShowInsufficientKillsMessage()
    {
        string message = notEnoughKillsMessage;
        if (!string.IsNullOrEmpty(currentKillsFormat))
            message += " " + string.Format(currentKillsFormat, GameManager.Instance.KillCount);

        SetMessage(message);
        messageTimer = messageDisplayTime;
    }

    private void ShowSuccessMessage()
    {
        SetMessage(successMessage);
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;

        if (messageTMPText != null)
            messageTMPText.text = message;
    }

    private void ClearMessage()
    {
        SetMessage(string.Empty);
    }

    private void LoadVictoryScene()
    {
        if (string.IsNullOrEmpty(victorySceneName))
            return;

        if (!IsSceneInBuildSettings(victorySceneName))
            return;

        SceneManager.LoadScene(victorySceneName);
    }

    private bool IsSceneInBuildSettings(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (string.Equals(name, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
