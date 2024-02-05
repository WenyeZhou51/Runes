using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class onImpactScript: MonoBehaviour
{

    private Rigidbody2D rb;
    private GameObject player;
    private Bullet bullet;
    private Vector2 previousPosition;

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        bullet = GetComponent<Bullet>();
        //rb.velocity = Vector3.zero; //for testing
        
    }


    //private void OnTriggerEnter2D(Collider2D collider)
    //{
    //    if (collider.gameObject.CompareTag("Enemy") || collider.gameObject.CompareTag("Wall"))
    //    {
    //        // Approximate normal: direction from the center of the collider to the projectile
    //        Vector2 directionFromColliderToProjectile = transform.position - collider.bounds.center;
    //        Vector2 approximateNormal = directionFromColliderToProjectile.normalized;

    //        // Reflect the projectile's direction
    //        Vector2 travelDirection = rb.velocity.normalized; // Assuming rb.velocity is the travel direction
    //        Vector2 reflectedDirection = Vector2.Reflect(travelDirection, approximateNormal);

    //        // Calculate new rotation
    //        float rotationZ = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
    //        Quaternion correctRotation = Quaternion.Euler(0f, 0f, rotationZ);

    //        // Spawn position for the new projectile
    //        float spawnOffsetDistance = 0.5f;
    //        Vector2 spawnPosition = transform.position + (Vector3)(reflectedDirection.normalized * spawnOffsetDistance);

    //        // Spawn the new projectile
    //        player.GetComponent<PlayerController>().Attack(spawnPosition, correctRotation, true);
    //    }
    //}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Wall"))
        {
            // Calculate the direction and length for the raycast
            Vector2 directionToCurrentPosition = (Vector2)transform.position - previousPosition;
            float distanceToCurrentPosition = directionToCurrentPosition.magnitude;

            // Cast a ray from the previous position to the current position
            RaycastHit2D hit = Physics2D.Raycast(previousPosition, directionToCurrentPosition.normalized, distanceToCurrentPosition);

            if (hit.collider != null)
            {
                // Use the hit point and normal for more accurate reflection
                Vector2 contactPoint = hit.point;
                Vector2 contactNormal = hit.normal;
                Vector2 travelDirection = directionToCurrentPosition.normalized; // Use the actual direction of movement
                Vector2 reflectedDirection = Vector2.Reflect(travelDirection, contactNormal);

                float rotationZ = Mathf.Atan2(reflectedDirection.y, reflectedDirection.x) * Mathf.Rad2Deg;
                Quaternion correctRotation = Quaternion.Euler(0f, 0f, rotationZ);

                // Adjust spawn position calculation
                float spawnOffsetDistance = 1f; // This distance may need to be adjusted based on your game's scale
                Vector2 spawnPosition = contactPoint + reflectedDirection.normalized * spawnOffsetDistance;

                // Ensure the new projectile spawns slightly away from the collision point to prevent immediate recollision
                spawnPosition += contactNormal * 0.1f;

                // Spawn the new projectile
                player.GetComponent<PlayerController>().Attack(spawnPosition, correctRotation, true);
            }
        }
    }

    void Update()
    {
        // Update previousPosition at the end of each frame
        previousPosition = transform.position;
    }









}




