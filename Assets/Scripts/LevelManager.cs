using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Edgar.Unity;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    // The hub world scene name
    public string hubWorldSceneName = "Level0.0";
    
    // Zone 1 scene name
    public string zone1SceneName = "Zone 1";
    
    // Current level information
    public int currentLevel = 0;
    public int currentSubLevel = 0;
    
    // Level generation settings
    public LevelGraph[] levelGraphs;
    public int randomSeed;
    public bool useRandomSeed = true;
    
    // References to generated levels
    private Dictionary<string, GameObject> generatedLevels = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Initialize random seed if using random seed
        if (useRandomSeed)
        {
            randomSeed = Random.Range(0, int.MaxValue);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if we're in a level that needs generation
        if (scene.name == zone1SceneName && !generatedLevels.ContainsKey(scene.name + "_" + currentSubLevel))
        {
            StartCoroutine(GenerateLevel());
        }
    }
    
    // Called when the player dies
    public void PlayerDied()
    {
        // Reset current level to hub world
        currentLevel = 0;
        currentSubLevel = 0;
        
        // Generate new random seed for next run
        if (useRandomSeed)
        {
            randomSeed = Random.Range(0, int.MaxValue);
        }
        
        // Clear generated levels
        generatedLevels.Clear();
        
        // Load hub world
        SceneManagerScript.Instance.loadScene(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + hubWorldSceneName + ".unity"));
    }
    
    // Load next level
    public void LoadNextLevel()
    {
        // If we're in the hub world, start Zone 1 with sublevel 1
        if (SceneManager.GetActiveScene().name == hubWorldSceneName)
        {
            currentLevel = 1;
            currentSubLevel = 1;
            
            // Load Zone 1
            int zoneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + zone1SceneName + ".unity");
            if (zoneIndex >= 0)
            {
                SceneManagerScript.Instance.loadScene(zoneIndex);
            }
            else
            {
                Debug.LogError($"Scene {zone1SceneName} not found in build settings!");
            }
            return;
        }
        
        // If we're in Zone 1, increment sublevel
        if (SceneManager.GetActiveScene().name == zone1SceneName)
        {
            currentSubLevel++;
            
            // If we've completed all 4 sublevels of Zone 1, return to hub world
            if (currentSubLevel > 4)
            {
                // Reset to hub world
                currentLevel = 0;
                currentSubLevel = 0;
                
                // Load hub world
                SceneManagerScript.Instance.loadScene(SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + hubWorldSceneName + ".unity"));
            }
            else
            {
                // Reload the Zone 1 scene with the new sublevel
                int zoneIndex = SceneUtility.GetBuildIndexByScenePath("Assets/Scenes/" + zone1SceneName + ".unity");
                if (zoneIndex >= 0)
                {
                    // Clear the current generated level
                    string currentLevelKey = zone1SceneName + "_" + (currentSubLevel - 1);
                    if (generatedLevels.ContainsKey(currentLevelKey))
                    {
                        generatedLevels.Remove(currentLevelKey);
                    }
                    
                    // Load Zone 1 again
                    SceneManagerScript.Instance.loadScene(zoneIndex);
                }
            }
        }
    }
    
    // Generate a random level using Edgar
    private IEnumerator GenerateLevel()
    {
        // Wait one frame to ensure scene is fully loaded
        yield return null;
        
        // Get the level generator
        DungeonGeneratorGrid2D dungeonGenerator = FindObjectOfType<DungeonGeneratorGrid2D>();
        
        // If there's no dungeon generator in the scene, exit
        if (dungeonGenerator == null)
        {
            Debug.LogWarning("No DungeonGeneratorGrid2D found in the scene.");
            yield break;
        }
        
        // Configure the generator
        dungeonGenerator.UseRandomSeed = false; // We'll control the seed manually
        dungeonGenerator.RandomGeneratorSeed = randomSeed + (currentLevel * 100) + currentSubLevel; // Unique seed for each level/sublevel
        
        // Select the appropriate level graph based on current level
        if (levelGraphs != null && levelGraphs.Length > 0 && currentLevel < levelGraphs.Length)
        {
            dungeonGenerator.FixedLevelGraphConfig.LevelGraph = levelGraphs[currentLevel];
        }
        
        // Generate the level
        dungeonGenerator.Generate();
        
        // Store reference to the generated level with sublevel info
        generatedLevels[SceneManager.GetActiveScene().name + "_" + currentSubLevel] = dungeonGenerator.gameObject;
        
        Debug.Log($"Generated level {currentLevel}.{currentSubLevel} with seed {dungeonGenerator.RandomGeneratorSeed}");
    }
} 