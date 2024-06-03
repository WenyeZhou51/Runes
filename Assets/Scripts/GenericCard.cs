using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class GenericCard : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
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
    public int index;
    public Vector2 initialPos;
    public Cell initialParent;
    public GameObject player;
    public bool dragging = false;
    public bool isUI = false;
    public Canvas uiCanvas;
    private Image uiImage;
    private RectTransform rectTransform;

    public void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        uiImage = GetComponent<Image>();
        uiCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        player = GameObject.FindGameObjectWithTag("Player");
        index = -1;
        
    }

    private void Start()
    {
        this.cardImage = card.getCardImage();
        this.cardName = card.getCardName();
        this.cardCost = card.getDelay();
        this.manaCost = card.getManaCost();
        //set sprite
        spriteRenderer.sprite = cardImage;
        uiImage.sprite = cardImage;
        


        //if parented to cell, then ui. else gameobject.
        if (parent == null)
        {
            spriteRenderer.sortingLayerName = "Default";
            ConvertToGameObject(true);
           
        }
        else
        {
            spriteRenderer.sortingLayerName = "UI";

            ConvertToUI(true);
        }
        //might have problems with no initialization for ui/gameobject state
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //might need later   
    }

    public void ConvertToUI(bool init = false)
    {

       
        // ensure same position
        if (init)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(50f, 50f);
            isUI = true;
        }
        else {
            //ensure same position
            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, screenPos, Camera.main, out Vector3 WorldPos);
            //parent
            
            //set pos
            rectTransform.position = WorldPos;
            //switch to image component
            uiImage.enabled = true;
            spriteRenderer.enabled = false;
            //same image size
            Vector2 spriteSize;
            spriteSize = spriteRenderer.bounds.size;
            rectTransform.sizeDelta = spriteSize;
            //tag as UI
            isUI = true;

        }

       

    }

    public void ConvertToGameObject(bool init = false)
    {
   

        // Ensure same position
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCanvas.worldCamera, rectTransform.position);
        float depth = 100f; 
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, depth));
        Debug.Log("Screen Point: " + screenPoint);
        Debug.Log("World Position: " + worldPosition);

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
        Vector2 worldSize = new Vector2(sizeDelta.x * lossyScale.x, sizeDelta.y * lossyScale.y);
        Debug.Log("sizeDelta: " + sizeDelta);
        Debug.Log("lossyScale: " + lossyScale);
        Debug.Log("worldSize: " + worldSize);
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
        Debug.Log("began drag");
        if(!isUI){ 
            ConvertToUI();
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
        
        UnityEngine.Color color = uiImage.color;
        color.a = 0.5f;
        uiImage.color = color;
        initialPos = RectTransformUtility.WorldToScreenPoint(uiCanvas.worldCamera, rectTransform.position);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End drag");
        if (parent == null)
        {
            index = -1;
        }
        canvasGroup.blocksRaycasts = true;
        

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool droppedInCell = false;
        bool inBar = false;
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Cell>() != null)
            {
                parent = result.gameObject.GetComponent<Cell>();
                parent.OnDrop(eventData);
                droppedInCell = true;
                ConvertToUI();
                Debug.Log("Dropped in cell");
                break;
            }
            if (result.gameObject.GetComponent<CardsBar>() != null) {
                inBar = true;
            }
        }
        if (droppedInCell)
        {

        }
        else
        {
            if (inBar)
            {
                returnToInitial();
            }
            else {

                ConvertToGameObject();

                
            }

        }

 

        dragging = false;
    }
    private void DetectCollision()
    {
        // Loop through all potential UI elements to check for collision
        foreach (var potentialTarget in FindObjectsOfType<RectTransform>())
        {
            if (potentialTarget == rectTransform)
                continue; // Skip the dragged element itself

            if (RectTransformUtility.RectangleContainsScreenPoint(potentialTarget, Input.mousePosition, uiCanvas.worldCamera))
            {
                // Collision detected
                HandleCollision(potentialTarget);
            }
        }
    }

    private void HandleCollision(RectTransform target)
    {
        if (target.GetComponent<Cell>()!=null) {
            rectTransform.SetParent(target);
            parent = target.GetComponent<Cell>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero; 

        }



    }

public void returnToInitial()
    {

        //Vector2 localPoint;
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    uiCanvas.transform as RectTransform, initialPos, uiCanvas.worldCamera, out localPoint);
        //rectTransform.anchoredPosition = localPoint;
    }

    void Update()
    {
        // Update logic if necessary
    }
}
