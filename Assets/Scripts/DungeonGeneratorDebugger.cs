using UnityEngine;
using Edgar.Unity;

public class DungeonGeneratorDebugger : MonoBehaviour
{
    [Header("Dungeon Generator Configuration")]
    public LevelGraph levelGraph;
    public int timeoutMs = 30000; // 30 seconds
    public bool enableDiagnostics = true;
    public bool generateOnStart = false;
    
    [Header("Debug Controls")]
    public KeyCode generateKey = KeyCode.G;
    
    private DungeonGeneratorGrid2D dungeonGenerator;
    
    void Start()
    {
        SetupDungeonGenerator();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(generateKey))
        {
            GenerateDungeon();
        }
    }
    
    void SetupDungeonGenerator()
    {
        // Find or create dungeon generator
        dungeonGenerator = FindObjectOfType<DungeonGeneratorGrid2D>();
        if (dungeonGenerator == null)
        {
            GameObject generatorObj = new GameObject("Dungeon Generator");
            dungeonGenerator = generatorObj.AddComponent<DungeonGeneratorGrid2D>();
        }
        
        // Configure the generator
        dungeonGenerator.GenerateOnStart = generateOnStart;
        dungeonGenerator.EnableDiagnostics = enableDiagnostics;
        dungeonGenerator.UseRandomSeed = true;
        
        // Set up level graph config
        if (dungeonGenerator.FixedLevelGraphConfig == null)
        {
            dungeonGenerator.FixedLevelGraphConfig = new FixedLevelGraphConfigGrid2D();
        }
        
        dungeonGenerator.FixedLevelGraphConfig.LevelGraph = levelGraph;
        dungeonGenerator.FixedLevelGraphConfig.UseCorridors = true;
        
        // Set up generator config with increased timeout
        if (dungeonGenerator.GeneratorConfig == null)
        {
            dungeonGenerator.GeneratorConfig = new DungeonGeneratorConfigGrid2D();
        }
        
        dungeonGenerator.GeneratorConfig.Timeout = timeoutMs;
        dungeonGenerator.GeneratorConfig.MinimumRoomDistance = 1;
        dungeonGenerator.GeneratorConfig.RepeatModeOverride = RepeatModeOverride.NoOverride;
        
        // Set up post processing config
        if (dungeonGenerator.PostProcessConfig == null)
        {
            dungeonGenerator.PostProcessConfig = new PostProcessingConfigGrid2D();
        }
        
        dungeonGenerator.PostProcessConfig.InitializeSharedTilemaps = true;
        dungeonGenerator.PostProcessConfig.CopyTilesToSharedTilemaps = true;
        dungeonGenerator.PostProcessConfig.CenterGrid = true;
        dungeonGenerator.PostProcessConfig.DisableRoomTemplatesRenderers = true;
        dungeonGenerator.PostProcessConfig.DisableRoomTemplatesColliders = true;
        
        Debug.Log($"Dungeon Generator configured with {timeoutMs}ms timeout and diagnostics {(enableDiagnostics ? "enabled" : "disabled")}");
    }
    
    public void GenerateDungeon()
    {
        if (dungeonGenerator == null)
        {
            Debug.LogError("Dungeon generator not found!");
            return;
        }
        
        if (levelGraph == null)
        {
            Debug.LogError("Level graph not assigned!");
            return;
        }
        
        Debug.Log("Starting dungeon generation...");
        
        try
        {
            dungeonGenerator.Generate();
            Debug.Log("Dungeon generation completed successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Dungeon generation failed: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }
    
    [ContextMenu("Test Generate")]
    public void TestGenerate()
    {
        GenerateDungeon();
    }
    
    [ContextMenu("Reset Generator")]
    public void ResetGenerator()
    {
        if (dungeonGenerator != null)
        {
            DestroyImmediate(dungeonGenerator.gameObject);
        }
        SetupDungeonGenerator();
    }
} 