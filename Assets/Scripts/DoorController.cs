using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool startLocked = false;
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    
    [Header("Door Sprite")]
    [Tooltip("Assign a custom door sprite. If left empty, a white square will be used as default.")]
    public Sprite customDoorSprite;
    
    [Header("Components")]
    public SpriteRenderer doorSprite;
    public Collider2D doorCollider;
    
    private bool isLocked;
    private static Sprite defaultWhiteSprite;
    
    private void Awake()
    {
        // Get components if not assigned
        if (doorSprite == null)
        {
            doorSprite = GetComponent<SpriteRenderer>();
            if (doorSprite == null)
            {
                doorSprite = GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        if (doorCollider == null)
        {
            doorCollider = GetComponent<Collider2D>();
        }
        
        // Ensure we have a sprite renderer
        if (doorSprite == null)
        {
            doorSprite = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set up the door sprite
        SetupDoorSprite();
    }
    
    private void Start()
    {
        // Set initial state
        SetLocked(startLocked);
        
        // Make sure this object has the Door tag
        if (gameObject.tag != "Door")
        {
            gameObject.tag = "Door";
        }
    }
    
    private void SetupDoorSprite()
    {
        if (doorSprite == null) return;
        
        // Use custom sprite if assigned, otherwise create/use default white sprite
        if (customDoorSprite != null)
        {
            doorSprite.sprite = customDoorSprite;
        }
        else
        {
            doorSprite.sprite = GetDefaultWhiteSprite();
        }
    }
    
    private static Sprite GetDefaultWhiteSprite()
    {
        // Create a default white sprite if it doesn't exist
        if (defaultWhiteSprite == null)
        {
            // Try to get Unity's built-in sprite first
            defaultWhiteSprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
            
            // If that fails, create a simple white texture
            if (defaultWhiteSprite == null)
            {
                Texture2D whiteTexture = new Texture2D(32, 32);
                Color[] pixels = new Color[32 * 32];
                for (int i = 0; i < pixels.Length; i++)
                {
                    pixels[i] = Color.white;
                }
                whiteTexture.SetPixels(pixels);
                whiteTexture.Apply();
                
                defaultWhiteSprite = Sprite.Create(whiteTexture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
                defaultWhiteSprite.name = "Default Door Sprite";
            }
        }
        
        return defaultWhiteSprite;
    }
    
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        
        Debug.Log($"DoorController.SetLocked({locked}) called on {gameObject.name}");
        
        // Update the collider
        if (doorCollider != null)
        {
            doorCollider.enabled = locked;
            Debug.Log($"  Collider enabled: {doorCollider.enabled}, isTrigger: {doorCollider.isTrigger}");
        }
        else
        {
            Debug.LogWarning($"  doorCollider is NULL! Cannot enable/disable door collision.");
        }
        
        // Update the visual appearance
        if (doorSprite != null)
        {
            doorSprite.color = locked ? lockedColor : unlockedColor;
            Debug.Log($"  Sprite color changed to: {(locked ? "locked" : "unlocked")} color");
        }
        else
        {
            Debug.LogWarning($"  doorSprite is NULL! Cannot update door appearance.");
        }
    }
    
    public bool IsLocked()
    {
        return isLocked;
    }
    
    /// <summary>
    /// Call this method to update the door sprite if you change the customDoorSprite at runtime
    /// </summary>
    public void RefreshSprite()
    {
        SetupDoorSprite();
    }
    
#if UNITY_EDITOR
    [ContextMenu("Setup Door with Default Settings")]
    private void SetupDoorWithDefaults()
    {
        // This context menu option helps users set up doors properly
        if (doorSprite == null)
        {
            doorSprite = GetComponent<SpriteRenderer>();
            if (doorSprite == null)
            {
                doorSprite = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        if (doorCollider == null)
        {
            doorCollider = GetComponent<BoxCollider2D>();
            if (doorCollider == null)
            {
                BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
                boxCollider.size = new Vector2(1, 1);
                boxCollider.enabled = false;
                doorCollider = boxCollider;
            }
        }
        
        SetupDoorSprite();
        gameObject.tag = "Door";
        
        Debug.Log("Door setup complete! You can now assign a custom sprite in the 'Door Sprite' section.");
    }
    
    [ContextMenu("Find Available Door Sprites")]
    private void FindAvailableDoorSprites()
    {
        Debug.Log("=== Available Door Sprites in Project ===");
        Debug.Log("Look for door sprites in:");
        Debug.Log("• Assets/Brackeys/2D Mega Pack/Environment/Gothic/Door.png");
        Debug.Log("• Assets/Brackeys/2D Mega Pack/Environment/Gothic/DoorUp.png");
        Debug.Log("• Assets/Brackeys/2D Mega Pack/Items & Icons/Arcade/BlueDoor.png");
        Debug.Log("• Assets/Brackeys/2D Mega Pack/Items & Icons/Arcade/GreenDoor.png");
        Debug.Log("Drag any of these sprites to the 'Custom Door Sprite' field to use them!");
    }
#endif
} 