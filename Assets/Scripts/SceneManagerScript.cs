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

    public void NextScene() {
        if (SceneManager.GetActiveScene().buildIndex != SceneManager.sceneCountInBuildSettings-1) {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
        
    }

    // Update is called once per frame
    float threshold = Screen.height*0.8f;
    void Update()
    {
       
    }


}

