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

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (modification != null)
        {
            modificationTypeName = modification.GetClass().AssemblyQualifiedName;
        }
        else
        {
            modificationTypeName = null;
        }
#endif
    }

    public void applyMod(ActionCards actionCard)
    {
        if (!string.IsNullOrEmpty(modificationTypeName))
        {
            System.Type scriptType = System.Type.GetType(modificationTypeName);
            if (scriptType != null)
            {
                actionCard.GetInstance().gameObject.AddComponent(scriptType);
            }
            else
            {
                Debug.LogError("Failed to find script type: " + modificationTypeName);
            }
        }
        else
        {
            Debug.LogWarning("No modification type specified.");
        }
    }
}
