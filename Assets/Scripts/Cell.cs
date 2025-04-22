using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cell: MonoBehaviour
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
    public bool onDropFinished = false;

    //instantiate and set card to card in generic card
    // Start is called before the first frame update
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void Start()
    {
        rectTransform.localScale = Vector2.one;


        player = GameObject.FindGameObjectWithTag("Player");


        targetCanvas = FindObjectOfType<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        if (containedCard != null)
        {
            GameObject genCard = Instantiate(genericCard, rectTransform);
            RectTransform genCardRectTransform = genCard.GetComponent<RectTransform>();

            genCard.GetComponent<GenericCard>().card = containedCard;
            genCard.GetComponent<GenericCard>().parent = this;

           
        }



    }


    public void handleDrop(GenericCard genCard) {

        if (genCard.initialParent != null)
        {

            if (this.containedCard != null)
            {
                parentWeapon.addToWeapon(containedCard, genCard.initialParent.cellIndex);
                genCard.initialParent.containedCard = this.containedCard;
                GenericCard curGenCard = GetComponentInChildren<GenericCard>();
                curGenCard.setParent(genCard.initialParent);
                Debug.Log("added from other cell");
            }
            parentWeapon.addToWeapon(genCard.card, cellIndex);
            this.containedCard = genCard.card;

        }
        else
        {
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











