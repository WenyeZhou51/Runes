using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeDialogueTrigger : MonoBehaviour
{
    [Header("Visual Cue")]
    [Header("ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    private bool playerInRange;
    private int interact_num = 0;

    private void Awake()
    {
        playerInRange = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((collision.gameObject.tag == "Player"))
        {
            playerInRange = false;
        }
    }

    private void Update()
    {
        if (playerInRange && !DialogueManager.GetInstance().dialoguePlaying)
        {
            if (Input.GetKeyDown(KeyCode.E)) {
                interact_num += 1;
                if (interact_num >= 5) {
                    DialogueManager.GetInstance().EnterDialogue(inkJSON);
                    interact_num = 0;
                }
               
            }

        }

    }
}
