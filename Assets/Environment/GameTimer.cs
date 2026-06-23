using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Timer")]
    [Tooltip("Время до перехода на следующую сцену, в секундах.")]
    public float timeLimit = 120f;

    [Tooltip("Имя сцены, которая будет загружена по истечении таймера.")]
    public string nextSceneName = "NextScene";

    [Header("UI")]
    [Tooltip("Текстовое поле UI для отображения оставшегося времени.")]
    public Text timerText;

    [Tooltip("Текстовое поле TMP для отображения оставшегося времени.")]
    public TMP_Text timerTMPText;

    [Tooltip("Формат строки таймера. Используется {0} для подстановки оставшегося времени.")]
    public string timerFormat = "Time: {0}";

    private float elapsedTime;
    private bool isRunning;

    void Start()
    {
        Debug.Log($"GameTimer: started. nextSceneName='{nextSceneName}', timeLimit={timeLimit}");
        GameManager.Instance.ResetKillCount();
        ResetTimer();
        StartTimer();
    }

    void Update()
    {
        if (!isRunning)
            return;

        elapsedTime += Time.deltaTime;
        float remainingTime = Mathf.Max(0f, timeLimit - elapsedTime);
        UpdateTimerDisplay(remainingTime);

        if (elapsedTime >= timeLimit)
        {
            isRunning = false;
            LoadNextScene();
        }
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

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay(timeLimit);
    }

    private void UpdateTimerDisplay(float remainingTime)
    {
        string formattedTime = FormatTime(remainingTime);

        if (timerText != null)
            timerText.text = string.Format(timerFormat, formattedTime);

        if (timerTMPText != null)
            timerTMPText.text = string.Format(timerFormat, formattedTime);
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        return minutes > 0 ? string.Format("{0:00}:{1:00}", minutes, seconds) : string.Format("{0:00}", seconds);
    }

    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("GameTimer: nextSceneName не задан.");
            return;
        }

        if (!IsSceneInBuildSettings(nextSceneName))
        {
            Debug.LogError($"GameTimer: сцена '{nextSceneName}' не найдена в Build Settings. Добавьте её в Window -> Build Settings -> Scenes in Build.");
            return;
        }

        Debug.Log($"GameTimer: загружаю сцену '{nextSceneName}'");
        SceneManager.LoadScene(nextSceneName);
    }
}
