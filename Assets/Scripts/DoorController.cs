using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public bool startLocked = false;
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;
    
    [Header("Components")]
    public SpriteRenderer doorSprite;
    public Collider2D doorCollider;
    
    private bool isLocked;
    
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
    
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        
        // Update the collider
        if (doorCollider != null)
        {
            doorCollider.enabled = locked;
        }
        
        // Update the visual appearance
        if (doorSprite != null)
        {
            doorSprite.color = locked ? lockedColor : unlockedColor;
        }
    }
    
    public bool IsLocked()
    {
        return isLocked;
    }
} 