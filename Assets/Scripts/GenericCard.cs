using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

//fix threshold checking. now world card that touch the threshold will be tp-ed back
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
    public float threshold;
    public GameObject player;
    public bool dragging = false;
    private Vector3 offset;

    public void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player");
        index = -1;
        threshold = Screen.height * 0.72f;
        offset = this.transform.position - player.transform.position;
        //Debug.Log("card spawned at:"+transform.position);
        //Debug.Log("card initialized");
    }
    public void OnPointerDown(PointerEventData eventData) {

    }


    public void OnDrag(PointerEventData eventData)
    {


        // The z-coordinate should be the distance from the camera to the object, not a static value.
        float distanceToScreen = mainCamera.WorldToScreenPoint(gameObject.transform.position).z;

        // Now you get the proper screen position with the distance to screen
        Vector3 screenPosition = new Vector3(eventData.position.x, eventData.position.y, distanceToScreen);

        // Convert the screen position to world position and apply it
        transform.position = mainCamera.ScreenToWorldPoint(screenPosition);
        

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (parent == null)
        {
            index = -1;
        }
        canvasGroup.blocksRaycasts = true;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
        // Check what was hit by the ray
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<Cell>() != null)
            {
                // Call the drop method on the cell
                result.gameObject.GetComponent<Cell>().OnDrop(eventData);
                break;
            }
            else if (Camera.main.WorldToScreenPoint(transform.position).y > threshold) { 
                returnToInitial();
            }
         
        }
        if (Camera.main.WorldToScreenPoint(transform.position).y < threshold && parent !=null) {
            parent.parentWeapon.removeFromWeapon(index);
            parent.manager.displayCells(parent.parentWeapon);
        }
        if (Camera.main.WorldToScreenPoint(transform.position).y < threshold)
        {
            spriteRenderer.sortingLayerName = "Default";
            spriteRenderer.sortingOrder = 2;
        }
        else if (parent != null )
        {
            spriteRenderer.sortingLayerName = "UI";
        }

        dragging = false;
        offset = this.transform.position - player.transform.position;


    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Camera.main.WorldToScreenPoint(transform.position).y > threshold)
        {

        }
        else {
            initialPos = Camera.main.ScreenToWorldPoint(eventData.position);
        }
        



        if (parent != null)
        {
            index = parent.GetComponent<Cell>().cellIndex;
        }
        canvasGroup.blocksRaycasts = false;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
        dragging = true;
        spriteRenderer.sortingLayerName = "UI";
    }

    public void returnToInitial(){
        Debug.Log("returned");
        this.transform.position = initialPos;
    }

    private void Start()
    {
        this.cardImage = card.getCardImage();
        this.cardName = card.getCardName();
        this.cardCost = card.getDelay();
        this.cardImage = card.getCardImage();
        this.manaCost = card.getManaCost();
        offset = this.transform.position - player.transform.position;

        GetComponent<SpriteRenderer>().sprite = cardImage;

        if (parent == null)
        {
            spriteRenderer.sortingLayerName = "Default";
        }
        else {
            spriteRenderer.sortingLayerName = "UI";
        }
    }


    
    // Update is called once per frame
    void Update()
    {
        
        if ((!dragging && Camera.main.WorldToScreenPoint(transform.position).y > threshold && parent != null))
        {
            this.transform.position = player.transform.position + offset;

        }


    }
}
