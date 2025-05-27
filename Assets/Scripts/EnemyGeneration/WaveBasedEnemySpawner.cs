using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Component for spawning enemies in waves
/// Useful for boss fights or special rooms
/// </summary>
public class WaveBasedEnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyWave
    {
        public string waveName = "Wave";
        public GameObject[] enemyPrefabs;
        public int enemyCount = 5;
        public float spawnDelay = 0.5f;
        public EnemySpawnUtils.PatternType spawnPattern = EnemySpawnUtils.PatternType.Circle;
        public float patternSpacing = 3f;
        public bool waitForWaveClear = true;
        public float delayAfterWave = 2f;
    }
    
    [System.Serializable]
    public class WaveEvent : UnityEvent<int> { }
    
    [Header("Wave Settings")]
    [SerializeField] private EnemyWave[] waves;
    [SerializeField] private bool autoStart = false;
    [SerializeField] private bool loopWaves = false;
    [SerializeField] private Transform spawnCenter;
    
    [Header("Events")]
    public UnityEvent onAllWavesCompleted;
    public WaveEvent onWaveStarted;
    public WaveEvent onWaveCompleted;
    
    private int currentWaveIndex = -1;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    
    private void Start()
    {
        // Use this object's position as spawn center if none specified
        if (spawnCenter == null)
            spawnCenter = transform;
            
        if (autoStart)
        {
            StartWaves();
        }
    }
    
    /// <summary>
    /// Start the wave sequence
    /// </summary>
    public void StartWaves()
    {
        if (waves == null || waves.Length == 0)
        {
            Debug.LogWarning("No waves configured for WaveBasedEnemySpawner");
            return;
        }
        
        currentWaveIndex = -1;
        StartNextWave();
    }
    
    /// <summary>
    /// Start the next wave in the sequence
    /// </summary>
    public void StartNextWave()
    {
        if (isSpawning)
        {
            StopSpawning();
        }
        
        currentWaveIndex++;
        
        // Loop back to first wave if needed
        if (currentWaveIndex >= waves.Length)
        {
            if (loopWaves)
            {
                currentWaveIndex = 0;
            }
            else
            {
                // All waves completed
                onAllWavesCompleted?.Invoke();
                return;
            }
        }
        
        // Start spawning the current wave
        spawnCoroutine = StartCoroutine(SpawnWave(currentWaveIndex));
    }
    
    /// <summary>
    /// Stop the current wave spawning
    /// </summary>
    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        
        isSpawning = false;
    }
    
    /// <summary>
    /// Clear all active enemies
    /// </summary>
    public void ClearEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        
        activeEnemies.Clear();
    }
    
    /// <summary>
    /// Spawn a specific wave
    /// </summary>
    private IEnumerator SpawnWave(int waveIndex)
    {
        isSpawning = true;
        EnemyWave wave = waves[waveIndex];
        
        // Notify wave started
        onWaveStarted?.Invoke(waveIndex);
        
        // Make sure we have valid enemy prefabs
        if (wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"Wave {waveIndex} has no enemy prefabs assigned");
            isSpawning = false;
            yield break;
        }
        
        // Generate spawn positions
        List<Vector2> spawnPositions = EnemySpawnUtils.GeneratePatternPositions(
            spawnCenter.position, 
            wave.spawnPattern, 
            wave.enemyCount, 
            wave.patternSpacing
        );
        
        // Spawn enemies with delay
        for (int i = 0; i < wave.enemyCount && i < spawnPositions.Count; i++)
        {
            GameObject prefab = wave.enemyPrefabs[UnityEngine.Random.Range(0, wave.enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, spawnPositions[i], Quaternion.identity);
            
            // Track the enemy
            activeEnemies.Add(enemy);
            
            // Add a component to track when the enemy is destroyed
            EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
            tracker.OnEnemyDestroyed += HandleEnemyDestroyed;
            
            // Wait before spawning next enemy
            yield return new WaitForSeconds(wave.spawnDelay);
        }
        
        isSpawning = false;
        
        // If we don't need to wait for wave clear, start next wave after delay
        if (!wave.waitForWaveClear)
        {
            yield return new WaitForSeconds(wave.delayAfterWave);
            StartNextWave();
        }
        // Otherwise, the next wave will be started when all enemies are cleared
    }
    
    /// <summary>
    /// Handle enemy destroyed event
    /// </summary>
    private void HandleEnemyDestroyed(GameObject enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
            
            // Check if wave is cleared
            if (activeEnemies.Count == 0 && !isSpawning)
            {
                // Wave completed
                onWaveCompleted?.Invoke(currentWaveIndex);
                
                // Start next wave after delay if current wave requires waiting for clear
                if (currentWaveIndex < waves.Length && waves[currentWaveIndex].waitForWaveClear)
                {
                    StartCoroutine(StartNextWaveAfterDelay(waves[currentWaveIndex].delayAfterWave));
                }
            }
        }
    }
    
    /// <summary>
    /// Start next wave after a delay
    /// </summary>
    private IEnumerator StartNextWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNextWave();
    }
    
    /// <summary>
    /// Helper component to track when an enemy is destroyed
    /// </summary>
    private class EnemyDeathTracker : MonoBehaviour
    {
        public event Action<GameObject> OnEnemyDestroyed;
        
        private void OnDestroy()
        {
            OnEnemyDestroyed?.Invoke(gameObject);
        }
    }
} 