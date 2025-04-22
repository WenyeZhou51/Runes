using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Weapon : MonoBehaviour
{
    private static List<Cards> persistentCards;  // Static list to maintain state

    public List<Cards> cards;
    public int index = 0;
    private float nextAttack = 0;
    public int size;
    public int reloadTime;
    public GameObject player;
    [SerializeField] List<Cards> defaultCards;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        player = GameObject.FindGameObjectWithTag("Player");

        if (persistentCards == null)
        {
            persistentCards = new List<Cards>();
            for (int i = 0; i < size; i++)
            {
                persistentCards.Add(null);
            }

            for (int i = 0; i < defaultCards.Count; i++)
            {
                Cards defCard = defaultCards[i];
                this.addToWeapon(defCard, i);
            }
        }

        cards = persistentCards;  // Link the instance cards to the static list
    }

    public void addToWeapon(Cards card, int insertIndex)
    {
        if (insertIndex >= 0 && insertIndex < size)
        {
            persistentCards.RemoveAt(insertIndex);
            persistentCards.Insert(insertIndex, card);
        }
    }

    public void removeFromWeapon(int removeIndex)
    {
        if (removeIndex >= 0 && removeIndex < size && persistentCards[removeIndex] != null)
        {
            persistentCards.RemoveAt(removeIndex);
            persistentCards.Insert(removeIndex, null);
        }
    }

    public void fireWeapon(Vector3 position, Quaternion rotation, bool external = false)
    {
        //pos rot is right here
        AuxCards curCard;
        if (GetTargetActionCard(index) == (null, -1, float.NaN))
        {
            return;
        }
        (ActionCards actionCard, int actionIndex, float manaCost) = GetTargetActionCard(index);

        //add time delay
        //time delay messing with the onImpact code.
        if (external)
        {
            if (actionCard != null)
            {
                actionCard.Use(position, rotation);
                while (!(persistentCards[index] is ActionCards))
                {
                    if (persistentCards[index] != null)
                    {
                        curCard = (AuxCards)persistentCards[index];
                        curCard.applyMod(actionCard);
                    }

                    if (index < persistentCards.Count - 1)
                    {
                        index++;
                    }
                    else
                    {
                        index = 0;
                        return;
                    }
                }
            }
        }
        else if (Time.time >= nextAttack)
        {
            bool reloaded = false;
            float fireCost = 0;

            if (actionCard != null)
            {
                if (manaCost < player.GetComponent<PlayerController>().mana)
                {
                    player.GetComponent<PlayerController>().mana -= manaCost;
                    actionCard.Use(position, rotation);
                    while (!(persistentCards[index] is ActionCards))
                    {
                        if (persistentCards[index] != null)
                        {
                            curCard = (AuxCards)persistentCards[index];
                            curCard.applyMod(actionCard);
                            fireCost += curCard.getDelay();
                        }

                        if (index < persistentCards.Count - 1)
                        {
                            index++;
                        }
                        else
                        {
                            if (external)
                            {
                                Debug.Log("truncated");
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
                    if (actionIndex == size - 1)
                    {
                        fireCost += reloadTime;
                    }
                    nextAttack = Time.time + fireCost / 100f;

                    if (index < persistentCards.Count - 1)
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

    public (ActionCards, int, float) GetTargetActionCard(int curIndex)
    {
        float manaCost = 0;
        int entryIndex = curIndex;
        if (persistentCards[curIndex] != null && persistentCards[curIndex] is ActionCards)
        {
            return ((ActionCards)persistentCards[curIndex], curIndex, persistentCards[curIndex].getManaCost());
        }
        else
        {
            if (curIndex < persistentCards.Count - 1)
            {
                curIndex++;
            }
            else
            {
                curIndex = 0;
            }
            if (persistentCards[curIndex] != null)
            {
                manaCost += persistentCards[curIndex].getManaCost();
            }
        }

        while (!(persistentCards[curIndex] is ActionCards) && curIndex != entryIndex)
        {
            if (curIndex < persistentCards.Count - 1)
            {
                curIndex++;
            }
            else
            {
                curIndex = 0;
            }
            if (persistentCards[curIndex] != null)
            {
                manaCost += persistentCards[curIndex].getManaCost();
            }
        }
        if (entryIndex == curIndex)
        {
            return (null, -1, float.NaN);
        }
        return ((ActionCards)persistentCards[curIndex], curIndex, manaCost);
    }
}
