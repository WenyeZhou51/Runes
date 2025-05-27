using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Aux Card", menuName = "Aux Card")]
public class AuxCards : Cards
{
#if UNITY_EDITOR
    public MonoScript modification;
#endif

    [SerializeField]
    private string modificationTypeName;
    
    // Flag to mark if this is a path modifier card
    [SerializeField]
    private bool isPathModifier = false;

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (modification != null)
        {
            modificationTypeName = modification.GetClass().AssemblyQualifiedName;
            
            // Check if this is a path modifier
            System.Type scriptType = modification.GetClass();
            isPathModifier = typeof(PathModifier).IsAssignableFrom(scriptType);
        }
        else
        {
            modificationTypeName = null;
            isPathModifier = false;
        }
#endif
    }

    public bool applyMod(ActionCards actionCard)
    {
        if (string.IsNullOrEmpty(modificationTypeName))
        {
            Debug.LogWarning("No modification type specified.");
            return false;
        }
        
        System.Type scriptType = System.Type.GetType(modificationTypeName);
        if (scriptType == null)
        {
            Debug.LogError("Failed to find script type: " + modificationTypeName);
            return false;
        }
        
        // Check if this is a path modifier
        if (isPathModifier)
        {
            // Check if there's already a path modifier on the projectile
            GameObject projectile = actionCard.GetInstance().gameObject;
            if (PathModifier.HasPathModifier(projectile))
            {
                Debug.LogWarning("Rejected " + getCardName() + ": A path modifier is already applied to this projectile.");
                return false;
            }
        }
        
        // Apply the modification
        actionCard.GetInstance().gameObject.AddComponent(scriptType);
        return true;
    }
}

