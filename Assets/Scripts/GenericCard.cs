using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class GenericCard : GenericGlyph, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Cards card;
    private Sprite cardImage;
    private int cardCost;
    private string cardName;
    private float manaCost;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;
    private SpriteRenderer spriteRenderer;
    public Cell parent;
    public Vector2 initialPos;
    public Cell initialParent;
    public GameObject player;
    public bool dragging = false;
    public bool isUI = false;
    public Canvas uiCanvas;
    private Image uiImage;
    private RectTransform rectTransform;
    private bool initialStateUI;
    public float cardsize = 50f;

    public void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();
        uiCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        player = GameObject.FindGameObjectWithTag("Player");
        
    }

    private void Start()
    {
        this.cardImage = card.getCardImage();
        this.cardName = card.getCardName();
        this.cardCost = card.getDelay();
        this.manaCost = card.getManaCost();
        //set sprite
        spawnDist = 1f;
        spriteRenderer.sprite = cardImage;
        uiImage.sprite = cardImage;
        if (glyph != null) {
            description = glyph.getGlyphDescription();
        }
        


        ConvertToUI(true);
        //if parented to cell, then ui. else gameobject.
        if (parent == null)
        {

            ////ConvertToGameObject();
            spriteRenderer.sortingLayerName = "Default";

        }
        else
        {
            spriteRenderer.sortingLayerName = "UI";

        }

    }

    public void setParent(Cell parent) {
        rectTransform.SetParent(parent.transform);
        rectTransform.localPosition = Vector3.zero;
        Debug.Log("new parent set");
    
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        //might need later   
    }

    public void ConvertToUI(bool init = false)
    {

        Debug.Log("converting to ui");
        // ensure same position
        if (init)
        {

            if (parent != null)
            {
                rectTransform.sizeDelta = new Vector2(cardsize, cardsize);
                rectTransform.anchoredPosition = Vector2.zero;
                Debug.Log(rectTransform.lossyScale);
                
            }
            else {
                
                
                rectTransform.sizeDelta = new Vector2(cardsize, cardsize);
                Debug.Log(rectTransform.lossyScale);
            }

            //rectTransform.SetParent(tempParent.transform);
            //rectTransform.anchoredPosition = Vector2.zero;s
            isUI = true;
        }
        else {
            rectTransform.SetParent(uiCanvas.transform, true);
            //ensure same position
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(uiCanvas.transform as RectTransform, screenPos, Camera.main, out Vector3 WorldPos);
            
            //switch to image component
            uiImage.enabled = true;
            spriteRenderer.enabled = false;
            //set pos
            rectTransform.position = WorldPos;
            Debug.Log("later lossy scale" + rectTransform.lossyScale);
            //same image size
            Vector2 spriteWorldSize = spriteRenderer.bounds.size;
            rectTransform.sizeDelta = new Vector2(spriteWorldSize.x / rectTransform.lossyScale.x, spriteWorldSize.y / rectTransform.lossyScale.y);

            //tag as UI
            isUI = true;

        }

       

    }

    public void ConvertToGameObject()
    {

        // Ensure same position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCanvas.worldCamera, rectTransform.position);
        float depth = 100f;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));

        //unparent
        parent = null;
        rectTransform.SetParent(null);
        //set position
        transform.position = worldPosition;
        //switch to spriterenderer
        spriteRenderer.enabled = true;
        uiImage.enabled = false;
        // Ensure same size
        
        Vector2 sizeDelta = rectTransform.sizeDelta;
        Vector3 lossyScale = rectTransform.lossyScale;
        Vector2 worldSize;

        worldSize = new Vector2(sizeDelta.x * lossyScale.x, sizeDelta.y * lossyScale.y);


        this.transform.localScale = worldSize;


       
        
        //toggle as gameobject
        isUI = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        
        //card should be ui at this point
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
        rectTransform.localPosition = localPoint;

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initialPos = transform.position;
        if (!isUI)
        {
            ConvertToUI();
            initialStateUI = false;
        }
        else {
            initialStateUI = true;
        }
        if (parent != null)
        {
            
            initialParent = parent;
            parent.parentWeapon.removeFromWeapon(parent.cellIndex);
            parent.containedCard = null;
            Debug.Log("removed");
        }
        else {
            initialParent = null;
        }
        initialPos = RectTransformUtility.WorldToScreenPoint(Camera.main,this.rectTransform.position);
        UnityEngine.Color color = uiImage.color;
        color.a = 0.5f;
        uiImage.color = color;
        
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {


        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool droppedInCell = false;
        bool inBar = false;
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Cell>() != null)
            {
                parent = result.gameObject.GetComponent<Cell>();
                rectTransform.SetParent(result.gameObject.GetComponent<Cell>().transform);
                rectTransform.localPosition = Vector3.zero;
                parent.handleDrop(this);
                droppedInCell = true;
                break;

            }
            if (result.gameObject.GetComponent<CardsBar>() != null) {
                inBar = true;
            }
        }
        if (!droppedInCell) {
            if (inBar)
            {
                returnToInitial();
            }
            else
            {

                ConvertToGameObject();
            }

        }
        


        dragging = false;
        canvasGroup.blocksRaycasts = true;
    }


public void returnToInitial()
    {
        if (initialStateUI) {
            rectTransform.localPosition = Vector3.zero;
            rectTransform.SetParent(initialParent.transform);
            initialParent.handleDrop(this);
        }
        else {
            Vector3 worldpoint;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform,initialPos,Camera.main,out worldpoint);
            rectTransform.position = worldpoint;
            ConvertToGameObject();
        }
    }

    void Update()
    {
        // Update logic if necessary
    }
}
