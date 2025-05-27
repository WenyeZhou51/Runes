using BulletHell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    // Start is called before the first frame update
    private static SceneManagerScript instance;
    public static SceneManagerScript Instance
    {
        get {
            if (instance == null)
            {
                instance = FindObjectOfType<SceneManagerScript>();
                if (instance == null) {
                    GameObject singleton = new GameObject("singleton");
                    instance = singleton.AddComponent<SceneManagerScript>();
                }

            }
            return instance;
            
                
        }
    
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);

        }
        else {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void restart() {
        SceneManager.LoadScene(0);
    
    }
    public void NextScene() {
        if (SceneManager.GetActiveScene().buildIndex != SceneManager.sceneCountInBuildSettings-1) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        
    }

    public void loadScene(int index,Vector3? spawnPos = null)
    {
        if (spawnPos != null) { 
            MyGameManager.Instance.PlayerPosition = spawnPos;
        }
        SceneManager.LoadScene(index);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ProjectileManager.Instance.RegisterEmitters();
    }


    // Update is called once per frame
    float threshold = Screen.height*0.8f;
    void Update()
    {
       
    }


}

