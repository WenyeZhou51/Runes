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
    protected GameObject popUp;
    private SpriteRenderer glyphSpriteRenderer;
    private Cards glyphCard;
    private Sprite glyphImage;
    public string glyphName;
    

    // Start is called before the first frame update
    private void Awake()
    {
       


    }
    void Start()
    {
        glyphSpriteRenderer = GetComponent<SpriteRenderer>();
        this.glyphImage = glyph.getGlyphImage();
        this.glyphCard = glyph.getGlyphCard() as Cards;
        if (glyphCard != null ) { glyphName = glyphCard.getCardName(); }
        glyphSpriteRenderer.sprite = glyphImage;
        DescriptionManager.Instance.RegisterGlyph(this);


    }

    public void HandleMouseOver() {
        if (popUp == null) {
            popUp = Instantiate(glyphPopUp);
            popUp.GetComponent<RectTransform>().localPosition = this.transform.position + spawnDist * Vector3.down;
            GlyphPopUp popUpScript = popUp.GetComponent<GlyphPopUp>();

            if (DescriptionManager.Instance.descriptionMap.TryGetValue(this.glyphName, out string description))
            {
                popUpScript.description = description;
            }
            else {
                popUpScript.description = "?";
            }

            
        }
        
       
    }
    public void HandleMouseExit()
    {
        if (popUp != null)
        {
            GlyphPopUp popUpScript = popUp.GetComponent<GlyphPopUp>();
            popUpScript.fading = true;
            popUp = null;
        }

    }
        


    // Update is called once per frame
    void Update()
    {
        
    }
}
