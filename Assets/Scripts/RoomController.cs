using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RoomController : MonoBehaviour
{
    [Header("Door Settings")]
    public List<GameObject> doors = new List<GameObject>();
    public bool lockDoorsOnEnter = true;
    
    [Header("Enemy Detection")]
    public Transform enemiesContainer; // Reference to the "Enemies" GameObject
    public bool roomCleared = false;
    
    [Header("Player Detection")]
    public LayerMask playerLayer; // Set this to the layer your player is on
    public Vector2 roomTriggerSize = new Vector2(10f, 10f); // Size of the trigger area
    
    // Internal tracking
    private bool playerInRoom = false;
    private BoxCollider2D roomTrigger;
    private List<GameObject> activeEnemies = new List<GameObject>();
    
    private void Awake()
    {
        // Create a trigger collider for player detection if one doesn't exist
        roomTrigger = GetComponent<BoxCollider2D>();
        if (roomTrigger == null)
        {
            roomTrigger = gameObject.AddComponent<BoxCollider2D>();
            roomTrigger.isTrigger = true;
            roomTrigger.size = roomTriggerSize;
        }
        
        // Find the enemies container if not assigned
        if (enemiesContainer == null)
        {
            enemiesContainer = transform.Find("Enemies");
        }
    }
    
    private void Start()
    {
        // Find all doors if not manually assigned
        RefreshDoorsList();
        
        // Initially set all doors to unlocked
        SetDoorsLocked(false);
        
        // Check if we have enemies
        UpdateEnemiesList();
    }
    
    private void Update()
    {
        // Only check for enemies if player is in the room and doors are locked
        if (playerInRoom && !roomCleared)
        {
            CheckEnemies();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the player entered the room
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            playerInRoom = true;
            
            // Lock doors if there are enemies and we should lock doors
            if (lockDoorsOnEnter && HasEnemies())
            {
                SetDoorsLocked(true);
                Debug.Log("Player entered room with enemies. Doors locked!");
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Check if the player left the room
        if (((1 << collision.gameObject.layer) & playerLayer) != 0)
        {
            playerInRoom = false;
        }
    }
    
    private void UpdateEnemiesList()
    {
        activeEnemies.Clear();
        
        // Modern approach: Find enemies directly in the room by checking for enemy components
        // This works regardless of whether enemies are in a container or direct children
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf && IsEnemyGameObject(child.gameObject))
            {
                activeEnemies.Add(child.gameObject);
            }
        }
        
        // Fallback: Check the deprecated "Enemies" container approach for backward compatibility
        if (activeEnemies.Count == 0 && enemiesContainer != null)
        {
            foreach (Transform enemy in enemiesContainer)
            {
                if (enemy.gameObject.activeSelf && IsEnemyGameObject(enemy.gameObject))
                {
                    activeEnemies.Add(enemy.gameObject);
                }
            }
        }
        
        // Debug logging for enemy detection
        Debug.Log($"Room {gameObject.name}: Found {activeEnemies.Count} active enemies (Total children: {transform.childCount})");
        if (activeEnemies.Count > 0)
        {
            Debug.Log($"  Active enemies: {string.Join(", ", activeEnemies.ConvertAll(e => e.name))}");
        }
        
        // If there are no enemies, mark the room as cleared
        if (activeEnemies.Count == 0)
        {
            roomCleared = true;
        }
    }
    
    /// <summary>
    /// Check if a GameObject is an enemy by looking for enemy components
    /// </summary>
    private bool IsEnemyGameObject(GameObject obj)
    {
        // Check for any of the enemy component types
        return obj.GetComponent<Enemy>() != null || 
               obj.GetComponent<EnemyController>() != null || 
               obj.GetComponent<BasicEnemy>() != null ||
               obj.CompareTag("Enemy");
    }
    
    private bool HasEnemies()
    {
        UpdateEnemiesList();
        return activeEnemies.Count > 0;
    }
    
    private void CheckEnemies()
    {
        // Update our list of active enemies
        UpdateEnemiesList();
        
        // If all enemies are defeated, unlock the doors
        if (activeEnemies.Count == 0 && !roomCleared)
        {
            roomCleared = true;
            SetDoorsLocked(false);
            Debug.Log("All enemies defeated! Doors unlocked!");
        }
    }
    
    public void SetDoorsLocked(bool locked)
    {
        Debug.Log($"SetDoorsLocked({locked}) called for room {gameObject.name}. Found {doors.Count} doors.");
        
        foreach (var door in doors)
        {
            if (door != null)
            {
                // If the door has a custom door controller, use it
                DoorController doorController = door.GetComponent<DoorController>();
                if (doorController != null)
                {
                    Debug.Log($"  Door {door.name}: Using DoorController. Collider: {(doorController.doorCollider != null ? "Found" : "NULL")}");
                    doorController.SetLocked(locked);
                }
                else
                {
                    Debug.Log($"  Door {door.name}: No DoorController, using SetActive({locked})");
                    // Otherwise just enable/disable the door
                    door.SetActive(locked);
                }
            }
            else
            {
                Debug.LogWarning($"  Found NULL door in doors list!");
            }
        }
    }
    
    /// <summary>
    /// Refreshes the list of doors in this room. Call this after doors are created by the level generation system.
    /// </summary>
    public void RefreshDoorsList()
    {
        doors.Clear(); // Clear existing doors to refresh completely
        
        // Look for door objects in the room
        foreach (Transform child in transform)
        {
            Debug.Log($"Checking child: {child.name}, tag: {child.tag}");
            if (child.CompareTag("Door"))
            {
                doors.Add(child.gameObject);
                Debug.Log($"  Added door: {child.name}");
                
                // Check door components
                DoorController dc = child.GetComponent<DoorController>();
                Collider2D col = child.GetComponent<Collider2D>();
                Debug.Log($"  DoorController: {dc != null}, Collider2D: {col != null}");
                if (col != null)
                {
                    Debug.Log($"  Collider enabled: {col.enabled}, isTrigger: {col.isTrigger}");
                }
            }
        }
        
        Debug.Log($"RefreshDoorsList completed. Found {doors.Count} doors in room {gameObject.name}");
    }
} 