using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Edgar.Unity;

/// <summary>
/// MonoBehaviour component version of the enemy generation system
/// Attach this to your dungeon generator game object
/// </summary>
public class EnemyGenerationComponent : DungeonGeneratorPostProcessingComponentGrid2D
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] basicEnemyPrefabs;
    [SerializeField] private GameObject[] eliteEnemyPrefabs;
    [SerializeField] private GameObject[] bossEnemyPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int minEnemiesPerRoom = 2;
    [SerializeField] private int maxEnemiesPerRoom = 5;
    [SerializeField] private float minDistanceFromWalls = 1.5f;
    [SerializeField] private float minDistanceBetweenEnemies = 2f;
    [SerializeField] private bool spawnElitesInNormalRooms = true;
    [SerializeField] private float eliteSpawnChance = 0.2f;

    [Header("Room Type Tags")]
    [SerializeField] private string normalRoomTag = "NormalRoom";
    [SerializeField] private string treasureRoomTag = "TreasureRoom";
    [SerializeField] private string bossRoomTag = "BossRoom";
    [SerializeField] private string startRoomTag = "StartRoom";

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    [SerializeField] private bool logSpawnInfo = false;

    public override void Run(DungeonGeneratorLevelGrid2D level)
    {
        if (basicEnemyPrefabs == null || basicEnemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned to the Enemy Generation Component!");
            return;
        }

        if (logSpawnInfo)
        {
            Debug.Log($"Starting enemy generation for {level.RoomInstances.Count} rooms");
        }

        // Get all rooms from the level
        foreach (var roomInstance in level.RoomInstances)
        {
            // Get the room instance GameObject
            GameObject roomGameObject = roomInstance.RoomTemplateInstance;
            
            // Skip enemy generation for specific room types
            RoomInfo roomInfoComponent = roomGameObject.GetComponent<RoomInfo>();
            if (ShouldSkipRoom(roomInfoComponent))
                continue;

            // Get room bounds
            Bounds roomBounds = GetRoomBounds(roomGameObject);
            
            // Determine room type and spawn appropriate enemies
            if (roomInfoComponent != null)
            {
                if (roomInfoComponent.CompareTag(bossRoomTag))
                {
                    if (logSpawnInfo) Debug.Log($"Spawning boss in room {roomGameObject.name}");
                    SpawnBossInRoom(roomBounds, roomGameObject);
                }
                else if (roomInfoComponent.CompareTag(normalRoomTag))
                {
                    int enemyCount;
                    
                    // Check if room has custom enemy count settings
                    if (roomInfoComponent.OverridesGlobalSettings())
                    {
                        enemyCount = UnityEngine.Random.Range(roomInfoComponent.GetMinEnemies(), roomInfoComponent.GetMaxEnemies() + 1);
                    }
                    else
                    {
                        enemyCount = UnityEngine.Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
                    }
                    
                    if (logSpawnInfo) Debug.Log($"Spawning {enemyCount} enemies in room {roomGameObject.name}");
                    SpawnEnemiesInRoom(roomBounds, roomGameObject, enemyCount);
                }
            }
            else
            {
                // Fallback if no RoomInfo component
                int enemyCount = UnityEngine.Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1);
                if (logSpawnInfo) Debug.Log($"Spawning {enemyCount} enemies in room {roomGameObject.name} (no RoomInfo)");
                SpawnEnemiesInRoom(roomBounds, roomGameObject, enemyCount);
            }
        }
        
        if (logSpawnInfo)
        {
            Debug.Log("Enemy generation completed");
        }
    }

    private bool ShouldSkipRoom(RoomInfo roomInfo)
    {
        if (roomInfo == null) return false;
        
        // Skip rooms that don't allow enemy spawning
        if (!roomInfo.AllowEnemySpawning())
            return true;
            
        // Skip start room and treasure rooms
        return roomInfo.CompareTag(startRoomTag) || roomInfo.CompareTag(treasureRoomTag);
    }

    private Bounds GetRoomBounds(GameObject roomObject)
    {
        Bounds bounds = new Bounds(roomObject.transform.position, Vector3.zero);
        
        // Find all tilemaps in the room
        Tilemap[] tilemaps = roomObject.GetComponentsInChildren<Tilemap>();
        
        foreach (var tilemap in tilemaps)
        {
            // Skip tilemaps that are used for decoration only
            if (tilemap.gameObject.CompareTag("Decoration"))
                continue;
                
            if (tilemap.GetComponent<TilemapCollider2D>() != null)
            {
                bounds.Encapsulate(tilemap.localBounds);
            }
        }
        
        // Shrink bounds to account for walls
        bounds.size = new Vector3(
            bounds.size.x - minDistanceFromWalls * 2,
            bounds.size.y - minDistanceFromWalls * 2,
            bounds.size.z
        );
        
        return bounds;
    }

    private void SpawnEnemiesInRoom(Bounds roomBounds, GameObject roomObject, int enemyCount)
    {
        List<Vector2> spawnPositions = new List<Vector2>();
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition(roomBounds, spawnPositions);
            
            if (spawnPos != Vector2.zero)
            {
                GameObject enemyPrefab = SelectEnemyPrefab(roomObject);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.transform.SetParent(roomObject.transform);
                spawnPositions.Add(spawnPos);
            }
        }
    }

    private void SpawnBossInRoom(Bounds roomBounds, GameObject roomObject)
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No boss prefabs assigned to the Enemy Generation Component!");
            return;
        }

        // Spawn boss in the center of the room
        Vector2 spawnPos = roomBounds.center;
        
        GameObject bossPrefab = bossEnemyPrefabs[UnityEngine.Random.Range(0, bossEnemyPrefabs.Length)];
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        boss.transform.SetParent(roomObject.transform);
    }

    private Vector2 GetValidSpawnPosition(Bounds roomBounds, List<Vector2> existingPositions)
    {
        const int MAX_ATTEMPTS = 50;
        
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            // Generate random position within room bounds
            Vector2 randomPos = new Vector2(
                UnityEngine.Random.Range(roomBounds.min.x, roomBounds.max.x),
                UnityEngine.Random.Range(roomBounds.min.y, roomBounds.max.y)
            );
            
            // Check if position is far enough from existing enemies
            bool isValidPosition = true;
            foreach (Vector2 existingPos in existingPositions)
            {
                if (Vector2.Distance(randomPos, existingPos) < minDistanceBetweenEnemies)
                {
                    isValidPosition = false;
                    break;
                }
            }
            
            if (isValidPosition)
            {
                // Perform raycast to ensure we're not spawning inside colliders
                if (!Physics2D.OverlapCircle(randomPos, 0.5f))
                {
                    return randomPos;
                }
            }
        }
        
        // If we couldn't find a valid position after max attempts
        Debug.LogWarning("Could not find valid spawn position after " + MAX_ATTEMPTS + " attempts");
        return Vector2.zero;
    }

    private GameObject SelectEnemyPrefab(GameObject roomObject)
    {
        // Check if we should spawn an elite enemy
        if (spawnElitesInNormalRooms && UnityEngine.Random.value < eliteSpawnChance && eliteEnemyPrefabs.Length > 0)
        {
            return eliteEnemyPrefabs[UnityEngine.Random.Range(0, eliteEnemyPrefabs.Length)];
        }
        
        // Otherwise spawn a basic enemy
        return basicEnemyPrefabs[UnityEngine.Random.Range(0, basicEnemyPrefabs.Length)];
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos)
            return;
            
        // Draw debug visualization for spawn areas
        // This will only work in the editor
    }
} 