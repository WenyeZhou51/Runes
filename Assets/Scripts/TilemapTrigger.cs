using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            SceneManagerScript.Instance.NextScene();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
