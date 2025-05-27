using UnityEngine;

// Base class for all path modifiers
public abstract class PathModifier : MonoBehaviour
{
    // This class serves as a marker for path modifiers
    // Both OrbitScript and WavePathScript will inherit from this
    
    // Static method to check if a GameObject already has a path modifier
    public static bool HasPathModifier(GameObject gameObject)
    {
        return gameObject.GetComponent<PathModifier>() != null;
    }
    
    // Static method to remove existing path modifiers
    public static void RemoveExistingPathModifiers(GameObject gameObject)
    {
        PathModifier[] modifiers = gameObject.GetComponents<PathModifier>();
        foreach (PathModifier modifier in modifiers)
        {
            Destroy(modifier);
        }
    }
} 