using UnityEngine;

public class AddAreaScript : MonoBehaviour
{

    private float sizeMultiplier = 1.5f;

    private float radiusIncrease = 2f;

    private void Start()
    {
        // Increase object scale
        transform.localScale *= sizeMultiplier;

        // If it's an ExplodeBolt, increase explosion radius
        ExplodeBolt explodeBolt = GetComponent<ExplodeBolt>();
        if (explodeBolt != null)
        {
            Debug.Log($"Original explosion radius: {explodeBolt.explosionRadius}");
            explodeBolt.explosionRadius += radiusIncrease;
            // Also scale the explosion visual effect
            if (explodeBolt.transform.localScale.x > 0)
            {
                explodeBolt.transform.localScale *= sizeMultiplier;
            }
            Debug.Log($"New explosion radius: {explodeBolt.explosionRadius}");
        }

        // Adjust collider size
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                boxCollider.size *= sizeMultiplier;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                circleCollider.radius *= sizeMultiplier;
            }
        }
    }
}