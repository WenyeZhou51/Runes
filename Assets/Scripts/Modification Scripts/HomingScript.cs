using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class HomingScript : MonoBehaviour {

    public float homingForce;
    public float homingRadius = 3f;  
    public string targetTag = "Enemy"; 
    private Transform target;        
    private Rigidbody2D rb;
    private float maxVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homingForce = rb.velocity.magnitude * 40;
        maxVelocity = rb.velocity.magnitude;
    }
    private void FixedUpdate()
    {
        if (target == null) {
            FindClosestEnemy();
        }
        else {
            Vector2 direction = (transform.position - target.position).normalized;
            Vector2 deltaVelocity = homingForce * direction*Time.fixedDeltaTime;
            rb.velocity -= deltaVelocity;
            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxVelocity);
        }
    
    }


    private void FindClosestEnemy()
    {
        // Find all GameObjects with the specified tag ("Enemy").
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(targetTag);

        // Initialize variables to keep track of the closest enemy and its distance.
        Transform closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        // Iterate through all enemies to find the closest one within homingRadius.
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= homingRadius && distance < closestDistance && !enemy.GetComponent<Enemy>().dying)
            {
                closestEnemy = enemy.transform;
                closestDistance = distance;
            }
        }

        // Set the closest enemy as the new target.
        target = closestEnemy;
    }


}
