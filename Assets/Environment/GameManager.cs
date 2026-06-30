using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("GameManager");
                instance = obj.AddComponent<GameManager>();
            }
            return instance;
        }
    }

    public int KillCount { get; private set; }
    [Tooltip("Необходимое количество убийств для выхода из уровня.")]
    public int requiredKills = 10;

    public bool HasReachedRequiredKills => KillCount >= requiredKills;
    public event System.Action<int> OnKillCountChanged;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ResetKillCount();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddKill()
    {
        KillCount++;
        OnKillCountChanged?.Invoke(KillCount);
    }

    public void ResetKillCount()
    {
        KillCount = 0;
        OnKillCountChanged?.Invoke(KillCount);
    }
}
