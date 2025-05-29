using UnityEngine;
using System.Collections.Generic;

namespace BulletHell
{
    public class ProjectileManager : MonoBehaviour
    {
        private bool Initialized = false;

        // Lists out all the projectile types found in Assets
        [SerializeField]
        private List<ProjectilePrefab> ProjectilePrefabs;

        // Each projectile type has its own material, therefore, own IndirectRenderer
        private Dictionary<int, IndirectRenderer> IndirectRenderers;

        // Cache the last accessed IndirectRenderer to reduce the Dict lookup for batches
        private int LastAccessedProjectileTypeIndex = -1;
        private IndirectRenderer LastAccessedRenderer;

        // Counters to keep track of Projectile Group information
        private Dictionary<int, ProjectileTypeCounters> ProjectileTypeCounters;

        [SerializeField]
        private ProjectileEmitterBase[] EmittersArray;
        private int MaxEmitters = 200;
        private int EmitterCount = 0;

        // Singleton
        private static ProjectileManager instance = null;
        public static ProjectileManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ProjectileManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject();
                        go.name = "ProjectileManager";
                        instance = go.AddComponent<ProjectileManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }
 

        void Awake()
        {
            Instance.Initialize();
        }

        [RuntimeInitializeOnLoadMethod]
        static void EnableInstance()
        {
            Instance.Initialized = false;
        }

        void Initialize()
        {
            if (!Initialized)
            {
                // Grab a list of Projectile Types founds in assets folder "ProjectilePrefabs"
                GameObject[] projectileTypes = Resources.LoadAll<GameObject>("ProjectilePrefabs");
                Debug.Log("loaded projectileprefabs");
                ProjectilePrefabs = new List<ProjectilePrefab>();
                IndirectRenderers = new Dictionary<int, IndirectRenderer>();
                ProjectileTypeCounters = new Dictionary<int, ProjectileTypeCounters>();
                                             
                // Process projectile types
                int currentIndex = 0;
                for (int n = 0; n < projectileTypes.Length; n++)
                {
                    ProjectilePrefab type = projectileTypes[n].GetComponent<ProjectilePrefab>();

                    // if script is not attached, then we skip as it is invalid.
                    if (type == null)
                        continue;

                    type.Initialize(currentIndex);
                    ProjectilePrefabs.Add(type);

                    // If material is set to be a static color ensure we do not send color data to shader
                    IndirectRenderers.Add(currentIndex, new IndirectRenderer(type.GetMaxProjectileCount(), type.Material, type.Mesh, type.IsStaticColor));
                    ProjectileTypeCounters.Add(currentIndex, new ProjectileTypeCounters());

                    currentIndex++;
                }

                EmittersArray = new ProjectileEmitterBase[MaxEmitters];

                // Get a list of all emitters in the scene
                RegisterEmitters();

                Initialized = true;
            }
        }

        // Adds a new emitter at runtime
        public void AddEmitter(ProjectileEmitterBase emitter, int allocation)
        {
            // Default projectile if no projectile type set
            if (emitter.ProjectilePrefab == null)
                emitter.ProjectilePrefab = GetProjectilePrefab(0);

            // Increment group counter
            ProjectileTypeCounters[emitter.ProjectilePrefab.Index].TotalGroups++;

            // Should be a way to not allocate more than projectile type will allow - across all emitters
            emitter.Initialize(allocation);

            EmitterCount++;
        }



