using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GenericGlyph : MonoBehaviour
{
    public Glyphs glyph;
    public float spawnDist = 0.5f;
    public GameObject glyphPopUp;
    private GameObject popUp;
    private SpriteRenderer glyphSpriteRenderer;
    protected string description;
    private ScriptableObject glyphCard;
    private Sprite glyphImage;
    

    // Start is called before the first frame update
    private void Awake()
    {
        glyphSpriteRenderer = GetComponent<SpriteRenderer>();
        this.glyphImage = glyph.getGlyphImage();
        this.description = glyph.getGlyphDescription();
        this.glyphCard = glyph.getGlyphCard();
        glyphSpriteRenderer.sprite = glyphImage;

    }
    void Start()
    {
        
        

        
    }

    public void OnMouseOver() {
        Debug.Log(gameObject.name);
        if (popUp == null) {
            Debug.Log("instantiated");
            popUp = Instantiate(glyphPopUp);
            popUp.GetComponent<RectTransform>().localPosition = this.transform.position + spawnDist * Vector3.down;
        }
       GlyphPopUp popUpScript = popUp.GetComponent<GlyphPopUp>();
       popUpScript.description = description;
    }
    public void OnMouseExit()
    {
        Debug.Log("exited");
        if (popUp != null)
        {
            GlyphPopUp popUpScript = popUp.GetComponent<GlyphPopUp>();
            popUpScript.fading = true;
            Debug.Log("fading!");
            popUp = null;
        }
        else {
            Debug.Log("popup is null");
        }
    }
        


    // Update is called once per frame
    void Update()
    {
        
    }
}
