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
    }

    public void ResetKillCount()
    {
        KillCount = 0;
    }
}