        public void RegisterEmitters()
        {
            // Register Emitters that are currently in the scene
            Debug.Log("registering emitters");
            
            // CRITICAL FIX: Clear the entire EmittersArray and reset EmitterCount
            for (int i = 0; i < EmittersArray.Length; i++)
            {
                EmittersArray[i] = null;
            }
            EmitterCount = 0;
            
            ProjectileEmitterBase[] emittersTemp = GameObject.FindObjectsOfType<ProjectileEmitterBase>();

            Debug.Log($"[ProjectileManager] Found {emittersTemp.Length} emitters in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");

            for (int n = 0; n < emittersTemp.Length; n++)
            {
                if (n >= EmittersArray.Length)
                {
                    Debug.LogError($"[ProjectileManager] Too many emitters ({emittersTemp.Length}) for MaxEmitters ({MaxEmitters}). Some emitters will not be registered!");
                    break;
                }
                
                EmittersArray[n] = emittersTemp[n];
                Debug.Log($"[ProjectileManager] Registering emitter {n}: {emittersTemp[n].gameObject.name}");
                // Default projectile if no projectile type set
                if (EmittersArray[n].ProjectilePrefab == null)
                    EmittersArray[n].ProjectilePrefab = GetProjectilePrefab(0);

                // Increment group counter
                ProjectileTypeCounters[EmittersArray[n].ProjectilePrefab.Index].TotalGroups++;
            }
            
            Debug.Log($"[ProjectileManager] Successfully registered {emittersTemp.Length} emitters, cleared {MaxEmitters - emittersTemp.Length} stale slots");

            // Initialize the emitters -- if value is set fo projectilePoolSize -- system will use it
            // Make sure to not assign projectilePoolSize larger than total material type projectile count
            for (int n = 0; n < EmittersArray.Length; n++)
            {
                if (EmittersArray[n] != null)
                {
                    int projectilesToAssign = EmittersArray[n].ProjectilePrefab.GetMaxProjectileCount();

                    if (projectilesToAssign == -1)
                    {
                        //EmittersArray[n].ProjectilePoolSize = 1000;
                        projectilesToAssign = 1000;
                    }
                    EmittersArray[n].Initialize(projectilesToAssign);
                    ProjectileTypeCounters[EmittersArray[n].ProjectilePrefab.Index].TotalProjectilesAssigned += projectilesToAssign;

                    EmitterCount++;
                    
                }
            }

 

        }



        public ProjectilePrefab GetProjectilePrefab(int index)
        {
            if (index < 0 || index >= ProjectilePrefabs.Count)
            {
                Debug.LogError("Index out of range: " + index);
                return null;
            }
            return ProjectilePrefabs[index];
        }


        public void UpdateBufferData(ProjectilePrefab projectileType, ProjectileData data)
        {
            if (projectileType.Index != LastAccessedProjectileTypeIndex)
            {
                LastAccessedProjectileTypeIndex = projectileType.Index;
                LastAccessedRenderer = IndirectRenderers[LastAccessedProjectileTypeIndex];
            }

            LastAccessedRenderer.UpdateBufferData(projectileType.BufferIndex, data);
            projectileType.IncrementBufferIndex();
        }

        public void Update()
        {
            // Example of adding an emitter at runtime
            //if (Input.GetKeyDown(KeyCode.F))
            //{
            //    GameObject go = GameObject.Instantiate(Resources.Load("Emitters/Spinner") as GameObject);
            //    go.transform.position = new Vector2(1, 0);
            //    AddEmitter(go.GetComponent<ProjectileEmitterAdvanced>(), 1000);
            //    RegisterEmitter(go.GetComponent<ProjectileEmitterAdvanced>());
            //}


            // Provides a simple way to update active Emitters if removing/adding them at runtime for debugging purposes
            // You should be using AddEmitter() if you want to add Emitters at runtime (See above comment).

            // CRITICAL FIX: Update emitters BEFORE drawing to ensure counts are calculated
            UpdateEmitters();  // Calculate projectile counts FIRST
            DrawEmitters();    // Draw based on calculated counts SECOND
        }
          
