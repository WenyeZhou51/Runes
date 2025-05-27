using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float damage = 25f;
    public float speed = 10f;
    public float lifetime = 3f;
    
    [Header("Visual")]
    public Color projectileColor = Color.blue;
    
    private Vector2 direction;
    private float timer;
    
    private void Awake()
    {
        // Add a sprite renderer if one doesn't exist
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
            spriteRenderer.color = projectileColor;
        }
        
        // Add a collider if one doesn't exist
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = 0.2f;
            collider.isTrigger = true;
        }
        
        // Make sure this object has the PlayerProjectile tag
        if (gameObject.tag != "PlayerProjectile")
        {
            gameObject.tag = "PlayerProjectile";
        }
    }
    
    private void Start()
    {
        timer = lifetime;
    }
    
    public void Initialize(Vector2 direction)
    {
        this.direction = direction.normalized;
    }
    
    private void Update()
    {
        // Move in the set direction
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Decrease lifetime
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy on collision with walls or other obstacles
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
        
        // Note: We don't destroy on enemy collision here
        // The EnemyController handles that to ensure damage is applied first
    }
} 