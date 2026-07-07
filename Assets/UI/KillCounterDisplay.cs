using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Game Clear Buttons")]
    [SerializeField] private string menuSceneName = "MainMenu";

    void Start()
    {
        UpdateKillDisplay();

        var mainMenuButton = transform.Find("MainMenu")?.GetComponent<Button>();
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);

        var exitButton = transform.Find("Exit")?.GetComponent<Button>();
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        if (GameManager.Instance != null)
            GameManager.Instance.OnKillCountChanged += OnKillCountChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnKillCountChanged -= OnKillCountChanged;
    }

    void OnKillCountChanged(int newKillCount)
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

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