        public void UpdateEmitters()
        {
            // First, let's check if our registered emitters still exist
            int registeredEmitterCount = 0;
            for (int i = 0; i < EmittersArray.Length; i++)
            {
                if (EmittersArray[i] != null)
                {
                    registeredEmitterCount++;
                }
            }
            Debug.Log($"[ProjectileManager] UpdateEmitters() start - {registeredEmitterCount} registered emitters still exist");
            
            //reset
            for (int n = 0; n < ProjectilePrefabs.Count; n++)
            {
                ProjectileTypeCounters[n].ActiveProjectiles = 0;
                ProjectilePrefabs[n].ResetBufferIndex();
            }

            Debug.Log($"[ProjectileManager] UpdateEmitters() - Processing {EmittersArray.Length} emitter slots, scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
            int activeEmitterCount = 0;
            int totalActiveProjectiles = 0;
            int nullEmitterCount = 0;
            int inactiveGameObjectCount = 0;
            int disabledComponentCount = 0;

            for (int n = 0; n < EmittersArray.Length; n++)
            {
                if (EmittersArray[n] != null)
                {
                    bool gameObjectActive = EmittersArray[n].gameObject.activeSelf;
                    bool componentEnabled = EmittersArray[n].enabled;
                    bool isActive = gameObjectActive && componentEnabled;
                    
                    if (!gameObjectActive)
                    {
                        inactiveGameObjectCount++;
                        Debug.Log($"[ProjectileManager] Emitter {n} ({EmittersArray[n].gameObject.name}) has inactive GameObject");
                    }
                    
                    if (!componentEnabled)
                    {
                        disabledComponentCount++;
                        Debug.Log($"[ProjectileManager] Emitter {n} ({EmittersArray[n].gameObject.name}) has disabled component");
                    }
                    
                    if (isActive)
                    {
                        activeEmitterCount++;
                        
                        // Store count before update
                        int countBefore = EmittersArray[n].ActiveProjectileCount;
                        
                        EmittersArray[n].UpdateEmitter(Time.deltaTime);
                        
                        // Store count after update
                        int countAfter = EmittersArray[n].ActiveProjectileCount;
                        
                        if (countAfter > 0)
                        {
                            Debug.Log($"[ProjectileManager] Emitter {n} ({EmittersArray[n].gameObject.name}): {countBefore} -> {countAfter} projectiles, PrefabIndex: {EmittersArray[n].ProjectilePrefab?.Index}");
                        }

                        // Update projectile counters
                        if (EmittersArray[n].ProjectilePrefab != null)
                        {
                            ProjectileTypeCounters[EmittersArray[n].ProjectilePrefab.Index].ActiveProjectiles += EmittersArray[n].ActiveProjectileCount;
                            totalActiveProjectiles += EmittersArray[n].ActiveProjectileCount;
                        }
                        else
                        {
                            Debug.LogWarning($"[ProjectileManager] Emitter {n} ({EmittersArray[n].gameObject.name}) has NULL ProjectilePrefab!");
                        }

                        // update outlines
                        if (EmittersArray[n].ProjectilePrefab?.Outline != null)
                            ProjectileTypeCounters[EmittersArray[n].ProjectilePrefab.Outline.Index].ActiveProjectiles += EmittersArray[n].ActiveOutlineCount;
                    }
                }
                else
                {
                    nullEmitterCount++;
                }
            }
            
            Debug.Log($"[ProjectileManager] UpdateEmitters() complete - {activeEmitterCount} active emitters, {totalActiveProjectiles} total projectiles");
            Debug.Log($"[ProjectileManager] Emitter status: {nullEmitterCount} null, {inactiveGameObjectCount} inactive GameObjects, {disabledComponentCount} disabled components");
        }

        public void DrawEmitters()
        {
            Debug.Log($"[ProjectileManager] DrawEmitters() - Processing {ProjectilePrefabs.Count} projectile types, scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
            
            int totalDrawnProjectiles = 0;
            
            // We draw all emitters at the same time based on their Projectile Type.  1 draw call per projectile type.
            for (int n = 0; n < ProjectilePrefabs.Count; n++)
            {
                int activeProjectiles = ProjectileTypeCounters[ProjectilePrefabs[n].Index].ActiveProjectiles;
                totalDrawnProjectiles += activeProjectiles;
                
                if (activeProjectiles > 0)
                {
                    Debug.Log($"[ProjectileManager] Drawing {activeProjectiles} projectiles for type {n} (Index: {ProjectilePrefabs[n].Index}) in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                }
                else
                {
                    Debug.Log($"[ProjectileManager] Type {n} (Index: {ProjectilePrefabs[n].Index}) has 0 active projectiles - skipping draw");
                }
                
                IndirectRenderers[ProjectilePrefabs[n].Index].Draw(ProjectileTypeCounters[ProjectilePrefabs[n].Index].ActiveProjectiles);
            }
            
            Debug.Log($"[ProjectileManager] DrawEmitters() complete - Drew {totalDrawnProjectiles} total projectiles");
        }
        
        void OnGUI()
        {
            int total = 0;
            for (int n = 0; n < ProjectileTypeCounters.Count; n++)
            {
                total += ProjectileTypeCounters[n].ActiveProjectiles;
            }
            GUI.Label(new Rect(5, 5, 300, 20), "Projectiles: " + total.ToString());
        }

        void OnApplicationQuit()
        {
            foreach (KeyValuePair<int, IndirectRenderer> kvp in IndirectRenderers)
            {
                kvp.Value.ReleaseBuffers(true);
            }
        }

        void OnDisable()
        {
            foreach (KeyValuePair<int, IndirectRenderer> kvp in IndirectRenderers)
            {
                kvp.Value.ReleaseBuffers(true);
            }
        }
    }

    public class ProjectileTypeCounters
    {
        public int ActiveProjectiles;
        public int TotalProjectilesAssigned;
        public int TotalGroups;
    }
}