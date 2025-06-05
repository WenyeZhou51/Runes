using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float health = 100f;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public int damage = 10;
    public float attackCooldown = 1f;
    
    [Header("References")]
    public GameObject attackEffect;
    public GameObject PopUp;  // Add damage popup reference
    
    private Transform player;
    private float lastAttackTime;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Find the player
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        
        // Add a collider if missing
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
        }
    }
    
    private void Update()
    {
        if (player == null)
        {
            // Try to find player again
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            return;
        }
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // If player is within detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Move towards player if not in attack range
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                // Attack player if cooldown is over
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    AttackPlayer();
                }
            }
        }
    }
    
    private void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;
        
        // Flip sprite based on movement direction
        if (direction.x < 0)
        {
            spriteRenderer.flipX = true;
        }
        else if (direction.x > 0)
        {
            spriteRenderer.flipX = false;
        }
    }
    
    private void AttackPlayer()
    {
        // Set last attack time
        lastAttackTime = Time.time;
        
        // Show attack effect if available
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position, Quaternion.identity);
        }
        
        // Get player controller and deal damage
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.TakeDamage(damage);
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        
        // Create damage popup if PopUp prefab is assigned
        if (PopUp != null)
        {
            GameObject popUp = Instantiate(PopUp);
            popUp.GetComponent<DamagePopUp>().damageNum = (int)damageAmount;
            popUp.GetComponent<RectTransform>().position = this.transform.position + 0.5f * Vector3.up;
        }
        
        // Flash red when hit
        StartCoroutine(FlashRed());
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    
    private void Die()
    {
        // Add death effects here if needed
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for player projectiles
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
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
