using UnityEngine;

public class TeleportBolt : Bullet
{
    private Rigidbody2D rb;
    private float bulletSpeed = 8f;
    private Vector2 previousPosition;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        previousPosition = transform.position;

        Vector2 initialVelocity = transform.right * bulletSpeed;
        rb.velocity = initialVelocity;
    }

    void Update()
    {
        previousPosition = transform.position;

        if (Time.time - creationTime > duration)
        {
            // Teleport player to bolt's position before destroying
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                player.transform.position = transform.position;
            }
            Destroy(gameObject);
        }
    }

    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collision detected with: {collision.gameObject.name}, Layer: {collision.gameObject.layer}, Has TilemapCollider: {collision.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null}, Has Wall Tag: {collision.gameObject.CompareTag("Wall")}");

        base.OnCollisionEnter2D(collision);

        if (collisionEnabled && (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.GetComponent<UnityEngine.Tilemaps.TilemapCollider2D>() != null))
        {
            Debug.Log("Collision check passed, attempting teleport");
            Vector2 directionToCurrentPosition = (Vector2)transform.position - previousPosition;
            float distanceToCurrentPosition = directionToCurrentPosition.magnitude;

            // Use LayerMask.GetMask to include all relevant layers
            int layerMask = LayerMask.GetMask("Default", "Wall"); // Add any other relevant layer names
            RaycastHit2D hit = Physics2D.Raycast(previousPosition, directionToCurrentPosition.normalized, distanceToCurrentPosition, layerMask);

            Debug.Log($"Raycast from {previousPosition} to {transform.position}, distance: {distanceToCurrentPosition}");

            if (hit.collider != null)
            {
                Debug.Log($"Raycast hit: {hit.collider.name}, Normal: {hit.normal}, Point: {hit.point}");
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    Vector2 playerToWall = hit.point - (Vector2)player.transform.position;
                    float dotProduct = Vector2.Dot(hit.normal, playerToWall);
                    Vector2 teleportOffset = hit.normal * (dotProduct > 0 ? -0.5f : 0.5f);
                    Vector2 teleportPosition = hit.point + teleportOffset;

                    Debug.Log($"Teleporting player from {player.transform.position} to {teleportPosition}");
                    player.transform.position = teleportPosition;
                }
            }
            else
            {
                // If raycast fails, use collision point directly
                Debug.Log("Raycast failed, using collision point");
                ContactPoint2D contact = collision.GetContact(0);
                Vector2 teleportOffset = contact.normal * 0.5f;
                Vector2 teleportPosition = contact.point + teleportOffset;

                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    player.transform.position = teleportPosition;
                }
            }

            Destroy(gameObject);
        }
    }
}