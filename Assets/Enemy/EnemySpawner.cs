using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    [SerializeField]
    private GameObject enemyPrefab;
    [SerializeField]
    private float spawnFrequency = 2f; // Частота спауна в секундах
    [SerializeField]
    private bool useSpawnProgression = true;
    [SerializeField]
    private AnimationCurve spawnIntervalOverTime = AnimationCurve.Linear(0f, 2f, 120f, 0.5f);
    [SerializeField]
    private float minSpawnInterval = 0.2f;
    [SerializeField]
    private int maxEnemies = 10; // Максимальное количество врагов на сцене
    [SerializeField]
    private int maxSpawnedEnemies = 20; // Максимальное количество врагов, которое спавнер может создать за всю игру
    [SerializeField]
    private bool isSpawning = true;

    [Header("Spawn Points")]
    [SerializeField]
    private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField]
    private bool randomSpawnPoint = true; // Выбирать случайную точку спауна

    [Header("Debug")]
    [SerializeField]
    private bool drawDebugGizmos = true;

    private float spawnTimer = 0f;
    private float elapsedTime = 0f;
    private int spawnedEnemiesCount = 0;
    private List<Enemy> activeEnemies = new List<Enemy>();

    void Start()
    {
        // Если нет указанных точек спауна, используем позицию самого спавнера
        if (spawnPoints.Count == 0)
        {
            spawnPoints.Add(transform);
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("Enemy Spawner: Enemy prefab не задан!");
        }

        spawnTimer = GetCurrentSpawnInterval();
    }

    void Update()
    {
        if (!isSpawning || enemyPrefab == null)
            return;

        if (spawnedEnemiesCount >= maxSpawnedEnemies)
        {
            StopSpawning();
            return;
        }

        elapsedTime += Time.deltaTime;

        // Очистить список от уничтоженных врагов
        activeEnemies.RemoveAll(enemy => enemy == null);

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && activeEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
            spawnTimer = GetCurrentSpawnInterval();
        }
    }

    void SpawnEnemy()
    {
        Transform spawnPoint = GetSpawnPoint();
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        
        spawnedEnemiesCount++;

        Enemy enemyComponent = newEnemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            activeEnemies.Add(enemyComponent);
        }

        Debug.Log($"Враг спавнен! Создано: {spawnedEnemiesCount}/{maxSpawnedEnemies}, активных: {activeEnemies.Count}");
    }

    Transform GetSpawnPoint()
    {
        if (randomSpawnPoint && spawnPoints.Count > 0)
        {
            return spawnPoints[Random.Range(0, spawnPoints.Count)];
        }
        return spawnPoints.Count > 0 ? spawnPoints[0] : transform;
    }

    // Публичные методы для управления спавнером
    public void SetSpawnFrequency(float frequency)
    {
        spawnFrequency = Mathf.Max(0.1f, frequency); // Минимум 0.1 сек
        spawnTimer = spawnFrequency;
    }

    public void SetMaxEnemies(int max)
    {
        maxEnemies = Mathf.Max(1, max);
    }

    public void StartSpawning()
    {
        isSpawning = true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }

    public void ClearAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
        spawnedEnemiesCount = 0;
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    private float GetCurrentSpawnInterval()
    {
        if (useSpawnProgression && spawnIntervalOverTime != null && spawnIntervalOverTime.length > 0)
        {
            return Mathf.Max(minSpawnInterval, spawnIntervalOverTime.Evaluate(elapsedTime));
        }

        return Mathf.Max(minSpawnInterval, spawnFrequency);
    }

    // Добавить точку спауна
    public void AddSpawnPoint(Transform point)
    {
        if (!spawnPoints.Contains(point))
        {
            spawnPoints.Add(point);
        }
    }

    // Удалить точку спауна
    public void RemoveSpawnPoint(Transform point)
    {
        spawnPoints.Remove(point);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
            return;

        Gizmos.color = Color.cyan;
        if (spawnPoints.Count == 0)
        {
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
        else
        {
            foreach (Transform point in spawnPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}
