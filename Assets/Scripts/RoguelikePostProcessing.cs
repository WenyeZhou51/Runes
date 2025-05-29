using UnityEngine;
using Edgar.Unity;
using System.Linq;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Game/Roguelike Post Processing", fileName = "RoguelikePostProcessing")]
public class RoguelikePostProcessing : DungeonGeneratorPostProcessingGrid2D
{
    // Prefab for the level exit
    public GameObject levelExitPrefab;
    
    [Header("Enemy Spawning")]
    // Chance to spawn enemies in each room (0-1)
    [Range(0, 1)]
    public float enemySpawnChance = 0.7f;
    
    // List of enemy prefabs that can be spawned
    public List<EnemySpawnData> enemyPrefabs = new List<EnemySpawnData>();
    
    // Total number of enemies to spawn per room (min/max)
    public Vector2Int enemiesPerRoom = new Vector2Int(2, 5);
    
    [Header("Enemy Placement")]
    // Margin from walls for enemy spawning
    public float enemySpawnMargin = 1f;
    
    // Whether to use predefined enemy positions or generate random positions
    public bool useRandomEnemyPositions = true;
    
    [Header("Room Control")]
    // Chance for a room to have enemies (0-1)
    [Range(0, 1)]
    public float roomWithEnemiesChance = 0.7f;
    
    // Whether to skip enemies in the first room
    public bool skipEnemiesInFirstRoom = true;
    
    // Whether to skip enemies in the last room
    public bool skipEnemiesInLastRoom = true;
    
    public override void Run(DungeonGeneratorLevelGrid2D level)
    {
        // Setup room controllers first
        SetupRoomControllers(level);
        
        // Handle enemy spawning
        if (useRandomEnemyPositions)
        {
            HandleRandomEnemyGeneration(level);
        }
        else
        {
            HandlePredefinedEnemies(level);
        }
        
        // Add level exit to the last room
        AddLevelExit(level);
    }
    
    private void SetupRoomControllers(DungeonGeneratorLevelGrid2D level)
    {
        int roomIndex = 0;
        int totalRooms = level.RoomInstances.Count;
        
        foreach (var roomInstance in level.RoomInstances)
        {
            bool isFirstRoom = roomIndex == 0;
            bool isLastRoom = roomIndex == totalRooms - 1;
            roomIndex++;
            
            // Add a RoomController to each room
            RoomController roomController = roomInstance.RoomTemplateInstance.AddComponent<RoomController>();
            
            // Configure the room controller
            roomController.lockDoorsOnEnter = true;
            
            // Set the player layer - assuming player is on layer 8
            roomController.playerLayer = 1 << 8;
            
            // Set the room trigger size based on the room bounds
            var roomBounds = GetRoomBounds(roomInstance);
            roomController.roomTriggerSize = new Vector2(
                roomBounds.size.x * 0.8f, 
                roomBounds.size.y * 0.8f
            );
            
            // Find doors in the room
            foreach (Transform child in roomInstance.RoomTemplateInstance.transform)
            {
                // Look for doors (objects with DoorGrid2D component)
                DoorGrid2D doorGrid = child.GetComponent<DoorGrid2D>();
                if (doorGrid != null)
                {
                    // Create a door game object at this position
                    GameObject doorObj = new GameObject("Door");
                    doorObj.transform.parent = roomInstance.RoomTemplateInstance.transform;
                    doorObj.transform.position = child.position;
                    doorObj.tag = "Door";
                    
                    // Add door components
                    SpriteRenderer doorSprite = doorObj.AddComponent<SpriteRenderer>();
                    doorSprite.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
                    doorSprite.color = Color.green;
                    
                    // Calculate door length from the From and To positions
                    float doorLength = Vector3.Distance(doorGrid.From, doorGrid.To);
                    
                    BoxCollider2D doorCollider = doorObj.AddComponent<BoxCollider2D>();
                    doorCollider.size = new Vector2(doorLength, 1);
                    doorCollider.enabled = false;
                    
                    DoorController doorController = doorObj.AddComponent<DoorController>();
                    doorController.doorSprite = doorSprite;
                    doorController.doorCollider = doorCollider;
                    
                    // Add this door to the room controller
                    roomController.doors.Add(doorObj);
                }
            }
        }
    }
    
