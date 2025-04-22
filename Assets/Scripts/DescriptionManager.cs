using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DescriptionManager : MonoBehaviour
{
    public static DescriptionManager Instance { get; private set; }

    public TMP_InputField inputField;
    public GenericGlyph currentGlyph = null;
    public List<GenericGlyph> allGlyphs = new List<GenericGlyph>();
    public Dictionary<string,string> descriptionMap= new Dictionary<string,string>();//glyphname : description
    public List<string> keyVisualizer;
    public List<string> valVisualizer;
    public void Update()
    {
        keyVisualizer = new List<string>();
        valVisualizer = new List<string>();
        foreach (var key in descriptionMap.Keys)
        {
            keyVisualizer.Add(key);
        }
        foreach (var val in descriptionMap.Values)
        {
            valVisualizer.Add(val);
        }
    }
    void Awake()
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

    void Start()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        
        HideInputField();


    }

    public void RegisterGlyph(GenericGlyph genGlyph)
    {
        allGlyphs.Add(genGlyph);
    }

    public void UnregisterGlyph(GenericGlyph genGlyph)
    {
        allGlyphs.Remove(genGlyph);
    }

    public void ShowInputField()
    {
        inputField.gameObject.SetActive(true);
    }

    public void HideInputField()
    {
        inputField.gameObject.SetActive(false);
    }

    public void HandleEndEdit()
    {
        string description;
        description = inputField.text;
        Debug.Log(description);
        UpdateDescription(currentGlyph, description);
        HideInputField();
    }
    public void UpdateDescription(GenericGlyph target, string description)
    {
        
        descriptionMap[target.glyphName] = description;
    }


}
