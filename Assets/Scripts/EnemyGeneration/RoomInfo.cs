using UnityEngine;

/// <summary>
/// Component to identify room types for the enemy generation system
/// </summary>
public class RoomInfo : MonoBehaviour
{
    public enum RoomType
    {
        Normal,
        Start,
        Boss,
        Treasure,
        Special
    }

    [SerializeField] private RoomType roomType = RoomType.Normal;
    
    [Header("Enemy Spawn Settings")]
    [SerializeField] private bool allowEnemySpawning = true;
    [SerializeField] private int minEnemies = 0;
    [SerializeField] private int maxEnemies = 0;
    [SerializeField] private bool overrideGlobalSettings = false;

    [Header("Special Room Settings")]
    [SerializeField] private bool isShop = false;
    [SerializeField] private bool isSecretRoom = false;

    public RoomType GetRoomType() => roomType;
    public bool AllowEnemySpawning() => allowEnemySpawning;
    public int GetMinEnemies() => minEnemies;
    public int GetMaxEnemies() => maxEnemies;
    public bool OverridesGlobalSettings() => overrideGlobalSettings;
    public bool IsShop() => isShop;
    public bool IsSecretRoom() => isSecretRoom;

    private void Awake()
    {
        // Set the tag based on room type
        switch (roomType)
        {
            case RoomType.Normal:
                gameObject.tag = "NormalRoom";
                break;
            case RoomType.Start:
                gameObject.tag = "StartRoom";
                break;
            case RoomType.Boss:
                gameObject.tag = "BossRoom";
                break;
            case RoomType.Treasure:
                gameObject.tag = "TreasureRoom";
                break;
            case RoomType.Special:
                gameObject.tag = "SpecialRoom";
                break;
        }
    }
} 