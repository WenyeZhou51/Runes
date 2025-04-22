using System.Collections;
using TMPro;
using UnityEngine;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    private static DialogueManager instance;
    public static DialogueManager GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<DialogueManager>();
            if (instance == null)
            {
                Debug.LogError("No DialogueManager found in scene!");
            }
        }
        return instance;
    }

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject player;
    [SerializeField] private float characterDelay = 0.05f;

    private Story currentStory;
    public bool dialoguePlaying { get; private set; }
    private float originalTimeScale;
    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogWarning("Found more than one DialogueManager in the scene. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        instance = this;

        // Validate required components
        if (dialoguePanel == null) Debug.LogError("DialoguePanel not assigned in DialogueManager!");
        if (dialogueText == null) Debug.LogError("DialogueText not assigned in DialogueManager!");
    }

    private void Start()
    {
        dialoguePlaying = false;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }

    public void EnterDialogue(TextAsset inkJSON)
    {
        if (inkJSON == null)
        {
            Debug.LogError("Attempted to start dialogue with null JSON!");
            return;
        }

        try
        {
            currentStory = new Story(inkJSON.text);
            dialoguePlaying = true;
            dialoguePanel.SetActive(true);
            originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            DisplayNextSentence();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to start dialogue: {e.Message}");
            ExitDialogue();
        }
    }

    private void Update()
    {
        if (!dialoguePlaying) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (currentStory != null && currentStory.canContinue)
            {
                DisplayNextSentence();
            }
            else
            {
                CheckForDialogueEnd();
                ExitDialogue();
            }
        }
    }

    private void DisplayNextSentence()
    {
        if (currentStory == null) return;

        if (currentStory.canContinue)
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            string sentence = currentStory.Continue();
            typingCoroutine = StartCoroutine(TypeSentence(sentence));
        }
        else
        {
            ExitDialogue();
        }
    }

    private IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(characterDelay);
        }
        typingCoroutine = null;
    }

    private void CheckForDialogueEnd()
    {
        if (currentStory == null || currentStory.variablesState == null) return;

        try
        {
            bool dialogueEnded = currentStory.variablesState["dialogueEnded"] != null &&
                               (bool)currentStory.variablesState["dialogueEnded"];
            if (dialogueEnded)
            {
                Debug.Log("Dialogue ended, quitting game");
                Application.Quit();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error checking dialogue end state: {e.Message}");
        }
    }

    public void ExitDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialoguePlaying = false;
        Time.timeScale = originalTimeScale;
        
        // Ensure we don't have a zero time scale after exiting dialogue
        if (Time.timeScale <= 0)
        {
            Time.timeScale = 1f;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";

        currentStory = null;
    }

    private void OnDisable()
    {
        ExitDialogue();
    }
}