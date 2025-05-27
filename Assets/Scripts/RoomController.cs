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
        
        // Only track enemies if we have an enemies container
        if (enemiesContainer != null)
        {
            foreach (Transform enemy in enemiesContainer)
            {
                if (enemy.gameObject.activeSelf)
                {
                    activeEnemies.Add(enemy.gameObject);
                }
            }
        }
        
        // If there are no enemies, mark the room as cleared
        if (activeEnemies.Count == 0)
        {
            roomCleared = true;
        }
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
        foreach (var door in doors)
        {
            if (door != null)
            {
                // If the door has a custom door controller, use it
                DoorController doorController = door.GetComponent<DoorController>();
                if (doorController != null)
                {
                    doorController.SetLocked(locked);
                }
                else
                {
                    // Otherwise just enable/disable the door
                    door.SetActive(locked);
                }
            }
        }
    }
    
    /// <summary>
    /// Refreshes the list of doors in this room. Call this after doors are created by the level generation system.
    /// </summary>
    public void RefreshDoorsList()
    {
        if (doors.Count == 0)
        {
            // Look for door objects in the room
            foreach (Transform child in transform)
            {
                if (child.CompareTag("Door"))
                {
                    doors.Add(child.gameObject);
                }
            }
        }
    }
} 