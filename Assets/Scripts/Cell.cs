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

    //instantiate and set card to card in generic card
    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        targetCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        if (containedCard != null) {

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main,rectTransform.position);

            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, Camera.main.nearClipPlane + distanceAboveCanvas));
            GameObject genCard = Instantiate(genericCard, worldPosition, Quaternion.identity);
            genCard.GetComponent<GenericCard>().card = containedCard;
            genCard.GetComponent<GenericCard>().parent = this;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log("Dropped on cell: " + cellIndex);

        // Get the dropped object
        GameObject droppedObject = eventData.pointerDrag;
        GenericCard genCard = droppedObject.GetComponent<GenericCard>();
        if (genCard.index != -1)
        {
            parentWeapon.addToWeapon(genCard.card, cellIndex);
            parentWeapon.removeFromWeapon(genCard.index);
            if (this.containedCard != null)
            {
                parentWeapon.addToWeapon(containedCard, genCard.index);
                Debug.Log("added from other cell");
            }
            manager.displayCells(parentWeapon);
            Destroy(droppedObject.gameObject);
        }
        else {
            if (this.containedCard == null)
            {
                parentWeapon.addToWeapon(genCard.card, cellIndex);
                Debug.Log("added form outside");
                manager.displayCells(parentWeapon);
                Destroy(droppedObject.gameObject);
            }
            else {
                genCard.returnToInitial();
            }
            
        }
        

        
        
        
    }




    // Update is called once per frame
    void Update()
    {
        
    }

}
