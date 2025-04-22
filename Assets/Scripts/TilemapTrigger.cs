using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapTrigger : MonoBehaviour
{
    public int SceneIndex = -1;
    public Vector3? spawnPos = null;
    public float posX = 0;
    public float posY = 0;
    // Start is called before the first frame update
    void Start()
    {
        if (posX != 0 || posY != 0) {
            spawnPos = new Vector3(posX, posY, 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            Debug.Log("transitioning");
            if (SceneIndex == -1)
            {
                SceneManagerScript.Instance.NextScene();
            }
            else {
                SceneManagerScript.Instance.loadScene(SceneIndex,spawnPos);
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
