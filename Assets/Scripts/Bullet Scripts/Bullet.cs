using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public float duration;
    public float creationTime;
    public onImpactScript impactScript;
    public bool collisionEnabled = true;

    protected virtual void Awake()
    {
        creationTime = Time.time; 
        impactScript = GetComponent<onImpactScript>();
    }
    protected void OnCollisionEnter2D(Collision2D collision)
    {
        if (impactScript != null)
        {
            collisionEnabled = false;
        }

        if (collisionEnabled)
        {
            if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Wall"))
            {

                Destroy(gameObject);
            }
        }



        

    }
}
