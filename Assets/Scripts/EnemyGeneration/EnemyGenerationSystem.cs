using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Edgar.Unity;
using System.Linq;

[CreateAssetMenu(menuName = "Rune/Enemy Generation/Enemy Generation System", fileName = "EnemyGenerationSystem")]
public class EnemyGenerationSystem : DungeonGeneratorPostProcessingGrid2D
{
    [Header("Level Exit")]
    [SerializeField] private GameObject levelExitPrefab;

    [Header("Player Spawning")]
    [SerializeField] private bool movePlayerToSpawnRoom = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool moveCameraWithPlayer = true;

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

    [Header("Room Controller Settings")]
    [SerializeField] private bool lockDoorsOnEnemies = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;

    public override void Run(DungeonGeneratorLevelGrid2D level)
    {
        // First, move player to spawn room
        if (movePlayerToSpawnRoom)
        {
            MovePlayerToSpawnRoom(level);
        }

        // Then setup room controllers to handle door locking
        SetupRoomControllers(level);

        if (basicEnemyPrefabs == null || basicEnemyPrefabs.Length == 0)
        {
            Debug.LogError("No enemy prefabs assigned to the Enemy Generation System!");
            return;
        }

        // Get all rooms from the level
        foreach (var roomInstance in level.RoomInstances)
        {
            // Get the room instance GameObject
            GameObject roomGameObject = roomInstance.RoomTemplateInstance;
            
            // Skip enemy generation for specific room types
            if (ShouldSkipRoom(roomGameObject.GetComponent<RoomInfo>()))
                continue;

            // Get room bounds
            Bounds roomBounds = GetRoomBounds(roomGameObject);
            
            // Determine room type and spawn appropriate enemies
            RoomInfo roomInfoComponent = roomGameObject.GetComponent<RoomInfo>();
            if (roomInfoComponent != null)
            {
                if (roomInfoComponent.CompareTag(bossRoomTag))
                {
                    SpawnBossInRoom(roomBounds, roomGameObject);
                }
                else if (roomInfoComponent.CompareTag(normalRoomTag))
                {
                    SpawnEnemiesInRoom(roomBounds, roomGameObject, 
                        UnityEngine.Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1));
                }
            }
            else
            {
                // Fallback if no RoomInfo component
                SpawnEnemiesInRoom(roomBounds, roomGameObject, 
                    UnityEngine.Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1));
            }
        }

        // Add level exit to the last room
        AddLevelExit(level);
        
        // CRITICAL: Re-register emitters after spawning new enemies
        // The dungeon generation process destroys the original emitters that were registered during scene load
        Debug.Log("[EnemyGenerationSystem] Re-registering emitters after enemy spawning");
        BulletHell.ProjectileManager.Instance.RegisterEmitters();
    }

    private void MovePlayerToSpawnRoom(DungeonGeneratorLevelGrid2D level)
    {
        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player == null)
        {
            Debug.LogWarning($"No player found with tag '{playerTag}'. Player will not be moved to spawn room.");
            return;
        }

        // Find the spawn room
        RoomInstanceGrid2D spawnRoom = null;
        foreach (var roomInstance in level.RoomInstances)
        {
            if (roomInstance.IsCorridor) continue;

            // Check if this is the spawn room by name or tag
            if (roomInstance.Room.GetDisplayName().ToLower().Contains("spawn"))
            {
                spawnRoom = roomInstance;
                break;
            }
        }

        // If no spawn room found, use the first room
        if (spawnRoom == null)
        {
            spawnRoom = level.RoomInstances.FirstOrDefault(r => !r.IsCorridor);
            if (spawnRoom != null)
            {
                Debug.LogWarning("No spawn room found, using first available room for player spawn.");
            }
        }

        if (spawnRoom == null)
        {
            Debug.LogError("No rooms available for player spawning!");
            return;
        }

        // Calculate spawn position (center of the room)
        Bounds roomBounds = GetRoomBounds(spawnRoom.RoomTemplateInstance);
        Vector3 spawnPosition = roomBounds.center;
        spawnPosition.z = player.transform.position.z; // Preserve player's Z position

        // Move player to spawn position
        player.transform.position = spawnPosition;

        Debug.Log($"Player moved to spawn room at position: {spawnPosition}");

        // Move camera if enabled
        if (moveCameraWithPlayer)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 cameraPosition = spawnPosition;
                cameraPosition.z = mainCamera.transform.position.z; // Preserve camera's Z position
                mainCamera.transform.position = cameraPosition;
                Debug.Log($"Camera moved to follow player at position: {cameraPosition}");
            }
        }
    }

    private void SetupRoomControllers(DungeonGeneratorLevelGrid2D level)
    {
        foreach (var roomInstance in level.RoomInstances)
        {
            // Add a RoomController to each room
            RoomController roomController = roomInstance.RoomTemplateInstance.AddComponent<RoomController>();
            
            // Configure the room controller
            roomController.lockDoorsOnEnter = lockDoorsOnEnemies;
            
            // Set the player layer - Player is on layer 3
            roomController.playerLayer = 1 << 3;
            
            // Set the room trigger size based on the room bounds
            var roomBounds = GetRoomBounds(roomInstance.RoomTemplateInstance);
            roomController.roomTriggerSize = new Vector2(
                roomBounds.size.x * 0.8f, 
                roomBounds.size.y * 0.8f
            );
            
            // Find or create enemies container
            Transform enemiesContainer = roomInstance.RoomTemplateInstance.transform.Find("Enemies");
            if (enemiesContainer == null)
            {
                enemiesContainer = new GameObject("Enemies").transform;
                enemiesContainer.SetParent(roomInstance.RoomTemplateInstance.transform);
                enemiesContainer.localPosition = Vector3.zero;
            }
            
            // Setup room controller for this room
            if (roomController != null)
            {
                roomController.enemiesContainer = enemiesContainer;
                
                // Find doors in the room using Edgar's door system
                foreach (var doorInstance in roomInstance.Doors)
                {
                    // Create a door game object at the door position
                    GameObject doorObj = new GameObject("Door");
                    doorObj.transform.parent = roomInstance.RoomTemplateInstance.transform;
                    
                    // Debug logging for door position calculation
                    if (showDebugGizmos)
                    {
                        Debug.Log($"=== Door Position Debug for Room: {roomInstance.RoomTemplateInstance.name} ===");
                        Debug.Log($"Door Line From: {doorInstance.DoorLine.From}");
                        Debug.Log($"Door Line To: {doorInstance.DoorLine.To}");
                        Debug.Log($"Room Instance Position: {roomInstance.Position}");
                        Debug.Log($"Room Transform Position: {roomInstance.RoomTemplateInstance.transform.position}");
                        Debug.Log($"Room Transform Local Position: {roomInstance.RoomTemplateInstance.transform.localPosition}");
                    }
                    
                    // Calculate door center position in local coordinates (relative to room template)
                    Vector3 doorLocalCenter = new Vector3(
                        (doorInstance.DoorLine.From.x + doorInstance.DoorLine.To.x) / 2f,
                        (doorInstance.DoorLine.From.y + doorInstance.DoorLine.To.y) / 2f,
                        0
                    );
                    
                    // Check if the room has a Grid component for proper coordinate transformation
                    Transform tilemapsRoot = roomInstance.RoomTemplateInstance.transform.Find("Tilemaps");
                    Grid roomGrid = null;
                    if (tilemapsRoot != null)
                    {
                        roomGrid = tilemapsRoot.GetComponent<Grid>();
                    }
                    
                    Vector3 doorWorldCenter;
                    if (roomGrid != null)
                    {
                        // Use Grid's coordinate system for proper transformation
                        Vector3 gridLocalPos = roomGrid.CellToLocal(Vector3Int.RoundToInt(doorLocalCenter));
                        doorWorldCenter = roomGrid.transform.TransformPoint(gridLocalPos);
                        
                        if (showDebugGizmos)
                        {
                            Debug.Log($"Using Grid transformation:");
                            Debug.Log($"  Grid CellToLocal: {gridLocalPos}");
                            Debug.Log($"  Grid Transform Position: {roomGrid.transform.position}");
                            Debug.Log($"  Final World Position (Grid): {doorWorldCenter}");
                        }
                    }
                    else
                    {
                        // Fallback: Convert local door position to world position using room's transform
                        doorWorldCenter = roomInstance.RoomTemplateInstance.transform.TransformPoint(doorLocalCenter);
                        
                        if (showDebugGizmos)
                        {
                            Debug.Log($"Using Transform.TransformPoint fallback:");
                            Debug.Log($"  Final World Position (Transform): {doorWorldCenter}");
                        }
                    }
                    
                    // Additional debug: Compare with old calculation
                    if (showDebugGizmos)
                    {
                        Vector3 oldCalculation = doorLocalCenter + roomInstance.Position;
                        Vector3 positionDifference = doorWorldCenter - oldCalculation;
                        Debug.Log($"Door Local Center: {doorLocalCenter}");
                        Debug.Log($"Old Calculation Result: {oldCalculation}");
                        Debug.Log($"New Calculation Result: {doorWorldCenter}");
                        Debug.Log($"Position Difference: {positionDifference} (magnitude: {positionDifference.magnitude})");
                        Debug.Log($"========================");
                    }
                    
                    doorObj.transform.position = doorWorldCenter;
                    doorObj.tag = "Door";
                    
                    // Calculate door length from the door line
                    float doorLength = Vector3.Distance(doorInstance.DoorLine.From, doorInstance.DoorLine.To);
                    if (doorLength == 0) doorLength = 1f; // Minimum door size
                    
                    // Add door controller first (it will handle sprite creation)
                    DoorController doorController = doorObj.AddComponent<DoorController>();
                    
                    // Add collider after DoorController has been set up
                    BoxCollider2D doorCollider = doorObj.AddComponent<BoxCollider2D>();
                    // Set collider size based on door orientation
                    if (doorInstance.IsHorizontal)
                    {
                        doorCollider.size = new Vector2(doorLength, 1);
                    }
                    else
                    {
                        doorCollider.size = new Vector2(1, doorLength);
                    }
                    doorCollider.enabled = false;
                    
                    // Set the collider reference in the door controller
                    doorController.doorCollider = doorCollider;
                    
                    // Add this door to the room controller
                    roomController.doors.Add(doorObj);
                }
                
                // Refresh the room controller's doors list to include the newly created doors
                roomController.RefreshDoorsList();
            }
        }
    }

    private bool ShouldSkipRoom(RoomInfo roomInfo)
    {
        if (roomInfo == null) return false;
        
        // Skip start room and treasure rooms
        return roomInfo.CompareTag(startRoomTag) || roomInfo.CompareTag(treasureRoomTag);
    }

    private Bounds GetRoomBounds(GameObject roomObject)
    {
        Bounds bounds = new Bounds(roomObject.transform.position, Vector3.zero);
        bool boundsInitialized = false;
        
        // Look specifically for the floor tilemap first
        Transform tilemapsRoot = roomObject.transform.Find("Tilemaps");
        if (tilemapsRoot != null)
        {
            var floorTilemap = tilemapsRoot.GetComponentsInChildren<Tilemap>()
                .FirstOrDefault(t => t.gameObject.name.ToLower().Contains("floor"));
                
            if (floorTilemap != null)
            {
                // Get bounds in world space
                bounds = floorTilemap.localBounds;
                // Convert local bounds to world space
                bounds.center += floorTilemap.transform.position;
                boundsInitialized = true;
                
                if (showDebugGizmos)
                {
                    Debug.Log($"Room {roomObject.name} bounds from floor: {bounds.center}, size: {bounds.size}");
                }
            }
        }
        
        // Fall back to the original method if floor tilemap not found
        if (!boundsInitialized)
        {
            // Find all tilemaps in the room
            Tilemap[] tilemaps = roomObject.GetComponentsInChildren<Tilemap>();
            
            foreach (var tilemap in tilemaps)
            {
                // Skip tilemaps that are used for decoration only
                if (tilemap.gameObject.CompareTag("Decoration"))
                    continue;
                    
                if (tilemap.GetComponent<TilemapCollider2D>() != null)
                {
                    // Convert local bounds to world space
                    Bounds worldBounds = tilemap.localBounds;
                    worldBounds.center += tilemap.transform.position;
                    bounds.Encapsulate(worldBounds);
                }
            }
            
            if (showDebugGizmos)
            {
                Debug.Log($"Room {roomObject.name} bounds from colliders: {bounds.center}, size: {bounds.size}");
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
        
        // Find the enemies container for this room
        Transform enemiesContainer = roomObject.transform.Find("Enemies");
        if (enemiesContainer == null)
        {
            Debug.LogError($"Enemies container not found in room {roomObject.name}! Make sure SetupRoomControllers runs before SpawnEnemiesInRoom.");
            return;
        }
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 spawnPos = GetValidSpawnPosition(roomBounds, spawnPositions);
            
            if (spawnPos != Vector2.zero)
            {
                GameObject enemyPrefab = SelectEnemyPrefab(roomObject);
                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
                enemy.transform.SetParent(enemiesContainer); // ← FIX: Set parent to enemies container, not room
                spawnPositions.Add(spawnPos);
                
                if (showDebugGizmos)
                {
                    Debug.Log($"Spawned enemy {enemy.name} in room {roomObject.name} at position {spawnPos}. Parent: {enemy.transform.parent.name}");
                }
            }
        }
        
        if (showDebugGizmos)
        {
            Debug.Log($"Total enemies spawned in {roomObject.name}: {enemiesContainer.childCount}");
        }
    }

    private void SpawnBossInRoom(Bounds roomBounds, GameObject roomObject)
    {
        if (bossEnemyPrefabs == null || bossEnemyPrefabs.Length == 0)
        {
            Debug.LogWarning("No boss prefabs assigned to the Enemy Generation System!");
            return;
        }

        // Find the enemies container for this room
        Transform enemiesContainer = roomObject.transform.Find("Enemies");
        if (enemiesContainer == null)
        {
            Debug.LogError($"Enemies container not found in room {roomObject.name}! Make sure SetupRoomControllers runs before SpawnBossInRoom.");
            return;
        }

        // Spawn boss in the center of the room
        Vector2 spawnPos = roomBounds.center;
        
        GameObject bossPrefab = bossEnemyPrefabs[UnityEngine.Random.Range(0, bossEnemyPrefabs.Length)];
        GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        boss.transform.SetParent(enemiesContainer); // ← FIX: Set parent to enemies container, not room
        
        if (showDebugGizmos)
        {
            Debug.Log($"Spawned boss {boss.name} in room {roomObject.name} at position {spawnPos}. Parent: {boss.transform.parent.name}");
        }
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
                // Create layermasks to check for valid floor & obstacles
                int obstacleLayer = LayerMask.GetMask("Obstacle", "Wall", "Collider");
                int floorLayer = LayerMask.GetMask("Floor", "Ground", "Default");
                
                // Check that we're not inside an obstacle
                bool hasObstacle = Physics2D.OverlapCircle(randomPos, 0.5f, obstacleLayer);
                
                if (!hasObstacle)
                {
                    // Additional check to ensure we're inside the room bounds
                    if (roomBounds.Contains(randomPos))
                    {
                        return randomPos;
                    }
                }
            }
        }
        
        // If we couldn't find a valid position after max attempts
        Debug.LogWarning("Could not find valid spawn position after " + MAX_ATTEMPTS + " attempts");
        // Return center of room as fallback (better than outside)
        return roomBounds.center;
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

    private void AddLevelExit(DungeonGeneratorLevelGrid2D level)
    {
        if (levelExitPrefab == null)
        {
            Debug.LogWarning("Level exit prefab not assigned in EnemyGenerationSystem!");
            return;
        }

        // Get the last room (furthest from the entrance)
        var lastRoom = level.RoomInstances.LastOrDefault();

        if (lastRoom == null)
        {
            Debug.LogError("No rooms found in the level!");
            return;
        }

        // Get the center position of the room
        var roomBounds = GetRoomBounds(lastRoom.RoomTemplateInstance);
        var roomCenter = roomBounds.center;

        // Instantiate the level exit at the room center
        var levelExit = Instantiate(levelExitPrefab, roomCenter, Quaternion.identity);

        // Make the level exit a child of the room
        levelExit.transform.parent = lastRoom.RoomTemplateInstance.transform;

        if (showDebugGizmos)
        {
            Debug.Log($"Level exit spawned in room at position: {roomCenter}");
        }
    }

    // Add visualization for debugging
    private void OnDrawGizmos()
    {
        if (showDebugGizmos && Application.isPlaying)
        {
            RoomInfo[] rooms = FindObjectsOfType<RoomInfo>();
            
            foreach (var room in rooms)
            {
                if (room != null)
                {
                    Bounds bounds = GetRoomBounds(room.gameObject);
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }
    }
} 