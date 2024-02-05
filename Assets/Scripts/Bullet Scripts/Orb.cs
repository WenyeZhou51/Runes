using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : Bullet
{
    private Rigidbody2D rb;
    private float bulletSpeed=10f;
    private float spread = 10;
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();

        // Calculate the initial velocity vector
        Vector2 initialVelocity = transform.right * bulletSpeed;
        Quaternion rotationQuaternion = Quaternion.AngleAxis(Random.Range(-spread, spread), Vector3.forward);

        // Rotate the Vector2 using the Quaternion.
        Vector2 newVelocity = rotationQuaternion * initialVelocity;
        // Set the Rigidbody2D's velocity directly
        rb.velocity = newVelocity;
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
