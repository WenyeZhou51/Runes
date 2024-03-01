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

    private void Awake()
    {
        
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");
        bullet = GetComponent<Bullet>();
        //rb.velocity = Vector3.zero; //for testing
        
    }

    public Vector2 ReverseVector(Vector2 vector)
    {
        return new Vector2(-vector.x, -vector.y);
    }

    // Function to reverse a 3D vector
    public Vector3 ReverseVector(Vector3 vector)
    {
        return new Vector3(-vector.x, -vector.y, -vector.z);
    }
    public Vector2 RandomDirection(float deviation)
    {
        // Generate a random angle within the deviation range
        float angle = UnityEngine.Random.Range(-deviation, deviation);

        // Convert the angle to radians
        angle *= Mathf.Deg2Rad;

        // Calculate the x and y components of the direction using trigonometry
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);

        // Return the normalized Vector2 direction
        return new Vector2(x, y).normalized;
    }
    public Quaternion Vector2ToQuaternion(Vector2 direction)
    {
        // Calculate the angle in radians using Mathf.Atan2
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Create a Quaternion with the calculated angle around the z-axis
        return Quaternion.Euler(0, 0, angle);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collision detected");
            Vector2 direction = transform.position - collision.transform.position;
            Vector2 collisionNormal = collision.contacts[0].normal;
            Vector2 reflected = Vector2.Reflect(direction, collisionNormal);
            Vector2 finalDir = reflected + RandomDirection(5);
            Vector2 reversed = ReverseVector(direction);
            Vector2 awayDir = (reversed + reflected).normalized;
            float spawnDist = 1f;
            Vector2 collisionPoint = collision.contacts[0].point;
            Vector2 spawnPosition = collisionPoint + awayDir * spawnDist;
            Debug.Log("spawn position"+spawnPosition+"finalDir" + finalDir);
            player.GetComponent<PlayerController>().Attack(spawnPosition, Vector2ToQuaternion(finalDir), true);
            Debug.Log("finished spawan");

        }
    }

    void Update()
    {
    }









}




