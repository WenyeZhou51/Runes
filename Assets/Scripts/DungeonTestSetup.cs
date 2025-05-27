using UnityEngine;
using Edgar.Unity;

[System.Serializable]
public class DungeonTestSetup : MonoBehaviour
{
    [Header("Test Configuration")]
    [Tooltip("The level graph to test with")]
    public LevelGraph testLevelGraph;
    
    [Tooltip("Timeout in milliseconds (30000 = 30 seconds)")]
    public int timeoutMs = 30000;
    
    [Tooltip("Enable diagnostics for debugging")]
    public bool enableDiagnostics = true;
    
    [Header("Controls")]
    [Tooltip("Key to press to generate dungeon")]
    public KeyCode generateKey = KeyCode.G;
    
    [Tooltip("Key to press to clear current dungeon")]
    public KeyCode clearKey = KeyCode.C;
    
    private DungeonGeneratorGrid2D generator;
    private GameObject generatedLevel;
    
    void Start()
    {
        SetupGenerator();
        
        // Show instructions
        Debug.Log("=== Dungeon Test Setup ===");
        Debug.Log($"Press {generateKey} to generate dungeon");
        Debug.Log($"Press {clearKey} to clear current dungeon");
        Debug.Log($"Timeout set to {timeoutMs}ms");
        Debug.Log($"Diagnostics: {(enableDiagnostics ? "Enabled" : "Disabled")}");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(generateKey))
        {
            TestGenerate();
        }
        
        if (Input.GetKeyDown(clearKey))
        {
            ClearDungeon();
        }
    }
    
    void SetupGenerator()
    {
        // Create generator if it doesn't exist
        if (generator == null)
        {
            GameObject generatorObj = new GameObject("Test Dungeon Generator");
            generator = generatorObj.AddComponent<DungeonGeneratorGrid2D>();
        }
        
        // Configure generator
        generator.GenerateOnStart = false;
        generator.EnableDiagnostics = enableDiagnostics;
        generator.UseRandomSeed = true;
        
        // Level graph config
        if (generator.FixedLevelGraphConfig == null)
            generator.FixedLevelGraphConfig = new FixedLevelGraphConfigGrid2D();
        
        generator.FixedLevelGraphConfig.LevelGraph = testLevelGraph;
        generator.FixedLevelGraphConfig.UseCorridors = true;
        
        // Generator config
        if (generator.GeneratorConfig == null)
            generator.GeneratorConfig = new DungeonGeneratorConfigGrid2D();
        
        generator.GeneratorConfig.Timeout = timeoutMs;
        generator.GeneratorConfig.MinimumRoomDistance = 1;
        
        // Post processing config
        if (generator.PostProcessConfig == null)
            generator.PostProcessConfig = new PostProcessingConfigGrid2D();
        
        generator.PostProcessConfig.InitializeSharedTilemaps = true;
        generator.PostProcessConfig.CopyTilesToSharedTilemaps = true;
        generator.PostProcessConfig.CenterGrid = true;
        generator.PostProcessConfig.DisableRoomTemplatesRenderers = false; // Keep renderers for testing
        generator.PostProcessConfig.DisableRoomTemplatesColliders = false; // Keep colliders for testing
    }
    
    [ContextMenu("Test Generate")]
    public void TestGenerate()
    {
        if (testLevelGraph == null)
        {
            Debug.LogError("No test level graph assigned! Please assign one in the inspector.");
            return;
        }
        
        Debug.Log("Starting test generation...");
        
        // Clear previous level
        ClearDungeon();
        
        try
        {
            var result = generator.Generate();
            Debug.Log("✓ Generation successful!");
            
            // Find the generated level
            generatedLevel = GameObject.Find("Generated Level");
            if (generatedLevel != null)
            {
                Debug.Log($"Generated level found with {generatedLevel.transform.childCount} rooms");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"✗ Generation failed: {e.Message}");
            
            if (e is TimeoutException)
            {
                Debug.LogError("This is a timeout error. Try:");
                Debug.LogError("1. Increase the timeout value");
                Debug.LogError("2. Simplify the level graph (fewer rooms/connections)");
                Debug.LogError("3. Check room template door configurations");
            }
        }
    }
    
    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        // Find and destroy generated level
        GameObject existingLevel = GameObject.Find("Generated Level");
        if (existingLevel != null)
        {
            DestroyImmediate(existingLevel);
            Debug.Log("Cleared existing dungeon");
        }
    }
    
    void OnValidate()
    {
        // Ensure timeout is reasonable
        if (timeoutMs < 5000)
        {
            timeoutMs = 5000;
            Debug.LogWarning("Timeout too low, set to minimum 5000ms");
        }
    }
} 