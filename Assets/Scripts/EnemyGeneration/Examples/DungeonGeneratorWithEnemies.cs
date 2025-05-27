using UnityEngine;
using Edgar.Unity;

namespace EnemyGeneration.Examples
{
    /// <summary>
    /// Example script showing how to use the enemy generation system with the dungeon generator
    /// </summary>
    public class DungeonGeneratorWithEnemies : MonoBehaviour
    {
        [Header("Dungeon Generator Reference")]
        [SerializeField] private GameObject dungeonGeneratorObject;
        
        [Header("Enemy Generation")]
        [SerializeField] private GameObject[] basicEnemyPrefabs;
        [SerializeField] private GameObject[] eliteEnemyPrefabs;
        [SerializeField] private GameObject[] bossPrefabs;
        
        private void Start()
        {
            // Make sure we have a dungeon generator
            if (dungeonGeneratorObject == null)
            {
                Debug.LogError("Dungeon Generator not assigned!");
                return;
            }
            
            // Check if we already have an enemy generation component
            EnemyGenerationComponent enemyGen = dungeonGeneratorObject.GetComponent<EnemyGenerationComponent>();
            
            // If not, add one
            if (enemyGen == null)
            {
                Debug.Log("Adding Enemy Generation Component to Dungeon Generator");
                enemyGen = dungeonGeneratorObject.AddComponent<EnemyGenerationComponent>();
                
                // Configure the component with our enemy prefabs
                SetupEnemyGeneration(enemyGen);
            }
            
            // Generate the dungeon
            // This works with the Edgar dungeon generator
            var generator = dungeonGeneratorObject.GetComponent<DungeonGeneratorGrid2D>();
            if (generator != null)
            {
                Debug.Log("Generating dungeon with enemy spawning");
                generator.Generate();
            }
            else
            {
                Debug.LogError("Could not find DungeonGeneratorGrid2D component!");
            }
        }
        
        /// <summary>
        /// Configure the enemy generation component with our settings
        /// </summary>
        private void SetupEnemyGeneration(EnemyGenerationComponent enemyGen)
        {
            // Use reflection to set the private fields
            // Note: In a real implementation, you might want to expose these fields as public
            // or provide public methods to set them
            
            var basicEnemyField = typeof(EnemyGenerationComponent).GetField("basicEnemyPrefabs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (basicEnemyField != null)
                basicEnemyField.SetValue(enemyGen, basicEnemyPrefabs);
                
            var eliteEnemyField = typeof(EnemyGenerationComponent).GetField("eliteEnemyPrefabs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (eliteEnemyField != null)
                eliteEnemyField.SetValue(enemyGen, eliteEnemyPrefabs);
                
            var bossEnemyField = typeof(EnemyGenerationComponent).GetField("bossEnemyPrefabs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (bossEnemyField != null)
                bossEnemyField.SetValue(enemyGen, bossPrefabs);
                
            // You can also set other fields like min/max enemies per room, etc.
        }
        
        /// <summary>
        /// Alternative approach: Create a ScriptableObject for enemy generation
        /// </summary>
        public void SetupWithScriptableObject()
        {
            // This method shows how to set up enemy generation using the ScriptableObject approach
            // For this to work, you need to have already created an EnemyGenerationSystem asset
            
            // Find the dungeon generator component
            var generator = dungeonGeneratorObject.GetComponent<DungeonGeneratorGrid2D>();
            if (generator == null)
            {
                Debug.LogError("Could not find DungeonGeneratorGrid2D component!");
                return;
            }
            
            // Create a new enemy generation system asset
            EnemyGenerationSystem enemyGenSystem = ScriptableObject.CreateInstance<EnemyGenerationSystem>();
            
            // Configure it
            // Note: In a real implementation, you would typically create this asset in the editor
            // and configure it there, then assign it to the generator
            
            // For demonstration purposes only:
            var basicEnemyField = typeof(EnemyGenerationSystem).GetField("basicEnemyPrefabs", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (basicEnemyField != null)
                basicEnemyField.SetValue(enemyGenSystem, basicEnemyPrefabs);
                
            // Add it to the generator's post-processing tasks
            // Note: This is just an example and may need to be adjusted based on your actual implementation
            
            // For Edgar dungeon generator:
            var postProcessingComponent = generator.GetComponent<DungeonGeneratorPostProcessingGrid2D>();
            if (postProcessingComponent != null)
            {
                // Add your custom post-processing task to the generator
                // This would typically be done in the Inspector
                Debug.Log("You would typically add the EnemyGenerationSystem to the generator's CustomPostProcessTasks in the Inspector");
            }
        }
    }
} 