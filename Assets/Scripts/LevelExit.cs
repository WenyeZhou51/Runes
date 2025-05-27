using UnityEngine;

public class LevelExit : MonoBehaviour
{
    private void Start()
    {
        // Make sure this object has the LevelExit tag
        if (gameObject.tag != "LevelExit")
        {
            gameObject.tag = "LevelExit";
        }
        
        // Make sure this object has a collider
        if (GetComponent<Collider2D>() == null)
        {
            // Add a trigger collider
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1f, 1f);
        }
    }
} 