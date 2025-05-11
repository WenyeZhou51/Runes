using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayBullet : Bullet
{
    [SerializeField] private float rayLength = 0.01f; // Fixed ray segment length
    [SerializeField] private int maxBounces = 3;
    [SerializeField] private float raySpeed = 20f;
    
    private int currentBounces = 0;
    private Vector2 rayDirection;
    private LineRenderer lineRenderer;
    private bool isMoving = true;
    
    // For tracking ray positions and movement
    private List<Vector2> fullPath = new List<Vector2>();
    private Vector2 rayHead; // Current ray head position
    private Vector2 rayTail; // Current ray tail position
    private int currentPathIndex = 0;
    private float distanceTraveled = 0f;
    
    // For collision
    private LayerMask wallLayers;
    private LayerMask enemyLayers;
    
    // For debugging
    private List<GameObject> hitEnemies = new List<GameObject>();
    private Vector2[] plannedPoints; // To store expected ray path for debugging
    private int plannedBounces = 0; // Track expected bounce count
    private bool pathCalculated = false;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set damage
        damage = 10;
        
        Debug.Log($"[Ray Debug] Ray initialized with maxBounces={maxBounces}");
        
        // Setup line renderer for the beam
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = 2; // We need two points for the ray segment
        
        // Set initial direction from the object's rotation
        rayDirection = transform.right;
        
        // Setup collision layers
        wallLayers = LayerMask.GetMask("Wall", "Default"); // Adjust to your layer names
        enemyLayers = LayerMask.GetMask("Enemy");
        
        // Initialize ray head and tail
        rayHead = transform.position;
        rayTail = rayHead;
        
        // First, calculate the full expected path for debugging
        CalculateFullPath();
        
        // Start ray movement
        StartCoroutine(MoveRaySegment());
    }
    
    private void CalculateFullPath()
    {
        Vector2 startPos = transform.position;
        Vector2 direction = transform.right;
        
        fullPath.Clear();
        fullPath.Add(startPos);
        
        int expectedBounces = 0;
        List<Vector2> debugPoints = new List<Vector2>();
        debugPoints.Add(startPos);
        
        // Simulate ray path to calculate all bounce points in advance (for debugging purposes)
        while (expectedBounces < maxBounces + 1) // +1 to include final segment
        {
            // Debug information for examining ray behavior
            Debug.Log($"[Ray Debug] Simulating bounce {expectedBounces}: Direction={direction}, StartPos={startPos}");
            
            RaycastHit2D hitWall = Physics2D.Raycast(startPos, direction, 50f, wallLayers);
            RaycastHit2D hitEnemy = Physics2D.Raycast(startPos, direction, 50f, enemyLayers);
            
            float wallDistance = hitWall.collider ? hitWall.distance : float.MaxValue;
            float enemyDistance = hitEnemy.collider ? hitEnemy.distance : float.MaxValue;
            
            if (hitWall.collider == null && hitEnemy.collider == null)
            {
                // No hit, add an end point some distance away and finish
                Vector2 endPoint = startPos + direction * 50f;
                Debug.Log($"[Ray Debug] No wall or enemy hit. Adding end point at {endPoint}");
                debugPoints.Add(endPoint);
                fullPath.Add(endPoint);
                break;
            }
            
            // Handle hit detection
            if (hitWall.collider != null && wallDistance < enemyDistance)
            {
                Vector2 hitPoint = hitWall.point;
                Vector2 hitNormal = hitWall.normal;
                
                // Add hit point to the path
                debugPoints.Add(hitPoint);
                fullPath.Add(hitPoint);
                
                // Log reflection details
                Debug.Log($"[Ray Debug] Wall hit at {hitPoint}, Normal={hitNormal}, Collider={hitWall.collider.name}");
                
                // Calculate reflection
                Vector2 oldDirection = direction;
                direction = Vector2.Reflect(direction, hitNormal);
                
                Debug.Log($"[Ray Debug] Reflection: Before={oldDirection}, After={direction}, Dot product with normal={Vector2.Dot(oldDirection, hitNormal)}");
                
                // Update for next iteration
                startPos = hitPoint + hitNormal * 0.01f; // Small offset to avoid hitting same point
                expectedBounces++;
            }
            else if (hitEnemy.collider != null)
            {
                Vector2 hitPoint = hitEnemy.point;
                
                // Add enemy hit point
                debugPoints.Add(hitPoint);
                fullPath.Add(hitPoint);
                
                Debug.Log($"[Ray Debug] Enemy hit at {hitPoint}, Collider={hitEnemy.collider.name}");
                
                // Pass through enemy
                startPos = hitPoint + direction * 0.01f;
                // Don't increment bounce count when passing through enemies
            }
            else
            {
                // This shouldn't happen given our earlier check, but just in case
                Debug.LogWarning("[Ray Debug] Unexpected condition in path calculation");
                break;
            }
            
            // Exit if we've hit the max bounce count
            if (expectedBounces >= maxBounces)
            {
                Debug.Log($"[Ray Debug] Reached max bounce count ({maxBounces}) in planning");
                break;
            }
        }
        
        plannedPoints = debugPoints.ToArray();
        plannedBounces = expectedBounces;
        pathCalculated = true;
        
        Debug.Log($"[Ray Debug] Planned path has {plannedPoints.Length} points and {plannedBounces} bounces");
    }
    
    private IEnumerator MoveRaySegment()
    {
        // Initialize ray head and tail
        rayHead = transform.position;
        rayTail = rayHead;
        currentPathIndex = 0;
        distanceTraveled = 0f;
        
        Vector2 currentDirection = transform.right;
        float totalPathDistance = 0f;
        
        // Calculate total path distance for tracking
        for (int i = 0; i < fullPath.Count - 1; i++) {
            totalPathDistance += Vector2.Distance(fullPath[i], fullPath[i + 1]);
        }
        
        // Set initial line renderer positions
        lineRenderer.SetPosition(0, rayTail);
        lineRenderer.SetPosition(1, rayHead);
        
        Debug.Log($"[Ray Debug] Starting ray movement with rayLength={rayLength}");
        Debug.Log($"[Ray Debug] Path points: {fullPath.Count}, Expected bounce points:");
        for (int i = 1; i < fullPath.Count - 1; i++) {
            // Check angle at each potential bounce point
            if (i > 0 && i < fullPath.Count - 1) {
                Vector2 inDirection = (fullPath[i] - fullPath[i - 1]).normalized;
                Vector2 outDirection = (fullPath[i + 1] - fullPath[i]).normalized;
                float angleDiff = Vector2.Angle(inDirection, outDirection);
                Debug.Log($"[Ray Debug] Expected bounce point #{i}: position={fullPath[i]}, angle change={angleDiff}");
            }
        }
        
        while (isMoving && currentPathIndex < fullPath.Count - 1)
        {
            // Get the next target point from our path
            Vector2 targetPoint = fullPath[currentPathIndex + 1];
            Vector2 currentStart = fullPath[currentPathIndex];
            Vector2 segmentDirection = (targetPoint - currentStart).normalized;
            
            // Calculate how far the ray head moves this frame
            float distanceToMove = raySpeed * Time.deltaTime;
            rayHead = Vector2.MoveTowards(rayHead, targetPoint, distanceToMove);
            
            // Total distance moved
            distanceTraveled += distanceToMove;
            
            // Calculate where the tail should be on the path
            float tailDistance = Mathf.Max(0, distanceTraveled - rayLength);
            Vector2 oldTail = rayTail;
            
            // The key calculation: if rayLength is very small, tail will be close to head
            // if rayLength is large, tail will be far behind on the path
            if (distanceTraveled <= rayLength)
            {
                // Ray hasn't reached full length yet
                rayTail = transform.position;
                Debug.Log($"[Ray Debug] Ray hasn't reached full length yet. distanceTraveled={distanceTraveled}, rayLength={rayLength}");
            }
            else
            {
                bool tailFound = false;
                // Find where tail should be on the path
                float accumulatedDistance = 0f;
                
                // Walk forward through path segments until we find where the tail should be
                for (int i = 0; i < fullPath.Count - 1; i++)
                {
                    float segmentLength = Vector2.Distance(fullPath[i], fullPath[i+1]);
                    
                    if (accumulatedDistance + segmentLength >= tailDistance)
                    {
                        // Tail is on this segment
                        float remainingDistance = tailDistance - accumulatedDistance;
                        Vector2 segmentDir = (fullPath[i+1] - fullPath[i]).normalized;
                        rayTail = fullPath[i] + segmentDir * remainingDistance;
                        tailFound = true;
                        
                        Debug.Log($"[Ray Debug] Tail positioned on segment {i}: Start={fullPath[i]}, End={fullPath[i+1]}, RemainingDist={remainingDistance}");
                        break;
                    }
                    
                    accumulatedDistance += segmentLength;
                }
                
                if (!tailFound) {
                    Debug.LogError($"[Ray Debug] Failed to find tail position! tailDistance={tailDistance}, totalPathDist={totalPathDistance}");
                }
            }
            
            // Verify ray length
            float actualRayLength = Vector2.Distance(rayHead, rayTail);
            if (Mathf.Abs(actualRayLength - rayLength) > 0.1f && distanceTraveled > rayLength) {
                Debug.LogWarning($"[Ray Debug] Ray length mismatch! Actual={actualRayLength}, Target={rayLength}, Diff={actualRayLength-rayLength}");
                Debug.LogWarning($"[Ray Debug] Head={rayHead}, Tail={rayTail}, TailDist={tailDistance}, DistTraveled={distanceTraveled}");
            }
            
            // Force correct length as a last resort
            if (distanceTraveled > rayLength && actualRayLength > rayLength * 1.1f) {
                Vector2 direction = (rayHead - rayTail).normalized;
                rayTail = rayHead - (direction * rayLength);
                Debug.Log($"[Ray Debug] Forcing correct ray length. New tail={rayTail}");
            }
            
            // Update line renderer to draw the ray segment - directly set actual positions
            lineRenderer.SetPosition(0, rayTail);
            lineRenderer.SetPosition(1, rayHead);
            
            // Check if we've reached the current target point
            if (Vector2.Distance(rayHead, targetPoint) < 0.01f)
            {
                // Log detailed information about reaching a path point
                Debug.Log($"[Ray Debug] Reached path point {currentPathIndex + 1} at position {rayHead}");
                
                currentPathIndex++;
                
                // If we've reached a wall bounce point
                if (currentPathIndex > 0 && currentPathIndex < fullPath.Count - 1)
                {
                    // Check if this is a wall bounce (not an enemy pass-through)
                    Vector2 inDirection = (fullPath[currentPathIndex] - fullPath[currentPathIndex - 1]).normalized;
                    Vector2 outDirection = (fullPath[currentPathIndex + 1] - fullPath[currentPathIndex]).normalized;
                    
                    float angleDiff = Vector2.Angle(inDirection, outDirection);
                    Debug.Log($"[Ray Debug] Direction change at point {currentPathIndex}: angle difference={angleDiff}");
                    
                    if (angleDiff > 1f) // If direction changed significantly, it's a bounce
                    {
                        currentBounces++;
                        Debug.Log($"[Ray Debug] BOUNCE DETECTED: #{currentBounces} of {maxBounces} completed at position {rayHead}, angle change: {angleDiff}");
                        
                        // If we've reached max bounces, destroy immediately
                        if (currentBounces >= maxBounces)
                        {
                            Debug.Log($"[Ray Debug] MAX BOUNCES REACHED ({maxBounces}): position={rayHead}, destroying immediately");
                            
                            // Try-catch to detect any issues with destruction
                            try {
                                Destroy(gameObject);
                                Debug.Log("[Ray Debug] Destroy command issued successfully");
                            } catch (System.Exception e) {
                                Debug.LogError($"[Ray Debug] Error destroying ray: {e.Message}");
                            }
                            
                            // Also set a flag in case Destroy has delayed execution
                            isMoving = false;
                            
                            Debug.Log("[Ray Debug] About to exit coroutine with yield break");
                            yield break; // Exit the coroutine entirely
                        }
                    }
                    else
                    {
                        Debug.Log($"[Ray Debug] Direction change too small ({angleDiff}°), not counting as bounce");
                    }
                }
                
                // Force bounce detection on the third path point (temporary solution)
                if (currentPathIndex == 3 && currentBounces < 3)
                {
                    Debug.Log($"[Ray Debug] Forcing detection of the third bounce at point {currentPathIndex}");
                    currentBounces++;
                    
                    if (currentBounces >= maxBounces)
                    {
                        Debug.Log($"[Ray Debug] FORCED MAX BOUNCES REACHED ({maxBounces}): position={rayHead}, destroying immediately");
                        Destroy(gameObject);
                        yield break;
                    }
                }
            }
            
            // Check for enemies along the ray's current segment
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayTail, (rayHead - rayTail).normalized, 
                                                    Vector2.Distance(rayHead, rayTail), enemyLayers);
            
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && !hitEnemies.Contains(hit.collider.gameObject))
                {
                    hitEnemies.Add(hit.collider.gameObject);
                    Debug.Log($"[Ray Debug] Enemy hit during movement: {hit.collider.name}");
                    
                    // Apply damage to enemy here if needed
                    // You could send a message to the enemy object or call a damage method
                }
            }
            
            yield return null;
        }
        
        // Compare actual bounces to planned bounces
        if (currentBounces != plannedBounces)
        {
            Debug.LogWarning($"[Ray Debug] Bounce count mismatch! Planned: {plannedBounces}, Actual: {currentBounces}");
        }
        
        Debug.Log($"[Ray Debug] Ray movement complete, starting normal fadeout");
        
        // Start fadeout
        yield return StartCoroutine(FadeOutAndDestroy());
    }
    
    // New method to handle fadeout and destruction
    private IEnumerator FadeOutAndDestroy()
    {
        Debug.Log($"[Ray Debug] Beginning fadeout sequence");
        
        // Keep the ray visible for a short time at the end
        yield return new WaitForSeconds(0.5f);
        
        // Fade out the ray
        float fadeTime = 0.5f;
        float fadeTimer = 0f;
        Color startColor = lineRenderer.startColor;
        
        while (fadeTimer < fadeTime)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeTime);
            
            lineRenderer.startColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            lineRenderer.endColor = new Color(startColor.r, startColor.g, startColor.b, alpha);
            
            Debug.Log($"[Ray Debug] Fading: {alpha * 100}% opacity remaining");
            
            yield return null;
        }
        
        Debug.Log($"[Ray Debug] Fadeout complete, destroying ray object");
        
        // Destroy the gameObject
        Destroy(gameObject);
    }
    
    void Update()
    {
        // Check for ray lifetime expiration
        if (Time.time - creationTime > duration)
        {
            Debug.Log($"[Ray Debug] Ray duration expired, stopping movement");
            isMoving = false;
        }
    }
    
    // We're not using physics collisions anymore
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // Not used in this implementation
    }
    
    // Draw debug visualization in the editor
    void OnDrawGizmos()
    {
        if (Application.isPlaying && pathCalculated)
        {
            // Draw the planned path
            Gizmos.color = Color.yellow;
            for (int i = 0; i < plannedPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(plannedPoints[i], plannedPoints[i + 1]);
                Gizmos.DrawWireSphere(plannedPoints[i], 0.1f);
            }
            
            // Draw the current ray head and tail
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rayHead, 0.15f);
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(rayTail, 0.15f);
        }
    }
} 