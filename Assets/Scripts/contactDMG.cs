using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class contactDMG : MonoBehaviour
{
    // Start is called before the first frame update
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") {
            collision.gameObject.GetComponent<PlayerController>().TakeDamage(20);
        }
    }
}