    private void HandlePredefinedEnemies(DungeonGeneratorLevelGrid2D level)
    {
        int roomIndex = 0;
        int totalRooms = level.RoomInstances.Count;
        
        // Iterate through all the rooms
        foreach (var roomInstance in level.RoomInstances)
        {
            bool isFirstRoom = roomIndex == 0;
            bool isLastRoom = roomIndex == totalRooms - 1;
            roomIndex++;
            
            // Skip enemies in first/last room if configured to do so
            if ((skipEnemiesInFirstRoom && isFirstRoom) || (skipEnemiesInLastRoom && isLastRoom))
            {
                continue;
            }
            
            // Get the transform that holds all the enemies
            var enemiesHolder = roomInstance.RoomTemplateInstance.transform.Find("Enemies");
            
            // Skip this room if there are no enemies
            if (enemiesHolder == null)
            {
                continue;
            }
            
            // Decide if this room should have enemies
            bool hasEnemies = Random.NextDouble() < roomWithEnemiesChance;
            if (!hasEnemies)
            {
                // Disable all enemies in this room
                foreach (Transform enemyTransform in enemiesHolder)
                {
                    enemyTransform.gameObject.SetActive(false);
                }
                continue;
            }
            
            // Iterate through all enemies (children of the enemiesHolder)
            foreach (Transform enemyTransform in enemiesHolder)
            {
                var enemy = enemyTransform.gameObject;
                
                // Roll a dice and check whether to spawn this enemy or not
                if (Random.NextDouble() < enemySpawnChance)
                {
                    enemy.SetActive(true);
                }
                else
                {
                    enemy.SetActive(false);
                }
            }
        }
    }
    
    private void HandleRandomEnemyGeneration(DungeonGeneratorLevelGrid2D level)
    {
        // Check if we have enemy prefabs to spawn
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("No enemy prefabs assigned to RoguelikePostProcessing!");
            return;
        }
        
        // Calculate total weight for weighted random selection
        float totalWeight = enemyPrefabs.Sum(e => e.spawnWeight);
        
        int roomIndex = 0;
        int totalRooms = level.RoomInstances.Count;
        
