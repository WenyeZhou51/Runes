using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Bullet
{
    private Rigidbody2D rb;
    private float bulletSpeed=30f;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        // Calculate the initial velocity vector
        Vector2 initialVelocity = transform.right * bulletSpeed;

        // Set the Rigidbody2D's velocity directly
        rb.velocity = initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - creationTime > duration)
        {
            Destroy(this.gameObject);
        }
    }

  
}
