using UnityEngine;

public class FrameRateLock : MonoBehaviour
{
    [Range(30, 240)]
    public int targetFrameRate = 60;

    [Header("Application")]
    public bool vSyncCount = false;

    void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncCount ? 1 : 0;
    }

    void OnValidate()
    {
        Application.targetFrameRate = targetFrameRate;
        QualitySettings.vSyncCount = vSyncCount ? 1 : 0;
    }
}
