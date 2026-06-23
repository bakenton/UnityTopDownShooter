using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button exitButton;

    [Header("Scene")]
    [SerializeField]
    private string gameSceneName = "GameScene";

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    public void PlayGame()
    {
        Debug.Log($"MainMenuUI: loading scene '{gameSceneName}'");
        SceneManager.LoadScene(gameSceneName);
    }

    public void ExitGame()
    {
        Debug.Log("MainMenuUI: exiting game");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
