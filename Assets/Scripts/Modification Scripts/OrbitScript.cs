using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitScript : PathModifier
{
    private GameObject player;
    private Rigidbody2D rb;
    private float orbitSpeed = 150f; // Degrees per second
    private float orbitDistance = 3f; // Distance from player
    private float initialAngle;
    private bool initialized = false;
    private float creationTime;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
        creationTime = Time.time;
        
        // Check if there's already a path modifier
        if (HasPathModifier(gameObject))
        {
            // If this is not the first path modifier, remove this component
            PathModifier existingModifier = gameObject.GetComponent<PathModifier>();
            if (existingModifier != this)
            {
                Debug.Log("Rejected OrbitScript: Another path modifier already exists");
                Destroy(this);
                return;
            }
        }
    }

    private void Start()
    {
        // If this component was destroyed in Awake, don't proceed
        if (this == null) return;
        
        // Store the initial velocity magnitude before changing it
        float initialSpeed = rb.velocity.magnitude;
        
        // Calculate the initial angle based on the projectile's position relative to the player
        Vector2 dirToPlayer = (Vector2)transform.position - (Vector2)player.transform.position;
        initialAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        
        // Disable original velocity
        rb.velocity = Vector2.zero;
        
        // Adjust orbit speed based on the initial projectile speed
        orbitSpeed = Mathf.Max(150f, initialSpeed * 20f);
        
        initialized = true;
        
        // Get component references
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            // Increase duration for orbiting projectiles
            bullet.duration *= 1.5f;
        }
    }

    private void FixedUpdate()
    {
        if (!initialized || player == null) return;
        
        // Calculate the current angle based on time
        float currentAngle = initialAngle + orbitSpeed * (Time.time - creationTime);
        
        // Convert angle to radians
        float angleRad = currentAngle * Mathf.Deg2Rad;
        
        // Calculate the new position
        Vector2 offset = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * orbitDistance;
        Vector2 newPosition = (Vector2)player.transform.position + offset;
        
        // Update position
        rb.MovePosition(newPosition);
        
        // Make the projectile rotate to face the orbit direction
        float rotationAngle = currentAngle + 90f; // +90 to make it perpendicular to orbit path
        transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
    }
} 