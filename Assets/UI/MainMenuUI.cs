using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float sceneLoadDelay = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] [Range(0f, 1f)] private float clickVolume = 1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
    }

    public void PlayGame()
    {
        PlayClickSound();
        Debug.Log($"MainMenuUI: loading scene '{gameSceneName}'");
        Invoke(nameof(LoadGameScene), sceneLoadDelay);
    }

    public void ExitGame()
    {
        PlayClickSound();
        Debug.Log("MainMenuUI: exiting game");
        Invoke(nameof(QuitGame), sceneLoadDelay);
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void PlayClickSound()
    {
        if (clickSound != null && audioSource != null)
            audioSource.PlayOneShot(clickSound, clickVolume);
    }
}
