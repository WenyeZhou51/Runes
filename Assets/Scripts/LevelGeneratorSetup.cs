using UnityEngine;
using Edgar.Unity;

public class LevelGeneratorSetup : MonoBehaviour
{
    // Reference to the dungeon generator
    private DungeonGeneratorGrid2D dungeonGenerator;
    
    // Reference to the roguelike post-processing
    public RoguelikePostProcessing roguelikePostProcessing;
    
    // Reference to the level exit prefab
    public GameObject levelExitPrefab;
    
    private void Awake()
    {
        // Get or add the dungeon generator
        dungeonGenerator = GetComponent<DungeonGeneratorGrid2D>();
        if (dungeonGenerator == null)
        {
            dungeonGenerator = gameObject.AddComponent<DungeonGeneratorGrid2D>();
        }
        
        // Disable automatic generation on start (LevelManager will handle it)
        dungeonGenerator.GenerateOnStart = false;
        
        // Add our custom post-processing
        if (roguelikePostProcessing != null)
        {
            // Set the level exit prefab
            roguelikePostProcessing.levelExitPrefab = levelExitPrefab;
            
            // Add the post-processing to the generator
            if (dungeonGenerator.CustomPostProcessTasks == null)
            {
                dungeonGenerator.CustomPostProcessTasks = new System.Collections.Generic.List<DungeonGeneratorPostProcessingGrid2D>();
            }
            
            dungeonGenerator.CustomPostProcessTasks.Add(roguelikePostProcessing);
        }
    }
} 