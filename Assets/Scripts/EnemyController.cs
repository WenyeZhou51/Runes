using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float health = 100f;
    public float moveSpeed = 2f;
    
    [Header("Visual")]
    public Color normalColor = Color.red;
    public Color damagedColor = Color.white;
    public float flashDuration = 0.1f;
    public GameObject PopUp;  // Add damage popup reference
    
    private SpriteRenderer spriteRenderer;
    private bool isDead = false;
    private float flashTimer = 0f;
    
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                // Create a sprite renderer if one doesn't exist
                GameObject visual = new GameObject("Visual");
                visual.transform.parent = transform;
                visual.transform.localPosition = Vector3.zero;
                
                spriteRenderer = visual.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
                spriteRenderer.color = normalColor;
            }
        }
        
        // Add a collider if one doesn't exist
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;
            collider.isTrigger = true;  // Set as trigger to work with OnTriggerEnter2D
        }
    }
    
    private void Update()
    {
        // Handle flash effect
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
            {
                spriteRenderer.color = normalColor;
            }
        }
        
        // Simple AI could be added here
        // For now, we'll just make the enemy move randomly
        if (!isDead && Random.value < 0.01f)
        {
            Vector2 randomDirection = Random.insideUnitCircle;
            transform.position += new Vector3(randomDirection.x, randomDirection.y, 0) * moveSpeed * Time.deltaTime;
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        health -= damage;
        
        // Create damage popup if PopUp prefab is assigned
        if (PopUp != null)
        {
            GameObject popUp = Instantiate(PopUp);
            popUp.GetComponent<DamagePopUp>().damageNum = (int)damage;
            popUp.GetComponent<RectTransform>().position = this.transform.position + 0.5f * Vector3.up;
        }
        
        // Flash effect
        spriteRenderer.color = damagedColor;
        flashTimer = flashDuration;
        
        // Check if dead
        if (health <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isDead = true;
        
        // Play death animation or effect here
        
        // Disable the enemy
        gameObject.SetActive(false);
        
        // Could also destroy after a delay
        // Destroy(gameObject, 1f);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for projectiles or player attacks
        if (collision.CompareTag("PlayerProjectile"))
        {
            // Handle both Bullet component (for older bullet system) and ProjectileController (for newer system)
            Bullet bulletComponent = collision.gameObject.GetComponent<Bullet>();
            ProjectileController projectileComponent = collision.gameObject.GetComponent<ProjectileController>();
            
            if (bulletComponent != null) {
                TakeDamage(bulletComponent.damage);
            } else if (projectileComponent != null) {
                TakeDamage(projectileComponent.damage);
            }
            
            // Destroy the projectile
            Destroy(collision.gameObject);
        }
    }
} 