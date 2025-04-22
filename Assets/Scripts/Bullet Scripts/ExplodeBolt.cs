using UnityEngine;
using System.Collections;

public class ExplodeBolt : Bullet
{
    [Header("Explosion Settings")]
    public float explosionRadius = 2f;
    public int explosionDamage = 5;
    public float explosionForce = 5f;

    [Header("Explosion Visual")]
    [SerializeField] private Sprite explosionCircleSprite; // Assign your circle sprite in the inspector

    private Rigidbody2D rb;
    private float bulletSpeed = 13f;
    private Vector2 previousPosition;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        previousPosition = transform.position;

        Vector2 initialVelocity = transform.right * bulletSpeed;
        rb.velocity = initialVelocity;
    }

    void Update()
    {
        previousPosition = transform.position;

        // Destroy if it exceeds duration
        if (Time.time - creationTime > duration)
        {
            Destroy(gameObject);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"ExplodeBolt collision with: {collision.gameObject.name}, CollisionEnabled: {collisionEnabled}, Enemy={collision.gameObject.CompareTag("Enemy")}, Wall={collision.gameObject.CompareTag("Wall")}");

        // Respect collisionEnabled from base collision
        base.OnCollisionEnter2D(collision);

        // Check if collision is valid (enemy, wall, or tilemap)
        if (collisionEnabled &&
            (collision.gameObject.CompareTag("Enemy") ||
             collision.gameObject.CompareTag("Wall") ||
             collision.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null))
        {
            Debug.Log("Attempting to explode");
            Explode();
        }
    }

    private void Explode()
    {
        Debug.Log($"Explode() called at position: {transform.position}");

        // Create a quick red-tinted sprite for visual feedback
        GameObject explosionEffect = new GameObject("ExplosionEffect");
        explosionEffect.transform.position = transform.position;
        SpriteRenderer sr = explosionEffect.AddComponent<SpriteRenderer>();
        sr.sprite = explosionCircleSprite;
        sr.color = new Color(1f, 0f, 0f, 0.3f);
        sr.sortingOrder = 9999; // Ensure overlay is on top

        // Scale the circle sprite to match the explosion radius
        if (sr.sprite != null)
        {
            float diameter = explosionRadius * 2f;
            float spriteWidth = sr.sprite.bounds.size.x;
            float spriteHeight = sr.sprite.bounds.size.y;
            explosionEffect.transform.localScale = new Vector3(
                diameter / spriteWidth,
                diameter / spriteHeight,
                1f
            );
        }

        // Remove explosion effect after a short delay
        Destroy(explosionEffect, 0.2f);

        // Overlap circle check for all objects in blast radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        Debug.Log($"Found {colliders.Length} colliders in explosion radius");

        foreach (Collider2D col in colliders)
        {
            Debug.Log($"Processing collider: {col.gameObject.name}, Tags: Enemy={col.CompareTag("Enemy")}, Player={col.CompareTag("Player")}");

            // Apply damage
            if (col.CompareTag("Enemy"))
            {
                var enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    Debug.Log($"Applying {explosionDamage} damage to enemy");
                    enemy.takeDamage(explosionDamage);
                }
            }
            else if (col.CompareTag("Player"))
            {
                var player = col.GetComponent<PlayerController>();
                if (player != null)
                {
                    Debug.Log($"Applying {explosionDamage} damage to player");
                    player.TakeDamage(explosionDamage);
                }
            }

            // Apply impulse force to push away
            var targetRb = col.GetComponent<Rigidbody2D>();
            if (targetRb != null)
            {
                Vector2 direction = (targetRb.transform.position - transform.position).normalized;
                Debug.Log($"Applying force to {col.gameObject.name} in direction {direction}");
                targetRb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }
        }

        Debug.Log("Destroying ExplodeBolt");
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}