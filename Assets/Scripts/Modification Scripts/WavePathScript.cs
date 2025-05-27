using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WavePathScript : PathModifier
{
    private GameObject player;
    private Rigidbody2D rb;
    private float waveFrequency = 5.0f; // Wave cycles per second
    private float waveAmplitude = 1.5f; // Height of the wave
    private float forwardSpeed = 8f; // Forward movement speed
    private bool initialized = false;
    private float creationTime;
    private Vector2 initialDirection;
    private Vector2 initialPosition;
    private float initialDistance;

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
                Debug.Log("Rejected WavePathScript: Another path modifier already exists");
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
        
        // Store initial direction and position
        initialDirection = rb.velocity.normalized;
        initialPosition = transform.position;
        
        // Calculate initial distance from player
        initialDistance = Vector2.Distance(transform.position, player.transform.position);
        
        // Disable original velocity
        rb.velocity = Vector2.zero;
        
        // Adjust forward speed based on the initial projectile speed
        forwardSpeed = Mathf.Max(8f, initialSpeed);
        
        initialized = true;
        
        // Get component references
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            // Increase duration for wave path projectiles
            bullet.duration *= 1.2f;
        }
    }

    private void FixedUpdate()
    {
        if (!initialized || player == null) return;
        
        // Time since creation
        float timeSinceCreation = Time.time - creationTime;
        
        // Calculate forward distance based on time and speed
        float forwardDistance = forwardSpeed * timeSinceCreation;
        
        // Calculate perpendicular direction (90 degrees to initial direction)
        Vector2 perpendicularDir = new Vector2(-initialDirection.y, initialDirection.x);
        
        // Calculate wave offset using sine function
        float waveOffset = Mathf.Sin(timeSinceCreation * waveFrequency * Mathf.PI * 2) * waveAmplitude;
        
        // Calculate the new position: start position + forward movement + wave offset
        Vector2 newPosition = initialPosition + (initialDirection * forwardDistance) + (perpendicularDir * waveOffset);
        
        // Update position
        rb.MovePosition(newPosition);
        
        // Calculate tangent angle to make projectile face the direction of movement
        float waveDerivative = Mathf.Cos(timeSinceCreation * waveFrequency * Mathf.PI * 2) * waveAmplitude * waveFrequency * Mathf.PI * 2;
        float tangentAngle = Mathf.Atan2(
            initialDirection.y + perpendicularDir.y * waveDerivative,
            initialDirection.x + perpendicularDir.x * waveDerivative
        ) * Mathf.Rad2Deg;
        
        // Apply rotation
        transform.rotation = Quaternion.Euler(0, 0, tangentAngle);
    }
} 