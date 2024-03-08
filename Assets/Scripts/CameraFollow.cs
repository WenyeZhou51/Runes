using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CameraFollow : MonoBehaviour
{
    private Transform player; // Reference to the player's Transform
    public bool initialized=false;
    private Canvas myCanvas;

    void Awake()
    {
        myCanvas = FindObjectOfType<Canvas>();
        // Find the player by tag and get its transform
        player = GameObject.FindGameObjectWithTag("Player").transform;
        //Debug.Log("camera position before:" + transform.position);
        transform.position = player.position + new Vector3(0, 0, -10);
        //Debug.Log("player position:" + player.position);
        //Debug.Log("camera position after:"+transform.position);
        Canvas.ForceUpdateCanvases();
        initialized = true;
        //Debug.Log("camera position after force update:" + transform.position);

    }

    void Update()
    {
        // Make sure the player has been found
        if (player != null)
        {
            transform.position = player.position + new Vector3(0, 0, -10);
            
        }
    }
}
