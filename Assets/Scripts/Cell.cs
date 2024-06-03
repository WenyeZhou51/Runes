using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell: MonoBehaviour,IDropHandler
{
    public Cards containedCard;
    public GameObject genericCard;
    public Weapon parentWeapon;
    private RectTransform rectTransform;
    public Canvas targetCanvas;
    public float distanceAboveCanvas = 1f;
    public CardsManager manager;
    public int cellIndex;
    private GameObject player;
    private Camera mainCamera;
      

    //instantiate and set card to card in generic card
    // Start is called before the first frame update
   
    private void Start()
    {

       
        
        player = GameObject.FindGameObjectWithTag("Player");


        targetCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        if (containedCard != null)
        {
            GameObject genCard = Instantiate(genericCard, rectTransform);
            Debug.Log("instantiated"+genCard.name);
            RectTransform genCardRectTransform = genCard.GetComponent<RectTransform>();

            genCard.GetComponent<GenericCard>().card = containedCard;
            genCard.GetComponent<GenericCard>().parent = this;

            genCardRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            genCardRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            genCardRectTransform.pivot = new Vector2(0.5f, 0.5f);
            genCardRectTransform.anchoredPosition = Vector2.zero;
        }
            



    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(rectTransform, Input.mousePosition, mainCamera))
        {
            Debug.Log("Ignored");
            return;
        }
        else {
            Debug.Log("Dropped on cell: " + cellIndex);
        }
        

        GameObject droppedObject = eventData.pointerDrag;
        GenericCard genCard = droppedObject.GetComponent<GenericCard>();
        if (genCard.initialParent != null)
        {
            
            if (this.containedCard != null)
            {
                parentWeapon.addToWeapon(containedCard, genCard.initialParent.cellIndex);
                genCard.initialParent.containedCard = this.containedCard;
                Debug.Log("added from other cell");
            }
            parentWeapon.addToWeapon(genCard.card, cellIndex);
            this.containedCard = genCard.card;

        }
        else {
            if (this.containedCard == null)
            {
                parentWeapon.addToWeapon(genCard.card, cellIndex);
                this.containedCard = genCard.card;
                Debug.Log("added from outside");
            }
            else
            {
                Debug.Log("invalid add from outside");
                genCard.returnToInitial();
            }
        }
            
 
       
           
        
    }






}




