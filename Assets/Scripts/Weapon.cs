using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    //public int cardSlots;
    public List<Cards> cards;
    public int index = 0;
    private float nextAttack = 0;
    public int size;
    public int reloadTime;
    public GameObject player;
    [SerializeField] List<Cards> defaultCards;
    public void addToWeapon(Cards card, int insertIndex) {
        if (insertIndex>=0 && insertIndex < size) {
            cards.RemoveAt(insertIndex);
            cards.Insert(insertIndex, card);
        }

    }

    public void removeFromWeapon(int removeIndex)
    {
        if (removeIndex >= 0 && removeIndex < size && cards[removeIndex]!=null)
        {
            cards.RemoveAt(removeIndex);
            cards.Insert(removeIndex, null);
        }

    }
    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");    
        cards = new List<Cards>();
        for (int i = 0; i < size; i++) {
            cards.Add(null);
        }

        for (int i = 0; i < defaultCards.Count; i++)
        {
            Cards defCard = defaultCards[i];
            this.addToWeapon(defCard,i);
        }



    }
    

public void fireWeapon(Vector3 position, Quaternion rotation, bool instant = false ) {
        
        //pos rot is right here
        bool reloaded = false;
        //add time delay
        //time delay messing with the onImpact code.
        if (Time.time >= nextAttack || instant) {
            float fireCost = 0;
            AuxCards curCard;
            (ActionCards actionCard, float manaCost) = GetTargetActionCard(index);
            if (actionCard != null)
            {
                if (manaCost < player.GetComponent<PlayerController>().mana)
                {
                    player.GetComponent<PlayerController>().mana -= manaCost;
                    actionCard.Use(position, rotation);
                    while (!(cards[index] is ActionCards))
                    {
                        if (cards[index] != null)
                        {
                            curCard = (AuxCards)cards[index];
                            curCard.applyMod(actionCard);
                            fireCost += curCard.getDelay();
                        }

                        if (index < cards.Count - 1)
                        {
                            index++;
                        }
                        else
                        {
                            if (instant)
                            {
                                Debug.Log("truncuted");
                                return;
                            }
                            index = 0;
                            if (reloaded == false)
                            {
                                fireCost += reloadTime;
                            }
                            reloaded = true;
                        }
                    }

                    fireCost += actionCard.getDelay();
                    nextAttack = Time.time + fireCost / 100f;

                    if (index < cards.Count - 1)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                    }
                }
               
            }
            
        }
        
    }

    private (ActionCards,float) GetTargetActionCard(int curIndex) {
        float manaCost = 0;
        int entryIndex = curIndex;
        if (cards[curIndex] != null && cards[curIndex] is ActionCards)
        {
            return ((ActionCards)cards[curIndex], cards[curIndex].getManaCost());
        }
        else {
            if (curIndex < cards.Count - 1)
            {
                curIndex++;
            }
            else
            {
                curIndex = 0;
            }
            if (cards[curIndex] != null) {
                manaCost += cards[curIndex].getManaCost();
            }

        }
        
        while (!(cards[curIndex] is ActionCards) && curIndex != entryIndex) {
            if (curIndex < cards.Count - 1)
            {
                curIndex++;
            }
            else { 
                curIndex = 0;
            }
            if (cards[curIndex] != null)
            {
                manaCost += cards[curIndex].getManaCost();
            }

        }
        return ((ActionCards) cards[curIndex],manaCost);
    }
}
