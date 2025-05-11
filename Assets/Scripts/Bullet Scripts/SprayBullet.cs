using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprayBullet : Bullet
{
    private Rigidbody2D rb;
    private float bulletSpeed = 15f;
    private float coneAngle = 90f; // Total spread angle in degrees
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        
        // Calculate the initial velocity with random angle within cone
        float randomAngle = Random.Range(-coneAngle/2, coneAngle/2);
        Quaternion spread = Quaternion.AngleAxis(randomAngle, Vector3.forward);
        Vector2 initialVelocity = spread * (transform.right * bulletSpeed);
        
        // Set the Rigidbody2D's velocity
        rb.velocity = initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - creationTime > duration)
        {
            Destroy(gameObject);
        }
    }
} 