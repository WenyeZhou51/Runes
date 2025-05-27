using UnityEngine;
using Edgar.Unity;

public class Zone1Setup : MonoBehaviour
{
    // Reference to the level graph for Zone 1
    public LevelGraph levelGraph;
    
    private void Awake()
    {
        // Make sure LevelManager exists
        if (LevelManager.Instance == null)
        {
            Debug.LogError("LevelManager not found! Make sure it's in the scene or loaded from a previous scene.");
            return;
        }
        
        // Create a GameObject for the dungeon generator if it doesn't exist
        GameObject generatorObj = GameObject.Find("DungeonGenerator");
        if (generatorObj == null)
        {
            generatorObj = new GameObject("DungeonGenerator");
        }
        
        // Add the dungeon generator component
        DungeonGeneratorGrid2D dungeonGenerator = generatorObj.GetComponent<DungeonGeneratorGrid2D>();
        if (dungeonGenerator == null)
        {
            dungeonGenerator = generatorObj.AddComponent<DungeonGeneratorGrid2D>();
        }
        
        // Configure the generator
        dungeonGenerator.GenerateOnStart = false; // LevelManager will handle generation
        
        // Set up the level graph
        if (dungeonGenerator.FixedLevelGraphConfig == null)
        {
            dungeonGenerator.FixedLevelGraphConfig = new FixedLevelGraphConfigGrid2D();
        }
        
        // Use the level graph from LevelManager if available
        if (LevelManager.Instance.levelGraphs != null && 
            LevelManager.Instance.levelGraphs.Length > 0 && 
            LevelManager.Instance.currentLevel < LevelManager.Instance.levelGraphs.Length)
        {
            dungeonGenerator.FixedLevelGraphConfig.LevelGraph = LevelManager.Instance.levelGraphs[LevelManager.Instance.currentLevel];
        }
        // Otherwise use the one assigned to this component
        else if (levelGraph != null)
        {
            dungeonGenerator.FixedLevelGraphConfig.LevelGraph = levelGraph;
        }
        
        // Enemy generation and level exit are now handled by the EnemyGenerationSystem
        // configured in the Zone 1 generation.asset file
    }
} 