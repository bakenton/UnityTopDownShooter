using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreenUI : MonoBehaviour
{
    public Button retryButton;
    public Button menuButton;
    public string gameSceneName = "GameScene";
    public string menuSceneName = "MainMenu";

    void Start()
    {
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (menuButton != null)
            menuButton.onClick.AddListener(GoToMenu);

        Time.timeScale = 0f; // Pause game
    }

    void RetryGame()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(gameSceneName);
    }

    void GoToMenu()
    {
        Time.timeScale = 1f; // Resume time
        SceneManager.LoadScene(menuSceneName);
    }

    void OnDestroy()
    {
        Time.timeScale = 1f; // Ensure time is resumed if scene unloads
    }
}
