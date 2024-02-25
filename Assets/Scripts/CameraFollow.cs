using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player; // Reference to the player's Transform

    void Start()
    {
        // Find the player by tag and get its transform
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Make sure the player has been found
        if (player != null)
        {
            // Update the camera's position to the player's position with the offset
            transform.position = player.position + new Vector3(0, 0, -10);
        }
    }
}