        // Iterate through all the rooms
        foreach (var roomInstance in level.RoomInstances)
        {
            bool isFirstRoom = roomIndex == 0;
            bool isLastRoom = roomIndex == totalRooms - 1;
            roomIndex++;
            
            // Skip enemies in first/last room if configured to do so
            if ((skipEnemiesInFirstRoom && isFirstRoom) || (skipEnemiesInLastRoom && isLastRoom))
            {
                continue;
            }
            
            // Decide if this room should have enemies
            bool hasEnemies = Random.NextDouble() < roomWithEnemiesChance;
            if (!hasEnemies)
            {
                continue;
            }
            
            // Get the room bounds
            var roomBounds = GetRoomBounds(roomInstance);
            
            // Skip if room is too small
            if (roomBounds.size.x <= enemySpawnMargin * 2 || roomBounds.size.y <= enemySpawnMargin * 2)
            {
                continue;
            }
            
            // Create an enemies holder if it doesn't exist
            Transform enemiesHolder = roomInstance.RoomTemplateInstance.transform.Find("Enemies");
            if (enemiesHolder == null)
            {
                enemiesHolder = new GameObject("Enemies").transform;
                enemiesHolder.SetParent(roomInstance.RoomTemplateInstance.transform);
                enemiesHolder.localPosition = Vector3.zero;
            }
            else
            {
                // Clear existing enemies if using random positions
                foreach (Transform child in enemiesHolder)
                {
                    Object.Destroy(child.gameObject);
                }
            }
            
            // Get the room controller
            RoomController roomController = roomInstance.RoomTemplateInstance.GetComponent<RoomController>();
            if (roomController != null)
            {
                roomController.enemiesContainer = enemiesHolder;
            }
            
            // Determine how many enemies to spawn in this room
            int enemiesToSpawn = UnityEngine.Random.Range(enemiesPerRoom.x, enemiesPerRoom.y + 1);
            
            // Spawn enemies
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                // Check if we should spawn an enemy based on the spawn chance
                if (Random.NextDouble() < enemySpawnChance)
                {
                    // Select a random enemy prefab based on weights
                    GameObject selectedEnemyPrefab = SelectRandomEnemyPrefab(totalWeight);
                    
                    if (selectedEnemyPrefab != null)
                    {
                        // Get a random position within the room bounds
                        Vector3 spawnPosition = GetRandomPositionInRoom(roomBounds);
                        
                        // Instantiate the enemy
                        GameObject enemy = Instantiate(selectedEnemyPrefab, spawnPosition, Quaternion.identity);
                        enemy.transform.SetParent(enemiesHolder);
                    }
                }
            }
        }
    }
    
    private GameObject SelectRandomEnemyPrefab(float totalWeight)
    {
        // Select a random enemy prefab based on weights
        float randomValue = (float)Random.NextDouble() * totalWeight;
        float weightSum = 0;
        
        foreach (var enemyData in enemyPrefabs)
        {
            weightSum += enemyData.spawnWeight;
            if (randomValue <= weightSum)
            {
                return enemyData.enemyPrefab;
            }
        }
        
        // Fallback to the first enemy if something went wrong
        return enemyPrefabs.Count > 0 ? enemyPrefabs[0].enemyPrefab : null;
    }
    
    private Bounds GetRoomBounds(RoomInstanceGrid2D roomInstance)
    {
        // Get the room template's bounds
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        
        // Find the Tilemaps GameObject
        Transform tilemapsRoot = roomInstance.RoomTemplateInstance.transform.Find("Tilemaps");
        if (tilemapsRoot != null)
        {
            // Look for floor tilemap to determine bounds
            var floorTilemap = tilemapsRoot.GetComponentsInChildren<UnityEngine.Tilemaps.Tilemap>()
                .FirstOrDefault(t => t.gameObject.name.ToLower().Contains("floor"));
                
            if (floorTilemap != null)
            {
                bounds = floorTilemap.localBounds;
                bounds.center += (Vector3)roomInstance.RoomTemplateInstance.transform.position;
                boundsInitialized = true;
            }
        }
        
        // If we couldn't find the floor tilemap, use a default size
        if (!boundsInitialized)
        {
            bounds = new Bounds(roomInstance.RoomTemplateInstance.transform.position, new Vector3(10, 10, 0));
        }
        
        return bounds;
    }
    
    private Vector3 GetRandomPositionInRoom(Bounds roomBounds)
    {
        // Calculate the area where enemies can spawn (with margin)
        float minX = roomBounds.min.x + enemySpawnMargin;
        float maxX = roomBounds.max.x - enemySpawnMargin;
        float minY = roomBounds.min.y + enemySpawnMargin;
        float maxY = roomBounds.max.y - enemySpawnMargin;
        
        const int MAX_ATTEMPTS = 30;
        
        for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
        {
            // Get a random position within the bounds
            float x = Mathf.Lerp(minX, maxX, (float)Random.NextDouble());
            float y = Mathf.Lerp(minY, maxY, (float)Random.NextDouble());
            Vector3 position = new Vector3(x, y, 0);
            
            // Check for obstacles
            int obstacleLayer = LayerMask.GetMask("Obstacle", "Wall", "Collider");
            bool hasObstacle = Physics2D.OverlapCircle(position, 0.5f, obstacleLayer);
            
            if (!hasObstacle && roomBounds.Contains(position))
            {
                return position;
            }
        }
        
        // Fallback to center of room if no valid position found
        Debug.LogWarning("Could not find valid enemy position in room after " + MAX_ATTEMPTS + " attempts");
        return roomBounds.center;
    }
    
    private void AddLevelExit(DungeonGeneratorLevelGrid2D level)
    {
        if (levelExitPrefab == null)
        {
            Debug.LogError("Level exit prefab not assigned in RoguelikePostProcessing!");
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
        var roomCenter = lastRoom.RoomTemplateInstance.transform.position;
        
        // Instantiate the level exit at the room center
        var levelExit = Instantiate(levelExitPrefab, roomCenter, Quaternion.identity);
        
        // Make the level exit a child of the room
        levelExit.transform.parent = lastRoom.RoomTemplateInstance.transform;
    }
}

// Class to hold data about enemy spawning
[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    [Range(0, 100)]
    public float spawnWeight = 1f;
    
    [Tooltip("Optional name for the inspector")]
    public string enemyName;
} 